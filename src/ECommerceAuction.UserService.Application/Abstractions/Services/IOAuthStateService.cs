namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Manages one-time OAuth state values used to protect the Google login redirect flow.
/// </summary>
public interface IOAuthStateService
{
    /// <summary>
    /// Creates a temporary state value before redirecting the browser to Google.
    /// </summary>
    Task<string> CreateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Validates and consumes a returned state value so it cannot be replayed.
    /// </summary>
    Task<bool> ValidateAndConsumeAsync(string state, CancellationToken cancellationToken);
}
