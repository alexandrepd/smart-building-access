using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartBuilding.Api.Contracts.AccessControl;
using SmartBuilding.Api.Contracts.Occupancy;
using SmartBuilding.Api.Security;

namespace SmartBuilding.Api.Hubs;

/// <summary>Server notifications for authorized access operations clients.</summary>
public interface IAccessClient
{
    /// <summary>Notifies clients of a recorded access decision.</summary>
    Task AccessEventCreated(AccessEventResponse accessEvent);
    /// <summary>Notifies clients of current occupancy after a granted decision.</summary>
    Task OccupancyChanged(CurrentOccupancyResponse[] occupancy);
}

/// <summary>Provides real-time access and occupancy notifications.</summary>
[Authorize(Policy = AuthorizationPolicies.AccessOperations)]
public sealed class AccessHub : Hub<IAccessClient>
{
    /// <summary>The hub's connection and negotiation path.</summary>
    public const string Path = "/hubs/access";
}
