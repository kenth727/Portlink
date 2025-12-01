using PortlinkApp.Core.Data;
using PortlinkApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace PortlinkApp.Core.Repositories;

public class VesselRepository : IVesselRepository
{
    private readonly PortlinkDbContext _context;

    public VesselRepository(PortlinkDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Vessel>> GetAllAsync(VesselStatus? status, VesselType? vesselType, int pageNumber, int pageSize)
    {
        var query = _context.Vessels.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        if (vesselType.HasValue)
        {
            query = query.Where(v => v.VesselType == vesselType.Value);
        }

        return await query
            .OrderBy(v => v.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Vessel?> GetByIdAsync(int id)
    {
        return await _context.Vessels
            .Include(v => v.PortCalls)
            .ThenInclude(pc => pc.Berth)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vessel?> GetByImoNumberAsync(string imoNumber)
    {
        return await _context.Vessels
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.ImoNumber == imoNumber);
    }

    public async Task<Vessel> AddAsync(Vessel vessel)
    {
        _context.Vessels.Add(vessel);
        await _context.SaveChangesAsync();
        return vessel;
    }

    public async Task UpdateAsync(Vessel vessel)
    {
        var existing = await _context.Vessels.FindAsync(vessel.Id);
        if (existing is null)
        {
            return;
        }

        existing.Name = vessel.Name;
        existing.ImoNumber = vessel.ImoNumber;
        existing.VesselType = vessel.VesselType;
        existing.FlagCountry = vessel.FlagCountry;
        existing.LengthOverall = vessel.LengthOverall;
        existing.Beam = vessel.Beam;
        existing.Draft = vessel.Draft;
        existing.CargoType = vessel.CargoType;
        existing.Capacity = vessel.Capacity;
        existing.Status = vessel.Status;
        existing.OwnerCompany = vessel.OwnerCompany;
        existing.AgentEmail = vessel.AgentEmail;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Vessels.FindAsync(id);
        if (entity is null)
        {
            return;
        }

        _context.Vessels.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync(VesselStatus? status, VesselType? vesselType)
    {
        var query = _context.Vessels.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        if (vesselType.HasValue)
        {
            query = query.Where(v => v.VesselType == vesselType.Value);
        }

        return await query.CountAsync();
    }
}
