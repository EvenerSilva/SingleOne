using System;
using System.Security.Cryptography;
using System.Text;

namespace SingleOneAPI.Util
{
    public class SerialNumberGenerator
    {
        /// <summary>
        /// Gera um número de série ou patrimônio único e aleatório.
        /// </summary>
        /// <param name="prefix">Prefixo para o número de série.</param>
        /// <param name="length">Comprimento do número de série gerado.</param>
        /// <returns>Número de série ou patrimônio gerado.</returns>
        public static string GenerateSerialNumber(string prefix, int length = 12)
        {
            string randomPart = GenerateRandomString(length);
            return $"{prefix}-{randomPart}";
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    result.Append(chars[(int)(num % (uint)chars.Length)]);
                }
            }

            return result.ToString();
        }
    }
}
