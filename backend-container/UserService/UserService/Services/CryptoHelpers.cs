using System.Security.Cryptography;
using System.Text;

namespace UserService.Services;

public static class CryptoHelpers
{
    public static string ComputeSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
