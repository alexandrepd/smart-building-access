using SmartBuilding.Application.Authentication;
using SmartBuilding.Application.Users;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.UnitTests.Application.Authentication;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task LoginAsync_ValidCredentials_IssuesTokenForPersistedRole()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin User",
            Email = "admin@example.com",
            PasswordHash = "stored-hash",
            Role = UserRole.Admin
        };
        var tokenService = new FakeAccessTokenService();
        var service = new AuthenticationService(
            new FakeUserRepository(user),
            new FakePasswordHashService(valid: true),
            tokenService);

        AuthenticationResult result = await service.LoginAsync(
            new LoginCommand(" ADMIN@example.com ", "correct password"),
            CancellationToken.None);

        Assert.Equal(AuthenticationStatus.Success, result.Status);
        Assert.Equal("token-value", result.AccessToken?.Token);
        Assert.Equal(UserRole.Admin, tokenService.User?.Role);
        Assert.Equal("admin@example.com", tokenService.User?.Email);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsGenericInvalidCredentials()
    {
        var passwordVerifier = new FakePasswordHashService(valid: true);
        var service = new AuthenticationService(
            new FakeUserRepository(),
            passwordVerifier,
            new FakeAccessTokenService());

        AuthenticationResult result = await service.LoginAsync(
            new LoginCommand("unknown@example.com", "correct password"),
            CancellationToken.None);

        Assert.Equal(AuthenticationStatus.InvalidCredentials, result.Status);
        Assert.Null(result.AccessToken);
        Assert.Equal(1, passwordVerifier.VerificationCount);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsInvalidCredentialsWithoutIssuingToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Inactive User",
            Email = "inactive@example.com",
            PasswordHash = "stored-hash",
            IsActive = false
        };
        var tokenService = new FakeAccessTokenService();
        var service = new AuthenticationService(
            new FakeUserRepository(user),
            new FakePasswordHashService(valid: true),
            tokenService);

        AuthenticationResult result = await service.LoginAsync(
            new LoginCommand(user.Email, "correct password"),
            CancellationToken.None);

        Assert.Equal(AuthenticationStatus.InvalidCredentials, result.Status);
        Assert.Null(tokenService.User);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "User",
            Email = "user@example.com",
            PasswordHash = "stored-hash"
        };
        var tokenService = new FakeAccessTokenService();
        var service = new AuthenticationService(
            new FakeUserRepository(user),
            new FakePasswordHashService(valid: false),
            tokenService);

        AuthenticationResult result = await service.LoginAsync(
            new LoginCommand(user.Email, "wrong password"),
            CancellationToken.None);

        Assert.Equal(AuthenticationStatus.InvalidCredentials, result.Status);
        Assert.Null(tokenService.User);
    }

    private sealed class FakeUserRepository(params User[] users) : IUserRepository
    {
        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<User>>(users);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(users.SingleOrDefault(user => user.Id == id));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(users.SingleOrDefault(user => user.Email == email));

        public Task<UserPersistenceResult> CreateAsync(
            string name,
            string email,
            string passwordHash,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<UserPersistenceResult> UpdateAsync(
            Guid id,
            string name,
            string email,
            bool isActive,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<DeleteUserResult> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<UserRoleUpdateResult> UpdateRoleAsync(
            Guid id,
            UserRole role,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class FakePasswordHashService(bool valid) : IPasswordHashService
    {
        public int VerificationCount { get; private set; }

        public string HashPassword(string password) => throw new NotSupportedException();

        public bool VerifyPassword(string? passwordHash, string password)
        {
            VerificationCount++;
            return valid;
        }
    }

    private sealed class FakeAccessTokenService : IAccessTokenService
    {
        public UserDto? User { get; private set; }

        public AccessTokenDto CreateToken(UserDto user)
        {
            User = user;
            return new AccessTokenDto("token-value", DateTimeOffset.UtcNow.AddMinutes(15));
        }
    }
}
