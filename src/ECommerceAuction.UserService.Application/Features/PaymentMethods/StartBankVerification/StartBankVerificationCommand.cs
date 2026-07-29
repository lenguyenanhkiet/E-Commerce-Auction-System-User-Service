using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.PaymentMethods.StartBankVerification;

public sealed record StartBankVerificationCommand(
    Guid UserId,
    string BankCode,
    string AccountNumber,
    string ExpectedAccountName,
    string CallbackUrl) : ICommand<StartBankVerificationResponse>;

public sealed record StartBankVerificationResponse(
    Guid VerificationId,
    string Status,
    string? RedirectUrl,
    DateTimeOffset? ExpiresAt);
