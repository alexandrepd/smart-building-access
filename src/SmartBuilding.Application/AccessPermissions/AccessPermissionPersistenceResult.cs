using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessPermissions;

public sealed record AccessPermissionPersistenceResult
{
    private AccessPermissionPersistenceResult(
        AccessPermissionSaveStatus status,
        AccessPermission? permission)
    {
        Status = status;
        Permission = permission;
    }

    public AccessPermissionSaveStatus Status { get; }

    public AccessPermission? Permission { get; }

    public static AccessPermissionPersistenceResult Succeeded(AccessPermission permission) =>
        new(AccessPermissionSaveStatus.Success, permission);

    public static AccessPermissionPersistenceResult MissingPermission() =>
        new(AccessPermissionSaveStatus.AccessPermissionNotFound, null);

    public static AccessPermissionPersistenceResult MissingUser() =>
        new(AccessPermissionSaveStatus.UserNotFound, null);

    public static AccessPermissionPersistenceResult MissingAccessPoint() =>
        new(AccessPermissionSaveStatus.AccessPointNotFound, null);
}
