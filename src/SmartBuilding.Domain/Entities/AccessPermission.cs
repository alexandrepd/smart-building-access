namespace SmartBuilding.Domain.Entities;

public class AccessPermission
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid AccessPointId { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public AccessPoint AccessPoint { get; set; } = null!;
}