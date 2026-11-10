namespace SmartBuilding.Application.Authentication;

public sealed record AccessTokenDto(string Token, DateTimeOffset ExpiresAt);
