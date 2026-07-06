using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Users.VerifyPhoneOtp;

/// <summary>
/// Validates data submitted to the verify-phone-otp API.
/// </summary>
public sealed class VerifyPhoneOtpCommandValidator
    : AbstractValidator<VerifyPhoneOtpCommand>
{
    public VerifyPhoneOtpCommandValidator()
    {
        RuleFor(command => command.OtpCode)
            .NotEmpty()
            .WithMessage("OTP code cannot be empty.")
            .Matches(@"^[0-9]{6}$")
            .WithMessage("OTP code must be exactly 6 digits.");
    }
}
