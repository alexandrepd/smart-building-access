namespace SmartBuilding.Application.Users;

public sealed record CreateUserCommand(string Name, string Email, string Password);