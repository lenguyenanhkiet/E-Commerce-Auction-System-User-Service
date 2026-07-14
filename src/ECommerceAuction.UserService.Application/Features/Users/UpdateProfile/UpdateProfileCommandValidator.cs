using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Users.UpdateProfile;

/// <summary>
/// Validates data submitted to the update-profile API.
/// </summary>
public sealed class UpdateProfileCommandValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number cannot be empty.")
            .Matches(@"^[0-9]{9,15}$")
            .WithMessage("Phone number must contain between 9 and 15 digits.");

        RuleFor(command => command.NewEmail)
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters.")
            .When(command => !string.IsNullOrWhiteSpace(command.NewEmail));
    }
}