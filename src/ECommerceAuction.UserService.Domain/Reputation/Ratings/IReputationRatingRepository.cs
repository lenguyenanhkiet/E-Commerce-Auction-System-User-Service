namespace ECommerceAuction.UserService.Domain.Reputation.Ratings;

public interface IReputationRatingRepository
{
    Task<TransactionRatingEligibility?> GetEligibilityAsync(
        string transactionType, Guid transactionId, Guid raterUserId,
        Guid targetUserId, CancellationToken cancellationToken = default);
    Task AddEligibilityAsync(TransactionRatingEligibility eligibility, CancellationToken cancellationToken = default);
    Task<UserRating?> GetRatingAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddRatingAsync(UserRating rating, CancellationToken cancellationToken = default);
    Task<bool> ExistsRatingAsync(string transactionType, Guid transactionId, Guid raterUserId, Guid targetUserId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserRating> Items, int Total)> GetGivenRatingsAsync(Guid raterUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<UserRating> Items, int Total)> GetReceivedRatingsAsync(Guid targetUserId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(int Count, double Average)> GetReceivedSummaryAsync(Guid targetUserId, CancellationToken cancellationToken = default);
}
