namespace SmartBuilding.Application.Authentication;

public enum AuthenticationStatus
{
    Success,
    InvalidCredentials
}

public sealed record AuthenticationResult(
    AuthenticationStatus Status,
    AccessTokenDto? AccessToken)
{
    public static AuthenticationResult Succeeded(AccessTokenDto token) =>
        new(AuthenticationStatus.Success, token);

    public static AuthenticationResult InvalidCredentials() =>
        new(AuthenticationStatus.InvalidCredentials, null);
}
