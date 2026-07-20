using System.Globalization;
using System.Text;

namespace ECommerceAuction.UserService.Application.Services.IdentityMatching;

/// <summary>
/// Folds the cosmetic differences out of card text before it is compared. OCR and humans disagree
/// constantly on accents, casing and spacing, and none of those differences mean "different person".
/// </summary>
public static class TextNormalizer
{
    /// <summary>
    /// Upper-cases, strips accents and collapses runs of whitespace to a single space.
    /// </summary>
    public static string NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var folded = RemoveDiacritics(value).ToUpperInvariant();
        return string.Join(' ', folded.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Reduces "Nữ"/"nữ"/" Nữ " to a single comparable token.
    /// </summary>
    public static string NormalizeGender(string? value) => NormalizeName(value);

    /// <summary>
    /// Keeps only the digits, so " 079 201.001-234 " and "079201001234" compare equal.
    /// </summary>
    public static string NormalizeIdentityNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var digits = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsDigit(ch))
            {
                digits.Append(ch);
            }
        }

        return digits.ToString();
    }

    /// <summary>
    /// Folds accents one character at a time. đ/Đ are mapped by hand: unlike every other accented
    /// Vietnamese letter they have no canonical decomposition, so FormD leaves them untouched.
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            if (ch is 'đ')
            {
                builder.Append('d');
            }
            else if (ch is 'Đ')
            {
                builder.Append('D');
            }
            else
            {
                var decomposed = ch.ToString().Normalize(NormalizationForm.FormD);
                if (CharUnicodeInfo.GetUnicodeCategory(decomposed[0]) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(decomposed[0]);
                }
            }
        }

        return builder.ToString();
    }
}
