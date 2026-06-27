using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Roles.UpdateRole;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(command => command.RoleId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.Description).MaximumLength(500);
        RuleFor(command => command.PrivilegeCodes)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(privilegeCodes => privilegeCodes.Count <= 100)
            .WithMessage("PrivilegeCodes cannot contain more than 100 items.");
        RuleForEach(command => command.PrivilegeCodes).NotEmpty().MaximumLength(100);
    }
}
