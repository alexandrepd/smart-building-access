namespace SmartBuilding.Application.Occupancy;

public sealed record OccupancySessionData(
    Guid Id,
    Guid UserId,
    string UserName,
    DateTime EnteredAt);
