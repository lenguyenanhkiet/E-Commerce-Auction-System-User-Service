using ECommerceAuction.UserService.Application.Services.IdentityMatching;

namespace ECommerceAuction.UserService.Application.Abstractions.Services;

/// <summary>
/// Reads an identity card image. It only reports what the card says — deciding whether that agrees
/// with the user's declaration is <see cref="IdentityMatcher"/>'s job, so the matching policy stays
/// in one place instead of being re-implemented by every provider.
/// </summary>
public interface IIdentityVerificationProvider
{
    /// <summary>
    /// Reads the card behind <paramref name="frontImageKey"/>. Only the front is read: the CCCD QR
    /// code and the printed fields the parser understands are both on the front, so the back is
    /// stored as evidence only.
    /// </summary>
    Task<IdentityExtractionResult> ExtractAsync(
        string frontImageKey,
        string backImageKey,
        CancellationToken cancellationToken);
}

/// <summary>
/// <paramref name="Success"/> means the image was read, NOT that it matches the declaration — feed
/// <paramref name="Extraction"/> to <see cref="IdentityMatcher"/> for that.
/// <para>
/// Expiry date and issue place are absent by design: the CCCD QR payload carries neither, and the
/// OCR parser does not read the back of the card. The matching fields on the verification record are
/// user-declared and UNVERIFIED — nothing checks them against the card.
/// </para>
/// </summary>
public sealed record IdentityExtractionResult(
    bool Success,
    IdentityExtraction? Extraction,
    DateOnly? ExtractedIssueDate,
    string? ExtractedPermanentAddress,
    string? FailureReason)
{
    public static IdentityExtractionResult Failed(string reason) =>
        new(false, null, null, null, reason);
}
