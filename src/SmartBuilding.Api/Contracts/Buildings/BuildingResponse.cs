using SmartBuilding.Application.Buildings;

namespace SmartBuilding.Api.Contracts.Buildings;

/// <summary>Represents a building returned by the API.</summary>
public sealed record BuildingResponse(
    Guid Id,
    string Name,
    string Address,
    bool IsActive,
    DateTimeOffset CreatedAt)
{
    internal static BuildingResponse FromDto(BuildingDto building)
    {
        return new BuildingResponse(
            building.Id,
            building.Name,
            building.Address,
            building.IsActive,
            new DateTimeOffset(building.CreatedAt));
    }
}