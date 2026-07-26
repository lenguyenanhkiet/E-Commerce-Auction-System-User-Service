using ECommerceAuction.UserService.Application.IdentityVerifications.VerifyAddress;
using ECommerceAuction.UserService.Application.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Users.Addresses;

public sealed class CompleteAddressVerificationServiceTests
{
    [Fact]
    public async Task CompleteAsync_VerifiesAddressAndAwardsOnePoint()
    {
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var occurredAt = new DateTimeOffset(
            2026, 7, 26, 6, 0, 0, TimeSpan.Zero);
        var profile = BuyerVerificationProfile.Create(userId, occurredAt);

        var verificationRepository =
            Substitute.For<IBuyerVerificationRepository>();
        verificationRepository.GetByUserIdAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(profile);

        var awardService = Substitute.For<IReputationAwardService>();
        var service = new CompleteAddressVerificationService(
            verificationRepository,
            awardService);

        await service.CompleteAsync(
            userId,
            addressId,
            occurredAt,
            CancellationToken.None);
        await service.CompleteAsync(
            userId,
            Guid.NewGuid(),
            occurredAt.AddMinutes(1),
            CancellationToken.None);

        Assert.True(profile.HasVerifiedAddress);
        Assert.Equal(occurredAt, profile.AddressVerifiedAt);
        await awardService.Received(1).AwardConfirmedAsync(
            userId,
            ReputationEntryTypes.ProfileVerification,
            ReputationReasons.AddressVerified,
            BuyerReputationPoints.AddressVerified,
            "USER_ADDRESS",
            addressId.ToString(),
            $"user:{userId}:address-verified:v1",
            occurredAt,
            Arg.Any<CancellationToken>());
    }
}
