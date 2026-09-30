using SmartBuilding.Application.Buildings;
using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Application.Buildings;

public class BuildingServiceTests
{
    [Fact]
    public async Task GetAllAsync_ExistingBuildings_ReturnsMappedBuildings()
    {
        var existing = new Building
        {
            Id = Guid.NewGuid(),
            Name = "HQ Lisbon",
            Address = "Lisbon",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        var repository = new FakeBuildingRepository(existing);
        var service = new BuildingService(repository);

        IReadOnlyList<BuildingDto> result =
            await service.GetAllAsync(CancellationToken.None);

        BuildingDto building = Assert.Single(result);
        Assert.Equal(existing.Id, building.Id);
        Assert.Equal(existing.Name, building.Name);
        Assert.Equal(existing.Address, building.Address);
        Assert.Equal(existing.IsActive, building.IsActive);
        Assert.Equal(existing.CreatedAt, building.CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownBuilding_ReturnsNull()
    {
        var service = new BuildingService(new FakeBuildingRepository());

        BuildingDto? result = await service.GetByIdAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceAroundValues_NormalizesValues()
    {
        var repository = new FakeBuildingRepository();
        var service = new BuildingService(repository);

        BuildingDto result = await service.CreateAsync(
            new CreateBuildingCommand("  North Office  ", "  Porto  "),
            CancellationToken.None);

        Assert.Equal("North Office", result.Name);
        Assert.Equal("Porto", result.Address);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_BlankName_ThrowsArgumentException()
    {
        var service = new BuildingService(new FakeBuildingRepository());

        Task Act() => service.CreateAsync(
            new CreateBuildingCommand("   ", "Lisbon"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task UpdateAsync_UnknownBuilding_ReturnsNull()
    {
        var service = new BuildingService(new FakeBuildingRepository());

        BuildingDto? result = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateBuildingCommand("Updated", "Porto", false),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingBuilding_NormalizesAndReplacesEditableState()
    {
        var existing = new Building
        {
            Id = Guid.NewGuid(),
            Name = "Old Name",
            Address = "Lisbon"
        };
        var service = new BuildingService(new FakeBuildingRepository(existing));

        BuildingDto? result = await service.UpdateAsync(
            existing.Id,
            new UpdateBuildingCommand("  Updated Name  ", "  Porto  ", false),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Porto", result.Address);
        Assert.False(result.IsActive);
    }

    [Theory]
    [InlineData(201, 1)]
    [InlineData(1, 501)]
    public async Task CreateAsync_ValueExceedsMaximumLength_ThrowsValidationException(
        int nameLength,
        int addressLength)
    {
        var service = new BuildingService(new FakeBuildingRepository());

        Task Act() => service.CreateAsync(
            new CreateBuildingCommand(
                new string('N', nameLength),
                new string('A', addressLength)),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task DeleteAsync_BuildingWithFloors_ReturnsConflictResult()
    {
        var repository = new FakeBuildingRepository
        {
            DeleteResult = DeleteBuildingResult.HasFloors
        };
        var service = new BuildingService(repository);

        DeleteBuildingResult result = await service.DeleteAsync(
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.Equal(DeleteBuildingResult.HasFloors, result);
    }

    private sealed class FakeBuildingRepository(params Building[] buildings)
        : IBuildingRepository
    {
        private readonly List<Building> buildings = [.. buildings];

        public DeleteBuildingResult DeleteResult { get; init; } = DeleteBuildingResult.Deleted;

        public Task<IReadOnlyList<Building>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Building>>(buildings);
        }

        public Task<Building?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(buildings.SingleOrDefault(building => building.Id == id));
        }

        public Task<Building> CreateAsync(
            string name,
            string address,
            CancellationToken cancellationToken)
        {
            var building = new Building
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            buildings.Add(building);

            return Task.FromResult(building);
        }

        public Task<Building?> UpdateAsync(
            Guid id,
            string name,
            string address,
            bool isActive,
            CancellationToken cancellationToken)
        {
            Building? building = buildings.SingleOrDefault(building => building.Id == id);
            if (building is null)
            {
                return Task.FromResult<Building?>(null);
            }

            building.Name = name;
            building.Address = address;
            building.IsActive = isActive;
            return Task.FromResult<Building?>(building);
        }

        public Task<DeleteBuildingResult> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(DeleteResult);
        }
    }
}