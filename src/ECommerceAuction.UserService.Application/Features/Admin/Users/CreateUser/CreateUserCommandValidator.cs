using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(command => command.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{8,15}$")
            .WithMessage("Phone number must contain 8 to 15 digits and may start with '+'.");

        RuleFor(command => command.FullName)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(command => command.Gender)
            .MaximumLength(20)
            .When(command => !string.IsNullOrWhiteSpace(command.Gender));

        RuleFor(command => command.Address)
            .MaximumLength(500)
            .When(command => !string.IsNullOrWhiteSpace(command.Address));

        RuleFor(command => command.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth is null || dateOfBirth.Value < DateOnly.FromDateTime(DateTime.UtcNow))
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
