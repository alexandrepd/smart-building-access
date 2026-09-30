using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.Floors;

public sealed class FloorService(IFloorRepository repository) : IFloorService
{
    public async Task<IReadOnlyList<FloorDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Floor> floors = await repository.GetAllAsync(cancellationToken);
        return floors.Select(ToDto).ToArray();
    }

    public async Task<FloorDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Floor? floor = await repository.GetByIdAsync(id, cancellationToken);
        return floor is null ? null : ToDto(floor);
    }

    public async Task<FloorSaveResult> CreateAsync(
        CreateFloorCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateBuildingId(command.BuildingId);
        string name = NormalizeName(command.Name);

        FloorPersistenceResult result = await repository.CreateAsync(
            command.BuildingId,
            command.Number,
            name,
            cancellationToken);

        return ToSaveResult(result);
    }

    public async Task<FloorSaveResult> UpdateAsync(
        Guid id,
        UpdateFloorCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateBuildingId(command.BuildingId);
        string name = NormalizeName(command.Name);

        FloorPersistenceResult result = await repository.UpdateAsync(
            id,
            command.BuildingId,
            command.Number,
            name,
            command.IsActive,
            cancellationToken);

        return ToSaveResult(result);
    }

    public Task<DeleteFloorResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }

    private static FloorSaveResult ToSaveResult(FloorPersistenceResult result)
    {
        return result.Status switch
        {
            FloorSaveStatus.Success => FloorSaveResult.Succeeded(ToDto(result.Floor!)),
            FloorSaveStatus.FloorNotFound => FloorSaveResult.MissingFloor(),
            FloorSaveStatus.BuildingNotFound => FloorSaveResult.MissingBuilding(),
            _ => throw new InvalidOperationException($"Unsupported floor save status: {result.Status}.")
        };
    }

    private static FloorDto ToDto(Floor floor)
    {
        return new FloorDto(
            floor.Id,
            floor.BuildingId,
            floor.Number,
            floor.Name,
            floor.IsActive);
    }

    private static void ValidateBuildingId(Guid buildingId)
    {
        if (buildingId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                nameof(CreateFloorCommand.BuildingId),
                "BuildingId is required.");
        }
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ApplicationValidationException(
                nameof(CreateFloorCommand.Name),
                "Name is required.");
        }

        string normalized = name.Trim();
        if (normalized.Length > 100)
        {
            throw new ApplicationValidationException(
                nameof(CreateFloorCommand.Name),
                "Name cannot exceed 100 characters.");
        }

        return normalized;
    }
}