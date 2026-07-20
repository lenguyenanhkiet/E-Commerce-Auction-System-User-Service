using System.ComponentModel.DataAnnotations;

namespace ECommerceAuction.UserService.Application.Services.IdentityMatching;

public sealed class IdentityMatchingOptions
{
    public const string SectionName = "IdentityVerification";

    /// <summary>
    /// Below this, a reading is treated as too unreliable to verify against. A QR decode scores 1.0;
    /// the OCR fallback reports Tesseract's mean confidence, typically around 0.9 on a clean photo.
    /// </summary>
    [Range(0.0, 1.0)]
    public decimal MinimumConfidence { get; init; } = 0.80m;
}
