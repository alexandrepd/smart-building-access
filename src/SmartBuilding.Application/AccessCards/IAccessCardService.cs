namespace SmartBuilding.Application.AccessCards;

public interface IAccessCardService
{
    Task<IReadOnlyList<AccessCardDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<AccessCardDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AccessCardSaveResult> CreateAsync(
        CreateAccessCardCommand command,
        CancellationToken cancellationToken);

    Task<AccessCardSaveResult> UpdateAsync(
        Guid id,
        UpdateAccessCardCommand command,
        CancellationToken cancellationToken);

    Task<AccessCardSaveResult> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken);
}
