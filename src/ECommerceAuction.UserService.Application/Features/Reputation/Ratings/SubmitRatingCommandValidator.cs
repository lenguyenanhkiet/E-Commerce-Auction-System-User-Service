using FluentValidation;

namespace ECommerceAuction.UserService.Application.Features.Reputation.Ratings;

public sealed class SubmitRatingCommandValidator : AbstractValidator<SubmitRatingCommand>
{
    public SubmitRatingCommandValidator()
    {
        RuleFor(x => x.TransactionType).Must(x => x is "ORDER" or "AUCTION");
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}
