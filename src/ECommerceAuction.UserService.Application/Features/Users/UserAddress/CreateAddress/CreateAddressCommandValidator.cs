using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.CreateAddress;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.RecipientPhone).NotEmpty().MaximumLength(12);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Ward).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
    }
}