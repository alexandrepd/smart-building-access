namespace SmartBuilding.Application.Occupancy;

public sealed record CurrentOccupantDto(
    Guid UserId,
    string UserName,
    DateTime EnteredAt,
    long DurationSeconds);
