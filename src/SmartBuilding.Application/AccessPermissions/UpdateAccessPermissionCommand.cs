namespace SmartBuilding.Application.AccessPermissions;

public sealed record UpdateAccessPermissionCommand(
    Guid UserId,
    Guid AccessPointId,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil,
    bool IsActive);
