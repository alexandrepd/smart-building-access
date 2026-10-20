using System.ComponentModel.DataAnnotations;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Api.Contracts.AccessControl;

/// <summary>Payload sent by an access point to request an access decision.</summary>
public sealed record AccessRequestRequest
{
    /// <summary>Card number presented to the access point.</summary>
    [Required]
    [MaxLength(100)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Card number must contain a non-whitespace character.")]
    public required string CardNumber { get; init; }

    /// <summary>ID of the access point receiving the request.</summary>
    [Required]
    public required Guid AccessPointId { get; init; }

    /// <summary>Whether the request is for entry or exit.</summary>
    [Required]
    [EnumDataType(typeof(AccessDirection))]
    public required AccessDirection Direction { get; init; }
}
