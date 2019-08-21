using System;
using System.Security.Cryptography;
using System.Text;
using DataAccess;

namespace Services.Utils
{
    public class PasswordUtils
    {
        public static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public static bool IsValidPassword(User user, string password)
        {
            return Convert.ToBase64String(GenerateSaltedHash(Encoding.UTF8.GetBytes(password), Convert.FromBase64String(user.PasswordSalt))) == user.PasswordHash;
        }

        public static byte[] GenerateSalt()
        {
            var randomProdiver = new RNGCryptoServiceProvider();
            var salt = new byte[20];
            randomProdiver.GetBytes(salt);

            return salt;
        }
    }
}
