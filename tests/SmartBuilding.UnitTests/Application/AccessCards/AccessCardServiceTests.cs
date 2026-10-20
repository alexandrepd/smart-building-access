using SmartBuilding.Application.AccessCards;
using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.UnitTests.Application.AccessCards;

public class AccessCardServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidCommand_TrimsCardNumberAndStoresExpirationInUtc()
    {
        Guid userId = Guid.NewGuid();
        DateTimeOffset expiration = new(2030, 1, 2, 3, 0, 0, TimeSpan.FromHours(3));
        var repository = new FakeAccessCardRepository();
        var service = new AccessCardService(repository);

        AccessCardSaveResult result = await service.CreateAsync(
            new CreateAccessCardCommand(userId, "  CARD-100  ", expiration),
            CancellationToken.None);

        Assert.Equal(AccessCardSaveStatus.Success, result.Status);
        Assert.Equal("CARD-100", repository.CardNumber);
        Assert.Equal(userId, repository.UserId);
        Assert.Equal(expiration.UtcDateTime, repository.ExpiresAt);
        Assert.Equal("CARD-100", result.AccessCard?.CardNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_BlankCardNumber_ThrowsValidationException(string cardNumber)
    {
        var service = new AccessCardService(new FakeAccessCardRepository());

        Task Act() => service.CreateAsync(
            new CreateAccessCardCommand(Guid.NewGuid(), cardNumber, null),
            CancellationToken.None);

        await Assert.ThrowsAsync<ApplicationValidationException>(Act);
    }

    [Fact]
    public async Task CreateAsync_UnknownUser_ReturnsUserNotFound()
    {
        var repository = new FakeAccessCardRepository
        {
            ResultStatus = AccessCardSaveStatus.UserNotFound
        };
        var service = new AccessCardService(repository);

        AccessCardSaveResult result = await service.CreateAsync(
            new CreateAccessCardCommand(Guid.NewGuid(), "CARD-100", null),
            CancellationToken.None);

        Assert.Equal(AccessCardSaveStatus.UserNotFound, result.Status);
        Assert.Null(result.AccessCard);
    }

    [Fact]
    public async Task CreateAsync_DuplicateCardNumber_ReturnsConflictStatus()
    {
        var repository = new FakeAccessCardRepository
        {
            ResultStatus = AccessCardSaveStatus.CardNumberAlreadyExists
        };
        var service = new AccessCardService(repository);

        AccessCardSaveResult result = await service.CreateAsync(
            new CreateAccessCardCommand(Guid.NewGuid(), "CARD-100", null),
            CancellationToken.None);

        Assert.Equal(AccessCardSaveStatus.CardNumberAlreadyExists, result.Status);
        Assert.Null(result.AccessCard);
    }

    [Fact]
    public async Task SetActiveAsync_RepositoryReportsMissingCard_ReturnsMissingCard()
    {
        var repository = new FakeAccessCardRepository
        {
            ResultStatus = AccessCardSaveStatus.AccessCardNotFound
        };
        var service = new AccessCardService(repository);

        AccessCardSaveResult result = await service.SetActiveAsync(
            Guid.NewGuid(),
            false,
            CancellationToken.None);

        Assert.Equal(AccessCardSaveStatus.AccessCardNotFound, result.Status);
        Assert.Null(result.AccessCard);
    }

    private sealed class FakeAccessCardRepository : IAccessCardRepository
    {
        public AccessCardSaveStatus ResultStatus { get; init; } = AccessCardSaveStatus.Success;

        public Guid UserId { get; private set; }

        public string CardNumber { get; private set; } = string.Empty;

        public DateTime? ExpiresAt { get; private set; }

        public Task<IReadOnlyList<AccessCard>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<AccessCard>>(Array.Empty<AccessCard>());
        }

        public Task<AccessCard?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult<AccessCard?>(null);
        }

        public Task<AccessCardPersistenceResult> CreateAsync(
            Guid userId,
            string cardNumber,
            DateTime? expiresAt,
            CancellationToken cancellationToken)
        {
            UserId = userId;
            CardNumber = cardNumber;
            ExpiresAt = expiresAt;
            return Task.FromResult(CreateResult(userId, cardNumber, expiresAt));
        }

        public Task<AccessCardPersistenceResult> UpdateAsync(
            Guid id,
            Guid userId,
            string cardNumber,
            DateTime? expiresAt,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CreateResult(userId, cardNumber, expiresAt));
        }

        public Task<AccessCardPersistenceResult> SetActiveAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(ResultStatus == AccessCardSaveStatus.Success
                ? AccessCardPersistenceResult.Succeeded(new AccessCard
                {
                    Id = id,
                    UserId = Guid.NewGuid(),
                    CardNumber = "CARD-100",
                    IsActive = isActive
                })
                : CreateFailure());
        }

        private AccessCardPersistenceResult CreateResult(
            Guid userId,
            string cardNumber,
            DateTime? expiresAt)
        {
            return ResultStatus == AccessCardSaveStatus.Success
                ? AccessCardPersistenceResult.Succeeded(new AccessCard
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CardNumber = cardNumber,
                    ExpiresAt = expiresAt
                })
                : CreateFailure();
        }

        private AccessCardPersistenceResult CreateFailure()
        {
            return ResultStatus switch
            {
                AccessCardSaveStatus.AccessCardNotFound =>
                    AccessCardPersistenceResult.MissingAccessCard(),
                AccessCardSaveStatus.UserNotFound =>
                    AccessCardPersistenceResult.MissingUser(),
                AccessCardSaveStatus.CardNumberAlreadyExists =>
                    AccessCardPersistenceResult.DuplicateCardNumber(),
                _ => throw new InvalidOperationException(
                    $"Unsupported fake result status: {ResultStatus}.")
            };
        }
    }
}
