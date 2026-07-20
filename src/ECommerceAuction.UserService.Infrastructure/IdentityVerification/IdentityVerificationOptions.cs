using System.ComponentModel.DataAnnotations;

namespace ECommerceAuction.UserService.Infrastructure.IdentityVerification;

public sealed class IdentityVerificationOptions
{
    public const string SectionName = "IdentityVerification";

    /// <summary>
    /// When true the mock provider is used, which reads nothing and passes any 12-digit number.
    /// Development only — leaving this on in production means anybody is "verified".
    /// </summary>
    public bool UseMockProvider { get; init; }

    /// <summary>
    /// Directory holding vie.traineddata + eng.traineddata. Empty means "tessdata" next to the
    /// assembly, which is where the Nexus.Ocr package copies them.
    /// </summary>
    public string TessDataPath { get; init; } = string.Empty;
}
