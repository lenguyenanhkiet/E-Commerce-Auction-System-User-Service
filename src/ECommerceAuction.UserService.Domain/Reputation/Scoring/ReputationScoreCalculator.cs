namespace ECommerceAuction.UserService.Domain.Reputation.Scoring;

public static class ReputationScoreCalculator
{
    public static int CalculateTransactionValue(decimal amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (amount <= 1_000_000m)
        {
            return checked((int)Math.Floor(amount / 10_000m));
        }

        if (amount <= 5_000_000m)
        {
            return checked(
                100 +
                (int)Math.Floor(
                    (amount - 1_000_000m) / 50_000m));
        }

        return checked(
            180 +
            (int)Math.Floor(
                (amount - 5_000_000m) / 100_000m));
    }

    public static int CalculateCodRefusalPenalty(decimal amount)
    {
        var expectedScore = CalculateTransactionValue(amount);
        var penalty = Math.Max(
            50,
            checked((int)Math.Ceiling(expectedScore * 1.5m)));

        return checked(-penalty);
    }

    public static int CalculateAuctionNonFulfillmentPenalty(decimal amount)
    {
        var transactionScore = CalculateTransactionValue(amount);
        var penalty = Math.Max(
            300,
            checked(transactionScore * 2));

        return checked(-penalty);
    }

    public static int CalculateProductReviewScore(bool hasMedia)
    {
        return hasMedia ? 5 : 2;
    }
}
