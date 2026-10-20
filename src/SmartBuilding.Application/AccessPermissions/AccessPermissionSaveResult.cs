namespace SmartBuilding.Application.AccessPermissions;

public sealed record AccessPermissionSaveResult(
    AccessPermissionSaveStatus Status,
    AccessPermissionDto? Permission)
{
    public static AccessPermissionSaveResult Succeeded(AccessPermissionDto permission) =>
        new(AccessPermissionSaveStatus.Success, permission);

    public static AccessPermissionSaveResult MissingPermission() =>
        new(AccessPermissionSaveStatus.AccessPermissionNotFound, null);

    public static AccessPermissionSaveResult MissingUser() =>
        new(AccessPermissionSaveStatus.UserNotFound, null);

    public static AccessPermissionSaveResult MissingAccessPoint() =>
        new(AccessPermissionSaveStatus.AccessPointNotFound, null);
}
