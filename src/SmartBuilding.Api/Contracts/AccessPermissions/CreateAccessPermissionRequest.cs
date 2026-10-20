using System.ComponentModel.DataAnnotations;

namespace SmartBuilding.Api.Contracts.AccessPermissions;

/// <summary>Payload used to grant a user access to an access point for a time interval.</summary>
public sealed record CreateAccessPermissionRequest
{
    /// <summary>ID of the user receiving the authorization.</summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>ID of the access point covered by the authorization.</summary>
    [Required]
    public required Guid AccessPointId { get; init; }

    /// <summary>Instant from which the authorization is valid.</summary>
    [Required]
    public required DateTimeOffset ValidFrom { get; init; }

    /// <summary>Optional instant when the authorization expires; null means no scheduled end.</summary>
    public DateTimeOffset? ValidUntil { get; init; }
}
