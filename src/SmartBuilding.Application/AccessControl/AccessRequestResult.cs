namespace SmartBuilding.Application.AccessControl;

public sealed record AccessRequestResult(
    AccessDecisionStatus Status,
    AccessEventDto? AccessEvent)
{
    public static AccessRequestResult Recorded(AccessEventDto accessEvent) =>
        new(AccessDecisionStatus.Recorded, accessEvent);

    public static AccessRequestResult MissingAccessPoint() =>
        new(AccessDecisionStatus.AccessPointNotFound, null);
}
