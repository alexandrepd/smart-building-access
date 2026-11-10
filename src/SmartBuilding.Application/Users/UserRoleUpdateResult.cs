namespace SmartBuilding.Application.Users;

public sealed record UserRoleUpdateResult(UserRoleUpdateStatus Status, UserDto? User)
{
    public static UserRoleUpdateResult Succeeded(UserDto user) =>
        new(UserRoleUpdateStatus.Success, user);

    public static UserRoleUpdateResult MissingUser() =>
        new(UserRoleUpdateStatus.UserNotFound, null);

    public static UserRoleUpdateResult LastAdministratorConflict() =>
        new(UserRoleUpdateStatus.LastAdministrator, null);
}
