namespace SmartBuilding.Domain.Entities;

public class OccupancySession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid FloorId { get; set; }

    public DateTime EnteredAt { get; set; }

    public DateTime? ExitedAt { get; set; }

    public User User { get; set; } = null!;

    public Floor Floor { get; set; } = null!;

    public bool TryClose(DateTime exitedAt)
    {
        if (ExitedAt is not null || exitedAt < EnteredAt)
        {
            return false;
        }

        ExitedAt = exitedAt;
        return true;
    }
}