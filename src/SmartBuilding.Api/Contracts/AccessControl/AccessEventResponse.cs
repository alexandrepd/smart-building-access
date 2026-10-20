using SmartBuilding.Application.AccessControl;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Api.Contracts.AccessControl;

/// <summary>Represents an audited access attempt and its decision.</summary>
public sealed record AccessEventResponse(
    Guid Id,
    Guid AccessPointId,
    Guid? AccessCardId,
    Guid? UserId,
    DateTimeOffset OccurredAt,
    AccessDirection Direction,
    AccessResult Result,
    string Reason)
{
    internal static AccessEventResponse FromDto(AccessEventDto accessEvent)
    {
        return new AccessEventResponse(
            accessEvent.Id,
            accessEvent.AccessPointId,
            accessEvent.AccessCardId,
            accessEvent.UserId,
            new DateTimeOffset(accessEvent.OccurredAt),
            accessEvent.Direction,
            accessEvent.Result,
            accessEvent.Reason);
    }
}
