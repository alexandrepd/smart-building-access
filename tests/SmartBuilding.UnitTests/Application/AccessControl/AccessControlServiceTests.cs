using SmartBuilding.Application.AccessControl;
using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.UnitTests.Application.AccessControl;

public class AccessControlServiceTests
{
    private static readonly DateTimeOffset RequestTime =
        new(2030, 1, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RequestAccessAsync_ValidCardAndPermission_GrantsAndRecordsDecision()
    {
        var repository = new FakeAccessControlRepository(ValidContext());
        var service = new AccessControlService(repository, new FrozenTimeProvider(RequestTime));

        AccessRequestResult result = await service.RequestAccessAsync(
            new AccessRequestCommand(" CARD-100 ", Guid.NewGuid(), AccessDirection.Entry),
            CancellationToken.None);

        AccessEventDto accessEvent = Assert.IsType<AccessEventDto>(result.AccessEvent);
        Assert.Equal(AccessDecisionStatus.Recorded, result.Status);
        Assert.Equal(AccessResult.Granted, accessEvent.Result);
        Assert.Equal("Access granted.", accessEvent.Reason);
        Assert.Equal(RequestTime.UtcDateTime, accessEvent.OccurredAt);
        Assert.Equal("CARD-100", repository.RequestedCardNumber);
        Assert.Single(repository.CreatedEvents);
        Assert.Equal(repository.CreatedEvents[0].Id, accessEvent.Id);
    }

    [Fact]
    public async Task RequestAccessAsync_ExitSupportedByAccessPoint_GrantsAccess()
    {
        var repository = new FakeAccessControlRepository(ValidContext());
        var service = new AccessControlService(repository, new FrozenTimeProvider(RequestTime));

        AccessRequestResult result = await service.RequestAccessAsync(
            new AccessRequestCommand("CARD-100", Guid.NewGuid(), AccessDirection.Exit),
            CancellationToken.None);

        Assert.Equal(AccessResult.Granted, result.AccessEvent?.Result);
        Assert.Equal(AccessDirection.Exit, result.AccessEvent?.Direction);
    }

    [Theory]
    [InlineData(AccessResult.CardNotFound)]
    [InlineData(AccessResult.CardInactive)]
    [InlineData(AccessResult.CardExpired)]
    [InlineData(AccessResult.UserInactive)]
    [InlineData(AccessResult.AccessPointInactive)]
    [InlineData(AccessResult.NoPermission)]
    [InlineData(AccessResult.PermissionExpired)]
    [InlineData(AccessResult.DirectionNotSupported)]
    public async Task RequestAccessAsync_DeniedCondition_RecordsDecision(AccessResult expected)
    {
        var repository = new FakeAccessControlRepository(ContextFor(expected));
        var service = new AccessControlService(repository, new FrozenTimeProvider(RequestTime));

        AccessRequestResult result = await service.RequestAccessAsync(
            new AccessRequestCommand("CARD-100", Guid.NewGuid(), AccessDirection.Entry),
            CancellationToken.None);

        Assert.Equal(AccessDecisionStatus.Recorded, result.Status);
        Assert.Equal(expected, result.AccessEvent?.Result);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessEvent?.Reason));
        Assert.Single(repository.CreatedEvents);
    }

    [Fact]
    public async Task RequestAccessAsync_AccessPointNotFound_ReturnsNotFoundWithoutAuditEvent()
    {
        var repository = new FakeAccessControlRepository(null);
        var service = new AccessControlService(repository, new FrozenTimeProvider(RequestTime));

        AccessRequestResult result = await service.RequestAccessAsync(
            new AccessRequestCommand("CARD-100", Guid.NewGuid(), AccessDirection.Entry),
            CancellationToken.None);

        Assert.Equal(AccessDecisionStatus.AccessPointNotFound, result.Status);
        Assert.Null(result.AccessEvent);
        Assert.Empty(repository.CreatedEvents);
    }

    [Fact]
    public async Task RequestAccessAsync_BlankCardNumber_ThrowsValidationException()
    {
        var service = new AccessControlService(
            new FakeAccessControlRepository(ValidContext()),
            new FrozenTimeProvider(RequestTime));

        Task Act() => service.RequestAccessAsync(
            new AccessRequestCommand("  ", Guid.NewGuid(), AccessDirection.Entry),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task RequestAccessAsync_UnknownDirection_ThrowsValidationException()
    {
        var service = new AccessControlService(
            new FakeAccessControlRepository(ValidContext()),
            new FrozenTimeProvider(RequestTime));

        Task Act() => service.RequestAccessAsync(
            new AccessRequestCommand("CARD-100", Guid.NewGuid(), (AccessDirection)99),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task GetEventsAsync_RepositoryEvents_ReturnsMappedEvents()
    {
        var accessEvent = new AccessEvent
        {
            Id = Guid.NewGuid(),
            AccessPointId = Guid.NewGuid(),
            OccurredAt = RequestTime.UtcDateTime,
            Direction = AccessDirection.Exit,
            Result = AccessResult.DirectionNotSupported,
            Reason = "Access point does not support the requested direction."
        };
        var repository = new FakeAccessControlRepository(ValidContext())
        {
            ExistingEvents = [accessEvent]
        };
        var service = new AccessControlService(repository, new FrozenTimeProvider(RequestTime));

        AccessEventDto result = Assert.Single(
            await service.GetEventsAsync(CancellationToken.None));

        Assert.Equal(accessEvent.Id, result.Id);
        Assert.Equal(accessEvent.Direction, result.Direction);
        Assert.Equal(accessEvent.Result, result.Result);
        Assert.Equal(accessEvent.Reason, result.Reason);
    }

    private static AccessDecisionContext ValidContext()
    {
        return new AccessDecisionContext(
            Guid.NewGuid(),
            AccessPointIsActive: true,
            SupportsEntry: true,
            SupportsExit: true,
            Guid.NewGuid(),
            AccessCardIsActive: true,
            RequestTime.AddDays(1).UtcDateTime,
            Guid.NewGuid(),
            UserIsActive: true,
            HasActivePermission: true,
            HasCurrentlyValidPermission: true);
    }

    private static AccessDecisionContext ContextFor(AccessResult result)
    {
        AccessDecisionContext context = ValidContext();
        return result switch
        {
            AccessResult.CardNotFound => context with
            {
                AccessCardId = null,
                AccessCardIsActive = null,
                UserId = null,
                UserIsActive = null
            },
            AccessResult.CardInactive => context with { AccessCardIsActive = false },
            AccessResult.CardExpired => context with
            {
                AccessCardExpiresAt = RequestTime.UtcDateTime
            },
            AccessResult.UserInactive => context with { UserIsActive = false },
            AccessResult.AccessPointInactive => context with { AccessPointIsActive = false },
            AccessResult.NoPermission => context with
            {
                HasActivePermission = false,
                HasCurrentlyValidPermission = false
            },
            AccessResult.PermissionExpired => context with
            {
                HasActivePermission = true,
                HasCurrentlyValidPermission = false
            },
            AccessResult.DirectionNotSupported => context with { SupportsEntry = false },
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    private sealed class FrozenTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeAccessControlRepository(AccessDecisionContext? context)
        : IAccessControlRepository
    {
        public string? RequestedCardNumber { get; private set; }

        public AccessEventDto? RecordedEventDto { get; private set; }

        public List<AccessEvent> CreatedEvents { get; } = [];

        public IReadOnlyList<AccessEvent> ExistingEvents { get; init; } = [];

        public Task<AccessDecisionContext?> GetDecisionContextAsync(
            string cardNumber,
            Guid accessPointId,
            DateTime occurredAt,
            CancellationToken cancellationToken)
        {
            RequestedCardNumber = cardNumber;
            return Task.FromResult(context);
        }

        public Task<AccessEvent> CreateEventAsync(
            AccessEvent accessEvent,
            CancellationToken cancellationToken)
        {
            CreatedEvents.Add(accessEvent);
            RecordedEventDto = new AccessEventDto(
                accessEvent.Id,
                accessEvent.AccessPointId,
                accessEvent.AccessCardId,
                accessEvent.UserId,
                accessEvent.OccurredAt,
                accessEvent.Direction,
                accessEvent.Result,
                accessEvent.Reason);
            return Task.FromResult(accessEvent);
        }

        public Task<IReadOnlyList<AccessEvent>> GetEventsAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ExistingEvents);
        }

        public Task<AccessEvent?> GetEventByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ExistingEvents.SingleOrDefault(accessEvent => accessEvent.Id == id));
        }
    }
}
