using SmartBuilding.Domain.Entities;
using SmartBuilding.Application.Common;

namespace SmartBuilding.Application.Buildings;

public sealed class BuildingService(IBuildingRepository repository) : IBuildingService
{
    public async Task<IReadOnlyList<BuildingDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Building> buildings =
            await repository.GetAllAsync(cancellationToken);

        return buildings.Select(ToDto).ToArray();
    }

    public async Task<BuildingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Building? building = await repository.GetByIdAsync(id, cancellationToken);
        return building is null ? null : ToDto(building);
    }

    public async Task<BuildingDto> CreateAsync(
        CreateBuildingCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        string name = NormalizeRequired(command.Name, nameof(command.Name), 200);
        string address = NormalizeRequired(command.Address, nameof(command.Address), 500);

        Building building = await repository.CreateAsync(
            name,
            address,
            cancellationToken);

        return ToDto(building);
    }

    public async Task<BuildingDto?> UpdateAsync(
        Guid id,
        UpdateBuildingCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        string name = NormalizeRequired(command.Name, nameof(command.Name), 200);
        string address = NormalizeRequired(command.Address, nameof(command.Address), 500);

        Building? building = await repository.UpdateAsync(
            id,
            name,
            address,
            command.IsActive,
            cancellationToken);

        return building is null ? null : ToDto(building);
    }

    public Task<DeleteBuildingResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }

    private static BuildingDto ToDto(Building building)
    {
        return new BuildingDto(
            building.Id,
            building.Name,
            building.Address,
            building.IsActive,
            building.CreatedAt);
    }

    private static string NormalizeRequired(string value, string parameterName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApplicationValidationException(parameterName, "Value is required.");
        }

        string normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ApplicationValidationException(
                parameterName,
                $"Value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}