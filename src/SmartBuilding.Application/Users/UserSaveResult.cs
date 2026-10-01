namespace SmartBuilding.Application.Users;

public sealed record UserSaveResult
{
    private UserSaveResult(UserSaveStatus status, UserDto? user)
    {
        Status = status;
        User = user;
    }

    public UserSaveStatus Status { get; }

    public UserDto? User { get; }

    public static UserSaveResult Succeeded(UserDto user) =>
        new(UserSaveStatus.Success, user);

    public static UserSaveResult MissingUser() =>
        new(UserSaveStatus.UserNotFound, null);

    public static UserSaveResult DuplicateEmail() =>
        new(UserSaveStatus.EmailAlreadyExists, null);
}