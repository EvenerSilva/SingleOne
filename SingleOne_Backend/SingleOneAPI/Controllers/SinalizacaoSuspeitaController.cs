using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    /// <summary>
    /// Controller para gerenciar sinalizações de suspeitas na portaria
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requer autenticação para todas as operações
    public class SinalizacaoSuspeitaController : ControllerBase
    {
        private readonly ISinalizacaoSuspeitaNegocio _negocio;
        private readonly IIpAddressService _ipAddressService;

        public SinalizacaoSuspeitaController(ISinalizacaoSuspeitaNegocio negocio, IIpAddressService ipAddressService)
        {
            _negocio = negocio;
            _ipAddressService = ipAddressService;
        }

        /// <summary>
        /// Criar nova sinalização de suspeita (usado pelo vigilante da portaria)
        /// </summary>
        [HttpPost("criar")]
        public async Task<ActionResult<SinalizacaoCriadaDTO>> CriarSinalizacao([FromBody] CriarSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Criando nova sinalização para colaborador ID: {dto.ColaboradorId}");
                
                // Capturar dados da requisição
                dto.IpAddress = _ipAddressService.GetClientIpAddress(Request.HttpContext);
                dto.UserAgent = Request.Headers["User-Agent"].ToString();
                
                var resultado = await _negocio.CriarSinalizacaoAsync(dto);
                
                if (resultado.Sucesso)
                {
                    Console.WriteLine($"[SINALIZACAO] Sinalização criada com sucesso - ID: {resultado.SinalizacaoId}");
                    return Ok(resultado);
                }
                else
                {
                    Console.WriteLine($"[SINALIZACAO] Erro ao criar sinalização: {resultado.Mensagem}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro interno: {ex.Message}");
                return StatusCode(500, new SinalizacaoCriadaDTO
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor"
                });
            }
        }

        /// <summary>
        /// Listar sinalizações com filtros e paginação
        /// </summary>
        [HttpPost("listar")]
        public async Task<ActionResult<SinalizacoesPaginadasDTO>> ListarSinalizacoes([FromBody] FiltroSinalizacoesDTO filtros)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Listando sinalizações com filtros: {filtros.Status}");
                
                var resultado = await _negocio.ListarSinalizacoesAsync(filtros);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao listar: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obter detalhes de uma sinalização específica
        /// </summary>
        [HttpGet("detalhes/{id}")]
        public async Task<ActionResult<SinalizacaoDetalhesDTO>> ObterDetalhes(int id)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Obtendo detalhes da sinalização ID: {id}");
                
                var resultado = await _negocio.ObterDetalhesAsync(id);
                
                if (resultado != null)
                {
                    return Ok(resultado);
                }
                else
                {
                    return NotFound("Sinalização não encontrada");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao obter detalhes: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Atualizar status de uma sinalização
        /// </summary>
        [HttpPut("atualizar-status")]
        public async Task<ActionResult> AtualizarStatus([FromBody] AtualizarStatusSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Atualizando status da sinalização ID: {dto.SinalizacaoId} para: {dto.Status}");
                
                var resultado = await _negocio.AtualizarStatusAsync(dto);
                
                if (resultado)
                {
                    return Ok(new { Mensagem = "Status atualizado com sucesso" });
                }
                else
                {
                    return BadRequest("Erro ao atualizar status");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao atualizar status: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Resolver uma sinalização (finalizar investigação)
        /// </summary>
        [HttpPut("resolver")]
        public async Task<ActionResult> ResolverSinalizacao([FromBody] ResolverSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Resolvendo sinalização ID: {dto.SinalizacaoId}");
                
                var resultado = await _negocio.ResolverSinalizacaoAsync(dto);
                
                if (resultado)
                {
                    return Ok(new { Mensagem = "Sinalização resolvida com sucesso" });
                }
                else
                {
                    return BadRequest("Erro ao resolver sinalização");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao resolver: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obter motivos de suspeita disponíveis
        /// </summary>
        [HttpGet("motivos")]
        public async Task<ActionResult<List<MotivoSuspeitaDTO>>> ObterMotivosSuspeita()
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Obtendo motivos de suspeita");
                
                var motivos = await _negocio.ObterMotivosSuspeitaAsync();
                return Ok(motivos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao obter motivos: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Obter estatísticas de sinalizações
        /// </summary>
        [HttpGet("estatisticas")]
        public async Task<ActionResult<EstatisticasSinalizacoesDTO>> ObterEstatisticas()
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Obtendo estatísticas");
                
                var estatisticas = await _negocio.ObterEstatisticasAsync();
                return Ok(estatisticas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao obter estatísticas: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Atribuir sinalização para investigador
        /// </summary>
        [HttpPut("atribuir")]
        public async Task<ActionResult> AtribuirInvestigador([FromBody] AtualizarStatusSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Atribuindo sinalização ID: {dto.SinalizacaoId} para investigador ID: {dto.InvestigadorId}");
                
                var resultado = await _negocio.AtribuirInvestigadorAsync(dto.SinalizacaoId, dto.InvestigadorId ?? 0);
                
                if (resultado)
                {
                    return Ok(new { Mensagem = "Sinalização atribuída com sucesso" });
                }
                else
                {
                    return BadRequest("Erro ao atribuir sinalização");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao atribuir: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        /// <summary>
        /// Arquivar sinalização (marcar como arquivada)
        /// </summary>
        [HttpPut("arquivar/{id}")]
        public async Task<ActionResult> ArquivarSinalizacao(int id)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO] Arquivando sinalização ID: {id}");
                
                var resultado = await _negocio.ArquivarSinalizacaoAsync(id);
                
                if (resultado)
                {
                    return Ok(new { Mensagem = "Sinalização arquivada com sucesso" });
                }
                else
                {
                    return BadRequest("Erro ao arquivar sinalização");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO] Erro ao arquivar: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }
    }
}
