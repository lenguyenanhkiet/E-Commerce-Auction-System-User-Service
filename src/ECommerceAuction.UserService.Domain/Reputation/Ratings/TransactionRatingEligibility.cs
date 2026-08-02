namespace ECommerceAuction.UserService.Domain.Reputation.Ratings;

public sealed class TransactionRatingEligibility
{
    private TransactionRatingEligibility() { }

    public Guid Id { get; private set; }
    public string TransactionType { get; private set; } = string.Empty;
    public Guid TransactionId { get; private set; }
    public Guid RaterUserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public DateTimeOffset OpensAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? SubmittedRatingId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static TransactionRatingEligibility Create(
        string transactionType, Guid transactionId, Guid raterUserId,
        Guid targetUserId, DateTimeOffset opensAt, DateTimeOffset expiresAt)
    {
        if (raterUserId == targetUserId) throw new ArgumentException("Rater and target must differ.");
        if (transactionId == Guid.Empty) throw new ArgumentException("Transaction is required.");
        if (transactionType is not ("ORDER" or "AUCTION"))
            throw new ArgumentException("Unsupported transaction type.");
        if (opensAt >= expiresAt) throw new ArgumentException("Rating window is invalid.");

        return new TransactionRatingEligibility
        {
            Id = Guid.NewGuid(), TransactionType = transactionType,
            TransactionId = transactionId, RaterUserId = raterUserId,
            TargetUserId = targetUserId, OpensAt = opensAt.ToUniversalTime(),
            ExpiresAt = expiresAt.ToUniversalTime(), CreatedAt = opensAt.ToUniversalTime(),
            UpdatedAt = opensAt.ToUniversalTime()
        };
    }

    public void MarkSubmitted(Guid ratingId, DateTimeOffset now)
    {
        if (RevokedAt.HasValue) throw new InvalidOperationException("Rating eligibility is revoked.");
        if (now < OpensAt) throw new InvalidOperationException("Rating window is not open.");
        if (now >= ExpiresAt) throw new InvalidOperationException("Rating eligibility has expired.");
        if (SubmittedRatingId.HasValue) throw new InvalidOperationException("Rating was already submitted.");
        SubmittedRatingId = ratingId;
        UpdatedAt = now.ToUniversalTime();
    }

    public void Revoke(DateTimeOffset now)
    {
        RevokedAt ??= now.ToUniversalTime();
        UpdatedAt = now.ToUniversalTime();
    }
}
