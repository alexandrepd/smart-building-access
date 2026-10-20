using System.Text;

namespace SmartBuilding.Api.Configuration;

internal sealed record JwtSettings(
    string Issuer,
    string Audience,
    string SigningKey,
    int AccessTokenLifetimeMinutes)
{
    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        string issuer = configuration["Authentication:Issuer"]
            ?? throw new InvalidOperationException(
                "Configuration 'Authentication:Issuer' is required.");
        string audience = configuration["Authentication:Audience"]
            ?? throw new InvalidOperationException(
                "Configuration 'Authentication:Audience' is required.");
        string signingKey = configuration["Authentication:SigningKey"]
            ?? throw new InvalidOperationException(
                "Configure 'Authentication:SigningKey' with User Secrets or a secure environment variable.");
        int lifetime = configuration.GetValue<int?>(
            "Authentication:AccessTokenLifetimeMinutes") ?? 15;

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "Authentication issuer and audience must not be empty.");
        }

        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            throw new InvalidOperationException(
                "Authentication signing key must contain at least 32 UTF-8 bytes.");
        }

        if (lifetime is < 5 or > 60)
        {
            throw new InvalidOperationException(
                "Authentication access token lifetime must be between 5 and 60 minutes.");
        }

        return new JwtSettings(issuer, audience, signingKey, lifetime);
    }
}
