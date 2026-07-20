using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Services.IdentityMatching;

namespace ECommerceAuction.UserService.Infrastructure.IdentityVerification;

/// <summary>
/// Development stand-in that reads no image at all. It reports a fixed card, so a submission only
/// passes matching when the user happens to declare exactly these values — it cannot tell a real
/// card from a forged one. Wiring this outside Development makes verification meaningless.
/// </summary>
public sealed class MockIdentityVerificationProvider : IIdentityVerificationProvider
{
    public Task<IdentityExtractionResult> ExtractAsync(
        string frontImageKey,
        string backImageKey,
        CancellationToken cancellationToken)
    {
        var extraction = new IdentityExtraction(
            FullName: "Lê Nguyễn Anh Kiệt",
            Gender: "Nam",
            DateOfBirth: new DateOnly(2004, 12, 8),
            IdentityNumber: "079204001234",
            Confidence: 0.95m);

        return Task.FromResult(new IdentityExtractionResult(
            Success: true,
            Extraction: extraction,
            ExtractedIssueDate: new DateOnly(2021, 12, 26),
            ExtractedPermanentAddress: "Quận 1, TP. Hồ Chí Minh",
            FailureReason: null));
    }
}
