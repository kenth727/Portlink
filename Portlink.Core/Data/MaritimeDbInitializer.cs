using PortlinkApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace PortlinkApp.Core.Data;

public static class MaritimeDbInitializer
{
    public static async Task SeedAsync(ApprenticeDbContext context)
    {
        if (await context.Vessels.AnyAsync())
        {
            return; // Already seeded
        }

        // Seed Berths
        var berths = new List<Berth>
        {
            new() { BerthCode = "TERM-A-01", TerminalName = "Container Terminal A", MaxVesselLength = 400, MaxDraft = 16, Facilities = "Cranes: 4, Fuel: Yes, Water: Yes, Power: Yes", Status = BerthStatus.Available },
            new() { BerthCode = "TERM-A-02", TerminalName = "Container Terminal A", MaxVesselLength = 400, MaxDraft = 16, Facilities = "Cranes: 4, Fuel: Yes, Water: Yes, Power: Yes", Status = BerthStatus.Available },
            new() { BerthCode = "TERM-B-01", TerminalName = "Bulk Terminal B", MaxVesselLength = 300, MaxDraft = 14, Facilities = "Conveyor: Yes, Fuel: Yes, Water: Yes", Status = BerthStatus.Available },
            new() { BerthCode = "TERM-C-01", TerminalName = "Tanker Terminal C", MaxVesselLength = 350, MaxDraft = 18, Facilities = "Fuel: Yes, Water: Yes, Oil Pipeline: Yes", Status = BerthStatus.Available },
            new() { BerthCode = "TERM-D-01", TerminalName = "RoRo Terminal D", MaxVesselLength = 250, MaxDraft = 12, Facilities = "Ramp: Yes, Fuel: Yes, Water: Yes", Status = BerthStatus.Available }
        };

        context.Berths.AddRange(berths);
        await context.SaveChangesAsync();

        // Seed Vessels
        var vessels = new List<Vessel>
        {
            new() { Name = "MSC Oscar", ImoNumber = "IMO9801079", VesselType = VesselType.Container, FlagCountry = "Panama", LengthOverall = 395.4m, Beam = 59m, Draft = 16m, CargoType = "Containers", Capacity = 19224, Status = VesselStatus.Approaching, OwnerCompany = "Mediterranean Shipping Company", AgentEmail = "agent@msc.com" },
            new() { Name = "Maersk Triple E", ImoNumber = "IMO9778268", VesselType = VesselType.Container, FlagCountry = "Denmark", LengthOverall = 399m, Beam = 59m, Draft = 14.5m, CargoType = "Containers", Capacity = 18270, Status = VesselStatus.Anchored, OwnerCompany = "Maersk Line", AgentEmail = "agent@maersk.com" },
            new() { Name = "Valemax Iron Ore", ImoNumber = "IMO9631044", VesselType = VesselType.BulkCarrier, FlagCountry = "Marshall Islands", LengthOverall = 362m, Beam = 65m, Draft = 23m, CargoType = "Iron Ore", Capacity = 400000, Status = VesselStatus.Approaching, OwnerCompany = "Vale", AgentEmail = "agent@vale.com" },
            new() { Name = "TI Europe", ImoNumber = "IMO9282346", VesselType = VesselType.Tanker, FlagCountry = "Belgium", LengthOverall = 380m, Beam = 68m, Draft = 24.5m, CargoType = "Crude Oil", Capacity = 441893, Status = VesselStatus.Docked, OwnerCompany = "Tankers International", AgentEmail = "agent@tankers.com" },
            new() { Name = "Harmony of the Seas", ImoNumber = "IMO9682891", VesselType = VesselType.Cruise, FlagCountry = "Bahamas", LengthOverall = 362m, Beam = 47.4m, Draft = 9.3m, CargoType = "Passengers", Capacity = 6780, Status = VesselStatus.Departed, OwnerCompany = "Royal Caribbean", AgentEmail = "agent@rccl.com" }
        };

        context.Vessels.AddRange(vessels);
        await context.SaveChangesAsync();

        // Seed Port Calls
        var now = DateTime.UtcNow;
        var portCalls = new List<PortCall>
        {
            new() { VesselId = vessels[0].Id, BerthId = berths[0].Id, EstimatedTimeOfArrival = now.AddHours(2), EstimatedTimeOfDeparture = now.AddHours(26), Status = PortCallStatus.Scheduled, CargoDescription = "Mixed Containers", CargoQuantity = 15000, CargoUnit = "TEU", PriorityLevel = 2 },
            new() { VesselId = vessels[1].Id, BerthId = berths[1].Id, EstimatedTimeOfArrival = now.AddHours(-1), EstimatedTimeOfDeparture = now.AddHours(18), ActualTimeOfArrival = now.AddHours(-1), Status = PortCallStatus.Berthed, CargoDescription = "Electronics & Auto Parts", CargoQuantity = 12000, CargoUnit = "TEU", PriorityLevel = 1 },
            new() { VesselId = vessels[2].Id, BerthId = berths[2].Id, EstimatedTimeOfArrival = now.AddHours(6), EstimatedTimeOfDeparture = now.AddHours(30), Status = PortCallStatus.Approaching, CargoDescription = "Iron Ore", CargoQuantity = 350000, CargoUnit = "tons", PriorityLevel = 3 },
            new() { VesselId = vessels[3].Id, BerthId = berths[3].Id, EstimatedTimeOfArrival = now.AddHours(-12), EstimatedTimeOfDeparture = now.AddHours(12), ActualTimeOfArrival = now.AddHours(-12), Status = PortCallStatus.InProgress, CargoDescription = "Crude Oil", CargoQuantity = 400000, CargoUnit = "tons", PriorityLevel = 1 }
        };

        context.PortCalls.AddRange(portCalls);
        await context.SaveChangesAsync();
    }
}
