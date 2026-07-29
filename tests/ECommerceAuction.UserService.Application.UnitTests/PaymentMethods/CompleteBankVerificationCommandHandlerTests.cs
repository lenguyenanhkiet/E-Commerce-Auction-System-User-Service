using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Features.PaymentMethods.CompleteBankVerification;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.PaymentMethods;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using NSubstitute;

namespace ECommerceAuction.UserService.Application.UnitTests.PaymentMethods;

public sealed class CompleteBankVerificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_VerifiesPaymentAndAwardsTwoPointsOnlyOnce()
    {
        var now = new DateTimeOffset(2026, 7, 26, 8, 0, 0, TimeSpan.Zero);
        var userId = Guid.NewGuid();
        var verification = BankAccountVerification.CreatePending(
            userId,
            BankVerificationProviders.Sandbox,
            "provider-reference",
            "VCB",
            "******1234",
            "fingerprint",
            "LE NGUYEN ANH KIET",
            now.AddMinutes(-1),
            now.AddMinutes(10));
        var profile = BuyerVerificationProfile.Create(userId, now.AddMinutes(-1));

        var provider = Substitute.For<IBankVerificationProvider>();
        provider.ProviderCode.Returns(BankVerificationProviders.Sandbox);
        provider.ValidateCallbackAsync(
                Arg.Any<IReadOnlyDictionary<string, string>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new BankVerificationCallbackResult(
                "provider-reference",
                "event-1",
                true,
                "LE NGUYEN ANH KIET",
                null,
                null,
                now));

        var bankRepository = Substitute.For<IBankAccountVerificationRepository>();
        bankRepository.GetByProviderReferenceAsync(
                BankVerificationProviders.Sandbox,
                "provider-reference",
                Arg.Any<CancellationToken>())
            .Returns(verification);
        var verificationRepository = Substitute.For<IBuyerVerificationRepository>();
        verificationRepository.GetByUserIdAsync(
                userId,
                Arg.Any<CancellationToken>())
            .Returns(profile);
        var awardService = Substitute.For<IReputationAwardService>();
        awardService.AwardConfirmedAsync(
                userId,
                ReputationEntryTypes.ProfileVerification,
                ReputationReasons.PaymentMethodVerified,
                BuyerReputationPoints.PaymentMethodVerified,
                Arg.Any<string>(),
                Arg.Any<string>(),
                $"user:{userId}:payment-method-verified:v1",
                now,
                Arg.Any<CancellationToken>())
            .Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var sut = new CompleteBankVerificationCommandHandler(
            provider,
            bankRepository,
            verificationRepository,
            awardService,
            unitOfWork);
        var command = new CompleteBankVerificationCommand(
            new Dictionary<string, string>(),
            "{}");

        var first = await sut.Handle(command, CancellationToken.None);
        var second = await sut.Handle(command, CancellationToken.None);

        Assert.True(first.ReputationAwarded);
        Assert.False(second.ReputationAwarded);
        Assert.True(profile.HasVerifiedPaymentMethod);
        await awardService.Received(1).AwardConfirmedAsync(
            userId,
            ReputationEntryTypes.ProfileVerification,
            ReputationReasons.PaymentMethodVerified,
            2,
            Arg.Any<string>(),
            Arg.Any<string>(),
            $"user:{userId}:payment-method-verified:v1",
            now,
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(2).SaveChangesAsync(
            Arg.Any<CancellationToken>());
    }
}
