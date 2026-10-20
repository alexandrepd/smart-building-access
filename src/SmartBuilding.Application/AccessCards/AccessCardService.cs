using SmartBuilding.Application.Common;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessCards;

public sealed class AccessCardService(IAccessCardRepository repository) : IAccessCardService
{
    public async Task<IReadOnlyList<AccessCardDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AccessCard> accessCards = await repository.GetAllAsync(cancellationToken);
        return accessCards.Select(ToDto).ToArray();
    }

    public async Task<AccessCardDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        AccessCard? accessCard = await repository.GetByIdAsync(id, cancellationToken);
        return accessCard is null ? null : ToDto(accessCard);
    }

    public async Task<AccessCardSaveResult> CreateAsync(
        CreateAccessCardCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateUserId(command.UserId);
        string cardNumber = NormalizeCardNumber(command.CardNumber);

        AccessCardPersistenceResult result = await repository.CreateAsync(
            command.UserId,
            cardNumber,
            command.ExpiresAt?.UtcDateTime,
            cancellationToken);

        return ToSaveResult(result);
    }

    public async Task<AccessCardSaveResult> UpdateAsync(
        Guid id,
        UpdateAccessCardCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateUserId(command.UserId);
        string cardNumber = NormalizeCardNumber(command.CardNumber);

        AccessCardPersistenceResult result = await repository.UpdateAsync(
            id,
            command.UserId,
            cardNumber,
            command.ExpiresAt?.UtcDateTime,
            cancellationToken);

        return ToSaveResult(result);
    }

    public async Task<AccessCardSaveResult> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        AccessCardPersistenceResult result = await repository.SetActiveAsync(
            id,
            isActive,
            cancellationToken);

        return ToSaveResult(result);
    }

    private static AccessCardSaveResult ToSaveResult(AccessCardPersistenceResult result)
    {
        return result.Status switch
        {
            AccessCardSaveStatus.Success =>
                AccessCardSaveResult.Succeeded(ToDto(result.AccessCard!)),
            AccessCardSaveStatus.AccessCardNotFound =>
                AccessCardSaveResult.MissingAccessCard(),
            AccessCardSaveStatus.UserNotFound =>
                AccessCardSaveResult.MissingUser(),
            AccessCardSaveStatus.CardNumberAlreadyExists =>
                AccessCardSaveResult.DuplicateCardNumber(),
            _ => throw new InvalidOperationException(
                $"Unsupported access card save status: {result.Status}.")
        };
    }

    private static AccessCardDto ToDto(AccessCard accessCard)
    {
        return new AccessCardDto(
            accessCard.Id,
            accessCard.UserId,
            accessCard.CardNumber,
            accessCard.IsActive,
            accessCard.ExpiresAt,
            accessCard.CreatedAt);
    }

    private static string NormalizeCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessCardCommand.CardNumber),
                "Card number is required.");
        }

        string normalized = cardNumber.Trim();
        if (normalized.Length > 100)
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessCardCommand.CardNumber),
                "Card number cannot exceed 100 characters.");
        }

        return normalized;
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ApplicationValidationException(
                nameof(CreateAccessCardCommand.UserId),
                "User ID is required.");
        }
    }
}
