using System.ComponentModel.DataAnnotations;

namespace SmartBuilding.Api.Contracts.Floors;

/// <summary>Payload used to replace the editable state of a floor.</summary>
public sealed record UpdateFloorRequest
{
    /// <summary>Building that contains the floor.</summary>
    public required Guid BuildingId { get; init; }

    /// <summary>Floor number, including negative numbers for basements.</summary>
    public required int Number { get; init; }

    /// <summary>Floor display name.</summary>
    [Required]
    [MaxLength(100)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must contain a non-whitespace character.")]
    public required string Name { get; init; }

    /// <summary>Whether the floor is available for operations.</summary>
    public required bool IsActive { get; init; }
}