using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.PaymentMethods;
using Microsoft.Extensions.Options;

namespace ECommerceAuction.UserService.Infrastructure.PaymentMethods;

public sealed class BankVerificationProvider : IBankVerificationProvider
{
    private readonly BankVerificationOptions _options;
    private readonly BankVerificationSignatureValidator _signatureValidator;

    public BankVerificationProvider(
        IOptions<BankVerificationOptions> options,
        BankVerificationSignatureValidator signatureValidator)
    {
        _options = options.Value;
        _signatureValidator = signatureValidator;
    }

    public string ProviderCode =>
        BankVerificationProviders.IsValid(_options.ProviderCode)
            ? _options.ProviderCode
            : BankVerificationProviders.Sandbox;

    public Task<StartBankVerificationResult> StartAsync(
        StartBankVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedAccount = string.Concat(
            request.AccountNumber.Where(char.IsLetterOrDigit));
        if (normalizedAccount.Length < 4)
        {
            throw new ArgumentException("Bank account number is invalid.");
        }

        var fingerprint = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(
                $"{request.BankCode.Trim().ToUpperInvariant()}:{normalizedAccount}")));
        var masked = $"{new string('*', normalizedAccount.Length - 4)}{normalizedAccount[^4..]}";
        var reference = Guid.NewGuid().ToString("N");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(
            Math.Max(1, _options.ExpirationMinutes));

        return Task.FromResult(new StartBankVerificationResult(
            reference,
            masked,
            fingerprint,
            null,
            expiresAt));
    }

    public Task<BankVerificationCallbackResult> ValidateCallbackAsync(
        IReadOnlyDictionary<string, string> headers,
        string rawBody,
        CancellationToken cancellationToken = default)
    {
        if (!headers.TryGetValue("X-Bank-Signature", out var signature) ||
            !_signatureValidator.IsValid(
                rawBody,
                signature,
                _options.CallbackSecret))
        {
            throw new UnauthorizedAccessException(
                "Invalid bank callback signature.");
        }

        var payload = JsonSerializer.Deserialize<CallbackPayload>(
            rawBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException(
                "Invalid bank callback payload.");

        return Task.FromResult(new BankVerificationCallbackResult(
            payload.ProviderReference,
            payload.ProviderEventId,
            payload.IsVerified,
            payload.VerifiedAccountName,
            payload.FailureCode,
            payload.FailureReason,
            payload.OccurredAt.ToUniversalTime()));
    }

    private sealed record CallbackPayload(
        string ProviderReference,
        string ProviderEventId,
        bool IsVerified,
        string? VerifiedAccountName,
        string? FailureCode,
        string? FailureReason,
        DateTimeOffset OccurredAt);
}
