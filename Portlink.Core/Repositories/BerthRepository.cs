using PortlinkApp.Core.Data;
using PortlinkApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace PortlinkApp.Core.Repositories;

public class BerthRepository : IBerthRepository
{
    private readonly ApprenticeDbContext _context;

    public BerthRepository(ApprenticeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Berth>> GetAllAsync(BerthStatus? status, int pageNumber, int pageSize)
    {
        var query = _context.Berths.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        return await query
            .OrderBy(b => b.BerthCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Berth>> GetAvailableBerthsAsync()
    {
        return await _context.Berths
            .AsNoTracking()
            .Where(b => b.Status == BerthStatus.Available)
            .OrderBy(b => b.BerthCode)
            .ToListAsync();
    }

    public async Task<Berth?> GetByIdAsync(int id)
    {
        return await _context.Berths
            .Include(b => b.PortCalls)
            .ThenInclude(pc => pc.Vessel)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Berth?> GetByBerthCodeAsync(string berthCode)
    {
        return await _context.Berths
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BerthCode == berthCode);
    }

    public async Task<Berth> AddAsync(Berth berth)
    {
        _context.Berths.Add(berth);
        await _context.SaveChangesAsync();
        return berth;
    }

    public async Task UpdateAsync(Berth berth)
    {
        var existing = await _context.Berths.FindAsync(berth.Id);
        if (existing is null)
        {
            return;
        }

        existing.BerthCode = berth.BerthCode;
        existing.TerminalName = berth.TerminalName;
        existing.MaxVesselLength = berth.MaxVesselLength;
        existing.MaxDraft = berth.MaxDraft;
        existing.Facilities = berth.Facilities;
        existing.Status = berth.Status;
        existing.Notes = berth.Notes;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Berths.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _context.Berths.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync(BerthStatus? status)
    {
        var query = _context.Berths.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        return await query.CountAsync();
    }
}
