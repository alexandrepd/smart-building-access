namespace SmartBuilding.Application.AccessPoints;

public interface IAccessPointService
{
    Task<IReadOnlyList<AccessPointDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<AccessPointDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccessPointSaveResult> CreateAsync(
        CreateAccessPointCommand command,
        CancellationToken cancellationToken);

    Task<AccessPointSaveResult> UpdateAsync(
        Guid id,
        UpdateAccessPointCommand command,
        CancellationToken cancellationToken);

    Task<DeleteAccessPointResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}