using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Core.Repositories;

public interface IAssignmentRepository
{
    Task<IReadOnlyList<Assignment>> GetByApprenticeAsync(int apprenticeId);
    Task<Assignment?> GetByIdAsync(int id);
    Task<Assignment> AddAsync(Assignment assignment);
    Task UpdateAsync(Assignment assignment);
    Task DeleteAsync(int id);
    Task<int> CountByApprenticeAsync(int apprenticeId);
}
