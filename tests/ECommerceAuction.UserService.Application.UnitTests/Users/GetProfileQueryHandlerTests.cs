using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.Users.GetProfile;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Users;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.Users;

public sealed class GetProfileQueryHandlerTests
{
    [Fact]
    public async Task Handle_AggregatesAccountAndBuyerVerificationState()
    {
        var user = new User(
            "verified@example.com",
            "HASHED",
            "Verified User",
            "0901234567");
        user.ConfirmPhoneChange();
        user.VerifyIdentity(
            "Verified User",
            "MALE",
            new DateOnly(1990, 1, 1));

        var occurredAt = new DateTimeOffset(
            2026, 7, 26, 4, 0, 0, TimeSpan.Zero);
        var buyerVerification = BuyerVerificationProfile.Create(
            user.Id,
            occurredAt);
        buyerVerification.VerifyEmail(occurredAt);
        buyerVerification.VerifyPhone(occurredAt);
        buyerVerification.VerifyIdentity(occurredAt);
        buyerVerification.VerifyAddress(occurredAt);
        buyerVerification.VerifyPaymentMethod(occurredAt);

        var identityVerification = new IdentityVerification(
            user.Id,
            user.FullName,
            "MALE",
            new DateOnly(1990, 1, 1),
            "012345678901",
            new DateOnly(2020, 1, 1),
            new DateOnly(2030, 1, 1),
            "Police Department",
            "Ho Chi Minh City",
            "front-key",
            "back-key");
        identityVerification.Verify(0.99m);

        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(user.Id);

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var oauthRepository = Substitute.For<IUserOAuthRepository>();
        oauthRepository.GetActiveRoleCodesAsync(
                user.Id,
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());
        oauthRepository.GetActivePrivilegeCodesAsync(
                user.Id,
                Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());

        var identityRepository =
            Substitute.For<IIdentityVerificationRepository>();
        identityRepository.GetByUserIdAsync(
                user.Id,
                Arg.Any<CancellationToken>())
            .Returns(identityVerification);
        var addressRepository = Substitute.For<IAddressRepository>();
        addressRepository.GetUserAddressesAsync(
                user.Id,
                Arg.Any<CancellationToken>())
            .Returns(new List<Address>());

        var reputationRepository =
            Substitute.For<IBuyerReputationRepository>();
        var buyerVerificationRepository =
            Substitute.For<IBuyerVerificationRepository>();
        buyerVerificationRepository.GetByUserIdAsync(
                user.Id,
                Arg.Any<CancellationToken>())
            .Returns(buyerVerification);

        var handler = new GetProfileQueryHandler(
            currentUser,
            userRepository,
            oauthRepository,
            identityRepository,
            addressRepository,
            reputationRepository,
            buyerVerificationRepository);

        var result = await handler.Handle(
            new GetProfileQuery(),
            CancellationToken.None);

        Assert.True(result.Verification.Email.IsVerified);
        Assert.True(result.Verification.Phone.IsVerified);
        Assert.True(result.Verification.Identity.IsVerified);
        Assert.Equal(
            buyerVerification.IdentityVerifiedAt,
            result.Verification.Identity.VerifiedAt);
        Assert.True(result.Verification.Address.IsVerified);
        Assert.True(result.Verification.PaymentMethod.IsVerified);
        Assert.True(result.Verification.IsFullyVerified);

        // Backward compatibility for existing frontend consumers.
        Assert.True(result.IsEmailConfirmed);
        Assert.True(result.IsPhoneConfirmed);
    }
}
