using SmartBuilding.Application.AccessPoints;

namespace SmartBuilding.Api.Contracts.AccessPoints;

/// <summary>Represents an access point returned by the API.</summary>
public sealed record AccessPointResponse(
    Guid Id,
    Guid FloorId,
    string Name,
    string Location,
    bool IsActive,
    bool SupportsEntry,
    bool SupportsExit,
    DateTimeOffset CreatedAt)
{
    internal static AccessPointResponse FromDto(AccessPointDto accessPoint)
    {
        return new AccessPointResponse(
            accessPoint.Id,
            accessPoint.FloorId,
            accessPoint.Name,
            accessPoint.Location,
            accessPoint.IsActive,
            accessPoint.SupportsEntry,
            accessPoint.SupportsExit,
            new DateTimeOffset(accessPoint.CreatedAt));
    }
}