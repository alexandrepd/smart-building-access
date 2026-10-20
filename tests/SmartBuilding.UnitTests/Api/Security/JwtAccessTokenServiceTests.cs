using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartBuilding.Api.Configuration;
using SmartBuilding.Api.Security;
using SmartBuilding.Application.Authentication;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.UnitTests.Api.Security;

public class JwtAccessTokenServiceTests
{
    [Fact]
    public void CreateToken_UsesConfiguredIssuerAudienceAndUserClaims()
    {
        const string signingKey = "unit-test-signing-key-with-at-least-32-bytes";
        var settings = new JwtSettings(
            "smart-building-tests",
            "smart-building-api-tests",
            signingKey,
            15);
        var service = new JwtAccessTokenService(settings, TimeProvider.System);
        var user = new UserDto(
            Guid.NewGuid(),
            "Admin User",
            "admin@example.com",
            true,
            DateTime.UtcNow,
            UserRole.Admin);

        AccessTokenDto token = service.CreateToken(user);

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(
            token.Token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,
                ValidateAudience = true,
                ValidAudience = settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
            },
            out SecurityToken validatedToken);

        Assert.Equal(
            SecurityAlgorithms.HmacSha256,
            Assert.IsType<JwtSecurityToken>(validatedToken).SignatureAlgorithm);
        Assert.Equal(user.Id.ToString(), principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
        Assert.Equal(user.Email, principal.FindFirstValue(JwtRegisteredClaimNames.Email));
        Assert.Equal(user.Name, principal.FindFirstValue("name"));
        Assert.Equal(nameof(UserRole.Admin), principal.FindFirstValue("role"));
        Assert.Equal(settings.Issuer, principal.FindFirstValue(JwtRegisteredClaimNames.Iss));
        Assert.Contains(settings.Audience, principal.FindAll(JwtRegisteredClaimNames.Aud)
            .Select(claim => claim.Value));
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);
    }
}
