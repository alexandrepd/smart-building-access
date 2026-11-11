namespace SmartBuilding.Application.Occupancy;

public sealed record CurrentOccupancyFloorDto(
    Guid BuildingId,
    string BuildingName,
    Guid FloorId,
    string FloorName,
    int OccupantCount,
    IReadOnlyList<CurrentOccupantDto> Occupants);
