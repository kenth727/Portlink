using System.ComponentModel.DataAnnotations;
using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Models.Maritime;

public class ManualPortOperationViewModel
{
    [Display(Name = "Vessel")]
    [Required]
    public int SelectedVesselId { get; set; }

    [Display(Name = "Berth")]
    [Required]
    public int SelectedBerthId { get; set; }

    [Display(Name = "Estimated Time of Arrival")]
    [DataType(DataType.DateTime)]
    [Required]
    public DateTime EstimatedTimeOfArrival { get; set; } = DateTime.UtcNow.AddHours(1);

    [Display(Name = "Estimated Time of Departure")]
    [DataType(DataType.DateTime)]
    [Required]
    public DateTime EstimatedTimeOfDeparture { get; set; } = DateTime.UtcNow.AddHours(12);

    [Display(Name = "Cargo Description")]
    public string? CargoDescription { get; set; }

    [Display(Name = "Cargo Quantity")]
    public decimal? CargoQuantity { get; set; }

    [Display(Name = "Cargo Unit")]
    public string? CargoUnit { get; set; }

    [Display(Name = "Test Vessel Length Overall (m)")]
    [Range(0, 2000)]
    public decimal? TestVesselLengthOverall { get; set; }

    [Display(Name = "Test Vessel Draft (m)")]
    [Range(0, 100)]
    public decimal? TestVesselDraft { get; set; }

    public IReadOnlyList<Vessel> Vessels { get; set; } = Array.Empty<Vessel>();

    public IReadOnlyList<Berth> Berths { get; set; } = Array.Empty<Berth>();

    public IReadOnlyList<PortCall> RecentPortCalls { get; set; } = Array.Empty<PortCall>();

    public string? LastOperationSummary { get; set; }
}
