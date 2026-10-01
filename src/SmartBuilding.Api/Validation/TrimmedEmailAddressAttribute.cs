using System.ComponentModel.DataAnnotations;

namespace SmartBuilding.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
internal sealed class TrimmedEmailAddressAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    public override bool IsValid(object? value)
    {
        return value is string email
            && EmailAddressValidator.IsValid(email.Trim());
    }
}