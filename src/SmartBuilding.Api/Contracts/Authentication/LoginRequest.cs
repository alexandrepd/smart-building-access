using System.ComponentModel.DataAnnotations;
using SmartBuilding.Api.Validation;

namespace SmartBuilding.Api.Contracts.Authentication;

/// <summary>Credentials used to obtain a short-lived access token.</summary>
public sealed record LoginRequest
{
    /// <summary>User's email address.</summary>
    [Required]
    [TrimmedEmailAddress]
    [MaxLength(320)]
    public required string Email { get; init; }

    /// <summary>User's password.</summary>
    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public required string Password { get; init; }
}
