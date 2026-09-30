using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.Floors;

public interface IFloorRepository
{
    Task<IReadOnlyList<Floor>> GetAllAsync(CancellationToken cancellationToken);

    Task<Floor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FloorPersistenceResult> CreateAsync(
        Guid buildingId,
        int number,
        string name,
        CancellationToken cancellationToken);

    Task<FloorPersistenceResult> UpdateAsync(
        Guid id,
        Guid buildingId,
        int number,
        string name,
        bool isActive,
        CancellationToken cancellationToken);

    Task<DeleteFloorResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}