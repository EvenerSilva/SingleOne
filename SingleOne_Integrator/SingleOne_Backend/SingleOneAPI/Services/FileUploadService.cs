using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly string _uploadPath;

        public FileUploadService(IConfiguration configuration)
        {
            _configuration = configuration;
            _uploadPath = _configuration["FileUpload:LogoPath"] ?? "wwwroot/logos";
            
            // Criar diretório se não existir
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadLogoAsync(IFormFile file, int clienteId)
        {
            Console.WriteLine($"[FILE-UPLOAD] Iniciando upload para cliente {clienteId}");
            Console.WriteLine($"[FILE-UPLOAD] Arquivo: {file?.FileName}, Tamanho: {file?.Length} bytes");
            
            if (file == null || file.Length == 0)
            {
                Console.WriteLine("[FILE-UPLOAD] ❌ Arquivo inválido ou vazio");
                throw new ArgumentException("Arquivo inválido");
            }

            // Validar tipo de arquivo
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            Console.WriteLine($"[FILE-UPLOAD] Extensão do arquivo: {fileExtension}");
            
            if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
            {
                Console.WriteLine($"[FILE-UPLOAD] ❌ Extensão não permitida: {fileExtension}");
                throw new ArgumentException("Tipo de arquivo não permitido. Use apenas JPG, PNG ou GIF");
            }

            // Validar tamanho (máximo 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                Console.WriteLine($"[FILE-UPLOAD] ❌ Arquivo muito grande: {file.Length} bytes");
                throw new ArgumentException("Arquivo muito grande. Tamanho máximo: 5MB");
            }

            Console.WriteLine($"[FILE-UPLOAD] ✅ Validações passaram");
            Console.WriteLine($"[FILE-UPLOAD] Diretório de upload: {_uploadPath}");

            // Gerar nome único para o arquivo
            var fileName = $"cliente_{clienteId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);
            
            Console.WriteLine($"[FILE-UPLOAD] Nome do arquivo: {fileName}");
            Console.WriteLine($"[FILE-UPLOAD] Caminho completo: {filePath}");

            // Salvar arquivo
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                Console.WriteLine($"[FILE-UPLOAD] ✅ Arquivo salvo com sucesso em: {filePath}");
                
                // Verificar se o arquivo foi realmente criado
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    Console.WriteLine($"[FILE-UPLOAD] ✅ Arquivo confirmado: {fileInfo.Length} bytes");
                }
                else
                {
                    Console.WriteLine($"[FILE-UPLOAD] ❌ Arquivo não foi criado!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILE-UPLOAD] ❌ Erro ao salvar arquivo: {ex.Message}");
                throw;
            }

            return fileName;
        }

        public bool DeleteLogo(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var filePath = Path.Combine(_uploadPath, fileName);
            
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public string GetLogoPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return $"/logos/{fileName}";
        }

        public async Task<string> UploadFileAsync(IFormFile file, string pasta)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Arquivo inválido");

            // Criar diretório específico para a pasta
            var pastaPath = Path.Combine(_uploadPath, pasta);
            if (!Directory.Exists(pastaPath))
            {
                Directory.CreateDirectory(pastaPath);
            }

            // Gerar nome único para o arquivo
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(pastaPath, fileName);

            // Salvar arquivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<byte[]> DownloadFileAsync(string fileName, string pasta)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            var pastaPath = Path.Combine(_uploadPath, pasta);
            var filePath = Path.Combine(pastaPath, fileName);

            if (!File.Exists(filePath))
                return null;

            return await File.ReadAllBytesAsync(filePath);
        }

        public bool DeleteFile(string fileName, string pasta)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            var pastaPath = Path.Combine(_uploadPath, pasta);
            var filePath = Path.Combine(pastaPath, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public string GetFilePath(string fileName, string pasta)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;

            return $"/{pasta}/{fileName}";
        }
    }
}
