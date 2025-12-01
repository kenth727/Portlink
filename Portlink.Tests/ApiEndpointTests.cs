using System.Text.Json;
using PortlinkApp.Api.Controllers;
using PortlinkApp.Api.Hubs;
using PortlinkApp.Api.Models.Maritime;
using PortlinkApp.Core.Data;
using PortlinkApp.Core.Entities;
using PortlinkApp.Core.Repositories;
using PortlinkApp.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PortlinkApp.Api.Services;

namespace PortlinkApp.Tests;

public class ApiEndpointTests
{
    private static PortlinkDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PortlinkDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new PortlinkDbContext(options);
    }

    [Fact]
    public async Task Vessels_GetAll_ReturnsPagedResult()
    {
        using var context = CreateContext(nameof(Vessels_GetAll_ReturnsPagedResult));
        context.Vessels.AddRange(
            new Vessel { Name = "MSC Oscar", ImoNumber = "IMO9801079", VesselType = VesselType.Container, FlagCountry = "Panama", LengthOverall = 395.4m, Beam = 59m, Draft = 16m, Status = VesselStatus.Approaching },
            new Vessel { Name = "Maersk Triple E", ImoNumber = "IMO9778268", VesselType = VesselType.Container, FlagCountry = "Denmark", LengthOverall = 399m, Beam = 59m, Draft = 14.5m, Status = VesselStatus.Anchored },
            new Vessel { Name = "TI Europe", ImoNumber = "IMO9282346", VesselType = VesselType.Tanker, FlagCountry = "Belgium", LengthOverall = 380m, Beam = 68m, Draft = 24.5m, Status = VesselStatus.Docked }
        );
        await context.SaveChangesAsync();

        var repository = new VesselRepository(context);
        var hub = new NoopPortOperationsHubContext();
        var controller = new VesselsController(repository, hub);

        var result = await controller.GetAll(null, null, pageNumber: 1, pageSize: 10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var json = JsonSerializer.Serialize(ok.Value);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var items = root.GetProperty("items");
        var totalCount = root.GetProperty("totalCount").GetInt32();

        Assert.Equal(3, items.GetArrayLength());
        Assert.Equal(3, totalCount);
    }

    [Fact]
    public async Task AI_Chat_UsesServiceAndReturnsAnswer()
    {
        using var context = CreateContext(nameof(AI_Chat_UsesServiceAndReturnsAnswer));

        var vessel = new Vessel
        {
            Name = "Harmony of the Seas",
            ImoNumber = "IMO9682891",
            VesselType = VesselType.Cruise,
            FlagCountry = "Bahamas",
            LengthOverall = 362m,
            Beam = 47.4m,
            Draft = 9.3m,
            Status = VesselStatus.Approaching
        };

        var berth = new Berth
        {
            BerthCode = "TERM-A-01",
            TerminalName = "Container Terminal A",
            MaxVesselLength = 400,
            MaxDraft = 16,
            Status = BerthStatus.Available
        };

        context.Vessels.Add(vessel);
        context.Berths.Add(berth);
        await context.SaveChangesAsync();

        var portCall = new PortCall
        {
            VesselId = vessel.Id,
            BerthId = berth.Id,
            EstimatedTimeOfArrival = DateTime.UtcNow.AddHours(2),
            EstimatedTimeOfDeparture = DateTime.UtcNow.AddHours(12),
            Status = PortCallStatus.Scheduled
        };

        context.PortCalls.Add(portCall);
        await context.SaveChangesAsync();

        var vesselRepo = new VesselRepository(context);
        var berthRepo = new BerthRepository(context);
        var portCallRepo = new PortCallRepository(context);
        var aiService = new TestAiService();

        var controller = new AIController(
            aiService,
            vesselRepo,
            berthRepo,
            portCallRepo,
            NullLogger<AIController>.Instance);

        var request = new ChatRequest("What vessels are currently approaching?");
        var result = await controller.Chat(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var json = JsonSerializer.Serialize(ok.Value);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        Assert.Equal(request.Question, root.GetProperty("question").GetString());
        Assert.Equal("stubbed answer", root.GetProperty("answer").GetString());

        Assert.Equal(request.Question, aiService.LastQuestion);
        Assert.Contains("Current Port Status", aiService.LastContext);
    }

    [Fact]
    public void LoadSimulator_Start_ReturnsOkResult()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var hub = new NoopPortOperationsHubContext();

        var simulator = new LoadSimulatorService(
            services,
            hub,
            NullLogger<LoadSimulatorService>.Instance);

        var controller = new LoadSimulatorController(simulator);

        var result = controller.Start(operationsPerSecond: 2);

        var ok = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(ok.Value);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Load simulator started", root.GetProperty("message").GetString());
        Assert.Equal(2, root.GetProperty("operationsPerSecond").GetInt32());
    }

    [Fact]
    public async Task Maritime_Manual_Post_DoesNotThrowAndCreatesPortCall()
    {
        using var context = CreateContext(nameof(Maritime_Manual_Post_DoesNotThrowAndCreatesPortCall));

        var vessel = new Vessel
        {
            Name = "Test Vessel",
            ImoNumber = "IMO0000001",
            VesselType = VesselType.Container,
            FlagCountry = "Panama",
            LengthOverall = 300,
            Beam = 48,
            Draft = 12,
            Status = VesselStatus.Approaching
        };

        var berth = new Berth
        {
            BerthCode = "TERM-A-01",
            TerminalName = "Container Terminal A",
            MaxVesselLength = 400,
            MaxDraft = 16,
            Status = BerthStatus.Available
        };

        context.Vessels.Add(vessel);
        context.Berths.Add(berth);
        await context.SaveChangesAsync();

        var vesselRepo = new VesselRepository(context);
        var berthRepo = new BerthRepository(context);
        var portCallRepo = new PortCallRepository(context);
        var hub = new NoopPortOperationsHubContext();

        var controller = new MaritimeController(
            vesselRepo,
            berthRepo,
            portCallRepo,
            hub);

        var viewModel = new ManualPortOperationViewModel
        {
            SelectedVesselId = vessel.Id,
            SelectedBerthId = berth.Id,
            EstimatedTimeOfArrival = DateTime.UtcNow.AddHours(1),
            EstimatedTimeOfDeparture = DateTime.UtcNow.AddHours(4),
            CargoDescription = "Containers",
            CargoQuantity = 100,
            CargoUnit = "TEU"
        };

        var result = await controller.Manual(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        var returnedModel = Assert.IsType<ManualPortOperationViewModel>(viewResult.Model);

        Assert.NotNull(returnedModel.LastOperationSummary);
        Assert.True(context.PortCalls.Any());
    }

    private sealed class TestAiService : IAIService
    {
        public string LastQuestion { get; private set; } = string.Empty;
        public string LastContext { get; private set; } = string.Empty;

        public Task<string> GetBerthRecommendation(int vesselId, string context)
        {
            LastContext = context;
            return Task.FromResult("stubbed berth recommendation");
        }

        public Task<string> AnswerQuestion(string question, string context)
        {
            LastQuestion = question;
            LastContext = context;
            return Task.FromResult("stubbed answer");
        }

        public Task<string> GenerateRealisticPortCallScenario() =>
            Task.FromResult("{}");

        public Task<bool> IsAvailable() => Task.FromResult(true);
    }

    private sealed class NoopPortOperationsHubContext : IHubContext<PortOperationsHub>
    {
        public IHubClients Clients { get; } = new NoopHubClients();
        public IGroupManager Groups { get; } = new NoopGroupManager();

        private sealed class NoopHubClients : IHubClients
        {
            private static readonly IClientProxy Proxy = new NoopClientProxy();

            public IClientProxy All => Proxy;
            public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => Proxy;
            public IClientProxy Client(string connectionId) => Proxy;
            public IClientProxy Clients(IReadOnlyList<string> connectionIds) => Proxy;
            public IClientProxy Group(string groupName) => Proxy;
            public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => Proxy;
            public IClientProxy Groups(IReadOnlyList<string> groupNames) => Proxy;
            public IClientProxy User(string userId) => Proxy;
            public IClientProxy Users(IReadOnlyList<string> userIds) => Proxy;
        }

        private sealed class NoopClientProxy : IClientProxy
        {
            public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default) =>
                Task.CompletedTask;
        }

        private sealed class NoopGroupManager : IGroupManager
        {
            public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) =>
                Task.CompletedTask;

            public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default) =>
                Task.CompletedTask;
        }
    }
}
