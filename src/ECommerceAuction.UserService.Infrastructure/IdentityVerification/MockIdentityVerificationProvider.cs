using ECommerceAuction.UserService.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Infrastructure.IdentityVerification
{
    public class MockIdentityVerificationProvider : IIdentityVerificationProvider
    {
        public Task<IdentityVerificationResult> VerifyAsync(string identityNumber, string frontImageUrl, string backImageUrl, CancellationToken cancellationToken)
        {
            var isValidFormat = !string.IsNullOrWhiteSpace(identityNumber) && identityNumber.Length == 12 && identityNumber.All(char.IsDigit);
            var result = isValidFormat
                ? new IdentityVerificationResult(true, 0.95m, ExtractedFullName: null, ExtractedDateOfBirth: null, FailureReason: null)
                : new IdentityVerificationResult(false, 0.10m, null, null, "Invalid identity number format");
            return Task.FromResult(result);
        }
    }
}
