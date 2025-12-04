using System;
using System.Security.Cryptography;
using System.Text;

namespace SingleOneIntegrator.Helpers
{
    /// <summary>
    /// Gerador de API Keys e Secrets
    /// </summary>
    public static class ApiKeyGenerator
    {
        /// <summary>
        /// Gera uma API Key (pública)
        /// </summary>
        /// <param name="isProduction">Se true, usa prefixo sk_live_, se false, sk_test_</param>
        /// <returns>API Key no formato sk_live_... ou sk_test_...</returns>
        public static string GenerateApiKey(bool isProduction = false)
        {
            var prefix = isProduction ? "sk_live_" : "sk_test_";
            var randomPart = GenerateRandomString(32);
            return prefix + randomPart;
        }

        /// <summary>
        /// Gera um API Secret (privado, para HMAC)
        /// </summary>
        /// <returns>API Secret no formato whsec_...</returns>
        public static string GenerateApiSecret()
        {
            var prefix = "whsec_";
            var randomPart = GenerateRandomString(40);
            return prefix + randomPart;
        }

        /// <summary>
        /// Gera string aleatória criptograficamente segura
        /// </summary>
        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new StringBuilder(length);

            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[sizeof(uint)];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    var num = BitConverter.ToUInt32(buffer, 0);
                    result.Append(chars[(int)(num % (uint)chars.Length)]);
                }
            }

            return result.ToString();
        }
    }
}


