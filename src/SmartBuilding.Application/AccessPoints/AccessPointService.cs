using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessPoints;

public sealed class AccessPointService(IAccessPointRepository repository)
    : IAccessPointService
{
    public async Task<IReadOnlyList<AccessPointDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessPoint> accessPoints =
            await repository.GetAllAsync(cancellationToken);
        return accessPoints.Select(ToDto).ToArray();
    }

    public async Task<AccessPointDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        AccessPoint? accessPoint = await repository.GetByIdAsync(id, cancellationToken);
        return accessPoint is null ? null : ToDto(accessPoint);
    }

    public async Task<AccessPointSaveResult> CreateAsync(
        CreateAccessPointCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateFloorId(command.FloorId);
        string name = NormalizeRequired(command.Name, nameof(command.Name), 150);
        string location = NormalizeRequired(command.Location, nameof(command.Location), 250);

        AccessPointPersistenceResult result = await repository.CreateAsync(
            command.FloorId,
            name,
            location,
            command.SupportsEntry,
            command.SupportsExit,
            cancellationToken);

        return ToSaveResult(result);
    }

    public async Task<AccessPointSaveResult> UpdateAsync(
        Guid id,
        UpdateAccessPointCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateFloorId(command.FloorId);
        string name = NormalizeRequired(command.Name, nameof(command.Name), 150);
        string location = NormalizeRequired(command.Location, nameof(command.Location), 250);

        AccessPointPersistenceResult result = await repository.UpdateAsync(
            id,
            command.FloorId,
            name,
            location,
            command.IsActive,
            command.SupportsEntry,
            command.SupportsExit,
            cancellationToken);

        return ToSaveResult(result);
    }

    public Task<DeleteAccessPointResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }

    private static AccessPointSaveResult ToSaveResult(AccessPointPersistenceResult result)
    {
        return result.Status switch
        {
            AccessPointSaveStatus.Success =>
                AccessPointSaveResult.Succeeded(ToDto(result.AccessPoint!)),
            AccessPointSaveStatus.AccessPointNotFound =>
                AccessPointSaveResult.MissingAccessPoint(),
            AccessPointSaveStatus.FloorNotFound =>
                AccessPointSaveResult.MissingFloor(),
            _ => throw new InvalidOperationException(
                $"Unsupported access point save status: {result.Status}.")
        };
    }

    private static AccessPointDto ToDto(AccessPoint accessPoint)
    {
        return new AccessPointDto(
            accessPoint.Id,
            accessPoint.FloorId,
            accessPoint.Name,
            accessPoint.Location,
            accessPoint.IsActive,
            accessPoint.SupportsEntry,
            accessPoint.SupportsExit,
            accessPoint.CreatedAt);
    }

    private static void ValidateFloorId(Guid floorId)
    {
        if (floorId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessPointCommand.FloorId),
                "FloorId is required.");
        }
    }

    private static string NormalizeRequired(string value, string propertyName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApplicationValidationException(propertyName, "Value is required.");
        }

        string normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ApplicationValidationException(
                propertyName,
                $"Value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}