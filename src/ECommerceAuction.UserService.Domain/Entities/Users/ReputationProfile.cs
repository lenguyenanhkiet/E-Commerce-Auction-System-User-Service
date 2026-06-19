namespace ECommerceAuction.UserService.Domain.Entities.Users;

/// <summary>
/// Stores the current reputation summary for a user.
/// </summary>
public class ReputationProfile
{
    private const int EmailVerificationPoint = 1;
    private const string SilverTrustLevel = "Silver";
    private const string GoldTrustLevel = "Gold";
    private const string PlatinumTrustLevel = "Platinum";
    private const string DiamondTrustLevel = "Diamond";

    public Guid UserId { get; set; }
    public int Score { get; set; } = 0;
    public string TrustLevel { get; set; } = SilverTrustLevel;
    public int TotalRatings { get; set; } = 0;
    public decimal AverageRating { get; set; } = 0;
    public int SuccessfulTransactions { get; set; } = 0;
    public int FailedTransactions { get; set; } = 0;
    public int SuccessfulAuctions { get; set; } = 0;
    public int FailedAuctions { get; set; } = 0;
    public int PenaltyCount { get; set; } = 0;
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Creates the default reputation profile for a user.
    /// </summary>
    public static ReputationProfile CreateDefault(Guid userId)
    {
        return new ReputationProfile
        {
            UserId = userId,
            Score = 0,
            TrustLevel = ResolveTrustLevel(0),
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates the default reputation profile when the account email has already been verified.
    /// </summary>
    public static ReputationProfile CreateForVerifiedEmail(Guid userId)
    {
        var profile = CreateDefault(userId);

        // ECA-6 OAuth2 Google / VerifyEmail: verified email gives the first reputation point.
        profile.AddEmailVerificationPoint();

        return profile;
    }

    /// <summary>
    /// Adds the one-time reputation point granted when the user proves ownership of the email address.
    /// </summary>
    public void AddEmailVerificationPoint()
    {
        Score += EmailVerificationPoint;
        TrustLevel = ResolveTrustLevel(Score);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Resolves the display rank from the accumulated reputation score.
    /// </summary>
    private static string ResolveTrustLevel(int score)
    {
        // ECA-6 Reputation: rank names follow the current Business Rules document.
        return score switch
        {
            <= 100 => SilverTrustLevel,
            <= 1_000 => GoldTrustLevel,
            <= 10_000 => PlatinumTrustLevel,
            _ => DiamondTrustLevel
        };
    }
}
