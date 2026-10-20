using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessCards;

public sealed record AccessCardPersistenceResult
{
    private AccessCardPersistenceResult(AccessCardSaveStatus status, AccessCard? accessCard)
    {
        Status = status;
        AccessCard = accessCard;
    }

    public AccessCardSaveStatus Status { get; }

    public AccessCard? AccessCard { get; }

    public static AccessCardPersistenceResult Succeeded(AccessCard accessCard) =>
        new(AccessCardSaveStatus.Success, accessCard);

    public static AccessCardPersistenceResult MissingAccessCard() =>
        new(AccessCardSaveStatus.AccessCardNotFound, null);

    public static AccessCardPersistenceResult MissingUser() =>
        new(AccessCardSaveStatus.UserNotFound, null);

    public static AccessCardPersistenceResult DuplicateCardNumber() =>
        new(AccessCardSaveStatus.CardNumberAlreadyExists, null);
}
