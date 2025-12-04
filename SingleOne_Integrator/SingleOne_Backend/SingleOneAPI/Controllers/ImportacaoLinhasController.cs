using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImportacaoLinhasController : ControllerBase
    {
        private readonly IImportacaoLinhasNegocio _negocio;
        private readonly IUsuarioNegocio _usuarioNegocio;
        private readonly ILogger<ImportacaoLinhasController> _logger;

        public ImportacaoLinhasController(
            IImportacaoLinhasNegocio negocio,
            IUsuarioNegocio usuarioNegocio,
            ILogger<ImportacaoLinhasController> logger)
        {
            _negocio = negocio;
            _usuarioNegocio = usuarioNegocio;
            _logger = logger;
        }

        #region Métodos Auxiliares

        /// <summary>
        /// Obter ID do usuário do token JWT
        /// </summary>
        private int ObterUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("[API] Token JWT inválido - Usuário não identificado");
                throw new UnauthorizedAccessException("Usuário não identificado");
            }

            return userId;
        }

        /// <summary>
        /// Obter cliente do usuário autenticado
        /// </summary>
        private int ObterClienteId()
        {
            var usuarioId = ObterUsuarioId();
            var usuario = _usuarioNegocio.BuscarPorId(usuarioId);
            
            if (usuario == null)
            {
                _logger.LogError($"[API] Usuário não encontrado - ID: {usuarioId}");
                throw new UnauthorizedAccessException("Usuário não encontrado");
            }

            return usuario.Cliente;
        }

        #endregion

        /// <summary>
        /// Upload e validação de arquivo de importação
        /// </summary>
        [HttpPost("Upload")]
        [RequestSizeLimit(10_485_760)] // 10MB
        public async Task<ActionResult<ResultadoValidacaoDTO>> UploadArquivo([FromForm] IFormFile arquivo)
        {
            try
            {
                _logger.LogInformation($"[API] Upload de arquivo de importação: {arquivo?.FileName}");

                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { mensagem = "Nenhum arquivo foi enviado" });
                }

                var clienteId = ObterClienteId();
                var usuarioId = ObterUsuarioId();

                _logger.LogInformation($"[API] Cliente: {clienteId}, Usuário: {usuarioId}");

                var resultado = await _negocio.ProcessarArquivo(arquivo, clienteId, usuarioId);

                _logger.LogInformation($"[API] Arquivo processado - Lote: {resultado.LoteId} - Total: {resultado.TotalRegistros}");
                _logger.LogInformation($"[API] 📦 Objeto resultado:");
                _logger.LogInformation($"[API] 📦   - LoteId: {resultado.LoteId}");
                _logger.LogInformation($"[API] 📦   - TotalRegistros: {resultado.TotalRegistros}");
                _logger.LogInformation($"[API] 📦   - TotalValidos: {resultado.TotalValidos}");
                _logger.LogInformation($"[API] 📦   - PodeImportar: {resultado.PodeImportar}");
                _logger.LogInformation($"[API] 📦   - Mensagem: {resultado.Mensagem}");
                _logger.LogInformation($"[API] 📦 Retornando resultado via Ok(resultado)");

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API] Erro ao processar upload: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Obtém detalhes da validação de um lote
        /// </summary>
        [HttpGet("Validacao/{loteId}")]
        public async Task<ActionResult<List<DetalheLinhaStagingDTO>>> ObterValidacao(
            Guid loteId,
            [FromQuery] string status = null)
        {
            try
            {
                var clienteId = ObterClienteId();
                var detalhes = await _negocio.ObterDetalhesValidacao(loteId, clienteId, status);
                return Ok(detalhes);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API] Erro ao obter validação: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Obtém resumo da validação de um lote
        /// </summary>
        [HttpGet("Resumo/{loteId}")]
        public async Task<ActionResult<ResumoValidacaoDTO>> ObterResumo(Guid loteId)
        {
            try
            {
                var clienteId = ObterClienteId();
                var resumo = await _negocio.ObterResumoValidacao(loteId, clienteId);
                return Ok(resumo);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API] Erro ao obter resumo: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Confirma e efetiva a importação
        /// </summary>
        [HttpPost("Confirmar/{loteId}")]
        public async Task<ActionResult<ResultadoImportacaoDTO>> ConfirmarImportacao(Guid loteId)
        {
            try
            {
                _logger.LogInformation($"[API] Confirmando importação - Lote: {loteId}");

                var clienteId = ObterClienteId();
                var usuarioId = ObterUsuarioId();

                var resultado = await _negocio.EfetivarImportacao(loteId, clienteId, usuarioId);

                _logger.LogInformation($"[API] Importação confirmada - Lote: {loteId} - Linhas: {resultado.LinhasCriadas}");

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API] Erro ao confirmar importação: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Cancela e limpa dados de staging
        /// </summary>
        [HttpDelete("Cancelar/{loteId}")]
        public async Task<ActionResult> CancelarImportacao(Guid loteId)
        {
            try
            {
                _logger.LogInformation($"[API] Cancelando importação - Lote: {loteId}");

                var clienteId = ObterClienteId();
                var sucesso = await _negocio.LimparStaging(loteId, clienteId);

                if (sucesso)
                {
                    _logger.LogInformation($"[API] Importação cancelada - Lote: {loteId}");
                    return Ok(new { mensagem = "Importação cancelada com sucesso" });
                }
                else
                {
                    return BadRequest(new { mensagem = "Erro ao cancelar importação" });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API] Erro ao cancelar importação: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Obtém histórico de importações
        /// </summary>
        [HttpGet("Historico")]
        public async Task<ActionResult<List<HistoricoImportacaoDTO>>> ObterHistorico([FromQuery] int? limite = 50)
        {
            try
            {
                var clienteId = ObterClienteId();
                var historico = await _negocio.ObterHistorico(clienteId, limite);
                return Ok(historico);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API] Erro ao obter histórico: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Download do template Excel para importação
        /// </summary>
        [HttpGet("Template")]
        [AllowAnonymous] // Permitir download sem autenticação
        public ActionResult BaixarTemplate()
        {
            try
            {
                _logger.LogInformation("[API] Download de template Excel");

                var arquivo = _negocio.GerarTemplateExcel();

                return File(
                    arquivo,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"template_importacao_linhas_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API] Erro ao gerar template: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}

