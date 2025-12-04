using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Jwt
{
    public class JwtSettings
    {
        // Chave de 64 caracteres (512 bits) para máxima segurança com JWT Bearer 6.0+
        // Mínimo exigido: 32 caracteres (256 bits)
        public static string Secret = "SingleOne2025!@#SecureJWTKey$%^ForAuthentication&*()PlusExtra";
    }
}
