using System.ComponentModel.DataAnnotations;
using SmartBuilding.Application.Common;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.Authentication;

public sealed class AuthenticationService(
    IUserRepository userRepository,
    IPasswordHashService passwordHashService,
    IAccessTokenService accessTokenService) : IAuthenticationService
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    public async Task<AuthenticationResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        string email = NormalizeEmail(command.Email);
        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ApplicationValidationException(
                nameof(LoginCommand.Password),
                "Password is required.");
        }

        User? user = await userRepository.GetByEmailAsync(email, cancellationToken);
        bool validPassword = passwordHashService.VerifyPassword(
            user?.PasswordHash,
            command.Password);
        if (user is null
            || !user.IsActive
            || !validPassword)
        {
            return AuthenticationResult.InvalidCredentials();
        }

        var userDto = new UserDto(
            user.Id,
            user.Name,
            user.Email,
            user.IsActive,
            user.CreatedAt,
            user.Role);
        return AuthenticationResult.Succeeded(accessTokenService.CreateToken(userDto));
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ApplicationValidationException(
                nameof(LoginCommand.Email),
                "Email is required.");
        }

        string normalized = email.Trim().ToLowerInvariant();
        if (normalized.Length > 320 || !EmailAddressValidator.IsValid(normalized))
        {
            throw new ApplicationValidationException(
                nameof(LoginCommand.Email),
                "Email address is invalid.");
        }

        return normalized;
    }
}
