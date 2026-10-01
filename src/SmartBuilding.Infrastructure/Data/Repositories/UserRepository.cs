using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class UserRepository(SmartBuildingDbContext dbContext) : IUserRepository
{
    private const string EmailUniqueIndex = "ix_users_email";
    private const string AccessCardUserForeignKey = "fk_access_cards_users_user_id";
    private const string AccessPermissionUserForeignKey = "fk_access_permissions_users_user_id";
    private const string OccupancySessionUserForeignKey = "fk_occupancy_sessions_users_user_id";

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<UserPersistenceResult> CreateAsync(
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        return await SaveAsync(user, cancellationToken);
    }

    public async Task<UserPersistenceResult> UpdateAsync(
        Guid id,
        string name,
        string email,
        bool isActive,
        CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return UserPersistenceResult.MissingUser();
        }

        user.Name = name;
        user.Email = email;
        user.IsActive = isActive;

        return await SaveAsync(user, cancellationToken);
    }

    public async Task<DeleteUserResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users
            .SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

        if (user is null)
        {
            return DeleteUserResult.NotFound;
        }

        bool hasDependents = await dbContext.AccessCards
            .AnyAsync(card => card.UserId == id, cancellationToken)
            || await dbContext.AccessPermissions
                .AnyAsync(permission => permission.UserId == id, cancellationToken)
            || await dbContext.OccupancySessions
                .AnyAsync(session => session.UserId == id, cancellationToken);

        if (hasDependents)
        {
            return DeleteUserResult.HasDependents;
        }

        dbContext.Users.Remove(user);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return DeleteUserResult.Deleted;
        }
        catch (DbUpdateException exception)
            when (IsDependentForeignKeyViolation(exception))
        {
            dbContext.Entry(user).State = EntityState.Detached;
            return DeleteUserResult.HasDependents;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(user).State = EntityState.Detached;
            return DeleteUserResult.NotFound;
        }
    }

    private async Task<UserPersistenceResult> SaveAsync(
        User user,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return UserPersistenceResult.Succeeded(user);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: EmailUniqueIndex
            })
        {
            dbContext.Entry(user).State = EntityState.Detached;
            return UserPersistenceResult.DuplicateEmail();
        }
    }

    private static bool IsDependentForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: AccessCardUserForeignKey
                or AccessPermissionUserForeignKey
                or OccupancySessionUserForeignKey
        };
    }
}