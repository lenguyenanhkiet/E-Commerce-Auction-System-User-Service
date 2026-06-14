namespace ECommerceAuction.UserService.Domain.Entities.Users;

public class ReputationProfile
{
    public Guid UserId { get; set; }
    public int Score { get; set; } = 0;
    public string TrustLevel { get; set; } = "NORMAL";
    public int TotalRatings { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public int SuccessfulTransactions { get; set; } = 0;
    public int FailedTransactions { get; set; } = 0;
    public int SuccessfulAuctions { get; set; } = 0;
    public int FailedAuctions { get; set; } = 0;
    public int PenaltyCount { get; set; } = 0;
    public DateTime UpdatedAt { get; set; }
}