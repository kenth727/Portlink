using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using PortlinkApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PortlinkApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "PortOperator")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IVesselRepository _vesselRepo;
    private readonly IBerthRepository _berthRepo;
    private readonly IPortCallRepository _portCallRepo;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IAIService aiService,
        IVesselRepository vesselRepo,
        IBerthRepository berthRepo,
        IPortCallRepository portCallRepo,
        ILogger<AIController> logger)
    {
        _aiService = aiService;
        _vesselRepo = vesselRepo;
        _berthRepo = berthRepo;
        _portCallRepo = portCallRepo;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<ActionResult<object>> Chat([FromBody] ChatRequest request)
    {
        var vessels = await _vesselRepo.GetAllAsync(null, null, 1, 100);
        var berths = await _berthRepo.GetAllAsync(null, 1, 100);
        var activePortCalls = await _portCallRepo.GetActiveAsync();
        var recentPortCalls = await _portCallRepo.GetUpcomingAsync(DateTime.UtcNow.AddHours(-24), 50);

        var approvedUpcoming = recentPortCalls
            .Where(pc => pc.Status == PortCallStatus.Approaching)
            .ToList();

        var pendingRequests = recentPortCalls
            .Where(pc => pc.Status == PortCallStatus.Scheduled)
            .ToList();

        string FormatPortCallLine(PortCall pc, string label) =>
            $"- [{label}] Vessel: {pc.Vessel?.Name ?? "Unknown"} | Berth: {pc.Berth?.BerthCode ?? "Unknown"} | " +
            $"Status: {pc.Status} | ETA: {pc.EstimatedTimeOfArrival:u} | " +
            $"ETD: {pc.EstimatedTimeOfDeparture:u} | " +
            $"Cargo: {(pc.CargoDescription ?? "N/A")} " +
            $"{(pc.CargoQuantity.HasValue ? pc.CargoQuantity.Value.ToString("0.##") : string.Empty)} {pc.CargoUnit}";

        var activeLines = activePortCalls
            .Select(pc => FormatPortCallLine(pc, "ACTIVE"));

        var approvedLines = approvedUpcoming
            .Select(pc => FormatPortCallLine(pc, "APPROVED_UPCOMING"));

        var pendingLines = pendingRequests
            .Select(pc => FormatPortCallLine(pc, "REQUESTED"));

        // Compute simple per-berth counts for different kinds of port calls
        var berthStats = berths.ToDictionary(
            b => b.Id,
            b => new BerthStats { BerthCode = b.BerthCode });

        void Increment(IDictionary<int, BerthStats> dict, PortCall pc, Action<BerthStats> inc)
        {
            if (dict.TryGetValue(pc.BerthId, out var stats))
            {
                inc(stats);
            }
        }

        foreach (var pc in activePortCalls)
        {
            Increment(berthStats, pc, s => s.Active++);
        }

        foreach (var pc in approvedUpcoming)
        {
            Increment(berthStats, pc, s => s.ApprovedUpcoming++);
        }

        foreach (var pc in pendingRequests)
        {
            Increment(berthStats, pc, s => s.Requested++);
        }

        var berthStatsLines = berthStats.Values
            .OrderByDescending(s => s.Requested + s.ApprovedUpcoming + s.Active)
            .Select(s =>
                $"- {s.BerthCode}: requested={s.Requested}, approvedUpcoming={s.ApprovedUpcoming}, active={s.Active}");

        var context = $@"
            Current Port Status:
            - Total Vessels: {vessels.Count}
            - Available Berths: {berths.Count(b => b.Status == BerthStatus.Available)}
            - Active Port Calls (Berthed/InProgress): {activePortCalls.Count}

            Status semantics:
            - Scheduled: Port call request from an agent, not yet approved.
            - Approaching: Approved port call; vessel is inbound for the assigned berth and time window.
            - Berthed / InProgress: Vessel is currently occupying the berth.
            - Completed / Cancelled / Delayed: Historical or non-active for berth occupancy.

            Guidance for interpreting questions:
            - ""Vessels looking to berth"" usually means vessels with either REQUESTED or APPROVED_UPCOMING port calls for that berth.
            - ""In the queue"" usually refers only to REQUESTED (Scheduled) port calls.
            - Do not invent additional port calls or counts beyond those listed or counted below.

            Vessels: {string.Join(", ", vessels.Select(v => $"{v.Name} ({v.VesselType})"))}
            Berths: {string.Join(", ", berths.Select(b => $"{b.BerthCode} - {b.Status}"))}

            Berth traffic summary (counts by status):
            {(berthStatsLines.Any() ? string.Join("\n", berthStatsLines) : "- None")}

            Active Port Calls (currently at berth):
            {(activeLines.Any() ? string.Join("\n", activeLines) : "- None")}

            Approved Upcoming Port Calls (Approaching, already accepted):
            {(approvedLines.Any() ? string.Join("\n", approvedLines) : "- None")}

            Pending Port Call Requests (Scheduled, not yet approved):
            {(pendingLines.Any() ? string.Join("\n", pendingLines) : "- None")}
            ";

        var answer = await _aiService.AnswerQuestion(request.Question, context);

        return Ok(new { question = request.Question, answer, timestamp = DateTime.UtcNow });
    }

    [HttpGet("recommend-berth/{vesselId}")]
    public async Task<ActionResult<object>> RecommendBerth(int vesselId)
    {
        var vessel = await _vesselRepo.GetByIdAsync(vesselId);
        if (vessel is null)
        {
            return NotFound();
        }

        var berths = await _berthRepo.GetAvailableBerthsAsync();

        var context = $@"
            Vessel: {vessel.Name}
            Type: {vessel.VesselType}
            Length: {vessel.LengthOverall}m, Draft: {vessel.Draft}m
            Cargo: {vessel.CargoType}

            Available Berths:
            {string.Join("\n", berths.Select(b => $"- {b.BerthCode}: Max Length {b.MaxVesselLength}m, Max Draft {b.MaxDraft}m, Facilities: {b.Facilities}"))}
            ";

        var recommendation = await _aiService.GetBerthRecommendation(vesselId, context);

        return Ok(new { vesselId, vesselName = vessel.Name, recommendation, timestamp = DateTime.UtcNow });
    }

    [HttpGet("health")]
    public async Task<ActionResult<object>> Health()
    {
        var available = await _aiService.IsAvailable();
        return Ok(new { lmStudioAvailable = available, timestamp = DateTime.UtcNow });
    }
}

public record ChatRequest(string Question);

file sealed class BerthStats
{
    public required string BerthCode { get; init; }
    public int Active { get; set; }
    public int ApprovedUpcoming { get; set; }
    public int Requested { get; set; }
}
