using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.FullName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Email)
            .EmailAddress()
            .MaximumLength(256)
            .When(command => !string.IsNullOrWhiteSpace(command.Email));

        RuleFor(command => command.Gender)
            .MaximumLength(20)
            .When(command => !string.IsNullOrWhiteSpace(command.Gender));

        RuleFor(command => command.Address)
            .MaximumLength(500)
            .When(command => !string.IsNullOrWhiteSpace(command.Address));

        RuleFor(command => command.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth is null || dateOfBirth.Value < DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime))
            .WithMessage("Date of birth must be in the past.");

        // Roles are optional; when present each code must be a non-empty string.
        RuleFor(command => command.RoleCodes)
            .Must(roleCodes => roleCodes is null || roleCodes.Count <= 10)
            .WithMessage("A user cannot be assigned more than 10 roles.");

        RuleForEach(command => command.RoleCodes)
            .NotEmpty()
            .MaximumLength(50)
            .When(command => command.RoleCodes is not null);
    }
}
