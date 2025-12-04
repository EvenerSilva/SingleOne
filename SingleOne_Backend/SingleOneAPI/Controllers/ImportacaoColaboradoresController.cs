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
    public class ImportacaoColaboradoresController : ControllerBase
    {
        private readonly IImportacaoColaboradoresNegocio _negocio;
        private readonly IUsuarioNegocio _usuarioNegocio;
        private readonly ILogger<ImportacaoColaboradoresController> _logger;

        public ImportacaoColaboradoresController(
            IImportacaoColaboradoresNegocio negocio,
            IUsuarioNegocio usuarioNegocio,
            ILogger<ImportacaoColaboradoresController> logger)
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
                _logger.LogError("[API-COLABORADORES] Token JWT inválido - Usuário não identificado");
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
                _logger.LogError($"[API-COLABORADORES] Usuário não encontrado - ID: {usuarioId}");
                throw new UnauthorizedAccessException("Usuário não encontrado");
            }

            return usuario.Cliente;
        }

        #endregion

        /// <summary>
        /// Upload e validação de arquivo de importação de colaboradores
        /// </summary>
        [HttpPost("Upload")]
        [RequestSizeLimit(10_485_760)] // 10MB
        public async Task<ActionResult<ResultadoValidacaoColaboradoresDTO>> UploadArquivo([FromForm] IFormFile arquivo)
        {
            try
            {
                _logger.LogInformation($"[API-COLABORADORES] Upload de arquivo de importação: {arquivo?.FileName}");

                if (arquivo == null || arquivo.Length == 0)
                {
                    return BadRequest(new { mensagem = "Nenhum arquivo foi enviado" });
                }

                var clienteId = ObterClienteId();
                var usuarioId = ObterUsuarioId();

                _logger.LogInformation($"[API-COLABORADORES] Cliente: {clienteId}, Usuário: {usuarioId}");

                var resultado = await _negocio.ProcessarArquivo(arquivo, clienteId, usuarioId);

                _logger.LogInformation($"[API-COLABORADORES] Arquivo processado - Lote: {resultado.LoteId} - Total: {resultado.TotalRegistros}");

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao processar upload: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Obtém detalhes da validação de um lote
        /// </summary>
        [HttpGet("Validacao/{loteId}")]
        public async Task<ActionResult<List<DetalheColaboradorStagingDTO>>> ObterValidacao(
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
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao obter validação: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Obtém resumo da validação de um lote
        /// </summary>
        [HttpGet("Resumo/{loteId}")]
        public async Task<ActionResult<ResumoValidacaoColaboradoresDTO>> ObterResumo(Guid loteId)
        {
            try
            {
                var clienteId = ObterClienteId();
                var resumo = await _negocio.ObterResumoValidacao(loteId, clienteId);
                return Ok(resumo);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao obter resumo: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Gera arquivo CSV com todos os erros de validação do lote
        /// </summary>
        [HttpGet("Erros/{loteId}/csv")]
        public async Task<ActionResult> ExportarErros(Guid loteId)
        {
            try
            {
                var clienteId = ObterClienteId();
                var detalhes = await _negocio.ObterDetalhesValidacao(loteId, clienteId, "E");

                if (detalhes == null || detalhes.Count == 0)
                {
                    return NotFound(new { mensagem = "Não foram encontrados erros para este lote." });
                }

                var csv = GerarCsvErros(detalhes);
                var fileName = $"erros_importacao_{loteId}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação exportar erros: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao exportar erros: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        private string GerarCsvErros(List<DetalheColaboradorStagingDTO> erros)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Linha,Nome,CPF,Matrícula,Mensagens");

            foreach (var erro in erros)
            {
                var mensagens = erro.Erros != null && erro.Erros.Count > 0
                    ? string.Join(" | ", erro.Erros)
                    : string.Join(" | ", erro.Avisos ?? new List<string>());

                mensagens = mensagens.Replace("\"", "\"\"");

                sb.AppendLine($"{erro.LinhaArquivo},\"{erro.NomeColaborador}\",\"{erro.Cpf}\",\"{erro.Matricula}\",\"{mensagens}\"");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Confirma e efetiva a importação
        /// </summary>
        [HttpPost("Confirmar/{loteId}")]
        public async Task<ActionResult<ResultadoImportacaoColaboradoresDTO>> ConfirmarImportacao(Guid loteId)
        {
            try
            {
                _logger.LogInformation($"[API-COLABORADORES] Confirmando importação - Lote: {loteId}");

                var clienteId = ObterClienteId();
                var usuarioId = ObterUsuarioId();

                var resultado = await _negocio.EfetivarImportacao(loteId, clienteId, usuarioId);

                _logger.LogInformation($"[API-COLABORADORES] Importação confirmada - Lote: {loteId} - Criados: {resultado.ColaboradoresCriados} - Atualizados: {resultado.ColaboradoresAtualizados}");

                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao confirmar importação: {ex.Message}");
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
                _logger.LogInformation($"[API-COLABORADORES] Cancelando importação - Lote: {loteId}");

                var clienteId = ObterClienteId();
                var sucesso = await _negocio.LimparStaging(loteId, clienteId);

                if (sucesso)
                {
                    _logger.LogInformation($"[API-COLABORADORES] Importação cancelada - Lote: {loteId}");
                    return Ok(new { mensagem = "Importação cancelada com sucesso" });
                }
                else
                {
                    return BadRequest(new { mensagem = "Erro ao cancelar importação" });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao cancelar importação: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Recriptografa CPFs e (opcionalmente) e-mails dos colaboradores do cliente corrente.
        /// </summary>
        [HttpPost("Manutencao/RecriptografarDocumentos")]
        public async Task<ActionResult<RecriptografarDocumentosResultadoDTO>> RecriptografarDocumentos([FromQuery] bool incluirEmails = true)
        {
            try
            {
                var clienteId = ObterClienteId();
                var usuarioId = ObterUsuarioId();

                _logger.LogInformation($"[API-COLABORADORES] Recriptografando documentos - Cliente: {clienteId} - Emails: {incluirEmails}");

                var resultado = await _negocio.RecriptografarDocumentosCliente(clienteId, usuarioId, incluirEmails);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação recriptografar documentos: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao recriptografar documentos: {ex.Message}");
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
                _logger.LogError($"[API-COLABORADORES] Erro de autenticação: {ex.Message}");
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao obter histórico: {ex.Message}");
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
                _logger.LogInformation("[API-COLABORADORES] Download de template Excel");

                var arquivo = _negocio.GerarTemplateExcel();

                return File(
                    arquivo,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"template_importacao_colaboradores_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"[API-COLABORADORES] Erro ao gerar template: {ex.Message}");
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}

