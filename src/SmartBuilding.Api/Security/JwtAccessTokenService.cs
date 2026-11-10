using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartBuilding.Api.Configuration;
using SmartBuilding.Application.Authentication;
using SmartBuilding.Application.Users;

namespace SmartBuilding.Api.Security;

internal sealed class JwtAccessTokenService(
    JwtSettings settings,
    TimeProvider timeProvider) : IAccessTokenService
{
    public AccessTokenDto CreateToken(UserDto user)
    {
        DateTimeOffset issuedAt = timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = issuedAt.AddMinutes(settings.AccessTokenLifetimeMinutes);
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.Name),
            new("role", user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: issuedAt.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AccessTokenDto(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
