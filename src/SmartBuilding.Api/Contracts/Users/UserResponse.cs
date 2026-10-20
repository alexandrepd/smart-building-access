using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Api.Contracts.Users;

/// <summary>Represents a user returned by the API; password material is never exposed.</summary>
public sealed record UserResponse(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    DateTimeOffset CreatedAt,
    UserRole Role)
{
    internal static UserResponse FromDto(UserDto user)
    {
        return new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.IsActive,
            new DateTimeOffset(user.CreatedAt),
            user.Role);
    }
}