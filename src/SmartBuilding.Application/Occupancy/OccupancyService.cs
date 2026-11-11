namespace SmartBuilding.Application.Occupancy;

public sealed class OccupancyService(
    IOccupancyRepository repository,
    TimeProvider timeProvider) : IOccupancyService
{
    public async Task<IReadOnlyList<CurrentOccupancyFloorDto>> GetCurrentOccupancyAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OccupancyFloorData> floors =
            await repository.GetCurrentOccupancyAsync(cancellationToken);
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;

        return floors.Select(floor =>
        {
            CurrentOccupantDto[] occupants = floor.Sessions
                .Select(session => new CurrentOccupantDto(
                    session.UserId,
                    session.UserName,
                    session.EnteredAt,
                    Math.Max(0, (long)(now - session.EnteredAt).TotalSeconds)))
                .ToArray();

            return new CurrentOccupancyFloorDto(
                floor.BuildingId,
                floor.BuildingName,
                floor.FloorId,
                floor.FloorName,
                occupants.Length,
                occupants);
        }).ToArray();
    }
}
