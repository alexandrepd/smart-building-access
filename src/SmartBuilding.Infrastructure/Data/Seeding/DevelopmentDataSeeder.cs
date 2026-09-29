using Microsoft.EntityFrameworkCore;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Seeding;

public sealed class DevelopmentDataSeeder(SmartBuildingDbContext dbContext)
{
    private static readonly Guid BuildingId = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid GroundFloorId = new("20000000-0000-0000-0000-000000000001");
    private static readonly Guid MainEntranceId = new("30000000-0000-0000-0000-000000000001");
    private static readonly DateTime SeedCreatedAt =
        new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(1937001)",
            cancellationToken);

        if (!await dbContext.Buildings.AnyAsync(
                building => building.Id == BuildingId,
                cancellationToken))
        {
            dbContext.Buildings.Add(new Building
            {
                Id = BuildingId,
                Name = "HQ Lisbon",
                Address = "Lisbon",
                CreatedAt = SeedCreatedAt
            });
        }

        if (!await dbContext.Floors.AnyAsync(
                floor => floor.Id == GroundFloorId,
                cancellationToken))
        {
            dbContext.Floors.Add(new Floor
            {
                Id = GroundFloorId,
                BuildingId = BuildingId,
                Number = 0,
                Name = "Ground Floor"
            });
        }

        if (!await dbContext.AccessPoints.AnyAsync(
                accessPoint => accessPoint.Id == MainEntranceId,
                cancellationToken))
        {
            dbContext.AccessPoints.Add(new AccessPoint
            {
                Id = MainEntranceId,
                FloorId = GroundFloorId,
                Name = "Main Entrance",
                Location = "Ground Floor Lobby",
                SupportsEntry = true,
                SupportsExit = true,
                CreatedAt = SeedCreatedAt
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}