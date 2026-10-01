using System.ComponentModel.DataAnnotations;
using SmartBuilding.Api.Validation;

namespace SmartBuilding.Api.Contracts.Users;

/// <summary>Payload used to create a user.</summary>
public sealed record CreateUserRequest
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

    /// <summary>Initial password. It is hashed before persistence and is never returned.</summary>
    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public required string Password { get; init; }
}