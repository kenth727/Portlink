using PortlinkApp.Core.Entities;

namespace PortlinkApp.Core.Repositories;

public interface IPortCallRepository
{
    Task<IReadOnlyList<PortCall>> GetAllAsync(PortCallStatus? status, int pageNumber, int pageSize);
    Task<IReadOnlyList<PortCall>> GetByVesselAsync(int vesselId);
    Task<IReadOnlyList<PortCall>> GetByBerthAsync(int berthId);
    Task<IReadOnlyList<PortCall>> GetUpcomingAsync(DateTime fromDate, int limit);
    Task<IReadOnlyList<PortCall>> GetActiveAsync();
    Task<PortCall?> GetByIdAsync(int id);
    Task<PortCall> AddAsync(PortCall portCall);
    Task UpdateAsync(PortCall portCall);
    Task DeleteAsync(int id);
    Task<int> CountAsync(PortCallStatus? status);
}
