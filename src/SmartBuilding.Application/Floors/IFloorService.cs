namespace SmartBuilding.Application.Floors;

public interface IFloorService
{
    Task<IReadOnlyList<FloorDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<FloorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FloorSaveResult> CreateAsync(
        CreateFloorCommand command,
        CancellationToken cancellationToken);

    Task<FloorSaveResult> UpdateAsync(
        Guid id,
        UpdateFloorCommand command,
        CancellationToken cancellationToken);

    Task<DeleteFloorResult> DeleteAsync(Guid id, CancellationToken cancellationToken);
}