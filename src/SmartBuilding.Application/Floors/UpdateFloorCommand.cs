namespace SmartBuilding.Application.Floors;

public sealed record UpdateFloorCommand(
    Guid BuildingId,
    int Number,
    string Name,
    bool IsActive);