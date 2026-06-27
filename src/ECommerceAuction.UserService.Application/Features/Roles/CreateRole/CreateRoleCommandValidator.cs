using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Roles.CreateRole;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9_\\.]+$")
            .WithMessage("Role code may contain only letters, numbers, underscores, and dots.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Description)
            .MaximumLength(500);

        RuleFor(command => command.PrivilegeCodes)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(privilegeCodes => privilegeCodes.Count <= 100)
            .WithMessage("PrivilegeCodes cannot contain more than 100 items.");

        RuleForEach(command => command.PrivilegeCodes)
            .NotEmpty()
            .MaximumLength(100);
    }
}
