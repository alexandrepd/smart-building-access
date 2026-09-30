namespace SmartBuilding.Application.Buildings;

public sealed record UpdateBuildingCommand(string Name, string Address, bool IsActive);