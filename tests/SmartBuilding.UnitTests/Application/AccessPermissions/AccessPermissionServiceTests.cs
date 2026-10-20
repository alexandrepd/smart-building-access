using SmartBuilding.Application.AccessPermissions;
using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Application.AccessPermissions;

public class AccessPermissionServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidCommand_NormalizesValidityToUtc()
    {
        DateTimeOffset validFrom = new(2030, 1, 2, 3, 0, 0, TimeSpan.FromHours(3));
        DateTimeOffset validUntil = validFrom.AddDays(2);
        var repository = new FakeAccessPermissionRepository();
        var service = new AccessPermissionService(repository);
        Guid userId = Guid.NewGuid();
        Guid accessPointId = Guid.NewGuid();

        AccessPermissionSaveResult result = await service.CreateAsync(
            new CreateAccessPermissionCommand(userId, accessPointId, validFrom, validUntil),
            CancellationToken.None);

        Assert.Equal(AccessPermissionSaveStatus.Success, result.Status);
        Assert.Equal(userId, repository.UserId);
        Assert.Equal(accessPointId, repository.AccessPointId);
        Assert.Equal(validFrom.UtcDateTime, repository.ValidFrom);
        Assert.Equal(validUntil.UtcDateTime, repository.ValidUntil);
        Assert.Equal(validFrom.UtcDateTime, result.Permission?.ValidFrom);
        Assert.True(result.Permission?.IsActive);
    }

    [Fact]
    public async Task CreateAsync_NoEndDate_PreservesOpenEndedValidity()
    {
        var repository = new FakeAccessPermissionRepository();
        var service = new AccessPermissionService(repository);
        DateTimeOffset validFrom = DateTimeOffset.UtcNow;

        AccessPermissionSaveResult result = await service.CreateAsync(
            new CreateAccessPermissionCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                validFrom,
                null),
            CancellationToken.None);

        Assert.Equal(AccessPermissionSaveStatus.Success, result.Status);
        Assert.Null(repository.ValidUntil);
        Assert.Null(result.Permission?.ValidUntil);
    }

    [Fact]
    public async Task CreateAsync_EndAtOrBeforeStart_ThrowsValidationException()
    {
        DateTimeOffset validFrom = DateTimeOffset.UtcNow;
        var service = new AccessPermissionService(new FakeAccessPermissionRepository());

        Task Act() => service.CreateAsync(
            new CreateAccessPermissionCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                validFrom,
                validFrom),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_EmptyUserId_ThrowsValidationException()
    {
        var service = new AccessPermissionService(new FakeAccessPermissionRepository());

        Task Act() => service.CreateAsync(
            new CreateAccessPermissionCommand(
                Guid.Empty,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                null),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Theory]
    [InlineData(AccessPermissionSaveStatus.UserNotFound)]
    [InlineData(AccessPermissionSaveStatus.AccessPointNotFound)]
    public async Task CreateAsync_MissingReference_ReturnsNotFoundStatus(
        AccessPermissionSaveStatus status)
    {
        var repository = new FakeAccessPermissionRepository { ResultStatus = status };
        var service = new AccessPermissionService(repository);

        AccessPermissionSaveResult result = await service.CreateAsync(
            new CreateAccessPermissionCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                null),
            CancellationToken.None);

        Assert.Equal(status, result.Status);
        Assert.Null(result.Permission);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPermission_ReplacesState()
    {
        var repository = new FakeAccessPermissionRepository();
        var service = new AccessPermissionService(repository);
        DateTimeOffset validFrom = DateTimeOffset.UtcNow;
        Guid userId = Guid.NewGuid();
        Guid accessPointId = Guid.NewGuid();

        AccessPermissionSaveResult result = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateAccessPermissionCommand(
                userId,
                accessPointId,
                validFrom,
                null,
                false),
            CancellationToken.None);

        Assert.Equal(AccessPermissionSaveStatus.Success, result.Status);
        Assert.Equal(userId, result.Permission?.UserId);
        Assert.Equal(accessPointId, result.Permission?.AccessPointId);
        Assert.False(result.Permission?.IsActive);
        Assert.Equal(validFrom.UtcDateTime, result.Permission?.ValidFrom);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryReportsMissingPermission_ReturnsNotFound()
    {
        var repository = new FakeAccessPermissionRepository
        {
            DeleteResult = DeleteAccessPermissionResult.NotFound
        };
        var service = new AccessPermissionService(repository);

        DeleteAccessPermissionResult result = await service.DeleteAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(DeleteAccessPermissionResult.NotFound, result);
    }

    private sealed class FakeAccessPermissionRepository : IAccessPermissionRepository
    {
        public AccessPermissionSaveStatus ResultStatus { get; init; } =
            AccessPermissionSaveStatus.Success;

        public DeleteAccessPermissionResult DeleteResult { get; init; } =
            DeleteAccessPermissionResult.Deleted;

        public Guid UserId { get; private set; }

        public Guid AccessPointId { get; private set; }

        public DateTime ValidFrom { get; private set; }

        public DateTime? ValidUntil { get; private set; }

        public Task<IReadOnlyList<AccessPermission>> GetAllAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AccessPermission>>(
                Array.Empty<AccessPermission>());
        }

        public Task<AccessPermission?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<AccessPermission?>(null);
        }

        public Task<AccessPermissionPersistenceResult> CreateAsync(
            Guid userId,
            Guid accessPointId,
            DateTime validFrom,
            DateTime? validUntil,
            CancellationToken cancellationToken)
        {
            UserId = userId;
            AccessPointId = accessPointId;
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            return Task.FromResult(CreateResult(userId, accessPointId, validFrom, validUntil, true));
        }

        public Task<AccessPermissionPersistenceResult> UpdateAsync(
            Guid id,
            Guid userId,
            Guid accessPointId,
            DateTime validFrom,
            DateTime? validUntil,
            bool isActive,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                CreateResult(userId, accessPointId, validFrom, validUntil, isActive));
        }

        public Task<DeleteAccessPermissionResult> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(DeleteResult);
        }

        private AccessPermissionPersistenceResult CreateResult(
            Guid userId,
            Guid accessPointId,
            DateTime validFrom,
            DateTime? validUntil,
            bool isActive)
        {
            return ResultStatus == AccessPermissionSaveStatus.Success
                ? AccessPermissionPersistenceResult.Succeeded(new AccessPermission
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AccessPointId = accessPointId,
                    ValidFrom = validFrom,
                    ValidUntil = validUntil,
                    IsActive = isActive
                })
                : ResultStatus switch
                {
                    AccessPermissionSaveStatus.AccessPermissionNotFound =>
                        AccessPermissionPersistenceResult.MissingPermission(),
                    AccessPermissionSaveStatus.UserNotFound =>
                        AccessPermissionPersistenceResult.MissingUser(),
                    AccessPermissionSaveStatus.AccessPointNotFound =>
                        AccessPermissionPersistenceResult.MissingAccessPoint(),
                    _ => throw new InvalidOperationException(
                        $"Unsupported fake result status: {ResultStatus}.")
                };
        }
    }
}
