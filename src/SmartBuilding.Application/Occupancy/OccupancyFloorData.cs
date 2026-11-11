namespace SmartBuilding.Application.Occupancy;

public sealed record OccupancyFloorData(
    Guid BuildingId,
    string BuildingName,
    Guid FloorId,
    string FloorName,
    IReadOnlyList<OccupancySessionData> Sessions);
