namespace SmartBuilding.Domain.Entities;

public class AccessCard
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CardNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public ICollection<AccessEvent> AccessEvents { get; set; }
        = new List<AccessEvent>();
}