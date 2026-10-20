using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Application.AccessControl;

public sealed record AccessRequestCommand(
    string CardNumber,
    Guid AccessPointId,
    AccessDirection Direction);
