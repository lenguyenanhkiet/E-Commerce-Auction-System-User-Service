using ECommerceAuction.UserService.Domain.Common;

namespace ECommerceAuction.UserService.Domain.Authentication;

public sealed class PasswordResetToken : BaseEntity
{
    private PasswordResetToken() { }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiryDate { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Creates a new password reset token for the specified user.
    /// </summary>
    public static PasswordResetToken Create(Guid userId, string token, DateTimeOffset expiryDate)
    {
        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiryDate = expiryDate,
            IsUsed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Determines whether the password reset token has expired.
    /// </summary>
    public bool IsExpired()
    {
        return ExpiryDate < DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the password reset token as used.
    /// </summary>
    public void MarkAsUsed()
    {
        IsUsed = true;
    }
}