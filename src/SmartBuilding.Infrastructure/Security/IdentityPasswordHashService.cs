using Microsoft.AspNetCore.Identity;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Security;

public sealed class IdentityPasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<User> passwordHasher = new();
    private readonly User hashingSubject = new();

    public string HashPassword(string password)
    {
        return passwordHasher.HashPassword(hashingSubject, password);
    }
}