namespace SmartBuilding.Application.AccessPermissions;

public interface IAccessPermissionService
{
    Task<IReadOnlyList<AccessPermissionDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<AccessPermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccessPermissionSaveResult> CreateAsync(
        CreateAccessPermissionCommand command,
        CancellationToken cancellationToken);

    Task<AccessPermissionSaveResult> UpdateAsync(
        Guid id,
        UpdateAccessPermissionCommand command,
        CancellationToken cancellationToken);

    Task<DeleteAccessPermissionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
