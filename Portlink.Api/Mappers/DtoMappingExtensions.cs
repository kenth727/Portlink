using PortlinkApp.Api.Dtos;
using PortlinkApp.Core.Entities;

namespace PortlinkApp.Api.Mappers;

public static class DtoMappingExtensions
{
    public static VesselDto ToDto(this Vessel vessel) =>
        new(
            vessel.Id,
            vessel.Name,
            vessel.ImoNumber,
            vessel.VesselType,
            vessel.FlagCountry,
            vessel.LengthOverall,
            vessel.Beam,
            vessel.Draft,
            vessel.CargoType,
            vessel.Capacity,
            vessel.Status,
            vessel.OwnerCompany,
            vessel.AgentEmail);

    public static BerthDto ToDto(this Berth berth) =>
        new(
            berth.Id,
            berth.BerthCode,
            berth.TerminalName,
            berth.MaxVesselLength,
            berth.MaxDraft,
            berth.Facilities,
            berth.Status,
            berth.Notes);

    public static PortCallDto ToDto(this PortCall portCall) =>
        new(
            portCall.Id,
            portCall.VesselId,
            portCall.Vessel?.Name ?? string.Empty,
            portCall.BerthId,
            portCall.Berth?.BerthCode ?? string.Empty,
            portCall.EstimatedTimeOfArrival,
            portCall.EstimatedTimeOfDeparture,
            portCall.ActualTimeOfArrival,
            portCall.ActualTimeOfDeparture,
            portCall.Status,
            portCall.CargoDescription,
            portCall.CargoQuantity,
            portCall.CargoUnit,
            portCall.Notes,
            portCall.DelayReason,
            portCall.PriorityLevel);

    public static Vessel ToEntity(this VesselRequest request, int? id = null)
    {
        return new Vessel
        {
            Id = id ?? 0,
            Name = request.Name,
            ImoNumber = request.ImoNumber,
            VesselType = request.VesselType,
            FlagCountry = request.FlagCountry,
            LengthOverall = request.LengthOverall,
            Beam = request.Beam,
            Draft = request.Draft,
            CargoType = request.CargoType,
            Capacity = request.Capacity,
            Status = request.Status,
            OwnerCompany = request.OwnerCompany,
            AgentEmail = request.AgentEmail
        };
    }

    public static Berth ToEntity(this BerthRequest request, int? id = null)
    {
        return new Berth
        {
            Id = id ?? 0,
            BerthCode = request.BerthCode,
            TerminalName = request.TerminalName,
            MaxVesselLength = request.MaxVesselLength,
            MaxDraft = request.MaxDraft,
            Facilities = request.Facilities,
            Status = request.Status,
            Notes = request.Notes
        };
    }

    public static PortCall ToEntity(this PortCallRequest request, int? id = null)
    {
        return new PortCall
        {
            Id = id ?? 0,
            VesselId = request.VesselId,
            BerthId = request.BerthId,
            EstimatedTimeOfArrival = request.EstimatedTimeOfArrival,
            EstimatedTimeOfDeparture = request.EstimatedTimeOfDeparture,
            ActualTimeOfArrival = request.ActualTimeOfArrival,
            ActualTimeOfDeparture = request.ActualTimeOfDeparture,
            Status = request.Status,
            CargoDescription = request.CargoDescription,
            CargoQuantity = request.CargoQuantity,
            CargoUnit = request.CargoUnit,
            Notes = request.Notes,
            DelayReason = request.DelayReason,
            PriorityLevel = request.PriorityLevel
        };
    }
}
