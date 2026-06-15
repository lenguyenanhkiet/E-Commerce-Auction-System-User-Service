namespace ECommerceAuction.UserService.Application.Features.Auth.RegisterAccount;

/// <summary>
/// Đạt + Kiệt - Email OTP: event raised after registration data is cached, so Email Service can send the OTP.
/// </summary>
public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FullName,
    string OtpCode,
    DateTime OtpExpiresAt);
