using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Repositories.Sellers;

public class SellerProfileRepository : ISellerProfileRepository
{
    private static readonly string[] ActiveStatuses =
    {
        SellerApplicationStatus.Pending,
        SellerApplicationStatus.UnderReview,
        SellerApplicationStatus.Approved,
    };

    private readonly ApplicationDbContext _context;

    public SellerProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SellerProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.SellerProfiles
            .Include(s => s.History)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<SellerProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.SellerProfiles
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasActiveApplicationAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.SellerProfiles
            .AnyAsync(
                s => s.UserId == userId && ActiveStatuses.Contains(s.Status),
                cancellationToken);
    }

    public async Task AddAsync(
        SellerProfile sellerProfile,
        CancellationToken cancellationToken)
    {
        await _context.SellerProfiles.AddAsync(sellerProfile, cancellationToken);
    }

    public async Task<(IReadOnlyList<SellerProfile> Items, int TotalCount)> GetPagedAsync(
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<SellerProfile> query = _context.SellerProfiles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
