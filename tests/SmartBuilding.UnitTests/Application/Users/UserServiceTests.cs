using SmartBuilding.Application.Common;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Application.Users;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidCommand_NormalizesEmailAndPersistsOnlyHash()
    {
        var repository = new FakeUserRepository();
        var hasher = new FakePasswordHashService();
        var service = new UserService(repository, hasher);

        UserSaveResult result = await service.CreateAsync(
            new CreateUserCommand("  Alex  ", "  Alex@Example.com  ", "A secure test password"),
            CancellationToken.None);

        Assert.Equal(UserSaveStatus.Success, result.Status);
        Assert.Equal("Alex", result.User?.Name);
        Assert.Equal("alex@example.com", result.User?.Email);
        Assert.DoesNotContain("Password", typeof(UserDto).GetProperties().Select(property => property.Name));
        Assert.Equal("hashed:A secure test password", repository.CreatedPasswordHash);
        Assert.Equal("A secure test password", hasher.PasswordReceived);
    }

    [Fact]
    public async Task CreateAsync_InvalidEmail_ThrowsValidationException()
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        Task Act() => service.CreateAsync(
            new CreateUserCommand("Alex", "not-an-email", "A secure test password"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task GetAllAsync_ExistingUsers_ReturnsDtosWithoutPasswordMaterial()
    {
        var existing = new User
        {
            Id = Guid.NewGuid(),
            Name = "Alex",
            Email = "alex@example.com",
            PasswordHash = "never-return-this"
        };
        var service = new UserService(
            new FakeUserRepository(existing),
            new FakePasswordHashService());

        UserDto result = Assert.Single(await service.GetAllAsync(CancellationToken.None));

        Assert.Equal(existing.Name, result.Name);
        Assert.Equal(existing.Email, result.Email);
        Assert.DoesNotContain("Password", typeof(UserDto).GetProperties()
            .Select(property => property.Name));
        Assert.DoesNotContain("Hash", typeof(UserDto).GetProperties()
            .Select(property => property.Name));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("             ")]
    public async Task CreateAsync_InvalidPassword_ThrowsValidationException(string password)
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        Task Act() => service.CreateAsync(
            new CreateUserCommand("Alex", "alex@example.com", password),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ReturnsConflictResult()
    {
        var repository = new FakeUserRepository
        {
            SaveStatus = UserSaveStatus.EmailAlreadyExists
        };
        var service = new UserService(repository, new FakePasswordHashService());

        UserSaveResult result = await service.CreateAsync(
            new CreateUserCommand("Alex", "alex@example.com", "A secure test password"),
            CancellationToken.None);

        Assert.Equal(UserSaveStatus.EmailAlreadyExists, result.Status);
        Assert.Null(result.User);
    }

    [Fact]
    public async Task UpdateAsync_ExistingUser_ChangesProfileWithoutChangingPasswordHash()
    {
        var existing = new User
        {
            Id = Guid.NewGuid(),
            Name = "Alex",
            Email = "alex@example.com",
            PasswordHash = "existing-hash"
        };
        var repository = new FakeUserRepository(existing);
        var service = new UserService(repository, new FakePasswordHashService());

        UserSaveResult result = await service.UpdateAsync(
            existing.Id,
            new UpdateUserCommand("Alex Updated", "ALEX.NEW@example.com", false),
            CancellationToken.None);

        Assert.Equal(UserSaveStatus.Success, result.Status);
        Assert.Equal("alex.new@example.com", result.User?.Email);
        Assert.False(result.User?.IsActive);
        Assert.Equal("existing-hash", repository.Users.Single().PasswordHash);
    }

    [Fact]
    public async Task UpdateAsync_UnknownUser_ReturnsNotFound()
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        UserSaveResult result = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateUserCommand("Alex", "alex@example.com", true),
            CancellationToken.None);

        Assert.Equal(UserSaveStatus.UserNotFound, result.Status);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ReturnsConflictResult()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Alex",
            Email = "alex@example.com",
            PasswordHash = "existing-hash"
        };
        var repository = new FakeUserRepository(user)
        {
            SaveStatus = UserSaveStatus.EmailAlreadyExists
        };
        var service = new UserService(repository, new FakePasswordHashService());

        UserSaveResult result = await service.UpdateAsync(
            user.Id,
            new UpdateUserCommand("Alex", "taken@example.com", true),
            CancellationToken.None);

        Assert.Equal(UserSaveStatus.EmailAlreadyExists, result.Status);
        Assert.Equal("alex@example.com", user.Email);
    }

    [Fact]
    public async Task DeleteAsync_UserWithDependents_ReturnsConflictResult()
    {
        var repository = new FakeUserRepository
        {
            DeleteResult = DeleteUserResult.HasDependents
        };
        var service = new UserService(repository, new FakePasswordHashService());

        DeleteUserResult result = await service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(DeleteUserResult.HasDependents, result);
    }

    [Fact]
    public async Task DeleteAsync_UserWithoutDependents_ReturnsDeleted()
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        DeleteUserResult result = await service.DeleteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(DeleteUserResult.Deleted, result);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceName_ThrowsValidationException()
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        Task Act() => service.CreateAsync(
            new CreateUserCommand("   ", "alex@example.com", "A secure test password"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_NameExceedsMaximumLength_ThrowsValidationException()
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        Task Act() => service.CreateAsync(
            new CreateUserCommand(new string('A', 201), "alex@example.com", "A secure test password"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_EmailExceedsMaximumLength_ThrowsValidationException()
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        Task Act() => service.CreateAsync(
            new CreateUserCommand("Alex", $"{new string('a', 309)}@example.com", "A secure test password"),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_PasswordExceedsMaximumLength_ThrowsValidationException()
    {
        var service = new UserService(new FakeUserRepository(), new FakePasswordHashService());

        Task Act() => service.CreateAsync(
            new CreateUserCommand("Alex", "alex@example.com", new string('P', 129)),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    private sealed class FakePasswordHashService : IPasswordHashService
    {
        public string? PasswordReceived { get; private set; }

        public string HashPassword(string password)
        {
            PasswordReceived = password;
            return $"hashed:{password}";
        }
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        private readonly List<User> users = [.. users];

        public UserSaveStatus SaveStatus { get; init; } = UserSaveStatus.Success;

        public DeleteUserResult DeleteResult { get; init; } = DeleteUserResult.Deleted;

        public string? CreatedPasswordHash { get; private set; }

        public IReadOnlyList<User> Users => users;

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<User>>(users);
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(users.SingleOrDefault(user => user.Id == id));
        }

        public Task<UserPersistenceResult> CreateAsync(
            string name,
            string email,
            string passwordHash,
            CancellationToken cancellationToken)
        {
            CreatedPasswordHash = passwordHash;
            if (SaveStatus == UserSaveStatus.EmailAlreadyExists)
            {
                return Task.FromResult(UserPersistenceResult.DuplicateEmail());
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = passwordHash
            };
            users.Add(user);
            return Task.FromResult(UserPersistenceResult.Succeeded(user));
        }

        public Task<UserPersistenceResult> UpdateAsync(
            Guid id,
            string name,
            string email,
            bool isActive,
            CancellationToken cancellationToken)
        {
            User? user = users.SingleOrDefault(user => user.Id == id);
            if (user is null)
            {
                return Task.FromResult(UserPersistenceResult.MissingUser());
            }

            if (SaveStatus == UserSaveStatus.EmailAlreadyExists)
            {
                return Task.FromResult(UserPersistenceResult.DuplicateEmail());
            }

            user.Name = name;
            user.Email = email;
            user.IsActive = isActive;
            return Task.FromResult(UserPersistenceResult.Succeeded(user));
        }

        public Task<DeleteUserResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(DeleteResult);
        }
    }
}