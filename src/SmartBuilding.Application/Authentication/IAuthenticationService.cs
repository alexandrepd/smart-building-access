namespace SmartBuilding.Application.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken);
}
