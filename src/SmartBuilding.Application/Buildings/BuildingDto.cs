namespace SmartBuilding.Application.Buildings;

public sealed record BuildingDto(
    Guid Id,
    string Name,
    string Address,
    bool IsActive,
    DateTime CreatedAt);