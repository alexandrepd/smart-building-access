namespace SmartBuilding.Application.AccessPoints;

public sealed record AccessPointDto(
    Guid Id,
    Guid FloorId,
    string Name,
    string Location,
    bool IsActive,
    bool SupportsEntry,
    bool SupportsExit,
    DateTime CreatedAt);