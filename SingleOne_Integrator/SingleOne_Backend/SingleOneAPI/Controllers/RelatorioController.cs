using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOneAPI.Models.ViewModels;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RelatorioController : ControllerBase
    {
        private readonly IRelatorioNegocio _negocio;
        private readonly ISinalizacaoSuspeitaNegocio _sinalizacaoNegocio;
        private readonly IUsuarioNegocio _usuarioNegocio;
        
        public RelatorioController(IRelatorioNegocio negocio, ISinalizacaoSuspeitaNegocio sinalizacaoNegocio, IUsuarioNegocio usuarioNegocio)
        {
            _negocio = negocio;
            _sinalizacaoNegocio = sinalizacaoNegocio;
            _usuarioNegocio = usuarioNegocio;
        }

        [HttpGet("[action]/{id}", Name ="HistoricoEquipamento")]
        public List<Equipamentohistoricovm> HistoricoEquipamento(int id)
        {
            return _negocio.HistoricoEquipamento(id);
        }

        // ✅ NOVO: Histórico por número de série (resolve conflito de IDs entre equipamentos e linhas)
        [HttpGet("[action]/{numeroSerie}", Name ="HistoricoEquipamentoPorNumeroSerie")]
        public List<Equipamentohistoricovm> HistoricoEquipamentoPorNumeroSerie(string numeroSerie)
        {
            return _negocio.HistoricoEquipamentoPorNumeroSerie(numeroSerie);
        }
        [HttpGet("[action]/{id}", Name ="EquipamentoComColaboradores")]
        public List<RequisicaoVM> EquipamentoComColaboradores(int id)
        {
            return _negocio.EquipamentosComColaboradores(id);
        }
        [HttpGet("[action]/{cliente}/{pagina}/{relatorio}/{pesquisa}", Name ="MovimentacoesColaboradores")]
        public MovimentacoesVM MovimentacoesColaboradores(int cliente, int pagina, string relatorio, string pesquisa)
        {
            return _negocio.MovimentacoesColaboradores(cliente, pagina, relatorio, pesquisa);
        }
        [HttpPost("[action]", Name = "ConsultarDetalhesEquipamentos")]
        public List<Vwequipamentosdetalhe> ConsultarDetalhesEquipamentos(Vwequipamentosdetalhe vw)
        {
            return _negocio.ConsultarDetalhesEquipamentos(vw);
        }


        [HttpGet("[action]/{cliente}", Name = "DashboardMobile")]
        public DashboardMobileVM DashboardMobile(int cliente)
        {
            return _negocio.DashboardMobile(cliente);
        }
        [HttpGet("[action]/{cliente}", Name ="DashboardWeb")]
        public DashboardWebVM DashboardWeb(int cliente)
        {
            return _negocio.DashboardWeb(cliente);
        }

        [HttpGet("[action]/{cliente}/{empresa}/{cc}", Name ="LaudosComValor")]
        public (List<Vwlaudo>, List<LaudoVM>) LaudosComValor(int cliente, int empresa, int cc)
        {
            return _negocio.LaudosComValor(cliente, empresa, cc);
        }

        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarLinhasTelefonicas")]
        public List<Equipamentohistoricovm> ListarLinhasTelefonicas(string pesquisa, int cliente)
        {
            return _negocio.ListarLinhasTelefonicas(pesquisa, cliente);
        }

        [HttpPost("[action]", Name = "ConsultarLogsAcesso")]
        public IActionResult ConsultarLogsAcesso([FromBody] LogAcessoFiltroVM filtros)
        {
            try
            {
                var logs = _negocio.ConsultarLogsAcesso(filtros);
                return Ok(logs);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("[action]", Name = "ConsultarGarantias")]
        public IActionResult ConsultarGarantias([FromBody] GarantiaFiltroVM filtros)
        {
            try
            {
                var garantias = _negocio.ConsultarGarantias(filtros);
                return Ok(garantias);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #region Sinalizações de Suspeitas

        /// <summary>
        /// Consultar sinalizações de suspeitas com filtros
        /// </summary>
        [HttpPost("[action]", Name = "ConsultarSinalizacoesSuspeitas")]
        public async Task<IActionResult> ConsultarSinalizacoesSuspeitas([FromBody] FiltroSinalizacoesDTO filtros)
        {
            try
            {
                var sinalizacoes = await _sinalizacaoNegocio.ListarSinalizacoesAsync(filtros);
                return Ok(sinalizacoes);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obter detalhes de uma sinalização específica
        /// </summary>
        [HttpGet("[action]/{id}", Name = "SinalizacaoSuspeita")]
        public async Task<IActionResult> ObterDetalhesSinalizacao(int id)
        {
            try
            {
                var detalhes = await _sinalizacaoNegocio.ObterDetalhesAsync(id);
                if (detalhes == null)
                {
                    return NotFound(new { message = "Sinalização não encontrada" });
                }
                return Ok(detalhes);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Alterar status de uma sinalização
        /// </summary>
        [HttpPut("[action]/{id}/Status", Name = "AlterarStatusSinalizacao")]
        public async Task<IActionResult> AlterarStatusSinalizacao(int id, [FromBody] AtualizarStatusSinalizacaoDTO dto)
        {
            try
            {
                dto.SinalizacaoId = id;
                var resultado = await _sinalizacaoNegocio.AtualizarStatusAsync(dto);
                if (resultado)
                {
                    return Ok(new { message = "Status atualizado com sucesso" });
                }
                else
                {
                    return BadRequest(new { message = "Erro ao atualizar status" });
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Atribuir investigador para uma sinalização
        /// </summary>
        [HttpPut("[action]/{id}/Investigador", Name = "AtribuirInvestigadorSinalizacao")]
        public async Task<IActionResult> AtribuirInvestigadorSinalizacao(int id, [FromBody] AtualizarStatusSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[RELATORIO_CONTROLLER] Atribuindo investigador - SinalizacaoId: {id}, DTO: {System.Text.Json.JsonSerializer.Serialize(dto)}");
                
                if (!dto.InvestigadorId.HasValue)
                {
                    Console.WriteLine($"[RELATORIO_CONTROLLER] ERRO: InvestigadorId não foi fornecido");
                    return BadRequest(new { message = "ID do investigador é obrigatório" });
                }

                Console.WriteLine($"[RELATORIO_CONTROLLER] Chamando negócio - SinalizacaoId: {id}, InvestigadorId: {dto.InvestigadorId.Value}");
                var resultado = await _sinalizacaoNegocio.AtribuirInvestigadorAsync(id, dto.InvestigadorId.Value);
                
                if (resultado)
                {
                    Console.WriteLine($"[RELATORIO_CONTROLLER] Investigador atribuído com sucesso");
                    return Ok(new { message = "Investigador atribuído com sucesso" });
                }
                else
                {
                    Console.WriteLine($"[RELATORIO_CONTROLLER] ERRO: Falha ao atribuir investigador no negócio");
                    return BadRequest(new { message = "Erro ao atribuir investigador" });
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[RELATORIO_CONTROLLER] EXCEÇÃO ao atribuir investigador: {ex.Message}");
                Console.WriteLine($"[RELATORIO_CONTROLLER] StackTrace: {ex.StackTrace}");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obter motivos de suspeita disponíveis
        /// </summary>
        [HttpGet("[action]", Name = "MotivosSuspeita")]
        public async Task<IActionResult> ObterMotivosSuspeita()
        {
            try
            {
                var motivos = await _sinalizacaoNegocio.ObterMotivosSuspeitaAsync();
                return Ok(motivos);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obter estatísticas de sinalizações
        /// </summary>
        [HttpGet("[action]", Name = "EstatisticasSinalizacoes")]
        public async Task<IActionResult> ObterEstatisticasSinalizacoes()
        {
            try
            {
                var estatisticas = await _sinalizacaoNegocio.ObterEstatisticasAsync();
                return Ok(estatisticas);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obter usuários ativos que podem ser investigadores
        /// Exclui o super user (administrador do sistema) da lista
        /// </summary>
        [HttpGet("UsuariosInvestigadores")]
        public IActionResult ObterUsuariosInvestigadores()
        {
            try
            {
                var usuarios = _usuarioNegocio.ListarUsuarios();
                
                var investigadores = usuarios
                    .Where(u => u.Ativo == true && u.Su != true) // Excluir super user (administrador do sistema)
                    .Select(u => new
                    {
                        id = u.Id,
                        nome = u.Nome,
                        email = u.Email,
                        adm = u.Adm,
                        operador = u.Operador,
                        ativo = u.Ativo
                    })
                    .OrderBy(u => u.nome)
                    .ToList();

                return Ok(investigadores);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[RELATORIO_CONTROLLER] Erro ao buscar investigadores: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region RELATÓRIO DE NÃO CONFORMIDADE DE ELEGIBILIDADE
        /***************************************************************************************************/
        /*************************** RELATÓRIO DE NÃO CONFORMIDADE DE ELEGIBILIDADE *********************/
        /***************************************************************************************************/

        /// <summary>
        /// Consulta relatório de não conformidade de elegibilidade
        /// Identifica colaboradores que possuem recursos mas não são elegíveis conforme as políticas
        /// </summary>
        [HttpPost("[action]", Name = "ConsultarNaoConformidadeElegibilidade")]
        public IActionResult ConsultarNaoConformidadeElegibilidade([FromBody] RelatorioNaoConformidadeFiltroVM filtros)
        {
            try
            {
                Console.WriteLine($"[NÃO CONFORMIDADE API] Consultar não conformidades - Cliente: {filtros.Cliente}");
                
                if (filtros == null)
                {
                    return BadRequest(new { Mensagem = "Filtros não podem ser nulos", Status = "400" });
                }

                if (filtros.Cliente <= 0)
                {
                    return BadRequest(new { Mensagem = "Cliente é obrigatório", Status = "400" });
                }

                var resultado = _negocio.ConsultarNaoConformidadeElegibilidade(filtros);
                
                Console.WriteLine($"[NÃO CONFORMIDADE API] ✅ Retornando {resultado.TotalRegistros} não conformidades");
                
                return Ok(new
                {
                    Status = "200",
                    Mensagem = $"Consulta realizada com sucesso. {resultado.TotalRegistros} não conformidade(s) encontrada(s).",
                    Data = resultado
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NÃO CONFORMIDADE API] ❌ Erro: {ex.Message}");
                Console.WriteLine($"[NÃO CONFORMIDADE API] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    Mensagem = "Erro ao consultar não conformidades: " + ex.Message, 
                    Status = "500" 
                });
            }
        }

        /// <summary>
        /// 📋 Obter empresas que possuem colaboradores ativos
        /// </summary>
        [HttpGet("FiltrosColaboradores/Empresas/{clienteId}")]
        public IActionResult ObterEmpresasComColaboradores(int clienteId)
        {
            try
            {
                Console.WriteLine($"[FILTROS COLABORADORES API] Buscando empresas com colaboradores - Cliente: {clienteId}");
                var empresas = _negocio.ObterEmpresasComColaboradores(clienteId);
                return Ok(empresas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILTROS COLABORADORES API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao buscar empresas: " + ex.Message });
            }
        }

        /// <summary>
        /// 📋 Obter localidades que possuem colaboradores ativos
        /// </summary>
        [HttpGet("FiltrosColaboradores/Localidades/{clienteId}")]
        public IActionResult ObterLocalidadesComColaboradores(int clienteId)
        {
            try
            {
                Console.WriteLine($"[FILTROS COLABORADORES API] Buscando localidades com colaboradores - Cliente: {clienteId}");
                var localidades = _negocio.ObterLocalidadesComColaboradores(clienteId);
                return Ok(localidades);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILTROS COLABORADORES API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao buscar localidades: " + ex.Message });
            }
        }

        /// <summary>
        /// 📋 Obter centros de custo que possuem colaboradores ativos
        /// </summary>
        [HttpGet("FiltrosColaboradores/CentrosCusto/{clienteId}")]
        public IActionResult ObterCentrosCustoComColaboradores(int clienteId)
        {
            try
            {
                Console.WriteLine($"[FILTROS COLABORADORES API] Buscando centros de custo com colaboradores - Cliente: {clienteId}");
                var centrosCusto = _negocio.ObterCentrosCustoComColaboradores(clienteId);
                return Ok(centrosCusto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILTROS COLABORADORES API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao buscar centros de custo: " + ex.Message });
            }
        }

        /// <summary>
        /// 👥 Consulta colaboradores que não possuem recursos associados
        /// </summary>
        [HttpPost("[action]", Name = "ConsultarColaboradoresSemRecursos")]
        public IActionResult ConsultarColaboradoresSemRecursos([FromBody] ColaboradoresSemRecursosFiltroVM filtros)
        {
            try
            {
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] 📋 Recebendo requisição...");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] ClienteId: {filtros?.ClienteId}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] Cargo: {filtros?.Cargo}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] TipoColaborador: {filtros?.TipoColaborador}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] Empresa: {filtros?.Empresa}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] Localidade: {filtros?.Localidade}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] CentroCusto: {filtros?.CentroCusto}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] Nome: {filtros?.Nome}");
                
                if (filtros.ClienteId == 0)
                {
                    return BadRequest(new { Mensagem = "Cliente é obrigatório", Status = "400" });
                }

                var colaboradores = _negocio.ConsultarColaboradoresSemRecursos(filtros);
                
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] ✅ Retornando {colaboradores.Count} colaboradores");
                
                return Ok(new
                {
                    Status = "200",
                    Mensagem = $"Consulta realizada com sucesso. {colaboradores.Count} colaborador(es) encontrado(s).",
                    Data = colaboradores
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] ❌ Erro: {ex.Message}");
                Console.WriteLine($"[COLABORADORES SEM RECURSOS API] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    Mensagem = "Erro ao consultar colaboradores sem recursos: " + ex.Message, 
                    Status = "500" 
                });
            }
        }

        #endregion

        #region MAPA DE RECURSOS
        /// <summary>
        /// 🗺️ Obter Mapa de Recursos com visualização hierárquica em árvore com drilldown
        /// </summary>
        [HttpPost("[action]", Name = "ObterMapaRecursos")]
        public IActionResult ObterMapaRecursos([FromBody] MapaRecursosFiltroVM filtros)
        {
            try
            {
                Console.WriteLine($"[MAPA RECURSOS API] 🗺️ Recebendo requisição - Cliente: {filtros?.ClienteId}");
                
                if (filtros == null)
                {
                    return BadRequest(new { Mensagem = "Filtros não podem ser nulos", Status = "400" });
                }

                if (filtros.ClienteId <= 0)
                {
                    return BadRequest(new { Mensagem = "Cliente é obrigatório", Status = "400" });
                }

                var resultado = _negocio.ObterMapaRecursos(filtros);
                
                Console.WriteLine($"[MAPA RECURSOS API] ✅ Mapa gerado com sucesso");
                
                return Ok(new
                {
                    Status = "200",
                    Mensagem = "Mapa de recursos gerado com sucesso.",
                    Data = resultado
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MAPA RECURSOS API] ❌ Erro: {ex.Message}");
                Console.WriteLine($"[MAPA RECURSOS API] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    Mensagem = "Erro ao gerar mapa de recursos: " + ex.Message, 
                    Status = "500" 
                });
            }
        }
        #endregion
    }
}
