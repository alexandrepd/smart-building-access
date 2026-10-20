using SmartBuilding.Application.AccessPermissions;

namespace SmartBuilding.Api.Contracts.AccessPermissions;

/// <summary>Represents a user's time-bounded authorization for an access point.</summary>
public sealed record AccessPermissionResponse(
    Guid Id,
    Guid UserId,
    Guid AccessPointId,
    bool IsActive,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil)
{
    internal static AccessPermissionResponse FromDto(AccessPermissionDto permission)
    {
        return new AccessPermissionResponse(
            permission.Id,
            permission.UserId,
            permission.AccessPointId,
            permission.IsActive,
            new DateTimeOffset(permission.ValidFrom),
            permission.ValidUntil is null
                ? null
                : new DateTimeOffset(permission.ValidUntil.Value));
    }
}
