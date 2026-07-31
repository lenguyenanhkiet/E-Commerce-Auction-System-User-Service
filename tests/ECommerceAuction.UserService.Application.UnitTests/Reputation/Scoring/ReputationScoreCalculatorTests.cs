using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using ECommerceAuction.UserService.Domain.Reputation.Common;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation.Scoring;

public sealed class ReputationScoreCalculatorTests
{
    [Theory]
    [InlineData("BUYER", true)]
    [InlineData("SELLER", true)]
    [InlineData("ADMIN", false)]
    [InlineData("", false)]
    public void ReputationRoles_RecognizesOnlyScoredRoles(
        string role,
        bool expected)
    {
        Assert.Equal(expected, ReputationRoles.IsValid(role));
    }

    [Fact]
    public void ReputationReasonCatalog_RejectsUnknownReason()
    {
        Assert.True(
            ReputationReasonCatalog.IsKnown(
                ReputationReasonCatalog.BuyerProfileEmailVerified));
        Assert.True(
            ReputationReasonCatalog.IsKnown(
                ReputationReasonCatalog.SellerAuctionShillBiddingConfirmed));
        Assert.False(ReputationReasonCatalog.IsKnown("custom.free-points"));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(9_999, 0)]
    [InlineData(10_000, 1)]
    [InlineData(500_000, 50)]
    [InlineData(1_000_000, 100)]
    [InlineData(1_050_000, 101)]
    [InlineData(5_000_000, 180)]
    [InlineData(6_000_000, 190)]
    public void CalculateTransactionValue_ReturnsApprovedTieredScore(
        decimal amount,
        int expected)
    {
        var actual =
            ReputationScoreCalculator.CalculateTransactionValue(amount);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(200_000, -50)]
    [InlineData(1_000_000, -150)]
    public void CalculateCodRefusalPenalty_UsesOnePointFiveMultiplierWithMinimum(
        decimal amount,
        int expected)
    {
        var actual =
            ReputationScoreCalculator.CalculateCodRefusalPenalty(amount);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(200_000, -300)]
    [InlineData(5_000_000, -360)]
    public void CalculateAuctionNonFulfillmentPenalty_UsesDoubleScoreWithMinimum(
        decimal amount,
        int expected)
    {
        var actual =
            ReputationScoreCalculator.CalculateAuctionNonFulfillmentPenalty(
                amount);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 5)]
    public void CalculateProductReviewScore_DoesNotStackMediaBonus(
        bool hasMedia,
        int expected)
    {
        var actual =
            ReputationScoreCalculator.CalculateProductReviewScore(hasMedia);

        Assert.Equal(expected, actual);
    }
}
