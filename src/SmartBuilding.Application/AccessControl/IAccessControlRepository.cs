using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Application.AccessControl;

public interface IAccessControlRepository
{
    Task<AccessDecisionContext?> GetDecisionContextAsync(
        string cardNumber,
        Guid accessPointId,
        DateTime occurredAt,
        CancellationToken cancellationToken);

    Task<AccessEvent> CreateEventAsync(
        AccessEvent accessEvent,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AccessEvent>> GetEventsAsync(CancellationToken cancellationToken);

    Task<AccessEvent?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken);
}
