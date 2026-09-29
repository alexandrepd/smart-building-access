namespace SmartBuilding.Domain.Enums;

public enum AccessResult
{
    Granted = 1,
    CardNotFound = 2,
    CardInactive = 3,
    CardExpired = 4,
    UserInactive = 5,
    AccessPointInactive = 6,
    NoPermission = 7,
    PermissionExpired = 8
}