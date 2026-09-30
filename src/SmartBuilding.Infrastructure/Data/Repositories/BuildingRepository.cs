using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartBuilding.Application.Buildings;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class BuildingRepository(SmartBuildingDbContext dbContext)
    : IBuildingRepository
{
    public async Task<IReadOnlyList<Building>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Buildings
            .AsNoTracking()
            .OrderBy(building => building.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Buildings
            .AsNoTracking()
            .SingleOrDefaultAsync(building => building.Id == id, cancellationToken);
    }

    public async Task<Building> CreateAsync(
        string name,
        string address,
        CancellationToken cancellationToken)
    {
        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = name,
            Address = address,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Buildings.Add(building);
        await dbContext.SaveChangesAsync(cancellationToken);
        return building;
    }

    public async Task<Building?> UpdateAsync(
        Guid id,
        string name,
        string address,
        bool isActive,
        CancellationToken cancellationToken)
    {
        Building? building = await dbContext.Buildings
            .SingleOrDefaultAsync(building => building.Id == id, cancellationToken);

        if (building is null)
        {
            return null;
        }

        building.Name = name;
        building.Address = address;
        building.IsActive = isActive;

        await dbContext.SaveChangesAsync(cancellationToken);
        return building;
    }

    public async Task<DeleteBuildingResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Building? building = await dbContext.Buildings
            .SingleOrDefaultAsync(building => building.Id == id, cancellationToken);

        if (building is null)
        {
            return DeleteBuildingResult.NotFound;
        }

        bool hasFloors = await dbContext.Floors
            .AnyAsync(floor => floor.BuildingId == id, cancellationToken);

        if (hasFloors)
        {
            return DeleteBuildingResult.HasFloors;
        }

        dbContext.Buildings.Remove(building);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation
            })
        {
            return DeleteBuildingResult.HasFloors;
        }

        return DeleteBuildingResult.Deleted;
    }
}