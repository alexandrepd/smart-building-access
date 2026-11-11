using Microsoft.EntityFrameworkCore;
using SmartBuilding.Application.Occupancy;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class OccupancyRepository(SmartBuildingDbContext dbContext)
    : IOccupancyRepository
{
    public async Task<IReadOnlyList<OccupancyFloorData>> GetCurrentOccupancyAsync(
        CancellationToken cancellationToken)
    {
        var floors = await dbContext.Floors
            .AsNoTracking()
            .OrderBy(floor => floor.Building.Name)
            .ThenBy(floor => floor.Number)
            .Select(floor => new
            {
                floor.BuildingId,
                BuildingName = floor.Building.Name,
                floor.Id,
                floor.Name
            })
            .ToArrayAsync(cancellationToken);

        var sessionsByFloor = (await dbContext.OccupancySessions
                .AsNoTracking()
                .Where(session => session.ExitedAt == null)
                .OrderBy(session => session.EnteredAt)
                .Select(session => new
                {
                    session.FloorId,
                    Session = new OccupancySessionData(
                        session.Id,
                        session.UserId,
                        session.User.Name,
                        session.EnteredAt)
                })
                .ToArrayAsync(cancellationToken))
            .GroupBy(item => item.FloorId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<OccupancySessionData>)group
                    .Select(item => item.Session)
                    .ToArray());

        return floors.Select(floor => new OccupancyFloorData(
                floor.BuildingId,
                floor.BuildingName,
                floor.Id,
                floor.Name,
                sessionsByFloor.GetValueOrDefault(floor.Id, Array.Empty<OccupancySessionData>())))
            .ToArray();
    }
}
