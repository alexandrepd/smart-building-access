using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessCards;

public interface IAccessCardRepository
{
    Task<IReadOnlyList<AccessCard>> GetAllAsync(CancellationToken cancellationToken);

    Task<AccessCard?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccessCardPersistenceResult> CreateAsync(
        Guid userId,
        string cardNumber,
        DateTime? expiresAt,
        CancellationToken cancellationToken);

    Task<AccessCardPersistenceResult> UpdateAsync(
        Guid id,
        Guid userId,
        string cardNumber,
        DateTime? expiresAt,
        CancellationToken cancellationToken);

    Task<AccessCardPersistenceResult> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken);
}
