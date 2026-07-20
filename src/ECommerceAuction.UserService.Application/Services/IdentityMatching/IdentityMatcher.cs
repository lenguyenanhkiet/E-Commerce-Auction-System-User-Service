namespace ECommerceAuction.UserService.Application.Services.IdentityMatching;

/// <summary>
/// What the user typed into the submission form.
/// </summary>
public sealed record IdentityDeclaration(
    string FullName,
    string Gender,
    DateOnly DateOfBirth,
    string IdentityNumber);

/// <summary>
/// What was actually read off the card image. Every field is nullable: a blurry photo yields
/// "unknown", which is never the same thing as "agrees".
/// <para>
/// Expiry date and issue place are absent by design — the CCCD QR payload does not carry them and
/// the OCR parser does not read them off the back of the card. Those two fields on the verification
/// record are user-declared and are NOT checked against the card by anything.
/// </para>
/// </summary>
public sealed record IdentityExtraction(
    string? FullName,
    string? Gender,
    DateOnly? DateOfBirth,
    string? IdentityNumber,
    decimal Confidence);

/// <summary>
/// <paramref name="FailureReason"/> is surfaced to the user, so it says what to fix.
/// </summary>
public sealed record IdentityMatchOutcome(bool IsMatch, string? FailureReason);

/// <summary>
/// Decides whether the declaration agrees with the card. Fields split into two tiers: the identity
/// number and the name must agree, while date of birth and gender are only checked when they could
/// actually be read — the parser drops them often enough that requiring them would reject honest
/// submissions.
/// </summary>
public static class IdentityMatcher
{
    public static IdentityMatchOutcome Match(
        IdentityDeclaration declared,
        IdentityExtraction extracted,
        decimal minimumConfidence)
    {
        if (extracted.Confidence < minimumConfidence)
        {
            return new IdentityMatchOutcome(
                false,
                "The identity card image was too unclear to read reliably. Please upload a sharper photo.");
        }

        var extractedNumber = TextNormalizer.NormalizeIdentityNumber(extracted.IdentityNumber);
        if (extractedNumber.Length == 0)
        {
            return new IdentityMatchOutcome(
                false,
                "The identity number could not be read from the card image.");
        }

        if (extractedNumber != TextNormalizer.NormalizeIdentityNumber(declared.IdentityNumber))
        {
            return new IdentityMatchOutcome(
                false,
                "The identity number provided does not match the one on the card.");
        }

        var extractedName = TextNormalizer.NormalizeName(extracted.FullName);
        if (extractedName.Length == 0)
        {
            return new IdentityMatchOutcome(
                false,
                "The full name could not be read from the card image.");
        }

        if (extractedName != TextNormalizer.NormalizeName(declared.FullName))
        {
            return new IdentityMatchOutcome(
                false,
                "The full name provided does not match the one on the card.");
        }

        if (extracted.DateOfBirth is { } cardDateOfBirth && cardDateOfBirth != declared.DateOfBirth)
        {
            return new IdentityMatchOutcome(
                false,
                "The date of birth provided does not match the one on the card.");
        }

        var extractedGender = TextNormalizer.NormalizeGender(extracted.Gender);
        if (extractedGender.Length > 0 &&
            extractedGender != TextNormalizer.NormalizeGender(declared.Gender))
        {
            return new IdentityMatchOutcome(
                false,
                "The gender provided does not match the one on the card.");
        }

        return new IdentityMatchOutcome(true, null);
    }
}
