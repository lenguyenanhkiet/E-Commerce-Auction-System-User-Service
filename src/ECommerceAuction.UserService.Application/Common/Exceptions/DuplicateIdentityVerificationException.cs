using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Common.Exceptions;

public sealed class DuplicateIdentityVerificationException : Exception
{
    public DuplicateIdentityVerificationException(
        Exception? innerException = null)
        : base(
            "An identity verification already exists for this user.",
            innerException)
    {
    }
}