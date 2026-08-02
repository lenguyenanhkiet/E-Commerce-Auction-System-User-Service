namespace ECommerceAuction.UserService.Domain.Reputation.Ratings;

public sealed class UserRating
{
    public const int MaxCommentLength = 1000;
    private UserRating() { }

    public Guid Id { get; private set; }
    public string TransactionType { get; private set; } = string.Empty;
    public Guid TransactionId { get; private set; }
    public Guid RaterUserId { get; private set; }
    public Guid TargetUserId { get; private set; }
    public int Score { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static UserRating Create(
        string transactionType, Guid transactionId, Guid raterUserId,
        Guid targetUserId, int score, string? comment, DateTimeOffset createdAt)
    {
        if (raterUserId == targetUserId) throw new ArgumentException("Self-rating is not allowed.");
        if (score is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(score));
        comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (comment?.Length > MaxCommentLength) throw new ArgumentException("Comment is too long.");

        return new UserRating
        {
            Id = Guid.NewGuid(), TransactionType = transactionType,
            TransactionId = transactionId, RaterUserId = raterUserId,
            TargetUserId = targetUserId, Score = score, Comment = comment,
            CreatedAt = createdAt.ToUniversalTime()
        };
    }
}
