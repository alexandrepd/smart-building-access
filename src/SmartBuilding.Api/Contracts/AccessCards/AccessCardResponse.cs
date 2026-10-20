using SmartBuilding.Application.AccessCards;

namespace SmartBuilding.Api.Contracts.AccessCards;

/// <summary>Represents an access card managed by the API.</summary>
public sealed record AccessCardResponse(
    Guid Id,
    Guid UserId,
    string CardNumber,
    bool IsActive,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt)
{
    internal static AccessCardResponse FromDto(AccessCardDto accessCard)
    {
        return new AccessCardResponse(
            accessCard.Id,
            accessCard.UserId,
            accessCard.CardNumber,
            accessCard.IsActive,
            accessCard.ExpiresAt is null
                ? null
                : new DateTimeOffset(accessCard.ExpiresAt.Value),
            new DateTimeOffset(accessCard.CreatedAt));
    }
}
