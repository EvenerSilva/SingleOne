using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SingleOne.Models;
using SingleOneAPI.Negocios.Interfaces;

namespace SingleOneAPI.Services
{
    public interface ITwoFactorService
    {
        Task<bool> IsTwoFactorEnabledAsync(int userId);
        Task<bool> IsTwoFactorRequiredAsync(int userId);
        Task<string> GenerateTOTPSecretAsync();
        Task<string> GenerateTOTPCodeAsync(string secret);
        Task<bool> ValidateTOTPCodeAsync(string secret, string code);
        Task<List<string>> GenerateBackupCodesAsync();
        Task<bool> ValidateBackupCodeAsync(int userId, string backupCode);
        Task<bool> EnableTwoFactorAsync(int userId, string secret, List<string> backupCodes);
        Task<bool> DisableTwoFactorAsync(int userId);
        Task<string> GenerateEmailCodeAsync();
        Task<bool> ValidateEmailCodeAsync(string storedCode, string inputCode);
        Task<bool> SendTwoFactorEmailAsync(string email, string code, string template);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private readonly IUsuarioNegocio _usuarioNegocio;
        private readonly Random _random;

        public TwoFactorService(IUsuarioNegocio usuarioNegocio)
        {
            _usuarioNegocio = usuarioNegocio;
            _random = new Random();
        }

        public async Task<bool> IsTwoFactorEnabledAsync(int userId)
        {
            try
            {
                var usuario = await Task.Run(() => _usuarioNegocio.BuscarPorId(userId));
                return usuario?.TwoFactorEnabled == true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsTwoFactorRequiredAsync(int userId)
        {
            try
            {
                var usuario = await Task.Run(() => _usuarioNegocio.BuscarPorId(userId));
                return usuario?.TwoFactorEnabled == true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GenerateTOTPSecretAsync()
        {
            return await Task.Run(() =>
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
                var secret = new StringBuilder();
                
                for (int i = 0; i < 32; i++)
                {
                    secret.Append(chars[_random.Next(chars.Length)]);
                }
                
                return secret.ToString();
            });
        }

        public async Task<string> GenerateTOTPCodeAsync(string secret)
        {
            return await Task.Run(() =>
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30; // 30 segundos por código
                var timestampBytes = BitConverter.GetBytes(timestamp);
                
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(timestampBytes);

                var secretBytes = Encoding.UTF8.GetBytes(secret);
                var hash = new HMACSHA1(secretBytes);
                var hashBytes = hash.ComputeHash(timestampBytes);
                
                var offset = hashBytes[hashBytes.Length - 1] & 0xf;
                var code = ((hashBytes[offset] & 0x7f) << 24) |
                           ((hashBytes[offset + 1] & 0xff) << 16) |
                           ((hashBytes[offset + 2] & 0xff) << 8) |
                           (hashBytes[offset + 3] & 0xff);
                
                return (code % 1000000).ToString("D6");
            });
        }

        public async Task<bool> ValidateTOTPCodeAsync(string secret, string code)
        {
            try
            {
                var generatedCode = await GenerateTOTPCodeAsync(secret);
                return generatedCode == code;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GenerateBackupCodesAsync()
        {
            return await Task.Run(() =>
            {
                var codes = new List<string>();
                for (int i = 0; i < 10; i++)
                {
                    codes.Add(_random.Next(100000, 999999).ToString());
                }
                return codes;
            });
        }

        public async Task<bool> ValidateBackupCodeAsync(int userId, string backupCode)
        {
            try
            {
                var usuario = await Task.Run(() => _usuarioNegocio.BuscarPorId(userId));
                if (usuario?.TwoFactorBackupCodes == null)
                    return false;

                var backupCodes = usuario.TwoFactorBackupCodes.Split(',').ToList();
                var isValid = backupCodes.Contains(backupCode);

                if (isValid)
                {
                    // Remover o código usado
                    backupCodes.Remove(backupCode);
                    usuario.TwoFactorBackupCodes = string.Join(",", backupCodes);
                    await Task.Run(() => _usuarioNegocio.Salvar(usuario));
                }

                return isValid;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EnableTwoFactorAsync(int userId, string secret, List<string> backupCodes)
        {
            try
            {
                var usuario = await Task.Run(() => _usuarioNegocio.BuscarPorId(userId));
                if (usuario == null)
                    return false;

                usuario.TwoFactorEnabled = true;
                usuario.TwoFactorSecret = secret;
                usuario.TwoFactorBackupCodes = string.Join(",", backupCodes);

                var result = await Task.Run(() => _usuarioNegocio.Salvar(usuario));
                return result.Contains("200");
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DisableTwoFactorAsync(int userId)
        {
            try
            {
                var usuario = await Task.Run(() => _usuarioNegocio.BuscarPorId(userId));
                if (usuario == null)
                    return false;

                usuario.TwoFactorEnabled = false;
                usuario.TwoFactorSecret = null;
                usuario.TwoFactorBackupCodes = null;

                var result = await Task.Run(() => _usuarioNegocio.Salvar(usuario));
                return result.Contains("200");
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GenerateEmailCodeAsync()
        {
            return await Task.Run(() => _random.Next(100000, 999999).ToString());
        }

        public async Task<bool> ValidateEmailCodeAsync(string storedCode, string inputCode)
        {
            return await Task.Run(() => storedCode == inputCode);
        }

        public async Task<bool> SendTwoFactorEmailAsync(string email, string code, string template)
        {
            try
            {
                // Aqui você implementaria o envio do email usando o serviço SMTP existente
                // Por enquanto, vamos apenas simular o sucesso
                await Task.Delay(100); // Simular delay de envio
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
