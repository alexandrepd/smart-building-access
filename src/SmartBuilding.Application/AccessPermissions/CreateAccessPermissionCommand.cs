namespace SmartBuilding.Application.AccessPermissions;

public sealed record CreateAccessPermissionCommand(
    Guid UserId,
    Guid AccessPointId,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil);
