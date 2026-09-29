namespace SmartBuilding.Domain.Entities;

using SmartBuilding.Domain.Enums;

public class AccessEvent
{
    public Guid Id { get; set; }

    public Guid AccessPointId { get; set; }

    public Guid? AccessCardId { get; set; }

    public Guid? UserId { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    public AccessDirection Direction { get; set; }

    public AccessResult Result { get; set; }

    public string Reason { get; set; } = string.Empty;

    public AccessPoint AccessPoint { get; set; } = null!;

    public AccessCard? AccessCard { get; set; }

    public User? User { get; set; }
}