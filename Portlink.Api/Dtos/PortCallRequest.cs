using System.ComponentModel.DataAnnotations;
using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Dtos;

public record PortCallRequest(
    [Required] int VesselId,
    [Required] int BerthId,
    [Required] DateTime EstimatedTimeOfArrival,
    [Required] DateTime EstimatedTimeOfDeparture,
    DateTime? ActualTimeOfArrival,
    DateTime? ActualTimeOfDeparture,
    PortCallStatus Status,
    [MaxLength(100)] string? CargoDescription,
    [Range(0, double.MaxValue)] decimal? CargoQuantity,
    [MaxLength(50)] string? CargoUnit,
    [MaxLength(1000)] string? Notes,
    [MaxLength(200)] string? DelayReason,
    [Range(1, 5)] int? PriorityLevel);
