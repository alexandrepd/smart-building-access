namespace SmartBuilding.Application.AccessPoints;

public enum DeleteAccessPointResult
{
    Deleted = 1,
    NotFound = 2,
    HasDependents = 3
}