namespace SmartBuilding.Application.AccessCards;

public sealed record AccessCardSaveResult(
    AccessCardSaveStatus Status,
    AccessCardDto? AccessCard)
{
    public static AccessCardSaveResult Succeeded(AccessCardDto accessCard) =>
        new(AccessCardSaveStatus.Success, accessCard);

    public static AccessCardSaveResult MissingAccessCard() =>
        new(AccessCardSaveStatus.AccessCardNotFound, null);

    public static AccessCardSaveResult MissingUser() =>
        new(AccessCardSaveStatus.UserNotFound, null);

    public static AccessCardSaveResult DuplicateCardNumber() =>
        new(AccessCardSaveStatus.CardNumberAlreadyExists, null);
}
