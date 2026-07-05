using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Abstractions.Services
{
    public interface IIdentityVerificationProvider
    {
        Task<IdentityVerificationResult> VerifyAsync(string identityNumber, string frontImageUrl, string backImageUrl, CancellationToken cancellationToken);
    }
    public sealed record IdentityVerificationResult(bool IsMatch, decimal ConfidenceScore, string? ExtractedFullName, DateOnly? ExtractedDateOfBirth, string? FailureReason);
}
