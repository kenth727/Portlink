using System.ComponentModel.DataAnnotations;
using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Dtos;

public record VesselRequest(
    [Required][MaxLength(100)] string Name,
    [Required][MaxLength(20)] string ImoNumber,
    VesselType VesselType,
    [Required][MaxLength(100)] string FlagCountry,
    [Range(0, 500)] decimal LengthOverall,
    [Range(0, 100)] decimal Beam,
    [Range(0, 50)] decimal Draft,
    [MaxLength(100)] string? CargoType,
    [Range(0, int.MaxValue)] int? Capacity,
    VesselStatus Status,
    [MaxLength(200)] string? OwnerCompany,
    [MaxLength(200)][EmailAddress] string? AgentEmail);
