using Microsoft.IdentityModel.Tokens;
using SingleOne.Models;
using SingleOne.Util;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SingleOne.Jwt
{
    public class JwtTokenService
    {
        public static string GenerateToken(Usuario user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID do usuário
                    new Claim("UserId", user.Id.ToString()), // Claim customizado para facilitar acesso
                    new Claim(ClaimTypes.Name, user.Nome),
                    new Claim(ClaimTypes.Email, user.Email),
                }),
                Expires = TimeZoneMapper.GetDateTimeNow().AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public static void RevokeToken(string token)
        {
            //TODO: Finalizar método
            var tokenHandler = new JwtSecurityTokenHandler();
            var res = tokenHandler.ReadJwtToken(token);
            var res2 = tokenHandler.ReadToken(token);
        }
    }
}
