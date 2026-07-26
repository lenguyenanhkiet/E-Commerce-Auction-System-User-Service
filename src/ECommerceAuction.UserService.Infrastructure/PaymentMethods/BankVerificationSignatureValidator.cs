using System.Security.Cryptography;
using System.Text;

namespace ECommerceAuction.UserService.Infrastructure.PaymentMethods;

public sealed class BankVerificationSignatureValidator
{
    public bool IsValid(string body, string signature, string secret)
    {
        if (string.IsNullOrWhiteSpace(signature) ||
            string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        try
        {
            var expected = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(secret),
                Encoding.UTF8.GetBytes(body));
            return CryptographicOperations.FixedTimeEquals(
                expected,
                Convert.FromHexString(signature));
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
