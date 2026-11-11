namespace SmartBuilding.Application.Occupancy;

public interface IOccupancyService
{
    Task<IReadOnlyList<CurrentOccupancyFloorDto>> GetCurrentOccupancyAsync(
        CancellationToken cancellationToken);
}
