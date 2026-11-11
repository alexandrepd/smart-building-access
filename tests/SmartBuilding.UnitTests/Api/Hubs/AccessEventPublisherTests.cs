using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using SmartBuilding.Api.Contracts.AccessControl;
using SmartBuilding.Api.Contracts.Occupancy;
using SmartBuilding.Api.Hubs;
using SmartBuilding.Application.Occupancy;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.UnitTests.Api.Hubs;

public class AccessEventPublisherTests
{
    [Theory]
    [InlineData(AccessResult.CardNotFound)]
    [InlineData(AccessResult.CardInactive)]
    [InlineData(AccessResult.CardExpired)]
    [InlineData(AccessResult.UserInactive)]
    [InlineData(AccessResult.AccessPointInactive)]
    [InlineData(AccessResult.NoPermission)]
    [InlineData(AccessResult.PermissionExpired)]
    [InlineData(AccessResult.DirectionNotSupported)]
    public async Task DeniedEvent_PublishesOnlyAccessEvent(AccessResult result)
    {
        var clients = new BroadcastClients();
        var occupancy = new OccupancyService();
        var publisher = CreatePublisher(clients, occupancy);
        var response = Event(result);

        await publisher.PublishAsync(response);

        Assert.Same(response, Assert.Single(clients.Events));
        Assert.Empty(clients.Occupancy);
        Assert.Equal(0, occupancy.ReadCount);
    }

    [Fact]
    public async Task AccessBroadcastFailure_StillPublishesOccupancyWithoutThrowing()
    {
        var clients = new BroadcastClients { FailAccessBroadcast = true };
        var occupancy = new OccupancyService();
        var publisher = CreatePublisher(clients, occupancy);

        await publisher.PublishAsync(Event(AccessResult.Granted));

        Assert.Equal(1, occupancy.ReadCount);
        Assert.Empty(Assert.Single(clients.Occupancy));
    }

    [Fact]
    public async Task OccupancyBroadcastFailure_DoesNotThrowOrUndoAccessBroadcast()
    {
        var clients = new BroadcastClients { FailOccupancyBroadcast = true };
        var occupancy = new OccupancyService();
        var publisher = CreatePublisher(clients, occupancy);
        var response = Event(AccessResult.Granted);

        await publisher.PublishAsync(response);

        Assert.Same(response, Assert.Single(clients.Events));
        Assert.Equal(1, occupancy.ReadCount);
    }

    private static AccessEventPublisher CreatePublisher(BroadcastClients clients, OccupancyService occupancy) =>
        new(new HubContext(clients), occupancy, NullLogger<AccessEventPublisher>.Instance);

    private static AccessEventResponse Event(AccessResult result) => new(
        Guid.NewGuid(), Guid.NewGuid(), null, null, DateTimeOffset.UtcNow,
        AccessDirection.Entry, result, "Test decision.");

    private sealed class OccupancyService : IOccupancyService
    {
        internal int ReadCount { get; private set; }

        public Task<IReadOnlyList<CurrentOccupancyFloorDto>> GetCurrentOccupancyAsync(
            CancellationToken cancellationToken)
        {
            Assert.Equal(CancellationToken.None, cancellationToken);
            ReadCount++;
            return Task.FromResult<IReadOnlyList<CurrentOccupancyFloorDto>>([]);
        }
    }

    private sealed class HubContext(BroadcastClients clients) : IHubContext<AccessHub, IAccessClient>
    {
        public IHubClients<IAccessClient> Clients => clients;
        public IGroupManager Groups => throw new NotSupportedException();
    }

    private sealed class BroadcastClients : IHubClients<IAccessClient>, IAccessClient
    {
        internal List<AccessEventResponse> Events { get; } = [];
        internal List<CurrentOccupancyResponse[]> Occupancy { get; } = [];
        internal bool FailAccessBroadcast { get; init; }
        internal bool FailOccupancyBroadcast { get; init; }
        public IAccessClient All => this;

        public Task AccessEventCreated(AccessEventResponse accessEvent)
        {
            if (FailAccessBroadcast)
            {
                throw new InvalidOperationException("Broadcast unavailable.");
            }

            Events.Add(accessEvent);
            return Task.CompletedTask;
        }

        public Task OccupancyChanged(CurrentOccupancyResponse[] occupancy)
        {
            if (FailOccupancyBroadcast)
            {
                throw new InvalidOperationException("Broadcast unavailable.");
            }

            Occupancy.Add(occupancy);
            return Task.CompletedTask;
        }

        public IAccessClient AllExcept(IReadOnlyList<string> excludedConnectionIds) =>
            throw new NotSupportedException();
        public IAccessClient Client(string connectionId) => throw new NotSupportedException();
        public IAccessClient Clients(IReadOnlyList<string> connectionIds) =>
            throw new NotSupportedException();
        public IAccessClient Group(string groupName) => throw new NotSupportedException();
        public IAccessClient GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) =>
            throw new NotSupportedException();
        public IAccessClient Groups(IReadOnlyList<string> groupNames) => throw new NotSupportedException();
        public IAccessClient User(string userId) => throw new NotSupportedException();
        public IAccessClient Users(IReadOnlyList<string> userIds) => throw new NotSupportedException();
    }
}
