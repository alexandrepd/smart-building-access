namespace SmartBuilding.Application.Floors;

public enum DeleteFloorResult
{
    Deleted = 1,
    NotFound = 2,
    HasDependents = 3
}