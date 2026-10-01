namespace SmartBuilding.Application.Users;

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt);