using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.Users;

public sealed record UserPersistenceResult
{
    private UserPersistenceResult(UserSaveStatus status, User? user)
    {
        Status = status;
        User = user;
    }

    public UserSaveStatus Status { get; }

    public User? User { get; }

    public static UserPersistenceResult Succeeded(User user) =>
        new(UserSaveStatus.Success, user);

    public static UserPersistenceResult MissingUser() =>
        new(UserSaveStatus.UserNotFound, null);

    public static UserPersistenceResult DuplicateEmail() =>
        new(UserSaveStatus.EmailAlreadyExists, null);
}