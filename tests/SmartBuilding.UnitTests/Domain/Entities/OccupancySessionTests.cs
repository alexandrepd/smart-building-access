using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Domain.Entities;

public class OccupancySessionTests
{
    [Fact]
    public void TryClose_ExitAtOrAfterEntry_ClosesSession()
    {
        DateTime enteredAt = new(2030, 1, 2, 10, 0, 0, DateTimeKind.Utc);
        var session = new OccupancySession { EnteredAt = enteredAt };
        DateTime exitedAt = enteredAt.AddHours(2);

        bool closed = session.TryClose(exitedAt);

        Assert.True(closed);
        Assert.Equal(exitedAt, session.ExitedAt);
    }

    [Fact]
    public void TryClose_ExitBeforeEntry_LeavesSessionOpen()
    {
        DateTime enteredAt = new(2030, 1, 2, 10, 0, 0, DateTimeKind.Utc);
        var session = new OccupancySession { EnteredAt = enteredAt };

        bool closed = session.TryClose(enteredAt.AddSeconds(-1));

        Assert.False(closed);
        Assert.Null(session.ExitedAt);
    }

    [Fact]
    public void TryClose_AlreadyClosedSession_DoesNotChangeExitTime()
    {
        DateTime enteredAt = new(2030, 1, 2, 10, 0, 0, DateTimeKind.Utc);
        DateTime firstExit = enteredAt.AddHours(1);
        var session = new OccupancySession { EnteredAt = enteredAt, ExitedAt = firstExit };

        bool closedAgain = session.TryClose(enteredAt.AddHours(2));

        Assert.False(closedAgain);
        Assert.Equal(firstExit, session.ExitedAt);
    }
}
