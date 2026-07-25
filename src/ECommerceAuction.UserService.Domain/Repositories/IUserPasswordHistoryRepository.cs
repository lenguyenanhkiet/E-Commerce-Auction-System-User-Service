using ECommerceAuction.UserService.Domain.Users;

namespace ECommerceAuction.UserService.Domain.Repositories;

/// <summary>
/// Repository contract for storing and querying a user's previously used password hashes.
/// </summary>
public interface IUserPasswordHistoryRepository
{
    /// <summary>
    /// Gets the most recent <paramref name="count"/> password hashes for a user, newest first.
    /// </summary>
    Task<IReadOnlyList<string>> GetRecentHashesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new password-history entry.
    /// </summary>
    Task AddAsync(
        UserPasswordHistory entry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Keeps only the newest <paramref name="keep"/> entries for a user and removes the rest.
    /// </summary>
    Task PruneOldEntriesAsync(
        Guid userId,
        int keep,
        CancellationToken cancellationToken = default);
}
