namespace SmartBuilding.Application.Floors;

public sealed record CreateFloorCommand(Guid BuildingId, int Number, string Name);