using ECommerceAuction.UserService.Domain.PaymentMethods;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.PaymentMethods;

public sealed class BankAccountVerificationRepository
    : IBankAccountVerificationRepository
{
    private readonly ApplicationDbContext _context;

    public BankAccountVerificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<BankAccountVerification?> GetByIdAsync(
        Guid verificationId,
        CancellationToken cancellationToken = default) =>
        _context.BankAccountVerifications.FirstOrDefaultAsync(
            x => x.Id == verificationId,
            cancellationToken);

    public Task<BankAccountVerification?> GetByProviderReferenceAsync(
        string provider,
        string providerReference,
        CancellationToken cancellationToken = default) =>
        _context.BankAccountVerifications.FirstOrDefaultAsync(
            x => x.Provider == provider &&
                 x.ProviderReference == providerReference,
            cancellationToken);

    public Task<bool> ExistsVerifiedFingerprintAsync(
        string accountFingerprint,
        CancellationToken cancellationToken = default) =>
        _context.BankAccountVerifications.AnyAsync(
            x => x.AccountFingerprint == accountFingerprint &&
                 x.Status == BankAccountVerificationStatuses.Verified,
            cancellationToken);

    public Task<bool> HasVerifiedAccountAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _context.BankAccountVerifications.AnyAsync(
            x => x.UserId == userId &&
                 x.Status == BankAccountVerificationStatuses.Verified,
            cancellationToken);

    public async Task<IReadOnlyList<BankAccountVerification>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context.BankAccountVerifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        BankAccountVerification verification,
        CancellationToken cancellationToken = default) =>
        await _context.BankAccountVerifications.AddAsync(
            verification,
            cancellationToken);
}
