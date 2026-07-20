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

        RuleFor(command => command.FrontImageKey)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(command => command.BackImageKey)
            .NotEmpty()
            .MaximumLength(500);
    }
}