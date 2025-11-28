using ApprenticeApp.Core.Data;
using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApprenticeApp.Core.Repositories;

public class ApprenticeRepository : IApprenticeRepository
{
    private readonly ApprenticeDbContext _context;

    public ApprenticeRepository(ApprenticeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Apprentice>> GetAllAsync(ApprenticeStatus? status, ApprenticeTrack? track, int pageNumber, int pageSize)
    {
        var query = _context.Apprentices.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (track.HasValue)
        {
            query = query.Where(a => a.Track == track.Value);
        }

        return await query
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(ApprenticeStatus? status, ApprenticeTrack? track)
    {
        var query = _context.Apprentices.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (track.HasValue)
        {
            query = query.Where(a => a.Track == track.Value);
        }

        return await query.CountAsync();
    }

    public async Task<IReadOnlyList<Apprentice>> GetActiveAsync()
    {
        return await _context.Apprentices.AsNoTracking()
            .Where(a => a.Status == ApprenticeStatus.Active)
            .OrderBy(a => a.LastName)
            .ToListAsync();
    }

    public async Task<Apprentice?> GetByIdAsync(int id)
    {
        return await _context.Apprentices
            .Include(a => a.Assignments)
            .ThenInclude(x => x.Mentor)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Apprentice> AddAsync(Apprentice apprentice)
    {
        _context.Apprentices.Add(apprentice);
        await _context.SaveChangesAsync();
        return apprentice;
    }

    public async Task UpdateAsync(Apprentice apprentice)
    {
        var existing = await _context.Apprentices.FindAsync(apprentice.Id);
        if (existing is null)
        {
            return;
        }

        existing.FirstName = apprentice.FirstName;
        existing.LastName = apprentice.LastName;
        existing.Email = apprentice.Email;
        existing.StartDate = apprentice.StartDate;
        existing.Track = apprentice.Track;
        existing.Status = apprentice.Status;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Apprentices.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _context.Apprentices.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
