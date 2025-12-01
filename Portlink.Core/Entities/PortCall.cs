using System.ComponentModel.DataAnnotations;

namespace PortlinkApp.Core.Entities;

public class PortCall
{
    public int Id { get; set; }

    public int VesselId { get; set; }

    public Vessel Vessel { get; set; } = null!;

    public int BerthId { get; set; }

    public Berth Berth { get; set; } = null!;

    public DateTime EstimatedTimeOfArrival { get; set; }

    public DateTime EstimatedTimeOfDeparture { get; set; }

    public DateTime? ActualTimeOfArrival { get; set; }

    public DateTime? ActualTimeOfDeparture { get; set; }

    public PortCallStatus Status { get; set; }

    [MaxLength(100)]
    public string? CargoDescription { get; set; }

    public decimal? CargoQuantity { get; set; } // in tons or TEU

    [MaxLength(50)]
    public string? CargoUnit { get; set; } // "tons", "TEU", "m³", etc.

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(200)]
    public string? DelayReason { get; set; }

    public int? PriorityLevel { get; set; } // 1-5, with 1 being highest priority
}
