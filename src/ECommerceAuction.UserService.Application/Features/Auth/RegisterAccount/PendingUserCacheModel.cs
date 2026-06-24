namespace ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;

/// <summary>
///Pass - RegisterAccount: temporary user data stored in Redis until the email OTP is verified.
/// </summary>
public sealed record PendingUserCacheModel(
    Guid Id,
    string Email,
    string PhoneNumber,
    string FullName,
    string PasswordHash,
    string OtpCode,
    DateTime ExpiresAt);
