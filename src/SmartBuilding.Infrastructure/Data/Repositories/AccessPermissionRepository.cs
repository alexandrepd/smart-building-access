using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartBuilding.Application.AccessPermissions;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class AccessPermissionRepository(SmartBuildingDbContext dbContext)
    : IAccessPermissionRepository
{
    private const string UserForeignKey = "fk_access_permissions_users_user_id";
    private const string AccessPointForeignKey =
        "fk_access_permissions_access_points_access_point_id";

    public async Task<IReadOnlyList<AccessPermission>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.AccessPermissions
            .AsNoTracking()
            .OrderBy(permission => permission.UserId)
            .ThenBy(permission => permission.AccessPointId)
            .ThenBy(permission => permission.ValidFrom)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AccessPermission?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.AccessPermissions
            .AsNoTracking()
            .SingleOrDefaultAsync(permission => permission.Id == id, cancellationToken);
    }

    public async Task<AccessPermissionPersistenceResult> CreateAsync(
        Guid userId,
        Guid accessPointId,
        DateTime validFrom,
        DateTime? validUntil,
        CancellationToken cancellationToken)
    {
        var permission = new AccessPermission
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccessPointId = accessPointId,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            IsActive = true
        };

        dbContext.AccessPermissions.Add(permission);
        return await SaveAsync(permission, cancellationToken);
    }

    public async Task<AccessPermissionPersistenceResult> UpdateAsync(
        Guid id,
        Guid userId,
        Guid accessPointId,
        DateTime validFrom,
        DateTime? validUntil,
        bool isActive,
        CancellationToken cancellationToken)
    {
        AccessPermission? permission = await dbContext.AccessPermissions
            .SingleOrDefaultAsync(permission => permission.Id == id, cancellationToken);

        if (permission is null)
        {
            return AccessPermissionPersistenceResult.MissingPermission();
        }

        permission.UserId = userId;
        permission.AccessPointId = accessPointId;
        permission.ValidFrom = validFrom;
        permission.ValidUntil = validUntil;
        permission.IsActive = isActive;
        return await SaveAsync(permission, cancellationToken);
    }

    public async Task<DeleteAccessPermissionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        AccessPermission? permission = await dbContext.AccessPermissions
            .SingleOrDefaultAsync(permission => permission.Id == id, cancellationToken);

        if (permission is null)
        {
            return DeleteAccessPermissionResult.NotFound;
        }

        dbContext.AccessPermissions.Remove(permission);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return DeleteAccessPermissionResult.Deleted;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(permission).State = EntityState.Detached;
            return DeleteAccessPermissionResult.NotFound;
        }
    }

    private async Task<AccessPermissionPersistenceResult> SaveAsync(
        AccessPermission permission,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return AccessPermissionPersistenceResult.Succeeded(permission);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: UserForeignKey
            })
        {
            dbContext.Entry(permission).State = EntityState.Detached;
            return AccessPermissionPersistenceResult.MissingUser();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: AccessPointForeignKey
            })
        {
            dbContext.Entry(permission).State = EntityState.Detached;
            return AccessPermissionPersistenceResult.MissingAccessPoint();
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(permission).State = EntityState.Detached;
            return AccessPermissionPersistenceResult.MissingPermission();
        }
    }
}
