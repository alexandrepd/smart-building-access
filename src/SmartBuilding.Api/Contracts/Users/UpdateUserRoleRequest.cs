using System.ComponentModel.DataAnnotations;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Api.Contracts.Users;

/// <summary>Payload used by an administrator to assign a user's role.</summary>
public sealed record UpdateUserRoleRequest
{
    /// <summary>Role to assign to the user.</summary>
    [EnumDataType(typeof(UserRole))]
    public required UserRole Role { get; init; }
}
