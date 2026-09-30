using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.Buildings;

public interface IBuildingRepository
{
    Task<IReadOnlyList<Building>> GetAllAsync(CancellationToken cancellationToken);

    Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Building> CreateAsync(
        string name,
        string address,
        CancellationToken cancellationToken);

    Task<Building?> UpdateAsync(
        Guid id,
        string name,
        string address,
        bool isActive,
        CancellationToken cancellationToken);

    Task<DeleteBuildingResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}