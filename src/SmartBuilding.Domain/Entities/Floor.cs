namespace SmartBuilding.Domain.Entities;

public class Floor
{
    public Guid Id { get; set; }

    public Guid BuildingId { get; set; }

    public int Number { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Building Building { get; set; } = null!;

    public ICollection<AccessPoint> AccessPoints { get; set; }
        = new List<AccessPoint>();

    public ICollection<OccupancySession> OccupancySessions { get; set; }
        = new List<OccupancySession>();
}