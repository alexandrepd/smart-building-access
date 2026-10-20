using Microsoft.AspNetCore.Identity;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Infrastructure.Security;

namespace SmartBuilding.UnitTests.Infrastructure;

public class IdentityPasswordHashServiceTests
{
    [Fact]
    public void HashPassword_ValidPassword_ProducesVerifiableNonPlainTextHash()
    {
        const string password = "A secure password for testing";
        var service = new IdentityPasswordHashService();
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };

        string passwordHash = service.HashPassword(password);
        var verifier = new PasswordHasher<User>();

        Assert.NotEqual(password, passwordHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            verifier.VerifyHashedPassword(user, passwordHash, password));
    }

    [Fact]
    public void HashPassword_SamePassword_UsesDifferentRandomSalt()
    {
        const string password = "A secure password for testing";
        var service = new IdentityPasswordHashService();

        string firstHash = service.HashPassword(password);
        string secondHash = service.HashPassword(password);

        Assert.NotEqual(firstHash, secondHash);
    }

    [Fact]
    public void HashPassword_WrongPassword_FailsVerification()
    {
        var service = new IdentityPasswordHashService();
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };

        string passwordHash = service.HashPassword("The correct secure password");
        var verifier = new PasswordHasher<User>();

        Assert.Equal(
            PasswordVerificationResult.Failed,
            verifier.VerifyHashedPassword(user, passwordHash, "a different password"));
        Assert.False(service.VerifyPassword(passwordHash, "a different password"));
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var service = new IdentityPasswordHashService();
        string hash = service.HashPassword("A secure password for testing");

        Assert.True(service.VerifyPassword(hash, "A secure password for testing"));
    }

    [Fact]
    public void VerifyPassword_NullHash_ReturnsFalse()
    {
        var service = new IdentityPasswordHashService();

        Assert.False(service.VerifyPassword(null, "A secure password for testing"));
    }
}