using System;
using System.Security.Cryptography;
using System.Text;

namespace SingleOneIntegrator.Helpers
{
    /// <summary>
    /// Helper para geração e validação de assinaturas HMAC-SHA256
    /// </summary>
    public static class HmacHelper
    {
        /// <summary>
        /// Gera assinatura HMAC-SHA256
        /// </summary>
        /// <param name="timestamp">Timestamp Unix em segundos</param>
        /// <param name="body">Corpo da requisição (JSON)</param>
        /// <param name="secret">API Secret</param>
        /// <returns>Assinatura no formato sha256=...</returns>
        public static string GenerateSignature(long timestamp, string body, string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Secret não pode ser vazio", nameof(secret));

            // Payload = timestamp + . + body
            var payload = $"{timestamp}.{body}";
            
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hash = hmac.ComputeHash(payloadBytes);
                var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return $"sha256={hashString}";
            }
        }

        /// <summary>
        /// Valida assinatura HMAC-SHA256
        /// </summary>
        /// <param name="signature">Assinatura recebida</param>
        /// <param name="timestamp">Timestamp Unix em segundos</param>
        /// <param name="body">Corpo da requisição (JSON)</param>
        /// <param name="secret">API Secret</param>
        /// <returns>True se válida, False caso contrário</returns>
        public static bool ValidateSignature(string signature, long timestamp, string body, string secret)
        {
            if (string.IsNullOrWhiteSpace(signature))
                return false;

            try
            {
                var expectedSignature = GenerateSignature(timestamp, body, secret);
                
                // Comparação segura contra timing attacks
                return SecureCompare(signature, expectedSignature);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Comparação de strings segura contra timing attacks
        /// </summary>
        private static bool SecureCompare(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }

        /// <summary>
        /// Valida se o timestamp está dentro da janela aceitável (5 minutos)
        /// </summary>
        /// <param name="timestamp">Timestamp Unix em segundos</param>
        /// <param name="maxDifferenceSeconds">Diferença máxima permitida em segundos (padrão: 300 = 5 minutos)</param>
        /// <returns>True se válido, False se expirado</returns>
        public static bool ValidateTimestamp(long timestamp, int maxDifferenceSeconds = 300)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var difference = Math.Abs(now - timestamp);
            return difference <= maxDifferenceSeconds;
        }
    }
}


