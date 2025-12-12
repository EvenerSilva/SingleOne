using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SingleOne.Models;
using SingleOne.Util;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ColaboradorController : ControllerBase
    {
        private readonly IColaboradorNegocio _negocio;
        
        public ColaboradorController(IColaboradorNegocio negocio)
        {
            //negocio = new ColaboradorNegocio(config, pdf);
            _negocio = negocio;
        }


        [HttpGet("[action]/{pesquisa}/{cliente}/{pagina}", Name ="ListarColaboradores")]
        public PagedResult<ColaboradoresVM> ListarColaboradores(string pesquisa, int cliente, int pagina, [FromQuery] string tipoFiltro = null)
        {
            Console.WriteLine($"[CONTROLLER] ListarColaboradores - Pesquisa: '{pesquisa}', Cliente: {cliente}, Página: {pagina}, TipoFiltro: '{tipoFiltro}'");
            return _negocio.ListarColaboradores(pesquisa, cliente, pagina, tipoFiltro);
        }

        [HttpGet("[action]/{pesquisa}/{cliente}", Name = "PesquisarColaboradores")]
        public List<Colaboradore> ListarColaboradores(string pesquisa, int cliente)
        {
            return _negocio.ListarColaboradores(pesquisa, cliente);
        }

        [HttpGet("[action]/{cliente}", Name = "ObterEstatisticas")]
        public ColaboradorEstatisticasDTO ObterEstatisticas(int cliente)
        {
            return _negocio.ObterEstatisticas(cliente);
        }

        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarColaboradoresAtivos")]
        public List<Colaboradore> ListarColaboradoresAtivos(string pesquisa, int cliente)
        {
            Console.WriteLine($"[CONTROLLER-ATIVOS] ========== REQUISIÇÃO RECEBIDA ==========");
            Console.WriteLine($"[CONTROLLER-ATIVOS] Pesquisa: '{pesquisa}'");
            Console.WriteLine($"[CONTROLLER-ATIVOS] Cliente: {cliente}");
            
            var resultado = _negocio.ListarColaboradoresAtivos(pesquisa, cliente);
            
            Console.WriteLine($"[CONTROLLER-ATIVOS] Total de colaboradores retornados: {resultado?.Count ?? 0}");
            if (resultado != null && resultado.Count > 0)
            {
                Console.WriteLine($"[CONTROLLER-ATIVOS] Primeiro colaborador: ID={resultado[0].Id}, Nome={resultado[0].Nome}");
                Console.WriteLine($"[CONTROLLER-ATIVOS] Propriedades do primeiro colaborador:");
                Console.WriteLine($"[CONTROLLER-ATIVOS]   - Empresa: {resultado[0].Empresa}");
                Console.WriteLine($"[CONTROLLER-ATIVOS]   - Localidade: {resultado[0].Localidade}");
                Console.WriteLine($"[CONTROLLER-ATIVOS]   - CentroCusto: {resultado[0].Centrocusto}");
                Console.WriteLine($"[CONTROLLER-ATIVOS]   - FilialId: {resultado[0].FilialId}");
                Console.WriteLine($"[CONTROLLER-ATIVOS]   - Setor: {resultado[0].Setor}");
                Console.WriteLine($"[CONTROLLER-ATIVOS]   - Cargo: {resultado[0].Cargo}");
            }
            Console.WriteLine($"[CONTROLLER-ATIVOS] ========== FIM REQUISIÇÃO ==========");
            
            return resultado;
        }

        [HttpGet("[action]/{id}", Name ="ObterColaboradorPorId")]
        public ColaboradorCompletoDTO ObterColaboradorPorId(int id)
        {
            return _negocio.ObterColaboradorPorID(id);
        }
        [HttpPost("[action]", Name ="SalvarColaborador")]
        public IActionResult SalvarColaborador(Colaboradore col)
        {
            try
            {
                return Ok(_negocio.SalvarColaborador(col));
            }
            catch (DomainException ex)
            {
                return StatusCode(409, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("[action]/{id}", Name ="ExcluirColaborador")]
        public void ExcluirColaborador(int id)
        {
            _negocio.ExcluirColaborador(id);
        }
        [HttpGet("[action]/{cliente}/{colaborador}/{usuarioLogado}/{byod}", Name ="TermoCompromisso")]
        public IActionResult TermoCompromisso(int cliente, int colaborador, int usuarioLogado, bool byod)
        {
            try
            {
                // Validações iniciais
                if (cliente <= 0)
                {
                    return BadRequest("Cliente inválido");
                }
                if (colaborador <= 0)
                {
                    return BadRequest("Colaborador inválido");
                }
                if (usuarioLogado <= 0)
                {
                    return BadRequest("Usuário inválido");
                }

                var termo = _negocio.TermoCompromisso(cliente, colaborador, usuarioLogado, byod);
                if (termo == null || termo.Length == 0)
                {
                    return BadRequest("Erro ao gerar termo: PDF vazio ou inválido");
                }
                // Converter byte[] para Base64 string para garantir compatibilidade com frontend
                string base64String = Convert.ToBase64String(termo);
                return Ok(base64String);
            }
            catch (Exception ex)
            {
                // Log detalhado do erro
                Console.WriteLine($"[TERMO_COMPROMISSO] ERRO no Controller: {ex.Message}");
                Console.WriteLine($"[TERMO_COMPROMISSO] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] InnerException: {ex.InnerException.Message}");
                }
                return BadRequest($"Erro ao gerar termo: {ex.Message}");
            }
        }
        [HttpGet("[action]/{cliente}/{colaborador}/{usuarioLogado}/{byod}", Name = "TermoPorEmail")]
        public async Task<string> TermoPorEmail(int cliente, int colaborador, int usuarioLogado, bool byod)
        {
            return await _negocio.TermoPorEmail(cliente, colaborador, usuarioLogado, byod);
        }
        [HttpGet("[action]/{colaborador}/{cliente}", Name ="NadaConsta")]
        public List<Vwnadaconstum> NadaConsta(int colaborador, int cliente)
        {
            return _negocio.NadaConsta(colaborador, cliente);
        }
        [HttpGet("[action]/{colaborador}/{cliente}/{usuarioLogado}", Name = "TermoNadaConsta")]
        public byte[] TermoNadaConsta(int colaborador, int cliente, int usuarioLogado)
        {
            return _negocio.TermoNadaConsta(colaborador, cliente, usuarioLogado);
        }
        [HttpGet("[action]/{pesquisa}/{cliente}/{filtro}", Name = "ColaboradoresComTermoPorAssinar")]
        public List<Termoscolaboradoresvm> ColaboradoresComTermoPorAssinar(string pesquisa, int cliente, string filtro)
        {
            return _negocio.ColaboradoresComTermoPorAssinar(pesquisa, cliente, filtro);
        }



        //Cargos e Cargos de descarte
        [HttpGet("[action]/{cliente}/{pesquisa}", Name ="ListarCargos")]
        public List<string> ListarCargos(int cliente, string pesquisa)
        {
            return _negocio.ListarCargos(cliente, pesquisa);
        }
        [HttpGet("[action]/{cliente}", Name ="ListarCargosDescarte")]
        public List<Descartecargo> ListarCargosDescarte(int cliente)
        {
            return _negocio.ListarCargosDeDescarte(cliente);
        }
        [HttpPost("[action]", Name ="SalvarCargoDescarte")]
        public void SalvarCargoDescarte(Descartecargo cargo)
        {
            _negocio.SalvarCargoDescarte(cargo);
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirCargoDescarte")]
        public void ExcluirCargoDescarte(int id)
        {
            _negocio.ExcluirCargoDescarte(id);
        }

        // Cargos de Confiança
        [HttpGet("cargosconfianca/ListarUnicos/{cliente}", Name = "ListarCargosUnicos")]
        public IActionResult ListarCargosUnicos(int cliente)
        {
            try
            {
                var cargos = _negocio.ListarCargosUnicos(cliente);
                return Ok(cargos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cargosconfianca/Listar/{cliente}", Name = "ListarCargosConfianca")]
        public IActionResult ListarCargosConfianca(int cliente)
        {
            try
            {
                var cargos = _negocio.ListarCargosConfianca(cliente);
                return Ok(cargos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("cargosconfianca/Salvar", Name = "SalvarCargoConfianca")]
        public IActionResult SalvarCargoConfianca([FromBody] CargoConfianca cargo)
        {
            try
            {
                var resultado = _negocio.SalvarCargoConfianca(cargo);
                return Ok(resultado);
            }
            catch (DomainException ex)
            {
                return Conflict(ex.Message); // 409 Conflict
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("cargosconfianca/Atualizar/{id}", Name = "AtualizarCargoConfianca")]
        public IActionResult AtualizarCargoConfianca(int id, [FromBody] CargoConfianca cargo)
        {
            try
            {
                var resultado = _negocio.AtualizarCargoConfianca(id, cargo);
                return Ok(resultado);
            }
            catch (DomainException ex)
            {
                // Se a mensagem contém "não encontrado", retorna 404; senão, 409
                if (ex.Message.Contains("não encontrado"))
                {
                    return NotFound(ex.Message);
                }
                return Conflict(ex.Message); // 409 Conflict
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("cargosconfianca/Excluir/{id}", Name = "ExcluirCargoConfianca")]
        public IActionResult ExcluirCargoConfianca(int id)
        {
            try
            {
                _negocio.ExcluirCargoConfianca(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("cargosconfianca/Verificar/{cargo}/{cliente}", Name = "VerificarCargoConfianca")]
        public IActionResult VerificarCargoConfianca(string cargo, int cliente)
        {
            try
            {
                var resultado = _negocio.VerificarCargoConfianca(cargo, cliente);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("[action]/{cliente}", Name ="ExportarTermosEmPDF")]
        [AllowAnonymous]
        public void ExportarTermosEmPDF(int cliente)
        {
            _negocio.ExportarTermosEmPDF(cliente);
        }

        [HttpPost("[action]")]
        public IActionResult RegistrarLocalizacaoAssinatura([FromBody] LocalizacaoAssinaturaDTO dados)
        {
            try
            {
                if (dados == null)
                {
                    return BadRequest("Dados de localização não informados");
                }

                // Log dos dados recebidos
                var logMessage = $"Localização registrada - " +
                    $"Colaborador: {dados.ColaboradorNome} (ID: {dados.ColaboradorId}), " +
                    $"Usuário: {dados.UsuarioLogadoId}, " +
                    $"IP: {dados.IP}, " +
                    $"Local: {dados.City}, {dados.Region}, {dados.Country}, " +
                    $"Coordenadas: {dados.Latitude}, {dados.Longitude}, " +
                    $"Precisão: {dados.Accuracy}m, " +
                    $"Ação: {dados.Acao}, " +
                    $"Timestamp: {dados.Timestamp}";

                // Salvar dados no banco usando lógica de negócio
                _negocio.RegistrarLocalizacaoAssinatura(dados);
                
                // Log no sistema
                Console.WriteLine($"[ASSINATURA_GEOLOCALIZACAO] {logMessage}");
                
                // Retorna sucesso
                return Ok(new { 
                    Mensagem = "Localização registrada com sucesso",
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // Log do erro
                Console.WriteLine($"[ERRO_GEOLOCALIZACAO] {ex.Message}");
                return StatusCode(500, "Erro interno do servidor ao registrar localização");
            }
        }
    }
}
