using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartBuilding.Application.Floors;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class FloorRepository(SmartBuildingDbContext dbContext) : IFloorRepository
{
    private const string FloorBuildingForeignKey = "fk_floors_buildings_building_id";
    private const string AccessPointFloorForeignKey = "fk_access_points_floors_floor_id";
    private const string OccupancySessionFloorForeignKey = "fk_occupancy_sessions_floors_floor_id";

    public async Task<IReadOnlyList<Floor>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Floors
            .AsNoTracking()
            .OrderBy(floor => floor.BuildingId)
            .ThenBy(floor => floor.Number)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Floor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Floors
            .AsNoTracking()
            .SingleOrDefaultAsync(floor => floor.Id == id, cancellationToken);
    }

    public async Task<FloorPersistenceResult> CreateAsync(
        Guid buildingId,
        int number,
        string name,
        CancellationToken cancellationToken)
    {
        var floor = new Floor
        {
            Id = Guid.NewGuid(),
            BuildingId = buildingId,
            Number = number,
            Name = name
        };

        dbContext.Floors.Add(floor);
        return await SaveAsync(floor, cancellationToken);
    }

    public async Task<FloorPersistenceResult> UpdateAsync(
        Guid id,
        Guid buildingId,
        int number,
        string name,
        bool isActive,
        CancellationToken cancellationToken)
    {
        Floor? floor = await dbContext.Floors
            .SingleOrDefaultAsync(floor => floor.Id == id, cancellationToken);

        if (floor is null)
        {
            return FloorPersistenceResult.MissingFloor();
        }

        floor.BuildingId = buildingId;
        floor.Number = number;
        floor.Name = name;
        floor.IsActive = isActive;

        return await SaveAsync(floor, cancellationToken);
    }

    public async Task<DeleteFloorResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Floor? floor = await dbContext.Floors
            .SingleOrDefaultAsync(floor => floor.Id == id, cancellationToken);

        if (floor is null)
        {
            return DeleteFloorResult.NotFound;
        }

        bool hasDependents = await dbContext.AccessPoints
            .AnyAsync(accessPoint => accessPoint.FloorId == id, cancellationToken)
            || await dbContext.OccupancySessions
                .AnyAsync(session => session.FloorId == id, cancellationToken);

        if (hasDependents)
        {
            return DeleteFloorResult.HasDependents;
        }

        dbContext.Floors.Remove(floor);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return DeleteFloorResult.Deleted;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: AccessPointFloorForeignKey or OccupancySessionFloorForeignKey
            })
        {
            dbContext.Entry(floor).State = EntityState.Detached;
            return DeleteFloorResult.HasDependents;
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(floor).State = EntityState.Detached;
            return DeleteFloorResult.NotFound;
        }
    }

    private async Task<FloorPersistenceResult> SaveAsync(
        Floor floor,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return FloorPersistenceResult.Succeeded(floor);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: FloorBuildingForeignKey
            })
        {
            dbContext.Entry(floor).State = EntityState.Detached;
            return FloorPersistenceResult.MissingBuilding();
        }
    }
}