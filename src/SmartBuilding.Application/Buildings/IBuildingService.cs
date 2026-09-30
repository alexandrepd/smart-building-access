namespace SmartBuilding.Application.Buildings;

public interface IBuildingService
{
    Task<IReadOnlyList<BuildingDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<BuildingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<BuildingDto> CreateAsync(
        CreateBuildingCommand command,
        CancellationToken cancellationToken);

    Task<BuildingDto?> UpdateAsync(
        Guid id,
        UpdateBuildingCommand command,
        CancellationToken cancellationToken);

    Task<DeleteBuildingResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}