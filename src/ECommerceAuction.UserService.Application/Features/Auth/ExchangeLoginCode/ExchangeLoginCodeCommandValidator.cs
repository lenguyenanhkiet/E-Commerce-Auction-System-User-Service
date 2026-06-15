using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Auth.ExchangeLoginCode;

/// <summary>
/// Validates the one-time login code exchange request.
/// </summary>
public sealed class ExchangeLoginCodeCommandValidator : AbstractValidator<ExchangeLoginCodeCommand>
{
    public ExchangeLoginCodeCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty()
            .MaximumLength(200);
    }
}
