namespace SmartBuilding.Application.Users;

public sealed record UpdateUserCommand(string Name, string Email, bool IsActive);