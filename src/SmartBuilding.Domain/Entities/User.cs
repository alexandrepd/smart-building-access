namespace SmartBuilding.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AccessCard> AccessCards { get; set; }
        = new List<AccessCard>();

    public ICollection<AccessPermission> AccessPermissions { get; set; }
        = new List<AccessPermission>();

    public ICollection<AccessEvent> AccessEvents { get; set; }
        = new List<AccessEvent>();

    public ICollection<OccupancySession> OccupancySessions { get; set; }
        = new List<OccupancySession>();
}