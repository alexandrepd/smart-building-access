namespace SmartBuilding.Application.AccessPermissions;

public sealed record AccessPermissionDto(
    Guid Id,
    Guid UserId,
    Guid AccessPointId,
    bool IsActive,
    DateTime ValidFrom,
    DateTime? ValidUntil);
