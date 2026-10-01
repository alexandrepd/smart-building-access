using System.ComponentModel.DataAnnotations;
using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.Users;

public sealed class UserService(
    IUserRepository repository,
    IPasswordHashService passwordHashService) : IUserService
{
    private static readonly EmailAddressAttribute EmailAddressValidator = new();

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<User> users = await repository.GetAllAsync(cancellationToken);
        return users.Select(ToDto).ToArray();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        User? user = await repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserSaveResult> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        string name = NormalizeRequired(command.Name, nameof(command.Name), 200);
        string email = NormalizeEmail(command.Email);
        ValidatePassword(command.Password);

        string passwordHash = passwordHashService.HashPassword(command.Password);
        UserPersistenceResult result = await repository.CreateAsync(
            name,
            email,
            passwordHash,
            cancellationToken);

        return ToSaveResult(result);
    }

    public async Task<UserSaveResult> UpdateAsync(
        Guid id,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        string name = NormalizeRequired(command.Name, nameof(command.Name), 200);
        string email = NormalizeEmail(command.Email);

        UserPersistenceResult result = await repository.UpdateAsync(
            id,
            name,
            email,
            command.IsActive,
            cancellationToken);

        return ToSaveResult(result);
    }

    public Task<DeleteUserResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return repository.DeleteAsync(id, cancellationToken);
    }

    private static UserSaveResult ToSaveResult(UserPersistenceResult result)
    {
        return result.Status switch
        {
            UserSaveStatus.Success => UserSaveResult.Succeeded(ToDto(result.User!)),
            UserSaveStatus.UserNotFound => UserSaveResult.MissingUser(),
            UserSaveStatus.EmailAlreadyExists => UserSaveResult.DuplicateEmail(),
            _ => throw new InvalidOperationException($"Unsupported user save status: {result.Status}.")
        };
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto(user.Id, user.Name, user.Email, user.IsActive, user.CreatedAt);
    }

    private static string NormalizeEmail(string email)
    {
        string normalized = NormalizeRequired(email, nameof(CreateUserCommand.Email), 320)
            .ToLowerInvariant();

        if (!EmailAddressValidator.IsValid(normalized))
        {
            throw new ApplicationValidationException(
                nameof(CreateUserCommand.Email),
                "Email address is invalid.");
        }

        return normalized;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ApplicationValidationException(
                nameof(CreateUserCommand.Password),
                "Password is required.");
        }

        if (password.Length < 12 || password.Length > 128)
        {
            throw new ApplicationValidationException(
                nameof(CreateUserCommand.Password),
                "Password must contain between 12 and 128 characters.");
        }
    }

    private static string NormalizeRequired(string value, string propertyName, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApplicationValidationException(propertyName, "Value is required.");
        }

        string normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ApplicationValidationException(
                propertyName,
                $"Value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }
}