using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessPermissions;

public sealed class AccessPermissionService(IAccessPermissionRepository repository)
    : IAccessPermissionService
{
    public async Task<IReadOnlyList<AccessPermissionDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessPermission> permissions =
            await repository.GetAllAsync(cancellationToken);
        return permissions.Select(ToDto).ToArray();
    }

    public async Task<AccessPermissionDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        AccessPermission? permission = await repository.GetByIdAsync(id, cancellationToken);
        return permission is null ? null : ToDto(permission);
    }

    public async Task<AccessPermissionSaveResult> CreateAsync(
        CreateAccessPermissionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateReferences(command.UserId, command.AccessPointId);
        (DateTime validFrom, DateTime? validUntil) =
            ValidateAndNormalizeValidity(command.ValidFrom, command.ValidUntil);

        AccessPermissionPersistenceResult result = await repository.CreateAsync(
            command.UserId,
            command.AccessPointId,
            validFrom,
            validUntil,
            cancellationToken);

        return ToSaveResult(result);
    }

    public async Task<AccessPermissionSaveResult> UpdateAsync(
        Guid id,
        UpdateAccessPermissionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateReferences(command.UserId, command.AccessPointId);
        (DateTime validFrom, DateTime? validUntil) =
            ValidateAndNormalizeValidity(command.ValidFrom, command.ValidUntil);

        AccessPermissionPersistenceResult result = await repository.UpdateAsync(
            id,
            command.UserId,
            command.AccessPointId,
            validFrom,
            validUntil,
            command.IsActive,
            cancellationToken);

        return ToSaveResult(result);
    }

    public Task<DeleteAccessPermissionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }

    private static AccessPermissionSaveResult ToSaveResult(
        AccessPermissionPersistenceResult result)
    {
        return result.Status switch
        {
            AccessPermissionSaveStatus.Success =>
                AccessPermissionSaveResult.Succeeded(ToDto(result.Permission!)),
            AccessPermissionSaveStatus.AccessPermissionNotFound =>
                AccessPermissionSaveResult.MissingPermission(),
            AccessPermissionSaveStatus.UserNotFound =>
                AccessPermissionSaveResult.MissingUser(),
            AccessPermissionSaveStatus.AccessPointNotFound =>
                AccessPermissionSaveResult.MissingAccessPoint(),
            _ => throw new InvalidOperationException(
                $"Unsupported access permission save status: {result.Status}.")
        };
    }

    private static AccessPermissionDto ToDto(AccessPermission permission)
    {
        return new AccessPermissionDto(
            permission.Id,
            permission.UserId,
            permission.AccessPointId,
            permission.IsActive,
            permission.ValidFrom,
            permission.ValidUntil);
    }

    private static void ValidateReferences(Guid userId, Guid accessPointId)
    {
        if (userId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessPermissionCommand.UserId),
                "User ID is required.");
        }

        if (accessPointId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessPermissionCommand.AccessPointId),
                "Access point ID is required.");
        }
    }

    private static (DateTime ValidFrom, DateTime? ValidUntil) ValidateAndNormalizeValidity(
        DateTimeOffset validFrom,
        DateTimeOffset? validUntil)
    {
        if (validFrom == default)
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessPermissionCommand.ValidFrom),
                "Valid from is required.");
        }

        if (validUntil is not null && validUntil <= validFrom)
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessPermissionCommand.ValidUntil),
                "Valid until must be later than valid from.");
        }

        return (validFrom.UtcDateTime, validUntil?.UtcDateTime);
    }
}
