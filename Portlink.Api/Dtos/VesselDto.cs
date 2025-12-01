using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Dtos;

public record VesselDto(
    int Id,
    string Name,
    string ImoNumber,
    VesselType VesselType,
    string FlagCountry,
    decimal LengthOverall,
    decimal Beam,
    decimal Draft,
    string? CargoType,
    int? Capacity,
    VesselStatus Status,
    string? OwnerCompany,
    string? AgentEmail);
