using ApprenticeApp.Core.Data;
using ApprenticeApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApprenticeApp.Core.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApprenticeDbContext _context;

    public AssignmentRepository(ApprenticeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Assignment>> GetByApprenticeAsync(int apprenticeId)
    {
        return await _context.Assignments.AsNoTracking()
            .Include(a => a.Mentor)
            .Where(a => a.ApprenticeId == apprenticeId)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<Assignment?> GetByIdAsync(int id)
    {
        return await _context.Assignments.AsNoTracking()
            .Include(a => a.Mentor)
            .Include(a => a.Apprentice)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Assignment> AddAsync(Assignment assignment)
    {
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task UpdateAsync(Assignment assignment)
    {
        var existing = await _context.Assignments.FindAsync(assignment.Id);
        if (existing is null)
        {
            return;
        }

        existing.Title = assignment.Title;
        existing.DueDate = assignment.DueDate;
        existing.Status = assignment.Status;
        existing.Notes = assignment.Notes;
        existing.MentorId = assignment.MentorId;
        existing.ApprenticeId = assignment.ApprenticeId;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Assignments.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _context.Assignments.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public Task<int> CountByApprenticeAsync(int apprenticeId)
    {
        return _context.Assignments.CountAsync(a => a.ApprenticeId == apprenticeId);
    }
}
