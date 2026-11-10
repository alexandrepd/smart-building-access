using Microsoft.AspNetCore.Identity;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Entities;
using System.Security.Cryptography;

namespace SmartBuilding.Infrastructure.Security;

public sealed class IdentityPasswordHashService : IPasswordHashService
{
    private static readonly string DummyPasswordHash = CreateDummyPasswordHash();
    private readonly PasswordHasher<User> passwordHasher = new();
    private readonly User hashingSubject = new();

    public string HashPassword(string password)
    {
        return passwordHasher.HashPassword(hashingSubject, password);
    }

    public bool VerifyPassword(string? passwordHash, string password)
    {
        return passwordHasher.VerifyHashedPassword(
                hashingSubject,
                passwordHash ?? DummyPasswordHash,
                password)
            != PasswordVerificationResult.Failed;
    }

    private static string CreateDummyPasswordHash()
    {
        var passwordHasher = new PasswordHasher<User>();
        string randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return passwordHasher.HashPassword(new User(), randomPassword);
    }
}