using SmartBuilding.Application.Occupancy;

namespace SmartBuilding.Api.Contracts.Occupancy;

/// <summary>Represents a floor's current occupancy and active sessions.</summary>
public sealed record CurrentOccupancyResponse(
    Guid BuildingId,
    string BuildingName,
    Guid FloorId,
    string FloorName,
    int OccupantCount,
    IReadOnlyList<CurrentOccupantResponse> Occupants)
{
    internal static CurrentOccupancyResponse FromDto(CurrentOccupancyFloorDto occupancy)
    {
        return new CurrentOccupancyResponse(
            occupancy.BuildingId,
            occupancy.BuildingName,
            occupancy.FloorId,
            occupancy.FloorName,
            occupancy.OccupantCount,
            occupancy.Occupants.Select(CurrentOccupantResponse.FromDto).ToArray());
    }
}

/// <summary>Represents a user currently present on a floor.</summary>
public sealed record CurrentOccupantResponse(
    Guid UserId,
    string UserName,
    DateTimeOffset EnteredAt,
    long DurationSeconds)
{
    internal static CurrentOccupantResponse FromDto(CurrentOccupantDto occupant)
    {
        return new CurrentOccupantResponse(
            occupant.UserId,
            occupant.UserName,
            new DateTimeOffset(occupant.EnteredAt),
            occupant.DurationSeconds);
    }
}
