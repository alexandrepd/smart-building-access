using SmartBuilding.Application.Common;
using SmartBuilding.Application.Floors;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Application.Floors;

public class FloorServiceTests
{
    [Fact]
    public async Task GetAllAsync_ExistingFloors_ReturnsMappedFloors()
    {
        var existing = new Floor
        {
            Id = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            Number = -1,
            Name = "Basement",
            IsActive = false
        };
        var service = new FloorService(new FakeFloorRepository(existing));

        FloorDto result = Assert.Single(
            await service.GetAllAsync(CancellationToken.None));

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(existing.BuildingId, result.BuildingId);
        Assert.Equal(existing.Number, result.Number);
        Assert.Equal(existing.Name, result.Name);
        Assert.Equal(existing.IsActive, result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_ValidCommand_NormalizesName()
    {
        var service = new FloorService(new FakeFloorRepository());

        FloorSaveResult result = await service.CreateAsync(
            new CreateFloorCommand(Guid.NewGuid(), 2, "  Second Floor  "),
            CancellationToken.None);

        Assert.Equal(FloorSaveStatus.Success, result.Status);
        Assert.Equal("Second Floor", result.Floor?.Name);
        Assert.Equal(2, result.Floor?.Number);
    }

    [Fact]
    public async Task CreateAsync_EmptyBuildingId_ThrowsValidationException()
    {
        var service = new FloorService(new FakeFloorRepository());

        Task Act() => service.CreateAsync(
            new CreateFloorCommand(Guid.Empty, 0, "Ground Floor"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_UnknownBuilding_ReturnsBuildingNotFound()
    {
        var repository = new FakeFloorRepository
        {
            SaveStatus = FloorSaveStatus.BuildingNotFound
        };
        var service = new FloorService(repository);

        FloorSaveResult result = await service.CreateAsync(
            new CreateFloorCommand(Guid.NewGuid(), 1, "First Floor"),
            CancellationToken.None);

        Assert.Equal(FloorSaveStatus.BuildingNotFound, result.Status);
        Assert.Null(result.Floor);
    }

    [Fact]
    public async Task UpdateAsync_ExistingFloor_ReplacesEditableState()
    {
        var existing = new Floor
        {
            Id = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            Number = 1,
            Name = "Old"
        };
        var service = new FloorService(new FakeFloorRepository(existing));
        Guid newBuildingId = Guid.NewGuid();

        FloorSaveResult result = await service.UpdateAsync(
            existing.Id,
            new UpdateFloorCommand(newBuildingId, 3, "  Third Floor  ", false),
            CancellationToken.None);

        Assert.Equal(FloorSaveStatus.Success, result.Status);
        Assert.Equal(newBuildingId, result.Floor?.BuildingId);
        Assert.Equal(3, result.Floor?.Number);
        Assert.Equal("Third Floor", result.Floor?.Name);
        Assert.False(result.Floor?.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_FloorWithDependents_ReturnsConflictResult()
    {
        var repository = new FakeFloorRepository
        {
            DeleteResult = DeleteFloorResult.HasDependents
        };
        var service = new FloorService(repository);

        DeleteFloorResult result = await service.DeleteAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(DeleteFloorResult.HasDependents, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_BlankName_ThrowsValidationException(string name)
    {
        var service = new FloorService(new FakeFloorRepository());

        Task Act() => service.CreateAsync(
            new CreateFloorCommand(Guid.NewGuid(), 0, name),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    private sealed class FakeFloorRepository(params Floor[] floors) : IFloorRepository
    {
        private readonly List<Floor> floors = [.. floors];

        public FloorSaveStatus SaveStatus { get; init; } = FloorSaveStatus.Success;

        public DeleteFloorResult DeleteResult { get; init; } = DeleteFloorResult.Deleted;

        public Task<IReadOnlyList<Floor>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Floor>>(floors);
        }

        public Task<Floor?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(floors.SingleOrDefault(floor => floor.Id == id));
        }

        public Task<FloorPersistenceResult> CreateAsync(
            Guid buildingId,
            int number,
            string name,
            CancellationToken cancellationToken)
        {
            if (SaveStatus != FloorSaveStatus.Success)
            {
                return Task.FromResult(SaveStatus == FloorSaveStatus.BuildingNotFound
                    ? FloorPersistenceResult.MissingBuilding()
                    : FloorPersistenceResult.MissingFloor());
            }

            var floor = new Floor
            {
                Id = Guid.NewGuid(),
                BuildingId = buildingId,
                Number = number,
                Name = name
            };
            floors.Add(floor);
            return Task.FromResult(FloorPersistenceResult.Succeeded(floor));
        }

        public Task<FloorPersistenceResult> UpdateAsync(
            Guid id,
            Guid buildingId,
            int number,
            string name,
            bool isActive,
            CancellationToken cancellationToken)
        {
            Floor? floor = floors.SingleOrDefault(floor => floor.Id == id);
            if (floor is null)
            {
                return Task.FromResult(
                    FloorPersistenceResult.MissingFloor());
            }

            floor.BuildingId = buildingId;
            floor.Number = number;
            floor.Name = name;
            floor.IsActive = isActive;
            return Task.FromResult(FloorPersistenceResult.Succeeded(floor));
        }

        public Task<DeleteFloorResult> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(DeleteResult);
        }
    }
}