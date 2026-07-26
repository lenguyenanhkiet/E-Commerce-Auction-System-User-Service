using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.PaymentMethods;

namespace ECommerceAuction.UserService.Application.Features.PaymentMethods.StartBankVerification;

public sealed class StartBankVerificationCommandHandler
    : ICommandHandler<StartBankVerificationCommand, StartBankVerificationResponse>
{
    private readonly IBankVerificationProvider _provider;
    private readonly IBankAccountVerificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public StartBankVerificationCommandHandler(
        IBankVerificationProvider provider,
        IBankAccountVerificationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _provider = provider;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StartBankVerificationResponse> Handle(
        StartBankVerificationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _provider.StartAsync(
            new Abstractions.Services.StartBankVerificationRequest(
                command.UserId,
                command.BankCode,
                command.AccountNumber,
                command.ExpectedAccountName,
                command.CallbackUrl),
            cancellationToken);

        if (await _repository.ExistsVerifiedFingerprintAsync(
                result.AccountFingerprint,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "This bank account has already been verified.");
        }

        var now = DateTimeOffset.UtcNow;
        var verification = BankAccountVerification.CreatePending(
            command.UserId,
            _provider.ProviderCode,
            result.ProviderReference,
            command.BankCode,
            result.MaskedAccountNumber,
            result.AccountFingerprint,
            command.ExpectedAccountName,
            now,
            result.ExpiresAt);

        await _repository.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new(
            verification.Id,
            verification.Status,
            result.RedirectUrl,
            verification.ExpiresAt);
    }
}
