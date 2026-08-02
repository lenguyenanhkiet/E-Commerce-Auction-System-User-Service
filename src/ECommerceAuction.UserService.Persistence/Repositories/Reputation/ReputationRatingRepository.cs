using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Reputation;

public sealed class ReputationRatingRepository(ApplicationDbContext context) : IReputationRatingRepository
{
    public Task<TransactionRatingEligibility?> GetEligibilityAsync(
        string transactionType, Guid transactionId, Guid raterUserId,
        Guid targetUserId, CancellationToken cancellationToken = default) =>
        context.TransactionRatingEligibilities.SingleOrDefaultAsync(x =>
            x.TransactionType == transactionType && x.TransactionId == transactionId &&
            x.RaterUserId == raterUserId && x.TargetUserId == targetUserId, cancellationToken);

    public async Task AddEligibilityAsync(TransactionRatingEligibility eligibility, CancellationToken cancellationToken = default) =>
        await context.TransactionRatingEligibilities.AddAsync(eligibility, cancellationToken);

    public Task<UserRating?> GetRatingAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.UserRatings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddRatingAsync(UserRating rating, CancellationToken cancellationToken = default) =>
        await context.UserRatings.AddAsync(rating, cancellationToken);

    public Task<bool> ExistsRatingAsync(string transactionType, Guid transactionId, Guid raterUserId, Guid targetUserId, CancellationToken cancellationToken = default) =>
        context.UserRatings.AnyAsync(x => x.TransactionType == transactionType &&
            x.TransactionId == transactionId && x.RaterUserId == raterUserId &&
            x.TargetUserId == targetUserId, cancellationToken);

    public async Task<(IReadOnlyList<UserRating> Items, int Total)> GetGivenRatingsAsync(
        Guid raterUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.UserRatings.AsNoTracking().Where(x => x.RaterUserId == raterUserId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(IReadOnlyList<UserRating> Items, int Total)> GetReceivedRatingsAsync(
        Guid targetUserId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.UserRatings.AsNoTracking().Where(x => x.TargetUserId == targetUserId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(int Count, double Average)> GetReceivedSummaryAsync(
        Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var query = context.UserRatings.AsNoTracking().Where(x => x.TargetUserId == targetUserId);
        var count = await query.CountAsync(cancellationToken);
        var average = count == 0 ? 0 : await query.AverageAsync(x => x.Score, cancellationToken);
        return (count, average);
    }
}
