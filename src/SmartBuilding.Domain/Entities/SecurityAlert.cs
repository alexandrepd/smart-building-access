namespace SmartBuilding.Domain.Entities;

using SmartBuilding.Domain.Enums;

public class SecurityAlert
{
    public Guid Id { get; set; }

    public Guid AccessPointId { get; set; }

    public AlertType Type { get; set; }

    public AlertStatus Status { get; set; } = AlertStatus.Open;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ResolvedAt { get; set; }

    public AccessPoint AccessPoint { get; set; } = null!;
}