namespace SmartBuilding.Application.AccessControl;

public interface IAccessControlService
{
    Task<AccessRequestResult> RequestAccessAsync(
        AccessRequestCommand command,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AccessEventDto>> GetEventsAsync(CancellationToken cancellationToken);

    Task<AccessEventDto?> GetEventByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
