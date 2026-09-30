namespace SmartBuilding.Application.AccessPoints;

public sealed record AccessPointSaveResult
{
    private AccessPointSaveResult(AccessPointSaveStatus status, AccessPointDto? accessPoint)
    {
        Status = status;
        AccessPoint = accessPoint;
    }

    public AccessPointSaveStatus Status { get; }

    public AccessPointDto? AccessPoint { get; }

    public static AccessPointSaveResult Succeeded(AccessPointDto accessPoint) =>
        new(AccessPointSaveStatus.Success, accessPoint);

    public static AccessPointSaveResult MissingAccessPoint() =>
        new(AccessPointSaveStatus.AccessPointNotFound, null);

    public static AccessPointSaveResult MissingFloor() =>
        new(AccessPointSaveStatus.FloorNotFound, null);
}