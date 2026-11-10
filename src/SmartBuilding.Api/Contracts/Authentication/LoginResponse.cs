namespace SmartBuilding.Api.Contracts.Authentication;

/// <summary>Contains a signed access token and the instant it expires.</summary>
public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
