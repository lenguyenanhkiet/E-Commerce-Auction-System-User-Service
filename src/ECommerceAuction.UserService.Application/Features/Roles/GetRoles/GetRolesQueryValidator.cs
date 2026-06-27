using ECommerceAuction.UserService.Domain.Entities.Users;
using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Roles.GetRoles;

public sealed class GetRolesQueryValidator : AbstractValidator<GetRolesQuery>
{
    private static readonly string[] AllowedStatuses =
        [RoleStatuses.Active, RoleStatuses.Inactive];
    private static readonly string[] AllowedSortFields =
        ["name", "code", "createdat", "updatedat"];
    private static readonly string[] AllowedDirections = ["asc", "desc"];

    public GetRolesQueryValidator()
    {
        RuleFor(query => query.Search).MaximumLength(150);

        RuleFor(query => query.Status!)
            .Must(status => AllowedStatuses.Contains(status.Trim().ToUpperInvariant()))
            .When(query => !string.IsNullOrWhiteSpace(query.Status))
            .WithMessage("Status must be ACTIVE or INACTIVE.");

        RuleFor(query => query.SortBy!)
            .Must(sortBy => AllowedSortFields.Contains(sortBy.Trim().ToLowerInvariant()))
            .When(query => !string.IsNullOrWhiteSpace(query.SortBy))
            .WithMessage("SortBy must be name, code, createdAt, or updatedAt.");

        RuleFor(query => query.SortDirection!)
            .Must(direction => AllowedDirections.Contains(direction.Trim().ToLowerInvariant()))
            .When(query => !string.IsNullOrWhiteSpace(query.SortDirection))
            .WithMessage("SortDirection must be asc or desc.");
    }
}
