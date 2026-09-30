using SmartBuilding.Application.Floors;

namespace SmartBuilding.Api.Contracts.Floors;

/// <summary>Represents a floor returned by the API.</summary>
public sealed record FloorResponse(
    Guid Id,
    Guid BuildingId,
    int Number,
    string Name,
    bool IsActive)
{
    internal static FloorResponse FromDto(FloorDto floor)
    {
        return new FloorResponse(
            floor.Id,
            floor.BuildingId,
            floor.Number,
            floor.Name,
            floor.IsActive);
    }
}