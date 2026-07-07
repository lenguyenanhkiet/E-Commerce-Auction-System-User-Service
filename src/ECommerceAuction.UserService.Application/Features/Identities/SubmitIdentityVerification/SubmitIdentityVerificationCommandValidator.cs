using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Identities.SubmitIdentityVerification;

public sealed class SubmitIdentityVerificationCommandValidator : AbstractValidator<SubmitIdentityVerificationCommand>
{
    public SubmitIdentityVerificationCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.IdentityNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.FrontImageUrl)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeAValidUrl)
            .WithMessage("FrontImageUrl must be a valid absolute URL.");

        RuleFor(command => command.BackImageUrl)
            .NotEmpty()
            .MaximumLength(500)
            .Must(BeAValidUrl)
            .WithMessage("BackImageUrl must be a valid absolute URL.");
    }

    private static bool BeAValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out _);
}
