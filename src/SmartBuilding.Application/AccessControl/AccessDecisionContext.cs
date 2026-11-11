namespace SmartBuilding.Application.AccessControl;

public sealed record AccessDecisionContext(
    Guid AccessPointId,
    Guid FloorId,
    bool AccessPointIsActive,
    bool SupportsEntry,
    bool SupportsExit,
    Guid? AccessCardId,
    bool? AccessCardIsActive,
    DateTime? AccessCardExpiresAt,
    Guid? UserId,
    bool? UserIsActive,
    bool HasActivePermission,
    bool HasCurrentlyValidPermission);
