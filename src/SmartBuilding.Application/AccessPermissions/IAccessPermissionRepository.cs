using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessPermissions;

public interface IAccessPermissionRepository
{
    Task<IReadOnlyList<AccessPermission>> GetAllAsync(CancellationToken cancellationToken);

    Task<AccessPermission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccessPermissionPersistenceResult> CreateAsync(
        Guid userId,
        Guid accessPointId,
        DateTime validFrom,
        DateTime? validUntil,
        CancellationToken cancellationToken);

    Task<AccessPermissionPersistenceResult> UpdateAsync(
        Guid id,
        Guid userId,
        Guid accessPointId,
        DateTime validFrom,
        DateTime? validUntil,
        bool isActive,
        CancellationToken cancellationToken);

    Task<DeleteAccessPermissionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
