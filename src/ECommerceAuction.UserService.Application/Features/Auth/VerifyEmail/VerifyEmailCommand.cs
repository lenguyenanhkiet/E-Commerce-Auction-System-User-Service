using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Auth.VerifyEmail;

/// <summary>
/// Đạt - VerifyEmail: verifies the OTP sent during registration and creates the SQL user.
/// </summary>
public sealed record VerifyEmailCommand(
    string Email,
    string OtpCode) : ICommand<Guid>;
