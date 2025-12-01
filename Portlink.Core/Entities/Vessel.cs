using System.ComponentModel.DataAnnotations;

namespace PortlinkApp.Core.Entities;

public class Vessel
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(20)]
    public required string ImoNumber { get; set; }

    public VesselType VesselType { get; set; }

    [MaxLength(100)]
    public required string FlagCountry { get; set; }

    public decimal LengthOverall { get; set; } // meters

    public decimal Beam { get; set; } // meters (width)

    public decimal Draft { get; set; } // meters (depth)

    [MaxLength(100)]
    public string? CargoType { get; set; }

    public int? Capacity { get; set; } // TEU for containers, DWT for others

    public VesselStatus Status { get; set; }

    [MaxLength(200)]
    public string? OwnerCompany { get; set; }

    [MaxLength(200)]
    public string? AgentEmail { get; set; }

    public ICollection<PortCall> PortCalls { get; set; } = new List<PortCall>();
}
