namespace SmartBuilding.Application.AccessPoints;

public sealed record CreateAccessPointCommand(
    Guid FloorId,
    string Name,
    string Location,
    bool SupportsEntry,
    bool SupportsExit);