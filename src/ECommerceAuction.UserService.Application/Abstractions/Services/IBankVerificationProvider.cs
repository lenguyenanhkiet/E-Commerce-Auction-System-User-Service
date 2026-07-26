using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Abstractions.Services;

public interface IBankVerificationProvider
{
    string ProviderCode { get; }

    Task<StartBankVerificationResult> StartAsync(
        StartBankVerificationRequest request,
        CancellationToken cancellationToken = default);

    Task<BankVerificationCallbackResult> ValidateCallbackAsync(
        IReadOnlyDictionary<string, string> headers,
        string rawBody,
        CancellationToken cancellationToken = default);
}

public sealed record StartBankVerificationRequest(
    Guid UserId,
    string BankCode,
    string AccountNumber,
    string ExpectedAccountName,
    string CallbackUrl);

public sealed record StartBankVerificationResult(
    string ProviderReference,
    string MaskedAccountNumber,
    string AccountFingerprint,
    string? RedirectUrl,
    DateTimeOffset? ExpiresAt);

public sealed record BankVerificationCallbackResult(
    string ProviderReference,
    string ProviderEventId,
    bool IsVerified,
    string? VerifiedAccountName,
    string? FailureCode,
    string? FailureReason,
    DateTimeOffset OccurredAt);