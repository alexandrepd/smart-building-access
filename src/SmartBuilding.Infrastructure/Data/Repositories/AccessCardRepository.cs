using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartBuilding.Application.AccessCards;
using SmartBuilding.Domain.Entities;

namespace SmartBuilding.Infrastructure.Data.Repositories;

public sealed class AccessCardRepository(SmartBuildingDbContext dbContext) : IAccessCardRepository
{
    private const string CardNumberUniqueIndex = "ix_access_cards_card_number";
    private const string UserForeignKey = "fk_access_cards_users_user_id";

    public async Task<IReadOnlyList<AccessCard>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.AccessCards
            .AsNoTracking()
            .OrderBy(accessCard => accessCard.CardNumber)
            .ToArrayAsync(cancellationToken);
    }

    public Task<AccessCard?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.AccessCards
            .AsNoTracking()
            .SingleOrDefaultAsync(accessCard => accessCard.Id == id, cancellationToken);
    }

    public async Task<AccessCardPersistenceResult> CreateAsync(
        Guid userId,
        string cardNumber,
        DateTime? expiresAt,
        CancellationToken cancellationToken)
    {
        var accessCard = new AccessCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardNumber = cardNumber,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.AccessCards.Add(accessCard);
        return await SaveAsync(accessCard, cancellationToken);
    }

    public async Task<AccessCardPersistenceResult> UpdateAsync(
        Guid id,
        Guid userId,
        string cardNumber,
        DateTime? expiresAt,
        CancellationToken cancellationToken)
    {
        AccessCard? accessCard = await dbContext.AccessCards
            .SingleOrDefaultAsync(accessCard => accessCard.Id == id, cancellationToken);

        if (accessCard is null)
        {
            return AccessCardPersistenceResult.MissingAccessCard();
        }

        accessCard.UserId = userId;
        accessCard.CardNumber = cardNumber;
        accessCard.ExpiresAt = expiresAt;
        return await SaveAsync(accessCard, cancellationToken);
    }

    public async Task<AccessCardPersistenceResult> SetActiveAsync(
        Guid id,
        bool isActive,
        CancellationToken cancellationToken)
    {
        AccessCard? accessCard = await dbContext.AccessCards
            .SingleOrDefaultAsync(accessCard => accessCard.Id == id, cancellationToken);

        if (accessCard is null)
        {
            return AccessCardPersistenceResult.MissingAccessCard();
        }

        accessCard.IsActive = isActive;
        return await SaveAsync(accessCard, cancellationToken);
    }

    private async Task<AccessCardPersistenceResult> SaveAsync(
        AccessCard accessCard,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return AccessCardPersistenceResult.Succeeded(accessCard);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: CardNumberUniqueIndex
            })
        {
            dbContext.Entry(accessCard).State = EntityState.Detached;
            return AccessCardPersistenceResult.DuplicateCardNumber();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: UserForeignKey
            })
        {
            dbContext.Entry(accessCard).State = EntityState.Detached;
            return AccessCardPersistenceResult.MissingUser();
        }
        catch (DbUpdateConcurrencyException)
        {
            dbContext.Entry(accessCard).State = EntityState.Detached;
            return AccessCardPersistenceResult.MissingAccessCard();
        }
    }
}
