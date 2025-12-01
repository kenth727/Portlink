using PortlinkApp.Core.Data;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace PortlinkApp.Core.Repositories;

public class PortCallRepository : IPortCallRepository
{
    private readonly PortlinkDbContext _context;

    public PortCallRepository(PortlinkDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PortCall>> GetAllAsync(PortCallStatus? status, int pageNumber, int pageSize)
    {
        var query = _context.PortCalls
            .Include(pc => pc.Vessel)
            .Include(pc => pc.Berth)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(pc => pc.Status == status.Value);
        }

        return await query
            .OrderBy(pc => pc.EstimatedTimeOfArrival)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PortCall>> GetByVesselAsync(int vesselId)
    {
        return await _context.PortCalls
            .Include(pc => pc.Vessel)
            .Include(pc => pc.Berth)
            .AsNoTracking()
            .Where(pc => pc.VesselId == vesselId)
            .OrderByDescending(pc => pc.EstimatedTimeOfArrival)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PortCall>> GetByBerthAsync(int berthId)
    {
        return await _context.PortCalls
            .Include(pc => pc.Vessel)
            .Include(pc => pc.Berth)
            .AsNoTracking()
            .Where(pc => pc.BerthId == berthId)
            .OrderByDescending(pc => pc.EstimatedTimeOfArrival)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PortCall>> GetUpcomingAsync(DateTime fromDate, int limit)
    {
        return await _context.PortCalls
            .Include(pc => pc.Vessel)
            .Include(pc => pc.Berth)
            .AsNoTracking()
            .Where(pc => pc.EstimatedTimeOfArrival >= fromDate &&
                        (pc.Status == PortCallStatus.Scheduled ||
                         pc.Status == PortCallStatus.Approaching))
            .OrderBy(pc => pc.EstimatedTimeOfArrival)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PortCall>> GetActiveAsync()
    {
        return await _context.PortCalls
            .Include(pc => pc.Vessel)
            .Include(pc => pc.Berth)
            .AsNoTracking()
            .Where(pc => pc.Status == PortCallStatus.Berthed ||
                        pc.Status == PortCallStatus.InProgress)
            .OrderBy(pc => pc.EstimatedTimeOfDeparture)
            .ToListAsync();
    }

    public async Task<PortCall?> GetByIdAsync(int id)
    {
        return await _context.PortCalls
            .Include(pc => pc.Vessel)
            .Include(pc => pc.Berth)
            .AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.Id == id);
    }

    public async Task<PortCall> AddAsync(PortCall portCall)
    {
        // Validate business rules before adding
        await ValidatePortCallAsync(portCall);

        _context.PortCalls.Add(portCall);
        await _context.SaveChangesAsync();
        return portCall;
    }

    public async Task UpdateAsync(PortCall portCall)
    {
        var existing = await _context.PortCalls.FindAsync(portCall.Id);
        if (existing is null)
        {
            return;
        }

        // Validate business rules if berth or time is changing
        if (existing.BerthId != portCall.BerthId ||
            existing.EstimatedTimeOfArrival != portCall.EstimatedTimeOfArrival ||
            existing.EstimatedTimeOfDeparture != portCall.EstimatedTimeOfDeparture)
        {
            await ValidatePortCallAsync(portCall, portCall.Id);
        }

        existing.VesselId = portCall.VesselId;
        existing.BerthId = portCall.BerthId;
        existing.EstimatedTimeOfArrival = portCall.EstimatedTimeOfArrival;
        existing.EstimatedTimeOfDeparture = portCall.EstimatedTimeOfDeparture;
        existing.ActualTimeOfArrival = portCall.ActualTimeOfArrival;
        existing.ActualTimeOfDeparture = portCall.ActualTimeOfDeparture;
        existing.Status = portCall.Status;
        existing.CargoDescription = portCall.CargoDescription;
        existing.CargoQuantity = portCall.CargoQuantity;
        existing.CargoUnit = portCall.CargoUnit;
        existing.Notes = portCall.Notes;
        existing.DelayReason = portCall.DelayReason;
        existing.PriorityLevel = portCall.PriorityLevel;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.PortCalls.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _context.PortCalls.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync(PortCallStatus? status)
    {
        var query = _context.PortCalls.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(pc => pc.Status == status.Value);
        }

        return await query.CountAsync();
    }

    /// <summary>
    /// Validates business rules for a port call
    /// </summary>
    private async Task ValidatePortCallAsync(PortCall portCall, int? excludePortCallId = null)
    {
        // 1. Validate vessel exists
        var vessel = await _context.Vessels.FindAsync(portCall.VesselId);
        if (vessel is null)
        {
            throw new EntityNotFoundException("Vessel", portCall.VesselId);
        }

        // 2. Validate berth exists
        var berth = await _context.Berths.FindAsync(portCall.BerthId);
        if (berth is null)
        {
            throw new EntityNotFoundException("Berth", portCall.BerthId);
        }

        // 3. Validate berth is available (not in maintenance)
        if (berth.Status == BerthStatus.UnderMaintenance)
        {
            throw new BerthNotAvailableException(berth.BerthCode, "Berth is under maintenance.");
        }

        // 4. Validate vessel dimensions fit berth constraints
        if (vessel.LengthOverall > berth.MaxVesselLength)
        {
            throw new BerthCapacityExceededException(
                berth.BerthCode,
                "length",
                (double)vessel.LengthOverall,
                (double)berth.MaxVesselLength);
        }

        if (vessel.Draft > berth.MaxDraft)
        {
            throw new BerthCapacityExceededException(
                berth.BerthCode,
                "draft",
                (double)vessel.Draft,
                (double)berth.MaxDraft);
        }

        // 5. Validate no overlapping port calls for the same berth
        //
        // For requested / queued calls (Scheduled), only treat already approved / active
        // calls as blocking. This lets operators queue overlapping requests and enforce
        // the constraint when a call is actually approved.
        var overlappingQuery = _context.PortCalls
            .Include(pc => pc.Vessel)
            .Where(pc => pc.BerthId == portCall.BerthId &&
                         pc.Status != PortCallStatus.Completed &&
                         pc.Status != PortCallStatus.Cancelled);

        if (portCall.Status == PortCallStatus.Scheduled)
        {
            overlappingQuery = overlappingQuery.Where(pc =>
                pc.Status == PortCallStatus.Approaching ||
                pc.Status == PortCallStatus.Arrived ||
                pc.Status == PortCallStatus.Berthed ||
                pc.Status == PortCallStatus.InProgress ||
                pc.Status == PortCallStatus.Delayed);
        }

        overlappingQuery = overlappingQuery.Where(pc =>
            pc.EstimatedTimeOfArrival < portCall.EstimatedTimeOfDeparture &&
            pc.EstimatedTimeOfDeparture > portCall.EstimatedTimeOfArrival);

        // Exclude current port call if updating
        if (excludePortCallId.HasValue)
        {
            overlappingQuery = overlappingQuery.Where(pc => pc.Id != excludePortCallId.Value);
        }

        var overlappingPortCall = await overlappingQuery.FirstOrDefaultAsync();
        if (overlappingPortCall is not null)
        {
            throw new OverlappingPortCallException(
                berth.BerthCode,
                overlappingPortCall.Vessel?.Name ?? "Unknown",
                overlappingPortCall.EstimatedTimeOfArrival,
                overlappingPortCall.EstimatedTimeOfDeparture);
        }

        // 6. Validate time range
        if (portCall.EstimatedTimeOfDeparture <= portCall.EstimatedTimeOfArrival)
        {
            throw new ValidationException("Estimated Time of Departure must be after Estimated Time of Arrival.");
        }
    }
}
