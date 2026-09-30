namespace SmartBuilding.Application.Floors;

public sealed record FloorDto(
    Guid Id,
    Guid BuildingId,
    int Number,
    string Name,
    bool IsActive);