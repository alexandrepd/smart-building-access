namespace SmartBuilding.Application.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserSaveResult> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken);

    Task<UserSaveResult> UpdateAsync(
        Guid id,
        UpdateUserCommand command,
        CancellationToken cancellationToken);

    Task<DeleteUserResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}