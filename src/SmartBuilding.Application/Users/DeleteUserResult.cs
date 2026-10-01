namespace SmartBuilding.Application.Users;

public enum DeleteUserResult
{
    Deleted = 1,
    NotFound = 2,
    HasDependents = 3
}