using PortlinkApp.Api.Hubs;
using PortlinkApp.Api.Mappers;
using PortlinkApp.Api.Models.Maritime;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace PortlinkApp.Api.Controllers;

[Route("Maritime")]
public class MaritimeController : Controller
{
    private readonly IVesselRepository _vesselRepository;
    private readonly IBerthRepository _berthRepository;
    private readonly IPortCallRepository _portCallRepository;
    private readonly IHubContext<PortOperationsHub> _hubContext;

    public MaritimeController(
        IVesselRepository vesselRepository,
        IBerthRepository berthRepository,
        IPortCallRepository portCallRepository,
        IHubContext<PortOperationsHub> hubContext)
    {
        _vesselRepository = vesselRepository;
        _berthRepository = berthRepository;
        _portCallRepository = portCallRepository;
        _hubContext = hubContext;
    }

    [HttpGet("Manual")]
    public async Task<IActionResult> Manual()
    {
        var model = new ManualPortOperationViewModel();
        await PopulateListsAsync(model);
        return View(model);
    }

    [HttpPost("Manual")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Manual(ManualPortOperationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateListsAsync(model);
            return View(model);
        }

        // Optional: override vessel dimensions for testing berth capacity rules
        if (model.TestVesselLengthOverall.HasValue || model.TestVesselDraft.HasValue)
        {
            var vessel = await _vesselRepository.GetByIdAsync(model.SelectedVesselId);
            if (vessel is not null)
            {
                if (model.TestVesselLengthOverall.HasValue)
                {
                    vessel.LengthOverall = model.TestVesselLengthOverall.Value;
                }

                if (model.TestVesselDraft.HasValue)
                {
                    vessel.Draft = model.TestVesselDraft.Value;
                }

                await _vesselRepository.UpdateAsync(vessel);
            }
        }

        static DateTime AsUtc(DateTime value) =>
            value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                // Treat unspecified values (e.g., from <input type="datetime-local">)
                // as local times and convert them to UTC.
                _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
            };

        var portCall = new PortCall
        {
            VesselId = model.SelectedVesselId,
            BerthId = model.SelectedBerthId,
            EstimatedTimeOfArrival = AsUtc(model.EstimatedTimeOfArrival),
            EstimatedTimeOfDeparture = AsUtc(model.EstimatedTimeOfDeparture),
            Status = PortCallStatus.Scheduled,
            CargoDescription = model.CargoDescription,
            CargoQuantity = model.CargoQuantity,
            CargoUnit = model.CargoUnit,
            PriorityLevel = 3
        };

        var created = await _portCallRepository.AddAsync(portCall);
        var full = await _portCallRepository.GetByIdAsync(created.Id);

        if (full is not null)
        {
            await _hubContext.Clients.All.SendAsync("PortCallChanged", full.ToDto());
            model.LastOperationSummary =
                $"Created port call for {full.Vessel?.Name} at berth {full.Berth?.BerthCode} " +
                $"(ETA (local): {full.EstimatedTimeOfArrival.ToLocalTime():g}).";
        }

        ModelState.Clear();
        await PopulateListsAsync(model);
        return View(model);
    }

    private async Task PopulateListsAsync(ManualPortOperationViewModel model)
    {
        model.Vessels = await _vesselRepository.GetAllAsync(null, null, 1, 100);
        model.Berths = await _berthRepository.GetAllAsync(null, 1, 100);
        model.RecentPortCalls = await _portCallRepository.GetUpcomingAsync(DateTime.UtcNow.AddHours(-24), 10);
    }
}
