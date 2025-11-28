using ApprenticeApp.Core.Entities;

namespace ApprenticeApp.Core.Repositories;

public interface IApprenticeRepository
{
    Task<IReadOnlyList<Apprentice>> GetAllAsync(ApprenticeStatus? status, ApprenticeTrack? track, int pageNumber, int pageSize);
    Task<IReadOnlyList<Apprentice>> GetActiveAsync();
    Task<Apprentice?> GetByIdAsync(int id);
    Task<Apprentice> AddAsync(Apprentice apprentice);
    Task UpdateAsync(Apprentice apprentice);
    Task DeleteAsync(int id);
    Task<int> CountAsync(ApprenticeStatus? status, ApprenticeTrack? track);
}
