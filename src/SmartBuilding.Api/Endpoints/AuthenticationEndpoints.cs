using Microsoft.AspNetCore.Http.HttpResults;
using SmartBuilding.Api.Contracts.Authentication;
using SmartBuilding.Application.Authentication;

namespace SmartBuilding.Api.Endpoints;

internal static class AuthenticationEndpoints
{
    internal static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/auth/login", LoginAsync)
            .AllowAnonymous()
            .WithTags("Authentication")
            .WithName("Login")
            .WithSummary("Authenticate a user and issue an access token")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<Results<Ok<LoginResponse>, ProblemHttpResult>> LoginAsync(
        LoginRequest request,
        IAuthenticationService service,
        CancellationToken cancellationToken)
    {
        AuthenticationResult result = await service.LoginAsync(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return result.Status switch
        {
            AuthenticationStatus.Success => TypedResults.Ok(
                new LoginResponse(
                    result.AccessToken!.Token,
                    result.AccessToken.ExpiresAt)),
            AuthenticationStatus.InvalidCredentials => TypedResults.Problem(
                title: "Invalid email or password.",
                statusCode: StatusCodes.Status401Unauthorized),
            _ => throw new InvalidOperationException(
                $"Unsupported authentication status: {result.Status}.")
        };
    }
}
