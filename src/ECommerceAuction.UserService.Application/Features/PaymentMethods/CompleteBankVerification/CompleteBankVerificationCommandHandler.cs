using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Application.Features.Reputation.Services;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.PaymentMethods;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;

namespace ECommerceAuction.UserService.Application.Features.PaymentMethods.CompleteBankVerification;

public sealed class CompleteBankVerificationCommandHandler
    : ICommandHandler<CompleteBankVerificationCommand, CompleteBankVerificationResponse>
{
    private readonly IBankVerificationProvider _provider;
    private readonly IBankAccountVerificationRepository _bankRepository;
    private readonly IBuyerVerificationRepository _buyerVerificationRepository;
    private readonly IReputationAwardService _reputationAwardService;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteBankVerificationCommandHandler(
        IBankVerificationProvider provider,
        IBankAccountVerificationRepository bankRepository,
        IBuyerVerificationRepository buyerVerificationRepository,
        IReputationAwardService reputationAwardService,
        IUnitOfWork unitOfWork)
    {
        _provider = provider;
        _bankRepository = bankRepository;
        _buyerVerificationRepository = buyerVerificationRepository;
        _reputationAwardService = reputationAwardService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompleteBankVerificationResponse> Handle(
        CompleteBankVerificationCommand command,
        CancellationToken cancellationToken)
    {
        var callback = await _provider.ValidateCallbackAsync(
            command.Headers,
            command.RawBody,
            cancellationToken);
        var verification = await _bankRepository.GetByProviderReferenceAsync(
            _provider.ProviderCode,
            callback.ProviderReference,
            cancellationToken)
            ?? throw new NotFoundException("Bank verification was not found.");

        if (!callback.IsVerified)
        {
            verification.Reject(
                callback.FailureCode ?? "VERIFICATION_FAILED",
                callback.FailureReason,
                callback.OccurredAt);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new(verification.Id, verification.Status, false);
        }

        var changed = verification.MarkVerified(
            callback.VerifiedAccountName
                ?? throw new BusinessRuleException("Verified account name is missing."),
            callback.OccurredAt);

        var profile = await _buyerVerificationRepository.GetByUserIdAsync(
            verification.UserId,
            cancellationToken);
        if (profile is null)
        {
            profile = BuyerVerificationProfile.Create(
                verification.UserId,
                callback.OccurredAt);
            await _buyerVerificationRepository.AddAsync(profile, cancellationToken);
        }

        var firstPaymentVerification =
            changed && profile.VerifyPaymentMethod(callback.OccurredAt);
        var awarded = false;
        if (firstPaymentVerification)
        {
            awarded = await _reputationAwardService.AwardConfirmedAsync(
                verification.UserId,
                ReputationEntryTypes.ProfileVerification,
                ReputationReasons.PaymentMethodVerified,
                BuyerReputationPoints.PaymentMethodVerified,
                "BANK_ACCOUNT_VERIFICATION",
                verification.Id.ToString(),
                $"user:{verification.UserId}:payment-method-verified:v1",
                callback.OccurredAt,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new(verification.Id, verification.Status, awarded);
    }
}
