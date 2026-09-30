using System.ComponentModel.DataAnnotations;

namespace SmartBuilding.Api.Contracts.Buildings;

/// <summary>Payload used to replace the editable state of a building.</summary>
public sealed record UpdateBuildingRequest
{
    /// <summary>Building display name.</summary>
    [Required]
    [MaxLength(200)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must contain a non-whitespace character.")]
    public required string Name { get; init; }

    /// <summary>Building postal address.</summary>
    [Required]
    [MaxLength(500)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Address must contain a non-whitespace character.")]
    public required string Address { get; init; }

    /// <summary>Whether the building is available for operations.</summary>
    public required bool IsActive { get; init; }
}