using System.Security.Cryptography;
using System.Text;

namespace Jumia_Clone.Helpers
{
    public static class PasswordHelpers
    {
        public static string HashPassword(string password)
        {
            byte[] fixedSalt = new byte[64];
            for (int i = 0; i < fixedSalt.Length; i++)
            {
                fixedSalt[i] = 1;
            }

            using var hmac = new HMACSHA512(fixedSalt);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            var hashBytes = new byte[fixedSalt.Length + hash.Length];
            Array.Copy(fixedSalt, 0, hashBytes, 0, fixedSalt.Length);
            Array.Copy(hash, 0, hashBytes, fixedSalt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Decode the stored hash
                var hashBytes = Convert.FromBase64String(storedHash);

                // Extract salt (first 64 bytes)
                var salt = new byte[64];
                Array.Copy(hashBytes, 0, salt, 0, 64);

                // Hash the input password with the extracted salt
                using var hmac = new HMACSHA512(salt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Compare the computed hash with the stored hash
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != hashBytes[64 + i])
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
