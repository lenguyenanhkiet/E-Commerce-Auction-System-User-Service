using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
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

    [Fact]
    public void ReputationLedgerEntry_EnforcesPendingLifecycle()
    {
        var entry = ReputationLedgerEntry.CreatePending(
            Guid.NewGuid(),
            ReputationEntryTypes.EcommerceTransaction,
            ReputationReasons.EcommerceOrderCompleted,
            3,
            "user-service",
            "ORDER",
            "order-1",
            "order:order-1:completed:v1",
            OccurredAt,
            OccurredAt.AddMinutes(5));

        Assert.Throws<InvalidOperationException>(
            () => entry.Confirm(OccurredAt.AddMinutes(4)));

        entry.Confirm(OccurredAt.AddMinutes(5));

        Assert.Equal(ReputationEntryStatuses.Confirmed, entry.Status);
        Assert.Throws<InvalidOperationException>(
            () => entry.Confirm(OccurredAt.AddMinutes(6)));
        Assert.Throws<InvalidOperationException>(
            () => entry.Cancel(OccurredAt.AddMinutes(6)));
    }

    [Fact]
    public void ReputationLedgerEntry_RejectsInvalidTransitions()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ReputationLedgerEntry.CreateConfirmed(
                Guid.NewGuid(),
                ReputationEntryTypes.ProfileVerification,
                ReputationReasons.EmailVerified,
                0,
                "user-service",
                "USER_EMAIL",
                "user-1",
                "user:user-1:email-verified:v1",
                OccurredAt));

        var pending = ReputationLedgerEntry.CreatePending(
            Guid.NewGuid(),
            ReputationEntryTypes.EcommerceTransaction,
            ReputationReasons.EcommerceOrderCompleted,
            1,
            "user-service",
            "ORDER",
            "order-2",
            "order:order-2:completed:v1",
            OccurredAt,
            OccurredAt.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(
            () => pending.MarkReversed(Guid.NewGuid(), OccurredAt.AddMinutes(2)));

        var confirmed = ReputationLedgerEntry.CreateConfirmed(
            Guid.NewGuid(),
            ReputationEntryTypes.ProfileVerification,
            ReputationReasons.EmailVerified,
            1,
            "user-service",
            "USER_EMAIL",
            "user-2",
            "user:user-2:email-verified:v1",
            OccurredAt);

        confirmed.MarkReversed(Guid.NewGuid(), OccurredAt.AddMinutes(1));
        Assert.Throws<InvalidOperationException>(
            () => confirmed.MarkReversed(Guid.NewGuid(), OccurredAt.AddMinutes(2)));
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
