namespace SmartBuilding.Application.AccessCards;

public sealed record CreateAccessCardCommand(
    Guid UserId,
    string CardNumber,
    DateTimeOffset? ExpiresAt);
