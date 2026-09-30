namespace SmartBuilding.Application.AccessPoints;

public sealed record UpdateAccessPointCommand(
    Guid FloorId,
    string Name,
    string Location,
    bool IsActive,
    bool SupportsEntry,
    bool SupportsExit);