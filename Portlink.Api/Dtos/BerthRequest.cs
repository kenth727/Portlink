using System.ComponentModel.DataAnnotations;
using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Dtos;

public record BerthRequest(
    [Required][MaxLength(50)] string BerthCode,
    [Required][MaxLength(200)] string TerminalName,
    [Range(0, 500)] decimal MaxVesselLength,
    [Range(0, 50)] decimal MaxDraft,
    [MaxLength(500)] string? Facilities,
    BerthStatus Status,
    [MaxLength(1000)] string? Notes);
