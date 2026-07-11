using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UpdateAddress;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty()
            .WithMessage("Address ID cannot be empty.");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Address line is required.")
            .MaximumLength(200)
            .WithMessage("Address line cannot exceed 200 characters.");

        RuleFor(x => x.Province)
            .MaximumLength(100)
            .WithMessage("Province cannot exceed 100 characters.");

        RuleFor(x => x.City)
            .MaximumLength(50)
            .WithMessage("City cannot exceed 50 characters.");

        RuleFor(x => x.Ward)
            .MaximumLength(100)
            .WithMessage("Ward cannot exceed 100 characters.");

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .WithMessage("Type cannot exceed 50 characters.");
    }

}
