using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Application.AccessControl;

public sealed record OccupancyTransition(
    Guid UserId,
    Guid FloorId,
    AccessDirection Direction,
    DateTime OccurredAt);
