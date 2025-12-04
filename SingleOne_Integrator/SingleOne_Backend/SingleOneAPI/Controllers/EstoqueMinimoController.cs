using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstoqueMinimoController : ControllerBase
    {
        private readonly IEstoqueMinimoNegocio _estoqueMinimoNegocio;

        public EstoqueMinimoController(IEstoqueMinimoNegocio estoqueMinimoNegocio)
        {
            _estoqueMinimoNegocio = estoqueMinimoNegocio;
        }

        /// <summary>
        /// Endpoint de teste para verificar se o controller está funcionando
        /// </summary>
        [HttpGet("teste")]
        public ActionResult<string> Teste()
        {
            return Ok("EstoqueMinimoController está funcionando!");
        }

        // =====================================================
        // ENDPOINTS PARA EQUIPAMENTOS
        // =====================================================

        /// <summary>
        /// Lista todas as configurações de estoque mínimo de equipamentos
        /// </summary>
        [HttpGet("equipamentos/{clienteId}")]
        public async Task<ActionResult<List<EstoqueMinimoEquipamentoDTO>>> ListarEquipamentos(int clienteId)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.ListarEquipamentosComDadosCalculados(clienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Busca configuração de estoque mínimo de equipamento por ID
        /// </summary>
        [HttpGet("equipamentos/buscar/{id}")]
        public async Task<ActionResult<EstoqueMinimoEquipamento>> BuscarEquipamento(int id)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.BuscarEquipamento(id);
                if (result == null)
                    return NotFound(new { Mensagem = "Configuração não encontrada", Status = "404" });
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Salva ou atualiza configuração de estoque mínimo de equipamento
        /// </summary>
        [HttpPost("equipamentos")]
        public async Task<ActionResult> SalvarEquipamento([FromBody] EstoqueMinimoEquipamento estoqueMinimo)
        {
            try
            {
                await _estoqueMinimoNegocio.SalvarEquipamento(estoqueMinimo);
                return Ok(new { Mensagem = "Configuração salva com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Exclui configuração de estoque mínimo de equipamento
        /// </summary>
        [HttpDelete("equipamentos/{id}")]
        public async Task<ActionResult> ExcluirEquipamento(int id)
        {
            try
            {
                await _estoqueMinimoNegocio.ExcluirEquipamento(id);
                return Ok(new { Mensagem = "Configuração excluída com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        // =====================================================
        // ENDPOINTS PARA LINHAS TELEFÔNICAS
        // =====================================================

        /// <summary>
        /// Lista todas as configurações de estoque mínimo de linhas telefônicas
        /// </summary>
        [HttpGet("linhas/{clienteId}")]
        public async Task<ActionResult<List<EstoqueMinimoLinha>>> ListarLinhas(int clienteId)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.ListarLinhas(clienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Busca configuração de estoque mínimo de linha por ID
        /// </summary>
        [HttpGet("linhas/buscar/{id}")]
        public async Task<ActionResult<EstoqueMinimoLinha>> BuscarLinha(int id)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.BuscarLinha(id);
                if (result == null)
                    return NotFound(new { Mensagem = "Configuração não encontrada", Status = "404" });
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Salva ou atualiza configuração de estoque mínimo de linha
        /// </summary>
        [HttpPost("linhas")]
        public async Task<ActionResult> SalvarLinha([FromBody] EstoqueMinimoLinha estoqueMinimo)
        {
            try
            {
                await _estoqueMinimoNegocio.SalvarLinha(estoqueMinimo);
                return Ok(new { Mensagem = "Configuração salva com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Exclui configuração de estoque mínimo de linha
        /// </summary>
        [HttpDelete("linhas/{id}")]
        public async Task<ActionResult> ExcluirLinha(int id)
        {
            try
            {
                await _estoqueMinimoNegocio.ExcluirLinha(id);
                return Ok(new { Mensagem = "Configuração excluída com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        // =====================================================
        // ENDPOINTS PARA RELATÓRIOS E ALERTAS
        // =====================================================

        /// <summary>
        /// Lista alertas de estoque baixo
        /// </summary>
        [HttpGet("alertas/{clienteId}")]
        public async Task<ActionResult<List<EstoqueAlertaVM>>> ListarAlertas(int clienteId)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.ListarAlertas(clienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Lista alertas específicos de equipamentos
        /// </summary>
        [HttpGet("alertas/equipamentos/{clienteId}")]
        public async Task<ActionResult<List<EstoqueEquipamentoAlertaVM>>> ListarAlertasEquipamentos(int clienteId)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.ListarAlertasEquipamentos(clienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Lista alertas específicos de linhas telefônicas
        /// </summary>
        [HttpGet("alertas/linhas/{clienteId}")]
        public async Task<ActionResult<List<EstoqueLinhaAlertaVM>>> ListarAlertasLinhas(int clienteId)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.ListarAlertasLinhas(clienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }

        /// <summary>
        /// Conta total de alertas por cliente
        /// </summary>
        [HttpGet("alertas/contar/{clienteId}")]
        public async Task<ActionResult<int>> ContarAlertas(int clienteId)
        {
            try
            {
                var result = await _estoqueMinimoNegocio.ContarAlertas(clienteId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
        }
    }
}
