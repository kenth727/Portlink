using PortlinkApp.Api.Hubs;
using PortlinkApp.Api.Mappers;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using PortlinkApp.Core.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PortlinkApp.Api.Services;

public class LoadSimulatorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<PortOperationsHub> _hubContext;
    private readonly ILogger<LoadSimulatorService> _logger;
    private readonly IAIService _aiService;
    private int _operationsPerSecond;
    private bool _isRunning;

    public LoadSimulatorService(
        IServiceProvider serviceProvider,
        IHubContext<PortOperationsHub> hubContext,
        ILogger<LoadSimulatorService> logger,
        IAIService aiService)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
        _aiService = aiService;
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
            var scenario = await _aiService.GenerateRealisticPortCallScenario();
            var scenarioData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(scenario);

            if (scenarioData is null)
            {
                return;
            }

            // Prefer AI-suggested IMO if unique, otherwise generate a unique simulator IMO
            string? suggestedImo = null;
            if (scenarioData.TryGetValue("imoNumber", out var imoElement))
            {
                suggestedImo = imoElement.GetString();
            }

            var uniqueImoNumber = await GenerateUniqueImoNumberAsync(vesselRepo, suggestedImo);

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

            var vessel = new Vessel
            {
                Name = scenarioData["vesselName"].GetString() ?? "Unknown Vessel",
                ImoNumber = uniqueImoNumber,
                VesselType = Enum.Parse<VesselType>(scenarioData["vesselType"].GetString() ?? "Container"),
                FlagCountry = scenarioData["flagCountry"].GetString() ?? "Unknown",
                LengthOverall = lengthOverall,
                Beam = beam,
                Draft = draft,
                CargoType = scenarioData["cargoDescription"].GetString(),
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
                CargoDescription = scenarioData["cargoDescription"].GetString(),
                CargoQuantity = scenarioData["cargoQuantity"].GetDecimal(),
                CargoUnit = scenarioData["cargoUnit"].GetString(),
                DelayReason = scenarioData.ContainsKey("delayReason") ? scenarioData["delayReason"].GetString() : null,
                PriorityLevel = scenarioData.ContainsKey("priorityLevel") ? scenarioData["priorityLevel"].GetInt32() : 3
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
        // First try the AI-suggested IMO number if provided
        if (!string.IsNullOrWhiteSpace(suggestedImo))
        {
            var existing = await vesselRepository.GetByImoNumberAsync(suggestedImo);
            if (existing is null)
            {
                return suggestedImo;
            }

            _logger.LogWarning(
                "AI suggested IMO {ImoNumber} which already exists. Generating a simulator IMO instead.",
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
