namespace SmartBuilding.Application.Occupancy;

public interface IOccupancyRepository
{
    Task<IReadOnlyList<OccupancyFloorData>> GetCurrentOccupancyAsync(
        CancellationToken cancellationToken);
}
