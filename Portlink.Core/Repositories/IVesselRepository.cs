using PortlinkApp.Core.Entities;

namespace PortlinkApp.Core.Repositories;

public interface IVesselRepository
{
    Task<IReadOnlyList<Vessel>> GetAllAsync(VesselStatus? status, VesselType? vesselType, int pageNumber, int pageSize);
    Task<Vessel?> GetByIdAsync(int id);
    Task<Vessel?> GetByImoNumberAsync(string imoNumber);
    Task<Vessel> AddAsync(Vessel vessel);
    Task UpdateAsync(Vessel vessel);
    Task DeleteAsync(int id);
    Task<int> CountAsync(VesselStatus? status, VesselType? vesselType);
}
