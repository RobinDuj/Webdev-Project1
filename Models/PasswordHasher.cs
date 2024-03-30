using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace projectfiets.Models
{
    public class PasswordHasher
    {
        private const int KeySize = 32;

        public static string HashPassword(string password)
        {
            byte[] hashed = KeyDerivation.Pbkdf2(
                password: password,
                salt: new byte[0],
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: KeySize);

            string hash = Convert.ToBase64String(hashed);
            return hash;
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: new byte[0],
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: KeySize);

            return hashBytes.SequenceEqual(computedHash);
        }
    }
}
