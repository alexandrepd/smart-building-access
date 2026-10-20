using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Application.AccessControl;

public sealed record AccessEventDto(
    Guid Id,
    Guid AccessPointId,
    Guid? AccessCardId,
    Guid? UserId,
    DateTime OccurredAt,
    AccessDirection Direction,
    AccessResult Result,
    string Reason);
