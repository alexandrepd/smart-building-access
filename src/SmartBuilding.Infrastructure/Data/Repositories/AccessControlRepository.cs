using Microsoft.EntityFrameworkCore;
using SmartBuilding.Application.AccessControl;
using SmartBuilding.Domain.Entities;
using SmartBuilding.Domain.Enums;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class AccessControlRepository(SmartBuildingDbContext dbContext)
    : IAccessControlRepository
{
    public async Task<AccessDecisionContext?> GetDecisionContextAsync(
        string cardNumber,
        Guid accessPointId,
        DateTime occurredAt,
        CancellationToken cancellationToken)
    {
        AccessPoint? accessPoint = await dbContext.AccessPoints
            .AsNoTracking()
            .SingleOrDefaultAsync(
                accessPoint => accessPoint.Id == accessPointId,
                cancellationToken);

        if (accessPoint is null)
        {
            return null;
        }

        AccessCard? accessCard = await dbContext.AccessCards
            .AsNoTracking()
            .Include(card => card.User)
            .SingleOrDefaultAsync(
                card => card.CardNumber == cardNumber,
                cancellationToken);

        bool hasActivePermission = false;
        bool hasCurrentlyValidPermission = false;
        if (accessCard is not null)
        {
            IQueryable<AccessPermission> permissions = dbContext.AccessPermissions
                .AsNoTracking()
                .Where(permission =>
                    permission.UserId == accessCard.UserId
                    && permission.AccessPointId == accessPointId
                    && permission.IsActive);

            hasActivePermission = await permissions.AnyAsync(cancellationToken);
            hasCurrentlyValidPermission = await permissions.AnyAsync(
                permission => permission.ValidFrom <= occurredAt
                    && (permission.ValidUntil == null || permission.ValidUntil > occurredAt),
                cancellationToken);
        }

        return new AccessDecisionContext(
            accessPoint.Id,
            accessPoint.FloorId,
            accessPoint.IsActive,
            accessPoint.SupportsEntry,
            accessPoint.SupportsExit,
            accessCard?.Id,
            accessCard?.IsActive,
            accessCard?.ExpiresAt,
            accessCard?.UserId,
            accessCard?.User.IsActive,
            hasActivePermission,
            hasCurrentlyValidPermission);
    }

    public async Task<AccessEvent> RecordEventAsync(
        AccessEvent accessEvent,
        OccupancyTransition? occupancyTransition,
        CancellationToken cancellationToken)
    {
        if (occupancyTransition is not null)
        {
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(cancellationToken);
            string lockKey =
                $"{occupancyTransition.UserId:N}:{occupancyTransition.FloorId:N}";
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({lockKey}, 0))",
                cancellationToken);

            OccupancySession? openSession = await dbContext.OccupancySessions
                .SingleOrDefaultAsync(
                    session => session.UserId == occupancyTransition.UserId
                        && session.FloorId == occupancyTransition.FloorId
                        && session.ExitedAt == null,
                    cancellationToken);

            if (occupancyTransition.Direction == AccessDirection.Entry
                && openSession is null)
            {
                dbContext.OccupancySessions.Add(new OccupancySession
                {
                    Id = Guid.NewGuid(),
                    UserId = occupancyTransition.UserId,
                    FloorId = occupancyTransition.FloorId,
                    EnteredAt = occupancyTransition.OccurredAt
                });
            }
            else if (occupancyTransition.Direction == AccessDirection.Exit
                && openSession is not null)
            {
                openSession.TryClose(occupancyTransition.OccurredAt);
            }

            dbContext.AccessEvents.Add(accessEvent);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return accessEvent;
        }

        dbContext.AccessEvents.Add(accessEvent);
        await dbContext.SaveChangesAsync(cancellationToken);
        return accessEvent;
    }

    public async Task<IReadOnlyList<AccessEvent>> GetEventsAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.AccessEvents
            .AsNoTracking()
            .OrderByDescending(accessEvent => accessEvent.OccurredAt)
            .ThenBy(accessEvent => accessEvent.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AccessEvent?> GetEventByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.AccessEvents
            .AsNoTracking()
            .SingleOrDefaultAsync(accessEvent => accessEvent.Id == id, cancellationToken);
    }
}
