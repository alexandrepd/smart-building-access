namespace SmartBuilding.Domain.Entities;

public class AccessPoint
{
    public Guid Id { get; set; }

    public Guid FloorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool SupportsEntry { get; set; }

    public bool SupportsExit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Floor Floor { get; set; } = null!;

    public ICollection<AccessPermission> AccessPermissions { get; set; }
        = new List<AccessPermission>();

    public ICollection<AccessEvent> AccessEvents { get; set; }
        = new List<AccessEvent>();

    public ICollection<SecurityAlert> SecurityAlerts { get; set; }
        = new List<SecurityAlert>();
}