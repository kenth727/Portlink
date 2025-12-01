using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Dtos;

public record BerthDto(
    int Id,
    string BerthCode,
    string TerminalName,
    decimal MaxVesselLength,
    decimal MaxDraft,
    string? Facilities,
    BerthStatus Status,
    string? Notes);
