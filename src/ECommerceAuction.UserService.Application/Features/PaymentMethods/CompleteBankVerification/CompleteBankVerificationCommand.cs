using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.PaymentMethods.CompleteBankVerification;

public sealed record CompleteBankVerificationCommand(
    IReadOnlyDictionary<string, string> Headers,
    string RawBody) : ICommand<CompleteBankVerificationResponse>;

public sealed record CompleteBankVerificationResponse(
    Guid VerificationId,
    string Status,
    bool ReputationAwarded);
