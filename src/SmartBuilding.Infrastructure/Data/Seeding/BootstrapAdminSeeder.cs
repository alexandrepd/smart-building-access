using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Infrastructure.Data.Seeding;

public sealed class BootstrapAdminSeeder(
    SmartBuildingDbContext dbContext,
    IPasswordHashService passwordHashService)
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    public async Task EnsureAdminExistsAsync(
        string? email,
        string? password,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(1937003)",
            cancellationToken);

        bool hasAdministrator = await dbContext.Users
            .AnyAsync(user => user.Role == UserRole.Admin, cancellationToken);
        if (hasAdministrator)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "No administrator exists. Configure Authentication:BootstrapAdmin:Email and Authentication:BootstrapAdmin:Password using a secure configuration provider.");
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();
        if (normalizedEmail.Length > 320 || !EmailAddressValidator.IsValid(normalizedEmail))
        {
            throw new InvalidOperationException(
                "Authentication:BootstrapAdmin:Email must be a valid email address.");
        }

        if (password.Length is < 12 or > 128)
        {
            throw new InvalidOperationException(
                "Authentication:BootstrapAdmin:Password must contain between 12 and 128 characters.");
        }

        User? user = await dbContext.Users
            .SingleOrDefaultAsync(
                candidate => candidate.Email == normalizedEmail,
                cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Initial Administrator",
                Email = normalizedEmail,
                PasswordHash = passwordHashService.HashPassword(password),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);
        }
        else
        {
            user.PasswordHash = passwordHashService.HashPassword(password);
            user.Role = UserRole.Admin;
            user.IsActive = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
