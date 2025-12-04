using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SingleOneAPI.Models.ViewModels;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciar protocolos de descarte
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProtocoloDescarteController : ControllerBase
    {
        private readonly IProtocoloDescarteNegocio _protocoloNegocio;

        public ProtocoloDescarteController(IProtocoloDescarteNegocio protocoloNegocio)
        {
            _protocoloNegocio = protocoloNegocio;
        }

        /// <summary>
        /// Listar protocolos de descarte do cliente
        /// </summary>
        [HttpGet("listar/{clienteId}")]
        public async Task<IActionResult> ListarProtocolos(int clienteId, [FromQuery] bool incluirInativos = false)
        {
            try
            {
                var protocolos = await _protocoloNegocio.ListarProtocolos(clienteId, incluirInativos);
                return Ok(protocolos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Obter protocolo específico
        /// </summary>
        [HttpGet("{protocoloId}")]
        public async Task<IActionResult> ObterProtocolo(int protocoloId)
        {
            try
            {
                var protocolo = await _protocoloNegocio.ObterProtocolo(protocoloId);
                if (protocolo == null)
                    return NotFound(new { mensagem = "Protocolo não encontrado" });

                return Ok(protocolo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Criar novo protocolo de descarte
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CriarProtocolo([FromBody] ProtocoloDescarteVM protocolo)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var novoProtocolo = await _protocoloNegocio.CriarProtocolo(protocolo, usuarioId);
                return CreatedAtAction(nameof(ObterProtocolo), new { protocoloId = novoProtocolo.Id }, novoProtocolo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Atualizar protocolo existente
        /// </summary>
        [HttpPut("{protocoloId}")]
        public async Task<IActionResult> AtualizarProtocolo(int protocoloId, [FromBody] ProtocoloDescarteVM protocolo)
        {
            try
            {
                if (protocoloId != protocolo.Id)
                    return BadRequest(new { mensagem = "ID do protocolo não confere" });

                var usuarioId = ObterUsuarioId();
                var protocoloAtualizado = await _protocoloNegocio.AtualizarProtocolo(protocolo, usuarioId);
                return Ok(protocoloAtualizado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Adicionar equipamento ao protocolo
        /// </summary>
        [HttpPost("{protocoloId}/equipamentos/{equipamentoId}")]
        public async Task<IActionResult> AdicionarEquipamento(int protocoloId, int equipamentoId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var item = await _protocoloNegocio.AdicionarEquipamento(protocoloId, equipamentoId, usuarioId);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Remover equipamento do protocolo
        /// </summary>
        [HttpDelete("{protocoloId}/equipamentos/{equipamentoId}")]
        public async Task<IActionResult> RemoverEquipamento(int protocoloId, int equipamentoId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var sucesso = await _protocoloNegocio.RemoverEquipamento(protocoloId, equipamentoId, usuarioId);
                
                if (!sucesso)
                    return NotFound(new { mensagem = "Equipamento não encontrado no protocolo" });

                return Ok(new { mensagem = "Equipamento removido com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Atualizar um processo específico de um item
        /// </summary>
        [HttpPut("itens/{itemId}/processo/{processo}")]
        public async Task<IActionResult> AtualizarProcessoItem(int itemId, string processo, [FromBody] AtualizarProcessoItemRequest request)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var item = await _protocoloNegocio.AtualizarProcessoItem(itemId, processo, request.Valor, usuarioId);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Atualizar processo de um equipamento no protocolo
        /// </summary>
        [HttpPut("itens/{itemId}/processo")]
        public async Task<IActionResult> AtualizarProcessoEquipamento(int itemId, [FromBody] AtualizarProcessoRequest request)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var item = await _protocoloNegocio.AtualizarProcessoEquipamento(
                    itemId, 
                    request.Sanitizacao, 
                    request.Descaracterizacao, 
                    request.Perfuracao, 
                    request.Evidencias, 
                    usuarioId);

                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Atualizar campo específico de um item (método sanitização, ferramenta, observações)
        /// </summary>
        [HttpPut("itens/{itemId}/campo/{campo}")]
        public async Task<IActionResult> AtualizarCampoItem(int itemId, string campo, [FromBody] AtualizarCampoRequest request)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                await _protocoloNegocio.AtualizarCampoItem(itemId, campo, request.Valor, usuarioId);
                return Ok(new { mensagem = "Campo atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Finalizar protocolo
        /// </summary>
        [HttpPost("{protocoloId}/finalizar")]
        public async Task<IActionResult> FinalizarProtocolo(int protocoloId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var protocolo = await _protocoloNegocio.FinalizarProtocolo(protocoloId, usuarioId);
                return Ok(protocolo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Cancelar protocolo
        /// </summary>
        [HttpPost("{protocoloId}/cancelar")]
        public async Task<IActionResult> CancelarProtocolo(int protocoloId)
        {
            try
            {
                var usuarioId = ObterUsuarioId();
                var sucesso = await _protocoloNegocio.CancelarProtocolo(protocoloId, usuarioId);
                
                if (!sucesso)
                    return NotFound(new { mensagem = "Protocolo não encontrado" });

                return Ok(new { mensagem = "Protocolo cancelado com sucesso" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Listar equipamentos disponíveis para adicionar ao protocolo
        /// </summary>
        [HttpGet("equipamentos-disponiveis/{clienteId}")]
        public async Task<IActionResult> ListarEquipamentosDisponiveis(int clienteId, [FromQuery] string filtro = "")
        {
            try
            {
                var equipamentos = await _protocoloNegocio.ListarEquipamentosDisponiveis(clienteId, filtro);
                return Ok(equipamentos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Obter estatísticas do protocolo
        /// </summary>
        [HttpGet("{protocoloId}/estatisticas")]
        public async Task<IActionResult> ObterEstatisticas(int protocoloId)
        {
            try
            {
                var estatisticas = await _protocoloNegocio.ObterEstatisticas(protocoloId);
                if (estatisticas == null)
                    return NotFound(new { mensagem = "Protocolo não encontrado" });

                return Ok(estatisticas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Validar se protocolo pode ser finalizado
        /// </summary>
        [HttpGet("{protocoloId}/validar-finalizacao")]
        public async Task<IActionResult> ValidarFinalizacao(int protocoloId)
        {
            try
            {
                var podeFinalizar = await _protocoloNegocio.ValidarFinalizacaoProtocolo(protocoloId);
                return Ok(new { podeFinalizar });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Gerar número de protocolo (para teste)
        /// </summary>
        [HttpGet("gerar-numero-protocolo")]
        public async Task<IActionResult> GerarNumeroProtocolo()
        {
            try
            {
                var numero = await _protocoloNegocio.GerarNumeroProtocolo();
                return Ok(new { protocolo = numero });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Gerar documento PDF de descarte
        /// </summary>
        [HttpGet("{protocoloId}/documento")]
        public async Task<IActionResult> GerarDocumentoDescarte(int protocoloId)
        {
            try
            {
                Console.WriteLine($"🎯 [CONTROLLER] GerarDocumentoDescarte chamado para protocolo ID: {protocoloId}");
                
                var usuarioId = ObterUsuarioId();
                Console.WriteLine($"👤 [CONTROLLER] Usuario ID: {usuarioId}");
                
                var pdfBytes = await _protocoloNegocio.GerarDocumentoDescarte(protocoloId, usuarioId);
                Console.WriteLine($"📄 [CONTROLLER] PDF gerado: {pdfBytes.Length} bytes");
                
                var protocolo = await _protocoloNegocio.ObterProtocolo(protocoloId);
                var nomeArquivo = $"DESCARTE_{protocolo.Protocolo}.pdf";
                Console.WriteLine($"✅ [CONTROLLER] Retornando arquivo: {nomeArquivo}");

                return File(pdfBytes, "application/pdf", nomeArquivo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [CONTROLLER] ERRO ao gerar documento:");
                Console.WriteLine($"   Mensagem: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   InnerException: {ex.InnerException.Message}");
                    Console.WriteLine($"   InnerStackTrace: {ex.InnerException.StackTrace}");
                }
                return BadRequest(new { mensagem = ex.Message, detalhes = ex.StackTrace });
            }
        }

        #region Métodos Auxiliares

        /// <summary>
        /// Obter ID do usuário do token
        /// </summary>
        private int ObterUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("Usuário não identificado");

            return userId;
        }

        #endregion
    }

    /// <summary>
    /// Request para atualizar processo de equipamento
    /// </summary>
    public class AtualizarProcessoRequest
    {
        public bool Sanitizacao { get; set; }
        public bool Descaracterizacao { get; set; }
        public bool Perfuracao { get; set; }
        public bool Evidencias { get; set; }
    }

    /// <summary>
    /// Request para atualizar um processo específico de um item
    /// </summary>
    public class AtualizarProcessoItemRequest
    {
        public bool Valor { get; set; }
    }

    /// <summary>
    /// Request para atualizar um campo específico de um item
    /// </summary>
    public class AtualizarCampoRequest
    {
        public string Valor { get; set; }
    }
}
