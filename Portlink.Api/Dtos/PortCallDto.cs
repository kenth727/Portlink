using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Dtos;

public record PortCallDto(
    int Id,
    int VesselId,
    string VesselName,
    int BerthId,
    string BerthCode,
    DateTime EstimatedTimeOfArrival,
    DateTime EstimatedTimeOfDeparture,
    DateTime? ActualTimeOfArrival,
    DateTime? ActualTimeOfDeparture,
    PortCallStatus Status,
    string? CargoDescription,
    decimal? CargoQuantity,
    string? CargoUnit,
    string? Notes,
    string? DelayReason,
    int? PriorityLevel);
