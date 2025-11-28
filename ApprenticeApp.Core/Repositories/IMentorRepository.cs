using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Core.Repositories;

public interface IMentorRepository
{
    Task<IReadOnlyList<Mentor>> GetAllAsync();
    Task<Mentor?> GetByIdAsync(int id);
    Task<Mentor> AddAsync(Mentor mentor);
    Task UpdateAsync(Mentor mentor);
    Task DeleteAsync(int id);
}
