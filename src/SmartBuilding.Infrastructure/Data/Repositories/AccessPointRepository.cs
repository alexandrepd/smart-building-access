using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartBuilding.Application.AccessPoints;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class AccessPointRepository(SmartBuildingDbContext dbContext)
    : IAccessPointRepository
{
    private const string AccessPointFloorForeignKey = "fk_access_points_floors_floor_id";
    private const string AccessPermissionAccessPointForeignKey = "fk_access_permissions_access_points_access_point_id";
    private const string AccessEventAccessPointForeignKey = "fk_access_events_access_points_access_point_id";
    private const string SecurityAlertAccessPointForeignKey = "fk_security_alerts_access_points_access_point_id";

    public async Task<IReadOnlyList<AccessPoint>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.AccessPoints
            .AsNoTracking()
            .OrderBy(accessPoint => accessPoint.FloorId)
            .ThenBy(accessPoint => accessPoint.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.AccessPoints
            .AsNoTracking()
            .SingleOrDefaultAsync(accessPoint => accessPoint.Id == id, cancellationToken);
    }

    public async Task<AccessPointPersistenceResult> CreateAsync(
        Guid floorId,
        string name,
        string location,
        bool supportsEntry,
        bool supportsExit,
        CancellationToken cancellationToken)
    {
        var accessPoint = new AccessPoint
        {
            Id = Guid.NewGuid(),
            FloorId = floorId,
            Name = name,
            Location = location,
            SupportsEntry = supportsEntry,
            SupportsExit = supportsExit,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.AccessPoints.Add(accessPoint);
        return await SaveAsync(accessPoint, cancellationToken);
    }

    public async Task<AccessPointPersistenceResult> UpdateAsync(
        Guid id,
        Guid floorId,
        string name,
        string location,
        bool isActive,
        bool supportsEntry,
        bool supportsExit,
        CancellationToken cancellationToken)
    {
        AccessPoint? accessPoint = await dbContext.AccessPoints
            .SingleOrDefaultAsync(accessPoint => accessPoint.Id == id, cancellationToken);

        if (accessPoint is null)
        {
            return AccessPointPersistenceResult.MissingAccessPoint();
        }

        accessPoint.FloorId = floorId;
        accessPoint.Name = name;
        accessPoint.Location = location;
        accessPoint.IsActive = isActive;
        accessPoint.SupportsEntry = supportsEntry;
        accessPoint.SupportsExit = supportsExit;

        return await SaveAsync(accessPoint, cancellationToken);
    }

    public async Task<DeleteAccessPointResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        AccessPoint? accessPoint = await dbContext.AccessPoints
            .SingleOrDefaultAsync(accessPoint => accessPoint.Id == id, cancellationToken);

        if (accessPoint is null)
        {
            return DeleteAccessPointResult.NotFound;
        }

        bool hasDependents = await dbContext.AccessPermissions
            .AnyAsync(permission => permission.AccessPointId == id, cancellationToken)
            || await dbContext.AccessEvents
                .AnyAsync(accessEvent => accessEvent.AccessPointId == id, cancellationToken)
            || await dbContext.SecurityAlerts
                .AnyAsync(alert => alert.AccessPointId == id, cancellationToken);

        if (hasDependents)
        {
            return DeleteAccessPointResult.HasDependents;
        }

        dbContext.AccessPoints.Remove(accessPoint);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return DeleteAccessPointResult.Deleted;
        }
        catch (DbUpdateException exception)
            when (IsDependentForeignKeyViolation(exception))
        {
            dbContext.Entry(accessPoint).State = EntityState.Detached;
            return DeleteAccessPointResult.HasDependents;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(accessPoint).State = EntityState.Detached;
            return DeleteAccessPointResult.NotFound;
        }
    }

    private async Task<AccessPointPersistenceResult> SaveAsync(
        AccessPoint accessPoint,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return AccessPointPersistenceResult.Succeeded(accessPoint);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: AccessPointFloorForeignKey
            })
        {
            dbContext.Entry(accessPoint).State = EntityState.Detached;
            return AccessPointPersistenceResult.MissingFloor();
        }
    }

    private static bool IsDependentForeignKeyViolation(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: AccessPermissionAccessPointForeignKey
                or AccessEventAccessPointForeignKey
                or SecurityAlertAccessPointForeignKey
        };
    }
}