namespace SmartBuilding.Domain.Entities;

public class Building
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
}