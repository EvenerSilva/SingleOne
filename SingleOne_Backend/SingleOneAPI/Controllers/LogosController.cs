using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace SingleOne.Controllers
{
    [Route("api/logos")]
    [ApiController]
    [AllowAnonymous]
    public class LogosController : ControllerBase
    {
        /// <summary>
        /// Serve a logo do cliente pelo nome do arquivo
        /// </summary>
        [HttpGet("{fileName}")]
        public IActionResult GetLogo(string fileName)
        {
            try
            {
                Console.WriteLine($"[GET-LOGO] ========== INÍCIO REQUISIÇÃO ==========");
                Console.WriteLine($"[GET-LOGO] Arquivo solicitado: {fileName}");
                
                if (string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine($"[GET-LOGO] ❌ Nome do arquivo vazio");
                    return BadRequest(new { Mensagem = "Nome do arquivo não pode ser vazio" });
                }

                // Sanitizar nome do arquivo para evitar path traversal
                var sanitizedFileName = Path.GetFileName(fileName);
                if (sanitizedFileName != fileName)
                {
                    Console.WriteLine($"[GET-LOGO] ⚠️ Tentativa de path traversal detectada: {fileName}");
                    return BadRequest(new { Mensagem = "Nome de arquivo inválido" });
                }

                var currentDir = Directory.GetCurrentDirectory();
                Console.WriteLine($"[GET-LOGO] Diretório atual: {currentDir}");
                
                var logosPath = Path.Combine(currentDir, "wwwroot", "logos");
                Console.WriteLine($"[GET-LOGO] Caminho de logos: {logosPath}");
                
                // Verificar se o diretório existe
                if (!Directory.Exists(logosPath))
                {
                    Console.WriteLine($"[GET-LOGO] ❌ Diretório de logos não existe: {logosPath}");
                    Console.WriteLine($"[GET-LOGO] Tentando criar diretório...");
                    try
                    {
                        Directory.CreateDirectory(logosPath);
                        Console.WriteLine($"[GET-LOGO] ✅ Diretório criado");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GET-LOGO] ❌ Erro ao criar diretório: {ex.Message}");
                        return StatusCode(500, new { Mensagem = "Erro ao acessar diretório de logos" });
                    }
                }
                
                var filePath = Path.Combine(logosPath, sanitizedFileName);
                Console.WriteLine($"[GET-LOGO] Caminho completo do arquivo: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"[GET-LOGO] ❌ Arquivo não encontrado: {filePath}");
                    
                    // Listar arquivos disponíveis para debug
                    if (Directory.Exists(logosPath))
                    {
                        var arquivos = Directory.GetFiles(logosPath);
                        Console.WriteLine($"[GET-LOGO] Arquivos disponíveis no diretório ({arquivos.Length}):");
                        foreach (var arquivo in arquivos.Take(10))
                        {
                            Console.WriteLine($"[GET-LOGO]   - {Path.GetFileName(arquivo)}");
                        }
                    }
                    
                    return NotFound(new { Mensagem = $"Logo não encontrada: {sanitizedFileName}" });
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = "image/png";
                
                if (sanitizedFileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                    sanitizedFileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "image/jpeg";
                }
                else if (sanitizedFileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "image/gif";
                }

                Console.WriteLine($"[GET-LOGO] ✅ Logo encontrada e servida: {sanitizedFileName} ({fileBytes.Length} bytes, tipo: {contentType})");
                Console.WriteLine($"[GET-LOGO] ========== FIM REQUISIÇÃO ==========");
                
                // Adicionar headers de cache
                Response.Headers.Add("Cache-Control", "public, max-age=3600");
                
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GET-LOGO] ❌ Erro ao servir logo: {ex.Message}");
                Console.WriteLine($"[GET-LOGO] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { Mensagem = "Erro ao servir logo: " + ex.Message });
            }
        }
    }
}

