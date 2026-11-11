using SmartBuilding.Application.Occupancy;

namespace SmartBuilding.UnitTests.Application.Occupancy;

public class OccupancyServiceTests
{
    private static readonly DateTimeOffset CurrentTime =
        new(2030, 1, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetCurrentOccupancyAsync_ActiveSessions_ReturnsFloorCountsAndElapsedDuration()
    {
        var emptyFloorId = Guid.NewGuid();
        var occupiedFloorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = new FakeOccupancyRepository(
        [
            new OccupancyFloorData(
                Guid.NewGuid(),
                "Headquarters",
                emptyFloorId,
                "Floor 1",
                []),
            new OccupancyFloorData(
                Guid.NewGuid(),
                "Headquarters",
                occupiedFloorId,
                "Floor 2",
                [
                    new OccupancySessionData(
                        Guid.NewGuid(),
                        userId,
                        "Alex",
                        CurrentTime.AddMinutes(-42).UtcDateTime)
                ])
        ]);
        var service = new OccupancyService(repository, new FrozenTimeProvider(CurrentTime));

        IReadOnlyList<CurrentOccupancyFloorDto> result =
            await service.GetCurrentOccupancyAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[0].OccupantCount);
        Assert.Equal(1, result[1].OccupantCount);
        Assert.Equal(2520, Assert.Single(result[1].Occupants).DurationSeconds);
        Assert.Equal(userId, result[1].Occupants[0].UserId);
    }

    [Fact]
    public async Task GetCurrentOccupancyAsync_FutureEntryTime_ClampsDurationToZero()
    {
        var repository = new FakeOccupancyRepository(
        [
            new OccupancyFloorData(
                Guid.NewGuid(),
                "Headquarters",
                Guid.NewGuid(),
                "Floor 1",
                [
                    new OccupancySessionData(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "Alex",
                        CurrentTime.AddMinutes(1).UtcDateTime)
                ])
        ]);
        var service = new OccupancyService(repository, new FrozenTimeProvider(CurrentTime));

        CurrentOccupancyFloorDto floor = Assert.Single(
            await service.GetCurrentOccupancyAsync(CancellationToken.None));

        Assert.Equal(0, Assert.Single(floor.Occupants).DurationSeconds);
    }

    private sealed class FrozenTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeOccupancyRepository(
        IReadOnlyList<OccupancyFloorData> floors) : IOccupancyRepository
    {
        public Task<IReadOnlyList<OccupancyFloorData>> GetCurrentOccupancyAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(floors);
        }
    }
}
