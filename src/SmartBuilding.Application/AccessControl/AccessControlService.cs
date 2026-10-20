using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Application.AccessControl;

public sealed class AccessControlService(
    IAccessControlRepository repository,
    TimeProvider timeProvider) : IAccessControlService
{
    private static readonly IReadOnlyDictionary<AccessResult, string> ResultReasons =
        new Dictionary<AccessResult, string>
        {
            [AccessResult.Granted] = "Access granted.",
            [AccessResult.CardNotFound] = "Access card was not found.",
            [AccessResult.CardInactive] = "Access card is inactive.",
            [AccessResult.CardExpired] = "Access card has expired.",
            [AccessResult.UserInactive] = "User is inactive.",
            [AccessResult.AccessPointInactive] = "Access point is inactive.",
            [AccessResult.NoPermission] =
                "No active permission exists for this user at this access point.",
            [AccessResult.PermissionExpired] =
                "No active permission is currently valid for this user at this access point.",
            [AccessResult.DirectionNotSupported] =
                "Access point does not support the requested direction."
        };

    public async Task<AccessRequestResult> RequestAccessAsync(
        AccessRequestCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        string cardNumber = NormalizeCardNumber(command.CardNumber);
        ValidateRequest(command.AccessPointId, command.Direction);

        DateTime occurredAt = timeProvider.GetUtcNow().UtcDateTime;
        AccessDecisionContext? context = await repository.GetDecisionContextAsync(
            cardNumber,
            command.AccessPointId,
            occurredAt,
            cancellationToken);

        if (context is null)
        {
            return AccessRequestResult.MissingAccessPoint();
        }

        AccessResult result = Evaluate(context, command.Direction, occurredAt);
        var accessEvent = new AccessEvent
        {
            Id = Guid.NewGuid(),
            AccessPointId = context.AccessPointId,
            AccessCardId = context.AccessCardId,
            UserId = context.UserId,
            OccurredAt = occurredAt,
            Direction = command.Direction,
            Result = result,
            Reason = ResultReasons[result]
        };

        AccessEvent recordedEvent = await repository.CreateEventAsync(
            accessEvent,
            cancellationToken);

        return AccessRequestResult.Recorded(ToDto(recordedEvent));
    }

    public async Task<IReadOnlyList<AccessEventDto>> GetEventsAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessEvent> events = await repository.GetEventsAsync(cancellationToken);
        return events.Select(ToDto).ToArray();
    }

    public async Task<AccessEventDto?> GetEventByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        AccessEvent? accessEvent = await repository.GetEventByIdAsync(id, cancellationToken);
        return accessEvent is null ? null : ToDto(accessEvent);
    }

    private static AccessResult Evaluate(
        AccessDecisionContext context,
        AccessDirection direction,
        DateTime occurredAt)
    {
        if (context.AccessCardId is null)
        {
            return AccessResult.CardNotFound;
        }

        if (context.AccessCardIsActive != true)
        {
            return AccessResult.CardInactive;
        }

        if (context.AccessCardExpiresAt is not null
            && context.AccessCardExpiresAt <= occurredAt)
        {
            return AccessResult.CardExpired;
        }

        if (context.UserIsActive != true)
        {
            return AccessResult.UserInactive;
        }

        if (!context.AccessPointIsActive)
        {
            return AccessResult.AccessPointInactive;
        }

        bool directionSupported = direction switch
        {
            AccessDirection.Entry => context.SupportsEntry,
            AccessDirection.Exit => context.SupportsExit,
            _ => false
        };
        if (!directionSupported)
        {
            return AccessResult.DirectionNotSupported;
        }

        if (!context.HasActivePermission)
        {
            return AccessResult.NoPermission;
        }

        if (!context.HasCurrentlyValidPermission)
        {
            return AccessResult.PermissionExpired;
        }

        return AccessResult.Granted;
    }

    private static AccessEventDto ToDto(AccessEvent accessEvent)
    {
        return new AccessEventDto(
            accessEvent.Id,
            accessEvent.AccessPointId,
            accessEvent.AccessCardId,
            accessEvent.UserId,
            accessEvent.OccurredAt,
            accessEvent.Direction,
            accessEvent.Result,
            accessEvent.Reason);
    }

    private static string NormalizeCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            throw new ApplicationValidationException(
                nameof(AccessRequestCommand.CardNumber),
                "Card number is required.");
        }

        string normalized = cardNumber.Trim();
        if (normalized.Length > 100)
        {
            throw new ApplicationValidationException(
                nameof(AccessRequestCommand.CardNumber),
                "Card number cannot exceed 100 characters.");
        }

        return normalized;
    }

    private static void ValidateRequest(Guid accessPointId, AccessDirection direction)
    {
        if (accessPointId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                nameof(AccessRequestCommand.AccessPointId),
                "Access point ID is required.");
        }

        if (!Enum.IsDefined(direction))
        {
            throw new ApplicationValidationException(
                nameof(AccessRequestCommand.Direction),
                "Access direction is invalid.");
        }
    }
}
