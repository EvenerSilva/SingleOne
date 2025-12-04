using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

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
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("Nome do arquivo não pode ser vazio");
                }

                var logosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos");
                var filePath = Path.Combine(logosPath, fileName);

                Console.WriteLine($"[GET-LOGO] Buscando logo: {fileName}");
                Console.WriteLine($"[GET-LOGO] Caminho completo: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"[GET-LOGO] ❌ Arquivo não encontrado: {filePath}");
                    return NotFound(new { Mensagem = "Logo não encontrada" });
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var contentType = "image/png";
                
                if (fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                    fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "image/jpeg";
                }
                else if (fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "image/gif";
                }

                Console.WriteLine($"[GET-LOGO] ✅ Logo encontrada e servida: {fileName} ({fileBytes.Length} bytes)");
                return File(fileBytes, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GET-LOGO] ❌ Erro ao servir logo: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao servir logo: " + ex.Message });
            }
        }
    }
}

