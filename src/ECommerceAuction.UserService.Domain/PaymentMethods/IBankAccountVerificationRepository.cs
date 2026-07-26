using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.PaymentMethods;

public interface IBankAccountVerificationRepository
{
    Task<BankAccountVerification?> GetByIdAsync(
        Guid verificationId,
        CancellationToken cancellationToken = default);

    Task<BankAccountVerification?> GetByProviderReferenceAsync(
        string provider,
        string providerReference,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsVerifiedFingerprintAsync(
        string accountFingerprint,
        CancellationToken cancellationToken = default);

    Task<bool> HasVerifiedAccountAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BankAccountVerification>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BankAccountVerification verification,
        CancellationToken cancellationToken = default);
}