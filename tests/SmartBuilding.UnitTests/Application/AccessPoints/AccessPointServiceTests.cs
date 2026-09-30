using SmartBuilding.Application.AccessPoints;
using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Application.AccessPoints;

public class AccessPointServiceTests
{
    [Fact]
    public async Task GetAllAsync_ExistingAccessPoints_ReturnsMappedDirectionFlags()
    {
        var existing = new AccessPoint
        {
            Id = Guid.NewGuid(),
            FloorId = Guid.NewGuid(),
            Name = "Main Entrance",
            Location = "Lobby",
            SupportsEntry = true,
            SupportsExit = false,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var service = new AccessPointService(new FakeAccessPointRepository(existing));

        AccessPointDto result = Assert.Single(
            await service.GetAllAsync(CancellationToken.None));

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(existing.FloorId, result.FloorId);
        Assert.Equal(existing.Name, result.Name);
        Assert.Equal(existing.Location, result.Location);
        Assert.True(result.SupportsEntry);
        Assert.False(result.SupportsExit);
        Assert.Equal(existing.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task CreateAsync_ValidCommand_NormalizesTextAndPreservesDirections()
    {
        var service = new AccessPointService(new FakeAccessPointRepository());

        AccessPointSaveResult result = await service.CreateAsync(
            new CreateAccessPointCommand(Guid.NewGuid(), "  Main Entrance  ", "  Lobby  ", true, false),
            CancellationToken.None);

        Assert.Equal(AccessPointSaveStatus.Success, result.Status);
        Assert.Equal("Main Entrance", result.AccessPoint?.Name);
        Assert.Equal("Lobby", result.AccessPoint?.Location);
        Assert.True(result.AccessPoint?.SupportsEntry);
        Assert.False(result.AccessPoint?.SupportsExit);
    }

    [Fact]
    public async Task CreateAsync_EmptyFloorId_ThrowsValidationException()
    {
        var service = new AccessPointService(new FakeAccessPointRepository());

        Task Act() => service.CreateAsync(
            new CreateAccessPointCommand(Guid.Empty, "Entrance", "Lobby", true, true),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_UnknownFloor_ReturnsFloorNotFound()
    {
        var repository = new FakeAccessPointRepository
        {
            SaveStatus = AccessPointSaveStatus.FloorNotFound
        };
        var service = new AccessPointService(repository);

        AccessPointSaveResult result = await service.CreateAsync(
            new CreateAccessPointCommand(Guid.NewGuid(), "Entrance", "Lobby", true, true),
            CancellationToken.None);

        Assert.Equal(AccessPointSaveStatus.FloorNotFound, result.Status);
        Assert.Null(result.AccessPoint);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAccessPoint_ReplacesEditableState()
    {
        var existing = new AccessPoint
        {
            Id = Guid.NewGuid(),
            FloorId = Guid.NewGuid(),
            Name = "Old",
            Location = "Old Location"
        };
        var service = new AccessPointService(new FakeAccessPointRepository(existing));
        Guid newFloorId = Guid.NewGuid();

        AccessPointSaveResult result = await service.UpdateAsync(
            existing.Id,
            new UpdateAccessPointCommand(newFloorId, "  Side Exit  ", "  East Wing  ", false, false, true),
            CancellationToken.None);

        Assert.Equal(AccessPointSaveStatus.Success, result.Status);
        Assert.Equal(newFloorId, result.AccessPoint?.FloorId);
        Assert.Equal("Side Exit", result.AccessPoint?.Name);
        Assert.Equal("East Wing", result.AccessPoint?.Location);
        Assert.False(result.AccessPoint?.IsActive);
        Assert.False(result.AccessPoint?.SupportsEntry);
        Assert.True(result.AccessPoint?.SupportsExit);
    }

    [Fact]
    public async Task UpdateAsync_UnknownAccessPoint_ReturnsNotFound()
    {
        var service = new AccessPointService(new FakeAccessPointRepository());

        AccessPointSaveResult result = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateAccessPointCommand(Guid.NewGuid(), "Entrance", "Lobby", true, true, true),
            CancellationToken.None);

        Assert.Equal(AccessPointSaveStatus.AccessPointNotFound, result.Status);
        Assert.Null(result.AccessPoint);
    }

    [Fact]
    public async Task DeleteAsync_AccessPointWithDependents_ReturnsConflictResult()
    {
        var repository = new FakeAccessPointRepository
        {
            DeleteResult = DeleteAccessPointResult.HasDependents
        };
        var service = new AccessPointService(repository);

        DeleteAccessPointResult result = await service.DeleteAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(DeleteAccessPointResult.HasDependents, result);
    }

    [Theory]
    [InlineData("", "Lobby")]
    [InlineData("Entrance", "   ")]
    [InlineData("   ", "Lobby")]
    public async Task CreateAsync_BlankRequiredText_ThrowsValidationException(
        string name,
        string location)
    {
        var service = new AccessPointService(new FakeAccessPointRepository());

        Task Act() => service.CreateAsync(
            new CreateAccessPointCommand(Guid.NewGuid(), name, location, true, true),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    private sealed class FakeAccessPointRepository(params AccessPoint[] accessPoints)
        : IAccessPointRepository
    {
        private readonly List<AccessPoint> accessPoints = [.. accessPoints];

        public AccessPointSaveStatus SaveStatus { get; init; } = AccessPointSaveStatus.Success;

        public DeleteAccessPointResult DeleteResult { get; init; } = DeleteAccessPointResult.Deleted;

        public Task<IReadOnlyList<AccessPoint>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AccessPoint>>(accessPoints);
        }

        public Task<AccessPoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(accessPoints.SingleOrDefault(accessPoint => accessPoint.Id == id));
        }

        public Task<AccessPointPersistenceResult> CreateAsync(
            Guid floorId,
            string name,
            string location,
            bool supportsEntry,
            bool supportsExit,
            CancellationToken cancellationToken)
        {
            if (SaveStatus != AccessPointSaveStatus.Success)
            {
                return Task.FromResult(AccessPointPersistenceResult.MissingFloor());
            }

            var accessPoint = new AccessPoint
            {
                Id = Guid.NewGuid(),
                FloorId = floorId,
                Name = name,
                Location = location,
                SupportsEntry = supportsEntry,
                SupportsExit = supportsExit,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            accessPoints.Add(accessPoint);
            return Task.FromResult(AccessPointPersistenceResult.Succeeded(accessPoint));
        }

        public Task<AccessPointPersistenceResult> UpdateAsync(
            Guid id,
            Guid floorId,
            string name,
            string location,
            bool isActive,
            bool supportsEntry,
            bool supportsExit,
            CancellationToken cancellationToken)
        {
            AccessPoint? accessPoint = accessPoints.SingleOrDefault(item => item.Id == id);
            if (accessPoint is null)
            {
                return Task.FromResult(AccessPointPersistenceResult.MissingAccessPoint());
            }

            if (SaveStatus == AccessPointSaveStatus.FloorNotFound)
            {
                return Task.FromResult(AccessPointPersistenceResult.MissingFloor());
            }

            accessPoint.FloorId = floorId;
            accessPoint.Name = name;
            accessPoint.Location = location;
            accessPoint.IsActive = isActive;
            accessPoint.SupportsEntry = supportsEntry;
            accessPoint.SupportsExit = supportsExit;
            return Task.FromResult(AccessPointPersistenceResult.Succeeded(accessPoint));
        }

        public Task<DeleteAccessPointResult> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(DeleteResult);
        }
    }
}