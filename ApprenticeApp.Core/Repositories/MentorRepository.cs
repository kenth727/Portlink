using ApprenticeApp.Core.Data;
using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApprenticeApp.Core.Repositories;

public class MentorRepository : IMentorRepository
{
    private readonly ApprenticeDbContext _context;

    public MentorRepository(ApprenticeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Mentor>> GetAllAsync()
    {
        return await _context.Mentors.AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Mentor?> GetByIdAsync(int id)
    {
        return await _context.Mentors.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Mentor> AddAsync(Mentor mentor)
    {
        _context.Mentors.Add(mentor);
        await _context.SaveChangesAsync();
        return mentor;
    }

    public async Task UpdateAsync(Mentor mentor)
    {
        _context.Mentors.Update(mentor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Mentors.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _context.Mentors.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
