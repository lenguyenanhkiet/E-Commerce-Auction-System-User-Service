namespace ECommerceAuction.UserService.Application.Common.Options;

/// <summary>
/// Configurable account security policy: login lockout and password expiry.
/// Bound from the "AccountPolicy" configuration section.
/// </summary>
public sealed class AccountPolicyOptions
{
    public const string SectionName = "AccountPolicy";

    /// <summary>
    /// Number of consecutive failed logins before the account is temporarily locked.
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// How long an account stays locked after too many failed logins, in minutes.
    /// </summary>
    public int LockoutMinutes { get; set; } = 15;

    /// <summary>
    /// How often the unlock sweeper runs, in seconds.
    /// </summary>
    public int UnlockSweepIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Password age (in days) after which the user is required to change their password.
    /// </summary>
    public int PasswordExpiryDays { get; set; } = 30;

    /// <summary>
    /// How often the password-expiry sweeper runs, in hours.
    /// </summary>
    public int PasswordExpirySweepIntervalHours { get; set; } = 24;
}
