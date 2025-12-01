using PortlinkApp.Core.Entities;

namespace PortlinkApp.Core.Repositories;

public interface IBerthRepository
{
    Task<IReadOnlyList<Berth>> GetAllAsync(BerthStatus? status, int pageNumber, int pageSize);
    Task<IReadOnlyList<Berth>> GetAvailableBerthsAsync();
    Task<Berth?> GetByIdAsync(int id);
    Task<Berth?> GetByBerthCodeAsync(string berthCode);
    Task<Berth> AddAsync(Berth berth);
    Task UpdateAsync(Berth berth);
    Task DeleteAsync(int id);
    Task<int> CountAsync(BerthStatus? status);
}
