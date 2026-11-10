using SmartBuilding.Domain.Entities;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Application.Users;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserPersistenceResult> CreateAsync(
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken);

    Task<UserPersistenceResult> UpdateAsync(
        Guid id,
        string name,
        string email,
        bool isActive,
        CancellationToken cancellationToken);

    Task<DeleteUserResult> DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<UserRoleUpdateResult> UpdateRoleAsync(
        Guid id,
        UserRole role,
        CancellationToken cancellationToken);
}