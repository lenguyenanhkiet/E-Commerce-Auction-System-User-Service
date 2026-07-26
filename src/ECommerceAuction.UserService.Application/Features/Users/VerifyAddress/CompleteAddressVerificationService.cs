using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;

namespace ECommerceAuction.UserService.Application.Features.Users.VerifyAddress;

public sealed class CompleteAddressVerificationService
{
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;
    private readonly IReputationAwardService _reputationAwardService;

    public CompleteAddressVerificationService(
        IBuyerVerificationRepository buyerVerificationRepository,
        IReputationAwardService reputationAwardService)
    {
        _buyerVerificationRepository = buyerVerificationRepository;
        _reputationAwardService = reputationAwardService;
    }

    public async Task CompleteAsync(
        Guid userId,
        Guid addressId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken = default)
    {
        var verificationProfile =
            await _buyerVerificationRepository.GetByUserIdAsync(
                userId,
                cancellationToken);

        if (verificationProfile is null)
        {
            verificationProfile = BuyerVerificationProfile.Create(
                userId,
                occurredAt);

            await _buyerVerificationRepository.AddAsync(
                verificationProfile,
                cancellationToken);
        }

        if (!verificationProfile.VerifyAddress(occurredAt))
        {
            return;
        }

        await _reputationAwardService.AwardConfirmedAsync(
            userId: userId,
            entryType: ReputationEntryTypes.ProfileVerification,
            reason: ReputationReasons.AddressVerified,
            points: BuyerReputationPoints.AddressVerified,
            sourceType: "USER_ADDRESS",
            sourceId: addressId.ToString(),
            idempotencyKey: $"user:{userId}:address-verified:v1",
            occurredAt: occurredAt,
            cancellationToken: cancellationToken);
    }
}
