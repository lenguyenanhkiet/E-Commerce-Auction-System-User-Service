using TrustLevels = ECommerceAuction.UserService.Domain.Entities.Reputation.TrustLevel;
namespace ECommerceAuction.UserService.Domain.Entities.Reputation;

/// <summary>
/// Stores the current reputation summary for a user.
/// </summary>
public class ReputationProfile
{
    public Guid UserId { get; set; }
    public int Score { get; set; } = 0;
    public string TrustLevel { get; set; } = TrustLevels.Silver;
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

   private void AddPoints(int points)
    {
        if (points <= 0) return;
        Score += points;
        TrustLevel = ResolveTrustLevel(Score);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddEmailVerificationPoint() =>AddPoints(ReputationPoints.EmailVerifiedPoint);
    public void AddPhoneVerificationPoint() => AddPoints(ReputationPoints.PhoneVerifiedPoint);
    public void AddVerifiedPaymentMethodPoint() => AddPoints(ReputationPoints.VerifiedPaymentMethodPoint);
    public void AddIdentificationVerificationPoint() => AddPoints(ReputationPoints.IdentificationVerifiedPoint);
    public void AddTaxVerificationPoint() => AddPoints(ReputationPoints.TaxVerifiedPoint);
    public void AddBusinessLicenseVerificationPoint() => AddPoints(ReputationPoints.BusinessLicenseVerifiedPoint);
    public void AddBusinessAddressVerificationPoint() => AddPoints(ReputationPoints.BusinessAddressVerifiedPoint);

    /// <summary>
    /// Resolves the display rank from the accumulated reputation score.
    /// </summary>
    private static string ResolveTrustLevel(int score)
    {
        //ECA - 6 Reputation: rank names follow the current Business Rules document.
        return score switch
        {
            <= 100 => TrustLevels.Silver,
            <= 1_000 => TrustLevels.Gold,
            <= 10_000 => TrustLevels.Platinum,
            _ => TrustLevels.Diamond
        };
    }
}
