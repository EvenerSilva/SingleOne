using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingleOneAPI.Services.Interface;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotaFiscalController : ControllerBase
    {
        private readonly INotaFiscalService _notaFiscalService;

        public NotaFiscalController(INotaFiscalService notaFiscalService)
        {
            _notaFiscalService = notaFiscalService;
        }

        [HttpPost("[action]/{notaFiscalId}")]
        public async Task<IActionResult> UploadArquivo(int notaFiscalId, IFormFile arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { message = "Nenhum arquivo foi enviado." });
                }

                // Obter o ID do usuário dos claims
                var userIdClaim = User.Claims.FirstOrDefault(c => 
                    c.Type == "UserId" || 
                    c.Type == ClaimTypes.NameIdentifier || 
                    c.Type == "sub")?.Value;
                
                int? usuarioId = null;
                
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId) && parsedId > 0)
                {
                    usuarioId = parsedId;
                }
                
                Console.WriteLine($"[NOTAFISCAL-CONTROLLER] Upload de arquivo - Usuario ID: {usuarioId?.ToString() ?? "NULL"}");
                
                var nomeArquivo = await _notaFiscalService.UploadArquivoNotaFiscal(notaFiscalId, arquivo, usuarioId);
                
                return Ok(new 
                { 
                    success = true, 
                    message = "Arquivo enviado com sucesso!", 
                    fileName = nomeArquivo 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAFISCAL-CONTROLLER] Erro no upload: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("[action]/{notaFiscalId}")]
        public async Task<IActionResult> DownloadArquivo(int notaFiscalId)
        {
            try
            {
                var (fileBytes, fileName) = await _notaFiscalService.DownloadArquivoNotaFiscal(notaFiscalId);
                
                var contentType = fileName.EndsWith(".pdf") ? "application/pdf" :
                                  fileName.EndsWith(".xml") ? "application/xml" :
                                  fileName.EndsWith(".png") ? "image/png" :
                                  fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg") ? "image/jpeg" :
                                  "application/octet-stream";
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAFISCAL-CONTROLLER] Erro no download: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("[action]/{notaFiscalId}")]
        public IActionResult RemoverArquivo(int notaFiscalId)
        {
            try
            {
                // Obter o ID do usuário dos claims
                var userIdClaim = User.Claims.FirstOrDefault(c => 
                    c.Type == "UserId" || 
                    c.Type == ClaimTypes.NameIdentifier || 
                    c.Type == "sub")?.Value;
                
                int? usuarioId = null;
                
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId) && parsedId > 0)
                {
                    usuarioId = parsedId;
                }
                
                Console.WriteLine($"[NOTAFISCAL-CONTROLLER] Remoção de arquivo - Usuario ID: {usuarioId?.ToString() ?? "NULL"}");
                
                _notaFiscalService.RemoverArquivoNotaFiscal(notaFiscalId, usuarioId);
                return Ok(new { success = true, message = "Arquivo removido com sucesso!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NOTAFISCAL-CONTROLLER] Erro ao remover arquivo: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

