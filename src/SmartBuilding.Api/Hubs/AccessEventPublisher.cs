using Microsoft.AspNetCore.SignalR;
using SmartBuilding.Api.Contracts.AccessControl;
using SmartBuilding.Api.Contracts.Occupancy;
using SmartBuilding.Application.Occupancy;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Api.Hubs;

internal sealed class AccessEventPublisher(
    IHubContext<AccessHub, IAccessClient> hubContext,
    IOccupancyService occupancyService,
    ILogger<AccessEventPublisher> logger)
{
    internal async Task PublishAsync(AccessEventResponse accessEvent)
    {
        // Persistence has already committed. Notification failures must not change
        // the REST decision, and a disconnected requester must not cancel broadcasts.
        try
        {
            await hubContext.Clients.All.AccessEventCreated(accessEvent);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not publish AccessEventCreated for access event {AccessEventId}.",
                accessEvent.Id);
        }

        if (accessEvent.Result != AccessResult.Granted)
        {
            return;
        }

        try
        {
            var occupancy = await occupancyService.GetCurrentOccupancyAsync(
                CancellationToken.None);
            await hubContext.Clients.All.OccupancyChanged(
                occupancy.Select(CurrentOccupancyResponse.FromDto).ToArray());
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not publish OccupancyChanged for access event {AccessEventId}.",
                accessEvent.Id);
        }
    }
}
