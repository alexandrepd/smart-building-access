using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessPoints;

public sealed record AccessPointPersistenceResult
{
    private AccessPointPersistenceResult(AccessPointSaveStatus status, AccessPoint? accessPoint)
    {
        Status = status;
        AccessPoint = accessPoint;
    }

    public AccessPointSaveStatus Status { get; }

    public AccessPoint? AccessPoint { get; }

    public static AccessPointPersistenceResult Succeeded(AccessPoint accessPoint) =>
        new(AccessPointSaveStatus.Success, accessPoint);

    public static AccessPointPersistenceResult MissingAccessPoint() =>
        new(AccessPointSaveStatus.AccessPointNotFound, null);

    public static AccessPointPersistenceResult MissingFloor() =>
        new(AccessPointSaveStatus.FloorNotFound, null);
}