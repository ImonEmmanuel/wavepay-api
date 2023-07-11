using System;
using System.Security.Cryptography;

namespace PaywaveAPICore.Extension
{
    public class GenerateStringExtension
    {
        public static string GenerateRandomBase64(int length = 32)
        {
            byte[] buffer = new byte[Convert.ToInt32(length)];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buffer);
            }

            return Convert.ToBase64String(buffer);
        }
    }
}
