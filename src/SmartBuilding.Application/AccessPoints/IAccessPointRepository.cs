using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessPoints;

public interface IAccessPointRepository
{
    Task<IReadOnlyList<AccessPoint>> GetAllAsync(CancellationToken cancellationToken);

    Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccessPointPersistenceResult> CreateAsync(
        Guid floorId,
        string name,
        string location,
        bool supportsEntry,
        bool supportsExit,
        CancellationToken cancellationToken);

    Task<AccessPointPersistenceResult> UpdateAsync(
        Guid id,
        Guid floorId,
        string name,
        string location,
        bool isActive,
        bool supportsEntry,
        bool supportsExit,
        CancellationToken cancellationToken);

    Task<DeleteAccessPointResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}