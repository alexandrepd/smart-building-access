using System.ComponentModel.DataAnnotations;

namespace SmartBuilding.Api.Contracts.AccessPoints;

/// <summary>Payload used to create an access point on a floor.</summary>
public sealed record CreateAccessPointRequest
{
    /// <summary>Floor containing the access point.</summary>
    public required Guid FloorId { get; init; }

    /// <summary>Access point display name.</summary>
    [Required]
    [MaxLength(150)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must contain a non-whitespace character.")]
    public required string Name { get; init; }

    /// <summary>Physical location description.</summary>
    [Required]
    [MaxLength(250)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Location must contain a non-whitespace character.")]
    public required string Location { get; init; }

    /// <summary>Whether this point supports entry.</summary>
    public required bool SupportsEntry { get; init; }

    /// <summary>Whether this point supports exit.</summary>
    public required bool SupportsExit { get; init; }
}