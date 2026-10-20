using System.ComponentModel.DataAnnotations;

namespace SmartBuilding.Api.Contracts.AccessCards;

/// <summary>Payload used to replace an access card's assignment and details.</summary>
public sealed record UpdateAccessCardRequest
{
    /// <summary>ID of the user who will hold the card.</summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>Unique identifier printed or encoded on the card.</summary>
    [Required]
    [MaxLength(100)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Card number must contain a non-whitespace character.")]
    public required string CardNumber { get; init; }

    /// <summary>Optional expiration instant, including its UTC offset.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}
