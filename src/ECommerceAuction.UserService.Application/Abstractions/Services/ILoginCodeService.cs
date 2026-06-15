namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Issues and consumes short-lived one-time login codes used between the backend OAuth callback and the frontend.
/// </summary>
public interface ILoginCodeService
{
    /// <summary>
    /// Creates a short-lived code that can later be exchanged for system tokens.
    /// </summary>
    Task<string> CreateAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates a login code, consumes it so it cannot be reused, and returns the linked user identifier.
    /// </summary>
    Task<Guid> ValidateAndConsumeAsync(string code, CancellationToken cancellationToken);
}
