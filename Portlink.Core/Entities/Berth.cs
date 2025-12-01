using System.ComponentModel.DataAnnotations;

namespace PortlinkApp.Core.Entities;

public class Berth
{
    public int Id { get; set; }

    [MaxLength(50)]
    public required string BerthCode { get; set; }

    [MaxLength(200)]
    public required string TerminalName { get; set; }

    public decimal MaxVesselLength { get; set; } // meters

    public decimal MaxDraft { get; set; } // meters

    [MaxLength(500)]
    public string? Facilities { get; set; } // e.g., "Cranes: 3, Fuel: Yes, Water: Yes"

    public BerthStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public ICollection<PortCall> PortCalls { get; set; } = new List<PortCall>();
}
