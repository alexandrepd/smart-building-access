namespace SmartBuilding.Application.AccessCards;

public sealed record UpdateAccessCardCommand(
    Guid UserId,
    string CardNumber,
    DateTimeOffset? ExpiresAt);
