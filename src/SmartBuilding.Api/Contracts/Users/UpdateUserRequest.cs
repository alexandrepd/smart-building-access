using System.ComponentModel.DataAnnotations;
using SmartBuilding.Api.Validation;

namespace SmartBuilding.Api.Contracts.Users;

/// <summary>Payload used to replace a user's profile and active state.</summary>
public sealed record UpdateUserRequest
{
    /// <summary>User's display name.</summary>
    [Required]
    [MaxLength(200)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name must contain a non-whitespace character.")]
    public required string Name { get; init; }

    /// <summary>User's email address, normalized to lowercase by the application.</summary>
    [Required]
    [TrimmedEmailAddress]
    [MaxLength(320)]
    public required string Email { get; init; }

    /// <summary>Whether the user is available for operations.</summary>
    public required bool IsActive { get; init; }
}