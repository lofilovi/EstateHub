using System.Security.Cryptography;
using System.Text;

namespace EstateHub_code.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
