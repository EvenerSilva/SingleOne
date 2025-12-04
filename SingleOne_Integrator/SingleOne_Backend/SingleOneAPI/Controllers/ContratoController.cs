using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingleOneAPI.Models.DTO;
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
    public class ContratoController : AbstractController<ContratoController>
    {
        private readonly IContratoService _contratoService;
        public ContratoController(IContratoService contratoService)
        {
            _contratoService = contratoService;
        }

        [HttpPost]
        public IActionResult CriarContrato([FromBody] CriarNovoContrato novoContrato) =>
            Execute(() => _contratoService.CriarContrato(novoContrato));

        [HttpPut]
        public IActionResult AtualizarContrato([FromBody] AtualizarContrato atualizarContrato) =>
            Execute(() => _contratoService.AtualizarContrato(atualizarContrato));

        [HttpGet("[action]/{cliente}")]
        public IActionResult Listar(int cliente) =>
            Execute(() => _contratoService.Listar(cliente));

        [HttpGet("[action]/{cliente}/{fornecedor}")]
        public IActionResult Listar(int cliente, int fornecedor) =>
            Execute(() => _contratoService.Listar(cliente, fornecedor));

        [HttpGet("[action]/{id}")]
        public IActionResult Detalhar(int id) =>
            Execute(() => _contratoService.ObterPorId(id));

        [HttpGet("[action]")]
        public IActionResult ListarStatus() =>
            Execute(() => _contratoService.ListarStatus());

        [HttpPost("[action]/{contratoId}")]
        public async Task<IActionResult> UploadArquivo(int contratoId, IFormFile arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { message = "Nenhum arquivo foi enviado." });
                }

                // Obter o ID do usuário dos claims
                Console.WriteLine($"[CONTRATO-CONTROLLER] === DEBUG CLAIMS ===");
                Console.WriteLine($"[CONTRATO-CONTROLLER] Total de claims: {User.Claims.Count()}");
                Console.WriteLine($"[CONTRATO-CONTROLLER] Claims disponíveis:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"[CONTRATO-CONTROLLER]   - {claim.Type} = {claim.Value}");
                }
                
                var userIdClaim = User.Claims.FirstOrDefault(c => 
                    c.Type == "UserId" || 
                    c.Type == ClaimTypes.NameIdentifier || 
                    c.Type == "sub")?.Value;
                
                int? usuarioId = null;
                
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedId) && parsedId > 0)
                {
                    usuarioId = parsedId;
                }
                
                Console.WriteLine($"[CONTRATO-CONTROLLER] Usuario ID encontrado: {usuarioId?.ToString() ?? "NULL"}");
                Console.WriteLine($"[CONTRATO-CONTROLLER] === FIM DEBUG CLAIMS ===");
                
                var nomeArquivo = await _contratoService.UploadArquivoContrato(contratoId, arquivo, usuarioId);
                
                return Ok(new 
                { 
                    success = true, 
                    message = "Arquivo enviado com sucesso!", 
                    fileName = nomeArquivo 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTRATO-CONTROLLER] Erro no upload: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("[action]/{contratoId}")]
        public async Task<IActionResult> DownloadArquivo(int contratoId)
        {
            try
            {
                var (fileBytes, fileName) = await _contratoService.DownloadArquivoContrato(contratoId);
                
                var contentType = fileName.EndsWith(".pdf") ? "application/pdf" :
                                  fileName.EndsWith(".docx") ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" :
                                  "application/msword";
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("[action]/{contratoId}")]
        public IActionResult RemoverArquivo(int contratoId)
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
                
                Console.WriteLine($"[CONTRATO-CONTROLLER] Remoção de arquivo - Usuario ID: {usuarioId?.ToString() ?? "NULL"}");
                
                _contratoService.RemoverArquivoContrato(contratoId, usuarioId);
                return Ok(new { success = true, message = "Arquivo removido com sucesso!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTRATO-CONTROLLER] Erro ao remover arquivo: {ex.Message}");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
