using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Identities.SubmitIdentityVerification
{
    public record SubmitIdentityVerificationCommand
        (
            Guid UserId,
            string IdentityNumber,
            string FrontImageUrl,
            string BackImageUrl
        ): ICommand<SubmitIdentityVerificationResponse>;
    
    public record SubmitIdentityVerificationResponse
        (
            Guid Id,
            string status,
            decimal? ConfidenceScore,
            string? RejectionReason
        );
}
