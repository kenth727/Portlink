using PortlinkApp.Api.Hubs;
using PortlinkApp.Api.Mappers;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace PortlinkApp.Api.Services;

public class LoadSimulatorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<PortOperationsHub> _hubContext;
    private readonly ILogger<LoadSimulatorService> _logger;
    private int _operationsPerSecond;
    private bool _isRunning;

    public LoadSimulatorService(
        IServiceProvider serviceProvider,
        IHubContext<PortOperationsHub> hubContext,
        ILogger<LoadSimulatorService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_isRunning)
            {
                await SimulatePortOperations(stoppingToken);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    public void Start(int operationsPerSecond)
    {
        _isRunning = true;
        _operationsPerSecond = operationsPerSecond;
        _logger.LogInformation("Load simulator started: {Ops} ops/sec", operationsPerSecond);
    }

    public void Stop()
    {
        _isRunning = false;
        _logger.LogInformation("Load simulator stopped");
    }

    private async Task SimulatePortOperations(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var vesselRepo = scope.ServiceProvider.GetRequiredService<IVesselRepository>();
        var berthRepo = scope.ServiceProvider.GetRequiredService<IBerthRepository>();
        var portCallRepo = scope.ServiceProvider.GetRequiredService<IPortCallRepository>();
        try
        {
            // Generate a simple, realistic scenario without relying on the AI service.
            var uniqueImoNumber = await GenerateUniqueImoNumberAsync(vesselRepo, suggestedImo: null);

            var berths = await berthRepo.GetAvailableBerthsAsync();
            if (!berths.Any())
            {
                _logger.LogInformation("No available berths found for load simulator. Skipping this cycle.");
                return;
            }

            // Pick a random available berth and size the vessel so it is likely to fit
            var berth = berths[Random.Shared.Next(berths.Count)];

            decimal lengthOverall;
            decimal draft;
            decimal beam;

            if (berth.MaxVesselLength > 0)
            {
                var factor = (decimal)(0.5 + Random.Shared.NextDouble() * 0.4); // 50–90% of max length
                lengthOverall = Math.Round(berth.MaxVesselLength * factor, 2);
            }
            else
            {
                lengthOverall = Random.Shared.Next(100, 400);
            }

            if (berth.MaxDraft > 0)
            {
                var factor = (decimal)(0.5 + Random.Shared.NextDouble() * 0.4); // 50–90% of max draft
                draft = Math.Round(berth.MaxDraft * factor, 2);
            }
            else
            {
                draft = Random.Shared.Next(8, 18);
            }

            // Beam roughly proportional to length with slight randomness
            beam = Math.Round(lengthOverall * 0.15m + (decimal)Random.Shared.NextDouble() * 2m, 2);

            // Generate basic vessel / cargo characteristics
            var vesselType = (VesselType)Random.Shared.Next(0, Enum.GetValues(typeof(VesselType)).Length);
            var vesselName = vesselType switch
            {
                VesselType.Container => "Sim Container Vessel",
                VesselType.Tanker => "Sim Tanker Vessel",
                VesselType.BulkCarrier => "Sim Bulk Carrier",
                VesselType.RoRo => "Sim RoRo Vessel",
                VesselType.Cruise => "Sim Cruise Ship",
                VesselType.GeneralCargo => "Sim General Cargo Vessel",
                VesselType.Reefer => "Sim Reefer Vessel",
                _ => "Sim Vessel"
            };

            var flagCountries = new[]
            {
                "Panama", "Denmark", "Marshall Islands", "Belgium",
                "Bahamas", "Singapore", "Norway", "United Kingdom"
            };
            var flagCountry = flagCountries[Random.Shared.Next(flagCountries.Length)];

            string cargoDescription;
            string cargoUnit;
            decimal cargoQuantity;

            switch (vesselType)
            {
                case VesselType.Container:
                case VesselType.GeneralCargo:
                case VesselType.Reefer:
                    cargoDescription = "Mixed Containers";
                    cargoUnit = "TEU";
                    cargoQuantity = Random.Shared.Next(500, 18001);
                    break;
                case VesselType.Tanker:
                    cargoDescription = "Crude Oil";
                    cargoUnit = "tons";
                    cargoQuantity = Random.Shared.Next(100000, 450001);
                    break;
                case VesselType.BulkCarrier:
                    cargoDescription = "Iron Ore";
                    cargoUnit = "tons";
                    cargoQuantity = Random.Shared.Next(50000, 350001);
                    break;
                case VesselType.RoRo:
                    cargoDescription = "Vehicles";
                    cargoUnit = "units";
                    cargoQuantity = Random.Shared.Next(200, 4001);
                    break;
                case VesselType.Cruise:
                    cargoDescription = "Passengers";
                    cargoUnit = "persons";
                    cargoQuantity = Random.Shared.Next(1000, 7001);
                    break;
                default:
                    cargoDescription = "General Cargo";
                    cargoUnit = "tons";
                    cargoQuantity = Random.Shared.Next(1000, 50001);
                    break;
            }

            var vessel = new Vessel
            {
                Name = vesselName,
                ImoNumber = uniqueImoNumber,
                VesselType = vesselType,
                FlagCountry = flagCountry,
                LengthOverall = lengthOverall,
                Beam = beam,
                Draft = draft,
                CargoType = cargoDescription,
                Status = VesselStatus.Approaching
            };

            var createdVessel = await vesselRepo.AddAsync(vessel);

            // Choose a time window that avoids overlapping existing port calls on this berth
            var existingPortCalls = await portCallRepo.GetByBerthAsync(berth.Id);
            var activePortCalls = existingPortCalls
                .Where(pc => pc.Status != PortCallStatus.Completed &&
                             pc.Status != PortCallStatus.Cancelled)
                .OrderBy(pc => pc.EstimatedTimeOfArrival)
                .ToList();

            var now = DateTime.UtcNow;
            DateTime eta;

            if (activePortCalls.Any())
            {
                var last = activePortCalls
                    .OrderByDescending(pc => pc.EstimatedTimeOfDeparture)
                    .First();

                var baseTime = last.EstimatedTimeOfDeparture > now
                    ? last.EstimatedTimeOfDeparture
                    : now;

                // Start sometime 30–180 minutes after the last departure / now
                eta = baseTime.AddMinutes(Random.Shared.Next(30, 181));
            }
            else
            {
                // No active calls for this berth; schedule in the near future
                eta = now.AddHours(Random.Shared.Next(1, 24));
            }

            var durationHours = Random.Shared.Next(8, 37); // 8–36 hours stay
            var etd = eta.AddHours(durationHours);

            var portCall = new PortCall
            {
                VesselId = createdVessel.Id,
                BerthId = berth.Id,
                EstimatedTimeOfArrival = eta,
                EstimatedTimeOfDeparture = etd,
                Status = PortCallStatus.Scheduled,
                CargoDescription = cargoDescription,
                CargoQuantity = cargoQuantity,
                CargoUnit = cargoUnit,
                DelayReason = null,
                PriorityLevel = Random.Shared.Next(1, 6)
            };

            var createdPortCall = await portCallRepo.AddAsync(portCall);

            await _hubContext.Clients.All.SendAsync("VesselChanged", createdVessel.ToDto(), cancellationToken: stoppingToken);
            await _hubContext.Clients.All.SendAsync("PortCallChanged", createdPortCall.ToDto(), cancellationToken: stoppingToken);
            await _hubContext.Clients.All.SendAsync("LoadSimulatorMetrics", new
            {
                operationsPerSecond = _operationsPerSecond,
                timestamp = DateTime.UtcNow,
                lastOperation = "Created port call for " + vessel.Name
            }, cancellationToken: stoppingToken);

            _logger.LogInformation("Simulated port call for {Vessel}", vessel.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in load simulator");
        }
    }

    /// <summary>
    /// Generates a unique IMO number for simulated vessels.
    /// Uses the AI-suggested IMO if it does not already exist; otherwise
    /// falls back to random IMO numbers and finally a time-based value.
    /// </summary>
    private async Task<string> GenerateUniqueImoNumberAsync(
        IVesselRepository vesselRepository,
        string? suggestedImo)
    {
        // First try the suggested IMO number if provided
        if (!string.IsNullOrWhiteSpace(suggestedImo))
        {
            var existing = await vesselRepository.GetByImoNumberAsync(suggestedImo);
            if (existing is null)
            {
                return suggestedImo;
            }

            _logger.LogWarning(
                "Suggested IMO {ImoNumber} which already exists. Generating a simulator IMO instead.",
                suggestedImo);
        }

        // Try a few random IMO numbers (IMO + 7 digits)
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var randomNumber = Random.Shared.Next(0, 10_000_000);
            var candidate = $"IMO{randomNumber:D7}";

            var existing = await vesselRepository.GetByImoNumberAsync(candidate);
            if (existing is null)
            {
                return candidate;
            }
        }

        // Fallback to a time-based IMO number if we somehow keep colliding
        var fallbackNumber = (DateTime.UtcNow.Ticks % 10_000_000);
        var fallbackImo = $"IMO{fallbackNumber:D7}";

        _logger.LogWarning(
            "Falling back to time-based IMO {ImoNumber} for simulated vessel.",
            fallbackImo);

        return fallbackImo;
    }
}
