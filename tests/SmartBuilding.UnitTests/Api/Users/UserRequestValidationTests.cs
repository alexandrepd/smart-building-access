using System.ComponentModel.DataAnnotations;
using SmartBuilding.Api.Contracts.Users;

namespace SmartBuilding.UnitTests.Api.Users;

public class UserRequestValidationTests
{
    [Fact]
    public void CreateUserRequest_ValidEmailWithOuterWhitespace_PassesValidation()
    {
        var request = new CreateUserRequest
        {
            Name = "Alex",
            Email = "  Alex@example.com  ",
            Password = "A secure test password"
        };
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(validationResults);
    }

    [Fact]
    public void CreateUserRequest_InvalidEmail_FailsValidation()
    {
        var request = new CreateUserRequest
        {
            Name = "Alex",
            Email = "not-an-email",
            Password = "A secure test password"
        };
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(request.Email)));
    }
}