namespace SmartBuilding.Application.AccessCards;

public sealed record AccessCardDto(
    Guid Id,
    Guid UserId,
    string CardNumber,
    bool IsActive,
    DateTime? ExpiresAt,
    DateTime CreatedAt);
