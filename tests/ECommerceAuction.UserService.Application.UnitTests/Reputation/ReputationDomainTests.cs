using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Common;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Reputation.Scoring;
using ECommerceAuction.UserService.Domain.Reputation.Seller;
using ECommerceAuction.UserService.Application.Features.Users.GetProfile;

namespace ECommerceAuction.UserService.Application.UnitTests.Reputation;

public sealed class ReputationDomainTests
{
    private static readonly DateTimeOffset OccurredAt =
        new(2026, 7, 26, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public void BuyerVerificationProfile_VerifiesAllFiveDimensionsOnlyOnce()
    {
        var profile = BuyerVerificationProfile.Create(Guid.NewGuid(), OccurredAt);

        Assert.True(profile.VerifyEmail(OccurredAt));
        Assert.True(profile.VerifyPhone(OccurredAt));
        Assert.True(profile.VerifyIdentity(OccurredAt));
        Assert.True(profile.VerifyAddress(OccurredAt));
        Assert.True(profile.VerifyPaymentMethod(OccurredAt));
        Assert.True(profile.IsFullyVerified);

        Assert.False(profile.VerifyEmail(OccurredAt.AddMinutes(1)));
        Assert.False(profile.VerifyPhone(OccurredAt.AddMinutes(1)));
        Assert.False(profile.VerifyIdentity(OccurredAt.AddMinutes(1)));
        Assert.False(profile.VerifyAddress(OccurredAt.AddMinutes(1)));
        Assert.False(profile.VerifyPaymentMethod(OccurredAt.AddMinutes(1)));
    }

    [Fact]
    public void BuyerReputationProfile_TracksPenaltySeparatelyFromEarnedPoints()
    {
        var profile = BuyerReputationProfile.Create(Guid.NewGuid(), OccurredAt);

        profile.ApplyConfirmedPoints(-5, OccurredAt);

        Assert.Equal(-5, profile.ConfirmedScore);
        Assert.Equal(0, profile.LifetimeEarnedPoints);
        Assert.Equal(5, profile.LifetimePenaltyPoints);
        Assert.Equal(1, profile.PenaltyCount);
        Assert.Equal(BuyerTrustLevels.Restricted, profile.TrustLevel);
    }

    [Fact]
    public void BuyerReputationProfile_ManagesPendingPoints()
    {
        var profile = BuyerReputationProfile.Create(Guid.NewGuid(), OccurredAt);

        profile.AddPendingPoints(5, OccurredAt);
        profile.ConfirmPendingPoints(3, OccurredAt.AddMinutes(1));
        profile.CancelPendingPoints(2, OccurredAt.AddMinutes(2));

        Assert.Equal(3, profile.ConfirmedScore);
        Assert.Equal(0, profile.PendingScore);
        Assert.Throws<ArgumentOutOfRangeException>(
            () => profile.AddPendingPoints(-1, OccurredAt));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => profile.ApplyConfirmedPoints(0, OccurredAt));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(-5)]
    public void ReputationLedgerEntry_Create_CapturesImmutableMutation(int delta)
    {
        var mutation = CreateMutation(delta);

        var entry = ReputationLedgerEntry.Create(
            mutation,
            scoreBefore: 10,
            scoreAfter: 10 + delta,
            createdAt: OccurredAt.AddHours(7));

        Assert.Equal(mutation.UserId, entry.UserId);
        Assert.Equal(ReputationRoles.Buyer, entry.Role);
        Assert.Equal(delta, entry.ScoreDelta);
        Assert.Equal(10, entry.ScoreBefore);
        Assert.Equal(10 + delta, entry.ScoreAfter);
        Assert.Equal(TimeSpan.Zero, entry.OccurredAt.Offset);
        Assert.Equal(TimeSpan.Zero, entry.CreatedAt.Offset);
    }

    [Fact]
    public void ReputationLedgerEntry_Create_RejectsInvalidMutation()
    {
        var valid = CreateMutation(1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ReputationLedgerEntry.Create(valid with { ScoreDelta = 0 }, 0, 0, OccurredAt));
        Assert.Throws<ArgumentException>(() =>
            ReputationLedgerEntry.Create(valid with { Role = "ADMIN" }, 0, 1, OccurredAt));
        Assert.Throws<ArgumentException>(() =>
            ReputationLedgerEntry.Create(valid with { ReasonCode = "unknown" }, 0, 1, OccurredAt));
        Assert.Throws<ArgumentException>(() =>
            ReputationLedgerEntry.Create(valid with { MessageId = Guid.Empty }, 0, 1, OccurredAt));
        Assert.Throws<ArgumentException>(() =>
            ReputationLedgerEntry.Create(valid with { IdempotencyKey = " " }, 0, 1, OccurredAt));
        Assert.Throws<InvalidOperationException>(() =>
            ReputationLedgerEntry.Create(valid, 0, 2, OccurredAt));
        Assert.Throws<OverflowException>(() =>
            ReputationLedgerEntry.Create(valid, int.MaxValue, int.MinValue, OccurredAt));
    }

    [Fact]
    public void ReputationLedgerEntry_CreateReversal_CreatesCompensatingEntryWithoutMutatingOriginal()
    {
        var original = ReputationLedgerEntry.Create(
            CreateMutation(5),
            10,
            15,
            OccurredAt);
        var originalSnapshot = (original.ScoreDelta, original.ScoreBefore, original.ScoreAfter);

        var reversal = ReputationLedgerEntry.CreateReversal(
            original,
            scoreBefore: 20,
            messageId: Guid.NewGuid(),
            correlationId: Guid.NewGuid(),
            idempotencyKey: "reversal:1",
            evidenceReference: "case-1",
            occurredAt: OccurredAt.AddDays(1),
            createdAt: OccurredAt.AddDays(1));

        Assert.NotEqual(original.Id, reversal.Id);
        Assert.Equal(original.Id, reversal.ReversesEntryId);
        Assert.Equal(-5, reversal.ScoreDelta);
        Assert.Equal(20, reversal.ScoreBefore);
        Assert.Equal(15, reversal.ScoreAfter);
        Assert.Equal(originalSnapshot, (original.ScoreDelta, original.ScoreBefore, original.ScoreAfter));
        Assert.Throws<InvalidOperationException>(() =>
            ReputationLedgerEntry.CreateReversal(
                reversal, 15, Guid.NewGuid(), null, "reversal:2", "case-2",
                OccurredAt.AddDays(2), OccurredAt.AddDays(2)));
    }

    [Fact]
    public void ReputationLedgerEntry_CreateReversal_RejectsMinimumIntegerDelta()
    {
        var original = ReputationLedgerEntry.Create(
            CreateMutation(int.MinValue),
            0,
            int.MinValue,
            OccurredAt);

        Assert.Throws<OverflowException>(() =>
            ReputationLedgerEntry.CreateReversal(
                original, 0, Guid.NewGuid(), null, "reversal:min", "case-min",
                OccurredAt, OccurredAt));
    }

    [Fact]
    public void BuyerAndSellerProfiles_KeepIndependentScoresAndRestrictions()
    {
        var userId = Guid.NewGuid();
        var buyer = BuyerReputationProfile.Create(userId, OccurredAt);
        var seller = SellerReputationProfile.Create(userId, OccurredAt);

        buyer.ApplyConfirmedDelta(-1, OccurredAt);
        buyer.ApplyAuctionRestriction(
            ReputationRestrictions.Blocked, null, true, "PAYMENT_DEFAULT", OccurredAt);
        seller.ApplyConfirmedDelta(50, OccurredAt);
        seller.ApplySellingRestriction(
            ReputationRestrictions.Restricted, OccurredAt.AddDays(1), false, null, OccurredAt);

        Assert.Equal(-1, buyer.ConfirmedScore);
        Assert.Equal(ReputationTrustLevels.Restricted, buyer.TrustLevel);
        Assert.Equal(50, seller.ConfirmedScore);
        Assert.Equal(ReputationTrustLevels.Trusted, seller.TrustLevel);
        Assert.Equal(ReputationRestrictions.Blocked, buyer.AuctionRestrictionStatus);
        Assert.Equal(ReputationRestrictions.Restricted, seller.SellingRestrictionStatus);

        buyer.ApplyConfirmedDelta(100, OccurredAt.AddHours(1));
        Assert.Equal(ReputationRestrictions.Blocked, buyer.AuctionRestrictionStatus);

        buyer.ClearAuctionRestriction(OccurredAt.AddHours(2));
        seller.ClearRestrictions(OccurredAt.AddHours(2));
        Assert.Equal(ReputationRestrictions.None, buyer.AuctionRestrictionStatus);
        Assert.Equal(ReputationRestrictions.None, seller.SellingRestrictionStatus);
    }

    [Fact]
    public void BuyerReputationProfile_RejectsZeroAndOverflow()
    {
        var profile = BuyerReputationProfile.Create(Guid.NewGuid(), OccurredAt);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => profile.ApplyConfirmedDelta(0, OccurredAt));
        profile.ApplyConfirmedDelta(int.MaxValue, OccurredAt);
        Assert.Throws<OverflowException>(
            () => profile.ApplyConfirmedDelta(1, OccurredAt));
    }

    [Fact]
    public void SellerReputationProfile_RejectsZeroAndOverflow()
    {
        var profile = SellerReputationProfile.Create(Guid.NewGuid(), OccurredAt);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => profile.ApplyConfirmedDelta(0, OccurredAt));
        profile.ApplyConfirmedDelta(int.MinValue, OccurredAt);
        Assert.Throws<OverflowException>(
            () => profile.ApplyConfirmedDelta(-1, OccurredAt));
    }

    [Fact]
    public void SellerReputationProfile_ScoreIncreaseDoesNotClearBlockingViolation()
    {
        var profile = SellerReputationProfile.Create(Guid.NewGuid(), OccurredAt);
        profile.ApplyAuctionRestriction(
            ReputationRestrictions.Blocked,
            null,
            true,
            "SHILL_BIDDING",
            OccurredAt);

        profile.ApplyConfirmedDelta(1_000, OccurredAt.AddMinutes(1));

        Assert.Equal(
            ReputationRestrictions.Blocked,
            profile.AuctionRestrictionStatus);
        Assert.True(profile.RequiresManualReview);
        Assert.Equal("SHILL_BIDDING", profile.BlockingViolationCode);
    }

    [Fact]
    public void ReputationProfiles_RejectInvalidRestrictions()
    {
        var buyer = BuyerReputationProfile.Create(Guid.NewGuid(), OccurredAt);
        var seller = SellerReputationProfile.Create(Guid.NewGuid(), OccurredAt);

        Assert.Throws<ArgumentException>(() =>
            buyer.ApplyAuctionRestriction(
                "UNKNOWN", null, false, null, OccurredAt));
        Assert.Throws<ArgumentException>(() =>
            seller.ApplySellingRestriction(
                ReputationRestrictions.None, null, false, null, OccurredAt));
    }

    private static ReputationMutation CreateMutation(int delta)
    {
        return new ReputationMutation(
            Guid.NewGuid(),
            ReputationRoles.Buyer,
            ReputationReasonCatalog.BuyerProfileEmailVerified,
            delta,
            "user-service",
            "USER",
            "user-1",
            $"mutation:{Guid.NewGuid():N}",
            "REPUTATION_V1",
            Guid.NewGuid(),
            null,
            null,
            OccurredAt);
    }
    [Fact]
    public void BuyerVerificationProfile_IsSourceOfTruthForAllVerificationState()
    {
        var propertyNames = typeof(BuyerVerificationProfile)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("IsEmailVerified", propertyNames);
        Assert.Contains("IsPhoneVerified", propertyNames);
        Assert.Contains("EmailVerifiedAt", propertyNames);
        Assert.Contains("PhoneVerifiedAt", propertyNames);
        Assert.Contains("IsIdentityVerified", propertyNames);
        Assert.Contains("IdentityVerifiedAt", propertyNames);
        Assert.Contains("IsFullyVerified", propertyNames);
    }

    [Fact]
    public void UserProfileResponse_ExposesAggregatedVerification()
    {
        var verificationProperty = typeof(UserProfileResponse)
            .GetProperty("Verification");

        Assert.NotNull(verificationProperty);
        Assert.Equal(
            "UserVerificationResponse",
            verificationProperty!.PropertyType.Name);
    }
}
