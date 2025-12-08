using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOneAPI.Models.ViewModels;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SingleOneAPI.Services;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfiguracoesController : ControllerBase
    {
        private readonly IConfiguracoesNegocio _negocio;
        private readonly IFileUploadService _fileUploadService;
        public ConfiguracoesController(IConfiguracoesNegocio negocio, IFileUploadService fileUploadService)
        {
            //this.negocio = new ConfiguracoesNegocio();
            _negocio = negocio;
            _fileUploadService = fileUploadService;
        }

        #region CLIENTES
        /***************************************************************************************************/
        /************************************************* CLIENTES ****************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}", Name ="ListarClientes")]
        public List<Cliente> ListarClientes(string pesquisa)
        {
            return _negocio.ListarClientes(pesquisa);
        }
        [HttpPost("[action]", Name ="SalvarCliente")]
        public IActionResult SalvarCliente([FromBody] Cliente cli)
        {
            try
            {
                if (cli == null)
                {
                    return BadRequest("Cliente não pode ser nulo");
                }

                var resultado = _negocio.SalvarCliente(cli);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                var status = resultadoObj.Status?.ToString();
                if (status == "200")
                {
                    return Ok(resultadoObj);
                }
                else if (status == "400" || status == "400.1")
                {
                    return BadRequest(resultadoObj);
                }
                else
                {
                    return BadRequest(resultadoObj);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao salvar cliente: " + ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirCliente")]
        public void ExcluirCliente(int id)
        {
            _negocio.ExcluirCliente(id);
        }

        [HttpPost("[action]", Name ="UploadLogoCliente")]
        public async Task<IActionResult> UploadLogoCliente([FromForm] IFormFile logo, [FromForm] int clienteId)
        {
            try
            {
                Console.WriteLine($"[UPLOAD-LOGO] Iniciando upload para cliente {clienteId}");
                Console.WriteLine($"[UPLOAD-LOGO] Logo recebida: {(logo != null ? $"Sim - {logo.FileName} ({logo.Length} bytes)" : "NÃO")}");
                Console.WriteLine($"[UPLOAD-LOGO] ClienteId recebido: {clienteId}");
                
                if (logo == null)
                {
                    Console.WriteLine("[UPLOAD-LOGO] ❌ Logo é nula");
                    return BadRequest(new { Mensagem = "Logo não pode ser nula", Status = "400" });
                }

                if (clienteId <= 0)
                {
                    Console.WriteLine($"[UPLOAD-LOGO] ❌ Cliente ID inválido: {clienteId}");
                    return BadRequest(new { Mensagem = "ID do cliente inválido", Status = "400" });
                }

                Console.WriteLine($"[UPLOAD-LOGO] Validando cliente {clienteId}...");
                
                // Verificar se o cliente existe
                var cliente = _negocio.ListarClientes("null").FirstOrDefault(c => c.Id == clienteId);
                if (cliente == null)
                {
                    Console.WriteLine($"[UPLOAD-LOGO] ❌ Cliente {clienteId} não encontrado");
                    return NotFound(new { Mensagem = "Cliente não encontrado", Status = "404" });
                }

                Console.WriteLine($"[UPLOAD-LOGO] ✅ Cliente encontrado: {cliente.Razaosocial}");
                Console.WriteLine($"[UPLOAD-LOGO] Iniciando upload do arquivo...");

                // Fazer upload da logo
                var fileName = await _fileUploadService.UploadLogoAsync(logo, clienteId);
                
                Console.WriteLine($"[UPLOAD-LOGO] ✅ Arquivo salvo: {fileName}");
                Console.WriteLine($"[UPLOAD-LOGO] Atualizando cliente no banco...");
                
                // Atualizar o cliente com o nome do arquivo da logo
                cliente.Logo = fileName;
                var resultado = _negocio.SalvarCliente(cliente);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                if (resultadoObj.Status?.ToString() != "200")
                {
                    Console.WriteLine($"[UPLOAD-LOGO] ❌ Erro ao atualizar cliente: {resultadoObj.Mensagem}");
                    return BadRequest(new { Mensagem = "Erro ao atualizar cliente: " + resultadoObj.Mensagem, Status = "400" });
                }

                Console.WriteLine($"[UPLOAD-LOGO] ✅ Cliente atualizado com logo: {fileName}");

                return Ok(new { 
                    Mensagem = "Logo enviada com sucesso!", 
                    Status = "200", 
                    FileName = fileName,
                    LogoPath = _fileUploadService.GetLogoPath(fileName)
                });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[UPLOAD-LOGO] ❌ ArgumentException: {ex.Message}");
                return BadRequest(new { Mensagem = ex.Message, Status = "400" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UPLOAD-LOGO] ❌ Exception: {ex.Message}");
                Console.WriteLine($"[UPLOAD-LOGO] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { Mensagem = "Erro ao fazer upload da logo: " + ex.Message, Status = "500" });
            }
        }


        /// <summary>
        /// Busca a logo do cliente baseado no domínio ou IP
        /// </summary>
        [HttpGet("[action]", Name = "BuscarLogoCliente")]
        [AllowAnonymous]
        public IActionResult BuscarLogoCliente([FromQuery] string dominio = null)
        {
            try
            {
                Console.WriteLine($"[BUSCAR-LOGO] Iniciando busca de logo para domínio: {dominio}");
                
                // Se não foi fornecido domínio, tentar obter do contexto da requisição
                if (string.IsNullOrEmpty(dominio))
                {
                    var host = Request.Headers["Host"].ToString();
                    var referer = Request.Headers["Referer"].ToString();
                    
                    Console.WriteLine($"[BUSCAR-LOGO] Host da requisição: {host}");
                    Console.WriteLine($"[BUSCAR-LOGO] Referer da requisição: {referer}");
                    
                    // Extrair domínio do host ou referer
                    if (!string.IsNullOrEmpty(host))
                    {
                        dominio = host.Split(':')[0]; // Remove porta se existir
                    }
                    else if (!string.IsNullOrEmpty(referer))
                    {
                        try
                        {
                            var uri = new Uri(referer);
                            dominio = uri.Host;
                        }
                        catch
                        {
                            dominio = null;
                        }
                    }
                }
                
                // Se ainda não temos domínio, retornar null
                if (string.IsNullOrEmpty(dominio))
                {
                    Console.WriteLine($"[BUSCAR-LOGO] ❌ Nenhum domínio identificado");
                    return Ok(new { Logo = (string)null, Mensagem = "Nenhum domínio identificado" });
                }
                
                Console.WriteLine($"[BUSCAR-LOGO] Buscando cliente por domínio: {dominio}");
                
                // Buscar cliente que tenha logo configurada
                var todosClientes = _negocio.ListarClientes("null");
                var cliente = todosClientes.FirstOrDefault(c => c.Ativo && !string.IsNullOrEmpty(c.Logo));
                
                if (cliente == null)
                {
                    Console.WriteLine($"[BUSCAR-LOGO] ❌ Nenhum cliente com logo encontrado");
                    return Ok(new { Logo = (string)null, Mensagem = "Nenhum cliente com logo encontrado" });
                }
                
                Console.WriteLine($"[BUSCAR-LOGO] ✅ Cliente encontrado: {cliente.Razaosocial} com logo: {cliente.Logo}");
                
                // Verificar se o arquivo da logo existe fisicamente
                if (!_fileUploadService.LogoExists(cliente.Logo))
                {
                    Console.WriteLine($"[BUSCAR-LOGO] ⚠️ Arquivo de logo não encontrado: {cliente.Logo}");
                    Console.WriteLine($"[BUSCAR-LOGO] Tentando encontrar outro arquivo de logo do cliente {cliente.Id}...");
                    
                    // Tentar encontrar outro arquivo de logo do mesmo cliente
                    var logosPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logos");
                    if (Directory.Exists(logosPath))
                    {
                        var arquivosLogo = Directory.GetFiles(logosPath, $"cliente_{cliente.Id}_*.png")
                            .Concat(Directory.GetFiles(logosPath, $"cliente_{cliente.Id}_*.jpg"))
                            .Concat(Directory.GetFiles(logosPath, $"cliente_{cliente.Id}_*.jpeg"))
                            .Concat(Directory.GetFiles(logosPath, $"cliente_{cliente.Id}_*.gif"))
                            .Select(f => Path.GetFileName(f))
                            .FirstOrDefault();
                        
                        if (!string.IsNullOrEmpty(arquivosLogo))
                        {
                            Console.WriteLine($"[BUSCAR-LOGO] ✅ Arquivo alternativo encontrado: {arquivosLogo}");
                            var logoPath = _fileUploadService.GetLogoPath(arquivosLogo);
                            return Ok(new { 
                                Logo = logoPath, 
                                ClienteNome = cliente.Razaosocial,
                                Mensagem = "Logo encontrada (arquivo alternativo)" 
                            });
                        }
                    }
                    
                    Console.WriteLine($"[BUSCAR-LOGO] ❌ Nenhum arquivo de logo válido encontrado para o cliente {cliente.Id}");
                    return Ok(new { 
                        Logo = (string)null, 
                        ClienteNome = cliente.Razaosocial,
                        Mensagem = "Logo não encontrada" 
                    });
                }
                
                // Retornar o caminho da logo
                var logoPathFinal = _fileUploadService.GetLogoPath(cliente.Logo);
                Console.WriteLine($"[BUSCAR-LOGO] ✅ Logo válida encontrada: {logoPathFinal}");
                return Ok(new { 
                    Logo = logoPathFinal, 
                    ClienteNome = cliente.Razaosocial,
                    Mensagem = "Logo encontrada com sucesso" 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BUSCAR-LOGO] ❌ Erro: {ex.Message}");
                Console.WriteLine($"[BUSCAR-LOGO] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { Mensagem = "Erro ao buscar logo: " + ex.Message, Status = "500" });
            }
        }
        #endregion

        // Métodos de Empresa movidos para EmpresasController

        #region CENTRO CUSTOS
        /***************************************************************************************************/
        /************************************************* CENTRO CUSTOS ***********************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarCentroCustos")]
        public List<CentrocustoVM> ListarCentroCustos(string pesquisa, int cliente)
        {
            return _negocio.ListarCentrosDeCustoVM(pesquisa, cliente);
        }
        [HttpGet("[action]/{id}", Name ="BuscarCentroPorID")]
        public CentrocustoVM BuscarCentroPorID(int id)
        {
            return _negocio.BuscarCentroCustoPorId(id);
        }
        [HttpGet("[action]/{idEmpresa}", Name ="ListarCentrosDaEmpresa")]
        public List<CentrocustoVM> ListarCentrosDaEmpresa(int idEmpresa)
        {
            return _negocio.BuscarPorEmpresaId(idEmpresa);
        }
        [HttpPost("[action]", Name ="SalvarCentroCusto")]
        public ActionResult SalvarCentroCusto([FromBody] CentrocustoVM cc)
        {
            try
            {
                if (cc == null)
                {
                    return BadRequest("Centro de custo não pode ser nulo");
                }

                var resultado = _negocio.SalvarCentroCusto(cc);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                var status = resultadoObj.Status?.ToString();
                if (status == "200")
                {
                    return Ok(resultadoObj);
                }
                else if (status == "200.1")
                {
                    return BadRequest(resultadoObj.Mensagem);
                }
                else
                {
                    return BadRequest(resultadoObj);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao salvar centro de custo: " + ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirCentroCusto")]
        public ActionResult ExcluirCentroCusto(int id)
        {
            try
            {
                var resultado = _negocio.ExcluirCentroCusto(id);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                if (resultadoObj.Status == "200")
                {
                    return Ok(resultadoObj);
                }
                else
                {
                    return BadRequest(resultadoObj.Mensagem);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao excluir centro de custo: " + ex.Message, Status = "500" });
            }
        }
        #endregion

        #region FORNECEDORES
        /***************************************************************************************************/
        /************************************************* FORNECEDORES ************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarFornecedores")]
        public List<Fornecedore> ListarFornecedores(string pesquisa, int cliente)
        {
            return _negocio.ListarFornecedores(pesquisa, cliente);
        }
        [HttpPost("[action]", Name ="SalvarFornecedor")]
        public ActionResult SalvarFornecedor([FromBody] Fornecedore fnc)
        {
            try
            {
                if (fnc == null)
                {
                    return BadRequest("Fornecedor não pode ser nulo");
                }

                var resultado = _negocio.SalvarFornecedor(fnc);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                var status = resultadoObj.Status?.ToString();
                if (status == "200")
                {
                    return Ok(resultadoObj);
                }
                else if (status == "400" || status == "400.1")
                {
                    return BadRequest(resultadoObj);
                }
                else
                {
                    return BadRequest(resultadoObj);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao salvar fornecedor: " + ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirFornecedor")]
        public void ExcluirFornecedor(int id)
        {
            _negocio.ExcluirFornecedor(id);
        }

        /// <summary>
        /// Lista fornecedores que são destinadores de resíduos (para protocolos de descarte)
        /// </summary>
        [HttpGet("[action]/{cliente}", Name = "ListarFornecedoresDestinadores")]
        public IActionResult ListarFornecedoresDestinadores(int cliente)
        {
            try
            {
                Console.WriteLine($"[FORNECEDORES-DESTINADORES] Listando destinadores para cliente: {cliente}");
                
                var destinadores = _negocio.ListarFornecedoresDestinadores(cliente);
                
                Console.WriteLine($"[FORNECEDORES-DESTINADORES] Encontrados {destinadores.Count} fornecedores destinadores");
                
                return Ok(destinadores);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FORNECEDORES-DESTINADORES] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { 
                    Mensagem = "Erro ao listar fornecedores destinadores: " + ex.Message, 
                    Status = "500" 
                });
            }
        }
        #endregion

        #region TIPOS DE EQUIPAMENTOS
        /***************************************************************************************************/
        /************************************************* TIPOS DE EQUIPAMENTOS ***************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarTiposRecursos")]
        public IActionResult ListarTiposRecursos(string pesquisa, int cliente)
        {
            try
            {
                var tipos = _negocio.ListarTiposDeRecursos(pesquisa, cliente);
                return Ok(tipos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TIPOS RECURSOS API] ❌ Erro: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                // Retornar lista vazia em caso de erro para evitar 500
                return Ok(new List<Tipoequipamento>());
            }
        }

        [HttpGet("[action]", Name = "ListarTiposAquisicao")]
        public List<Tipoaquisicao> ListarTiposAquisicao()
        {
            return _negocio.ListarTiposAquisicao();
        }

        [HttpPost("[action]", Name ="SalvarTipoRecurso")]
        public ActionResult SalvarTipoRecurso(Tipoequipamento te)
        {
            try
            {
                if (te == null)
                {
                    return BadRequest("Tipo de equipamento não pode ser nulo");
                }

                var resultado = _negocio.SalvarTipoRecurso(te);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                if (resultadoObj.Status == "200")
                {
                    return Ok(resultadoObj);
                }
                else
                {
                    return BadRequest(resultadoObj.Mensagem);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao salvar tipo de recurso: " + ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{idTipo}/{idCliente}", Name ="ExcluirTipoRecurso")]
        public ActionResult ExcluirTipoRecurso(int idTipo, int idCliente)
        {
            try
            {
                _negocio.ExcluirTipoRecurso(idTipo, idCliente);
                return Ok(new { Mensagem = "Tipo de recurso excluído com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao excluir tipo de recurso: " + ex.Message, Status = "500" });
            }
        }
        #endregion

        #region FABRICANTES
        /***************************************************************************************************/
        /************************************************* FABRICANTES *************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name = "ListarFabricantes")]
        public List<Fabricante> ListarFabricantes(string pesquisa, int cliente)
        {
            return _negocio.ListarFabricantes(pesquisa, cliente);
        }
        [HttpGet("[action]/{tipo}/{cliente}", Name = "ListarFabricantesPorTipoRecurso")]
        public List<Fabricante> ListarFabricantesPorTipoRecurso(int tipo, int cliente)
        {
            return _negocio.ListarFabricantesPorTipoRecurso(tipo, cliente);
        }
        [HttpPost("[action]", Name ="SalvarFabricante")]
        public ActionResult SalvarFabricante(Fabricante fab)
        {
            try
            {
                Console.WriteLine($"=== SALVANDO FABRICANTE ===");
                Console.WriteLine($"ID: {fab?.Id}");
                Console.WriteLine($"Cliente: {fab?.Cliente}");
                Console.WriteLine($"Tipo Equipamento: {fab?.Tipoequipamento}");
                Console.WriteLine($"Descrição: {fab?.Descricao}");
                Console.WriteLine($"Ativo: {fab?.Ativo}");

                if (fab == null)
                {
                    Console.WriteLine("Fabricante é nulo!");
                    return BadRequest("Fabricante não pode ser nulo");
                }

                var resultado = _negocio.SalvarFabricante(fab);
                Console.WriteLine($"Resultado do negócio: {resultado}");
                
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                Console.WriteLine($"Status: {resultadoObj.Status}");
                Console.WriteLine($"Mensagem: {resultadoObj.Mensagem}");
                
                if (resultadoObj.Status == "200")
                {
                    return Ok(resultadoObj);
                }
                else
                {
                    return BadRequest(resultadoObj.Mensagem);
                }
            }
            catch (EntidadeJaExisteEx ex)
            {
                Console.WriteLine($"Entidade já existe: {ex.Message}");
                return StatusCode(409, new { Mensagem = ex.Message, Status = "409" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Mensagem = "Erro ao salvar fabricante: " + ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirFabricante")]
        public ActionResult ExcluirFabricante(int id)
        {
            try
            {
                _negocio.ExcluirFabricante(id);
                return Ok(new { Mensagem = "Fabricante excluído com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao excluir fabricante: " + ex.Message, Status = "500" });
            }
        }
        #endregion

        #region MODELOS
        /***************************************************************************************************/
        /************************************************* MODELOS *****************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarModelos")]
        public List<Modelo> ListarModelos(string pesquisa, int cliente)
        {
            return _negocio.ListarModelos(pesquisa, cliente);
        }
        [HttpGet("[action]/{fabricante}/{cliente}", Name ="ListarModelosDoFabricante")]
        public List<Modelo> ListarModelosDoFabricante(int fabricante, int cliente)
        {
            return _negocio.ListarModelosDoFabricante(fabricante, cliente);
        }
        [HttpPost("[action]", Name ="SalvarModelo")]
        public ActionResult SalvarModelo(Modelo md)
        {
            try
            {
                Console.WriteLine($"=== SALVANDO MODELO ===");
                Console.WriteLine($"ID: {md?.Id}");
                Console.WriteLine($"Cliente: {md?.Cliente}");
                Console.WriteLine($"Fabricante: {md?.Fabricante}");
                Console.WriteLine($"Descrição: {md?.Descricao}");
                Console.WriteLine($"Ativo: {md?.Ativo}");

                if (md == null)
                {
                    Console.WriteLine("Modelo é nulo!");
                    return BadRequest("Modelo não pode ser nulo");
                }

                var resultado = _negocio.SalvarModelo(md);
                Console.WriteLine($"Resultado do negócio: {resultado}");
                
                return Ok(new { Mensagem = resultado, Status = "200" });
            }
            catch (EntidadeJaExisteEx ex)
            {
                Console.WriteLine($"Entidade já existe: {ex.Message}");
                return StatusCode(409, new { Mensagem = ex.Message, Status = "409" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Mensagem = "Erro ao salvar modelo: " + ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirModelo")]
        public ActionResult ExcluirModelo(int id)
        {
            try
            {
                _negocio.ExcluirModelo(id);
                return Ok(new { Mensagem = "Modelo excluído com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao excluir modelo: " + ex.Message, Status = "500" });
            }
        }
        #endregion

        #region NOTAS FISCAIS
        /***************************************************************************************************/
        /******************************************** NOTAS FISCAIS ****************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarNotasFiscais")]
        public List<NotaFiscalListagemVM> ListarNotasFiscais(string pesquisa, int cliente)
        {
            return _negocio.ListarNotasFiscais(pesquisa, cliente);
        }
        [HttpGet("[action]/{id}", Name ="BuscarNotaPorId")]
        public Notasfiscai BuscarNotaPorId(int id)
        {
            return _negocio.BuscarNotaPorId(id);
        }

        [HttpGet("[action]/{id}", Name = "VisualizarNotaFiscal")]
        public VisualizarNotaFiscalVM VisualizarNotaFiscal(int id)
        {
            return _negocio.VisualizarNotaFiscal(id);
        }

        [HttpPost("[action]", Name ="SalvarNotaFiscal")]
        public void SalvarNotaFiscal(Notasfiscai nf)
        {
            _negocio.SalvarNotaFiscal(nf);
        }
        [HttpPost("[action]", Name = "AdicionarItemNota")]
        public void AdicionarItemNota(Notasfiscaisiten nfi)
        {
            _negocio.AdicionarItemNota(nfi);
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirNotaFiscal")]
        public IActionResult ExcluirNotaFiscal(int id)
        {
            try
            {
                Console.WriteLine($"[CONTROLLER] Excluindo nota fiscal ID: {id}");
                _negocio.ExcluirNotaFiscal(id);
                return Ok(new { Mensagem = "Nota fiscal excluída com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ❌ Erro ao excluir nota fiscal ID {id}: {ex.Message}");
                return StatusCode(500, new { Mensagem = ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{id}", Name = "ExcluirItemNota")]
        public void ExcluirItemNota(int id)
        {
            _negocio.ExcluirItemNota(id);
        }
        [HttpPost("[action]", Name ="LiberarParaEstoque")]
        public void LiberarParaEstoque(NotaFiscalVM vm)
        {
            _negocio.LiberarParaEstoque(vm);
        }
        #endregion

        #region LAUDOS
        /***************************************************************************************************/
        /******************************************** LAUDOS ***********************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarLaudos")]
        //public List<Laudo> ListarLaudos(string pesquisa, int cliente)
        //{
        //    return negocio.ListarLaudos(pesquisa, cliente);
        //}
        public List<Vwlaudo> ListarLaudos(string pesquisa, int cliente)
        {
            return _negocio.ListarLaudos(pesquisa, cliente);
        }
        [HttpGet("[action]/{id}", Name ="BuscarLaudoPorId")]
        public Laudo BuscarLaudoPorId(int id)
        {
            return _negocio.BuscarLaudoPorID(id);
        }
        [HttpPost("[action]", Name = "SalvarLaudo")]
        public IActionResult SalvarLaudo(Laudo laudo)
        {
            try
            {
                _negocio.SalvarLaudo(laudo);
                
                // Retornar o ID do laudo criado/atualizado
                return Ok(new { 
                    Mensagem = laudo.Id == 0 ? "Laudo criado com sucesso!" : "Laudo atualizado com sucesso!", 
                    Status = "200", 
                    Id = laudo.Id,
                    Sucesso = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Mensagem = "Erro ao salvar laudo: " + ex.Message, 
                    Status = "500",
                    Sucesso = false
                });
            }
        }
        [HttpPost("[action]", Name ="EncerrarLaudo")]
        public void EncerrarLaudo(Laudo laudo)
        {
            _negocio.EncerrarLaudo(laudo);
        }
        [HttpGet("[action]/{id}", Name ="GerarLaudoEmPDF")]
        public byte[] GerarLaudoEmPDF(int id, [FromQuery] int? templateId = null)
        {
            return _negocio.GerarLaudoEmPDF(id, templateId);
        }
        #endregion

        #region LOCALIZAÇÃO
        /***************************************************************************************************/
        /******************************************** LOCALIZAÇÃO ******************************************/
        /***************************************************************************************************/
        /// <summary>
        /// ✅ CORREÇÃO: Endpoint com nome singular para compatibilidade com frontend
        /// </summary>
        [HttpGet("[action]/{cliente}", Name ="ListarLocalidade")]
        public List<Localidade> ListarLocalidade(int cliente)
        {
            return _negocio.ListarLocalidade(cliente);
        }
        
        /// <summary>
        /// ✅ Endpoint alternativo com nome plural (mantido para compatibilidade)
        /// </summary>
        [HttpGet("[action]/{cliente}", Name ="ListarLocalidades")]
        public List<Localidade> ListarLocalidades(int cliente)
        {
            return _negocio.ListarLocalidade(cliente);
        }
        [HttpPost("[action]", Name ="SalvarLocalidade")]
        public IActionResult SalvarLocalidade([FromBody] Localidade local)
        {
            try
            {
                if (local == null)
                {
                    return BadRequest("Localidade não pode ser nula");
                }

                _negocio.SalvarLocalidade(local);
                
                if (local.Id == 0)
                {
                    return Ok(new { Mensagem = "Localidade criada com sucesso!", Status = "200", Id = local.Id });
                }
                else
                {
                    return Ok(new { Mensagem = "Localidade atualizada com sucesso!", Status = "200", Id = local.Id });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao salvar localidade: " + ex.Message, Status = "500" });
            }
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirLocalidade")]
        public void ExcluirLocalidade(int id)
        {
            _negocio.ExcluirLocalidade(id);
        }
        
        /// <summary>
        /// ✅ NOVO: Retorna apenas as localidades de uma empresa específica
        /// Garante consistência: colaborador só pode ter localidade da sua empresa
        /// </summary>
        [HttpGet("[action]/{empresaId}", Name ="LocalidadesDaEmpresa")]
        public List<Localidade> LocalidadesDaEmpresa(int empresaId)
        {
            return _negocio.ListarLocalidadesDaEmpresa(empresaId);
        }
        #endregion

        #region FILIAIS
        /***************************************************************************************************/
        /******************************************** FILIAIS **********************************************/
        /***************************************************************************************************/
        [HttpGet("[action]/{pesquisa}/{cliente}", Name ="ListarFiliais")]
        public IActionResult ListarFiliais(string pesquisa, int cliente)
        {
            Console.WriteLine($"[FILIAIS] ==========================================");
            Console.WriteLine($"[FILIAIS] ListarFiliais chamado - Pesquisa: '{pesquisa}', Cliente: {cliente}");
            Console.WriteLine($"[FILIAIS] Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"[FILIAIS] ==========================================");
            
            try
            {
                Console.WriteLine($"[FILIAIS] Chamando _negocio.ListarFiliais...");
                var resultado = _negocio.ListarFiliais(pesquisa, cliente);
                Console.WriteLine($"[FILIAIS] Resultado recebido: {resultado?.Count ?? 0} filiais");
                
                if (resultado != null && resultado.Count > 0)
                {
                    Console.WriteLine($"[FILIAIS] Primeira filial: ID={resultado[0].Id}, Nome={resultado[0].Nome}");
                }
                else
                {
                    Console.WriteLine($"[FILIAIS] Lista vazia ou nula");
                }
                
                Console.WriteLine($"[FILIAIS] ==========================================");
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FILIAIS] ❌ ERRO: {ex.Message}");
                Console.WriteLine($"[FILIAIS] StackTrace: {ex.StackTrace}");
                Console.WriteLine($"[FILIAIS] ==========================================");
                
                // Retornar erro 500 com detalhes para debug
                return StatusCode(500, new { 
                    Mensagem = "Erro interno ao listar filiais", 
                    Erro = ex.Message,
                    Status = "500",
                    Timestamp = DateTime.Now
                });
            }
        }

        [HttpGet("[action]/{id}", Name ="ObterFilialPorId")]
        public Filial ObterFilialPorId(int id)
        {
            return _negocio.BuscarFilialPeloID(id);
        }

        [HttpPost("[action]", Name ="SalvarFilial")]
        public IActionResult SalvarFilial([FromBody] Filial filial)
        {
            try
            {
                if (filial == null)
                {
                    return BadRequest("Filial não pode ser nula");
                }

                var resultado = _negocio.SalvarFilial(filial);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                var status = resultadoObj.Status?.ToString();
                if (status == "200")
                {
                    return Ok(resultadoObj);
                }
                else if (status == "400" || status == "400.1")
                {
                    return BadRequest(resultadoObj);
                }
                else
                {
                    return BadRequest(resultadoObj);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao salvar filial: " + ex.Message, Status = "500" });
            }
        }

        [HttpDelete("[action]/{id}", Name ="ExcluirFilial")]
        public IActionResult ExcluirFilial(int id)
        {
            try
            {
                var resultado = _negocio.ExcluirFilial(id);
                return Ok(new { success = true, data = resultado, mensagem = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, mensagem = ex.Message });
            }
        }
        
        /// <summary>
        /// ✅ NOVO: Retorna apenas as filiais de uma empresa específica
        /// Garante consistência: colaborador só pode ter filial da sua empresa
        /// </summary>
        [HttpGet("[action]/{empresaId}", Name ="FiliaisDaEmpresa")]
        public List<Filial> FiliaisDaEmpresa(int empresaId)
        {
            return _negocio.ListarFiliaisDaEmpresa(empresaId);
        }
        
        /// <summary>
        /// ✅ NOVO: Retorna apenas as filiais de uma empresa em uma localidade específica
        /// Garante consistência: filial deve pertencer à empresa E à localidade
        /// </summary>
        [HttpGet("[action]/{empresaId}/{localidadeId}", Name ="FiliaisPorLocalidade")]
        public List<Filial> FiliaisPorLocalidade(int empresaId, int localidadeId)
        {
            return _negocio.ListarFiliaisPorLocalidade(empresaId, localidadeId);
        }
        #endregion

        #region TEMPLATES
        /***************************************************************************************************/
        /******************************************** TEMPLATES ********************************************/
        /***************************************************************************************************/
        [HttpGet("[action]", Name ="ListarTiposDeTemplates")]
        public List<Templatetipo> ListarTiposDeTemplates()
        {
            return _negocio.ListarTiposDeTemplate();
        }
        [HttpGet("[action]/{cliente}", Name ="ListarTemplates")]
        public List<Template> ListarTemplates(int cliente)
        {
            return _negocio.ListarTemplates(cliente);
        }
        
        [HttpGet("[action]/{cliente}/{tipo}", Name ="ListarTemplatesPorTipo")]
        public List<Template> ListarTemplatesPorTipo(int cliente, int tipo)
        {
            return _negocio.ListarTemplatesPorTipo(cliente, tipo);
        }
        [HttpGet("[action]/{id}", Name ="ObterTemplatePorID")]
        public Template ObterTemplatePorID(int id)
        {
            return _negocio.ObterTemplatePorId(id);
        }
        [HttpPost("[action]", Name ="VisualizarTemplate")]
        public IActionResult VisualizarTemplate(TemplateVM template)
        {
            try
            {
                // ✅ CORREÇÃO: Validar entrada antes de processar
                if (template == null)
                {
                    Console.WriteLine("[VISUALIZAR-TEMPLATE] ❌ Template é nulo");
                    return BadRequest(new { Mensagem = "Template não pode ser nulo", Status = "400" });
                }

                if (template.UsuarioLogado <= 0)
                {
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] ❌ Usuário inválido: {template.UsuarioLogado}");
                    return BadRequest(new { Mensagem = "Usuário logado inválido", Status = "400" });
                }

                if (string.IsNullOrEmpty(template.Conteudo))
                {
                    Console.WriteLine("[VISUALIZAR-TEMPLATE] ❌ Conteúdo vazio");
                    return BadRequest(new { Mensagem = "Conteúdo do template não pode ser vazio", Status = "400" });
                }

                // ✅ CORREÇÃO: Processar e retornar PDF como FileResult (compatível com byte[])
                var pdf = _negocio.VisualizarTemplate(template);
                
                if (pdf == null || pdf.Length == 0)
                {
                    Console.WriteLine("[VISUALIZAR-TEMPLATE] ❌ PDF vazio ou nulo");
                    return StatusCode(500, new { Mensagem = "Erro ao gerar PDF: resultado vazio", Status = "500" });
                }

                Console.WriteLine($"[VISUALIZAR-TEMPLATE] ✅ PDF gerado com sucesso - Tamanho: {pdf.Length} bytes");
                return File(pdf, "application/pdf", "template.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] ❌ Erro no controller: {ex.Message}");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] Tipo de exceção: {ex.GetType().Name}");
                Console.WriteLine($"[VISUALIZAR-TEMPLATE] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[VISUALIZAR-TEMPLATE] InnerException: {ex.InnerException.Message}");
                }
                
                // ✅ CORREÇÃO: Retornar JSON com Content-Type explícito para erros
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Detalhes: {ex.InnerException.Message}";
                }
                
                var errorResponse = new { 
                    Mensagem = $"Erro ao visualizar template: {errorMessage}", 
                    Status = "500",
                    Erro = ex.GetType().Name
                };
                
                // Garantir que retorna como JSON
                Response.ContentType = "application/json";
                return StatusCode(500, errorResponse);
            }
        }
        [HttpPost("[action]", Name ="SalvarTemplate")]
        public void SalvarTemplate(Template tmp)
        {
            _negocio.SalvarTemplate(tmp);
        }
        [HttpDelete("[action]/{id}", Name ="ExcluirTemplate")]
        public void ExcluirTemplate(int id)
        {
            _negocio.ExcluirTemplate(id);
        }
        #endregion

        #region PARAMETROS
        /***************************************************************************************************/
        /******************************************** PARAMETROS *******************************************/
        /***************************************************************************************************/

        [HttpGet("[action]/{cliente}", Name = "BuscarParametros")]
        public IActionResult BuscarParametros(int cliente)
        {
            try
            {
                var parametro = _negocio.ObterParametros(cliente);
                if (parametro == null)
                {
                    // Retornar um objeto com valores padrão ao invés de null para evitar erro 500
                    return Ok(new Parametro 
                    { 
                        Cliente = cliente,
                        EmailDescontosEnabled = false,
                        SmtpEnabled = false,
                        SmtpEnableSSL = false,
                        SmtpPort = 587,
                        TwoFactorEnabled = false,
                        TwoFactorType = "email",
                        TwoFactorExpirationMinutes = 5,
                        TwoFactorMaxAttempts = 3,
                        TwoFactorLockoutMinutes = 15,
                        TwoFactorEmailTemplate = "Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos."
                    });
                }
                return Ok(parametro);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PARAMETROS] Erro ao buscar parâmetros: {ex.Message}");
                return StatusCode(500, new { message = "Erro ao buscar parâmetros", error = ex.Message });
            }
        }

        [HttpPost("[action]", Name = "SalvarParametros")]
        public IActionResult SalvarParametros(Parametro p)
        {
            try
            {
                _negocio.SalvarParametro(p);
                
                // Buscar o registro atualizado para retornar com o ID correto
                var parametroAtualizado = _negocio.ObterParametros(p.Cliente);
                if (parametroAtualizado != null)
                {
                    return Ok(parametroAtualizado);
                }
                
                return Ok(new { message = "Parâmetros salvos com sucesso" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] Erro ao salvar parâmetros: {ex.Message}");
                return StatusCode(500, new { message = "Erro ao salvar parâmetros", error = ex.Message });
            }
        }

        [HttpPost("[action]", Name = "TestarConexaoSMTP")]
        public IActionResult TestarConexaoSMTP([FromBody] Parametro parametro)
        {
            try
            {
                // Validar se os campos obrigatórios estão preenchidos
                if (string.IsNullOrEmpty(parametro.SmtpHost) || 
                    !parametro.SmtpPort.HasValue || 
                    string.IsNullOrEmpty(parametro.SmtpLogin) || 
                    string.IsNullOrEmpty(parametro.SmtpPassword) || 
                    string.IsNullOrEmpty(parametro.SmtpEmailFrom))
                {
                    return BadRequest(new { success = false, message = "Todos os campos SMTP são obrigatórios para o teste." });
                }

                // Tentar conectar ao servidor SMTP
                using (var smtpClient = new SmtpClient(parametro.SmtpHost, parametro.SmtpPort.Value))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(parametro.SmtpLogin, parametro.SmtpPassword);
                    smtpClient.EnableSsl = parametro.SmtpEnableSSL ?? false;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Timeout = 10000; // 10 segundos para teste

                    // Configurar para ignorar problemas de certificado SSL durante o teste
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    // Testar a conexão tentando enviar um e-mail de teste
                    // Criar uma mensagem de teste simples
                    var testMessage = new MailMessage();
                    testMessage.From = new MailAddress(parametro.SmtpEmailFrom, "Teste SMTP");
                    testMessage.To.Add(parametro.SmtpEmailFrom); // Enviar para o próprio e-mail
                    testMessage.Subject = "Teste de Conexão SMTP - SingleOne";
                    testMessage.Body = "Este é um e-mail de teste para verificar a conexão SMTP. Se você recebeu este e-mail, a configuração está funcionando corretamente.";
                    testMessage.IsBodyHtml = false;

                    // Tentar enviar o e-mail de teste
                    smtpClient.Send(testMessage);
                    
                    return Ok(new { success = true, message = "Conexão SMTP testada com sucesso! E-mail de teste enviado." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Erro ao testar conexão SMTP: {ex.Message}" });
            }
        }
        #endregion

        #region LAUDO EVIDÊNCIAS
        /***************************************************************************************************/
        /**************************************** LAUDO EVIDÊNCIAS *****************************************/
        /***************************************************************************************************/

        [HttpGet("[action]/{laudoId}", Name = "ListarEvidenciasLaudo")]
        public IActionResult ListarEvidenciasLaudo(int laudoId)
        {
            try
            {
                var evidencias = _negocio.ListarEvidenciasLaudo(laudoId);
                return Ok(evidencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao listar evidências: " + ex.Message, Status = "500" });
            }
        }

        [HttpPost("[action]", Name = "UploadEvidenciaLaudo")]
        public async Task<IActionResult> UploadEvidenciaLaudo([FromForm] IFormFile arquivo, [FromForm] int laudoId, [FromForm] int usuarioId)
        {
            try
            {
                if (arquivo == null)
                {
                    return BadRequest(new { Mensagem = "Arquivo não pode ser nulo", Status = "400" });
                }

                if (laudoId <= 0)
                {
                    return BadRequest(new { Mensagem = "ID do laudo inválido", Status = "400" });
                }

                if (usuarioId <= 0)
                {
                    return BadRequest(new { Mensagem = "ID do usuário inválido", Status = "400" });
                }

                // Verificar se já existem 6 evidências para este laudo
                var evidenciasExistentes = _negocio.ListarEvidenciasLaudo(laudoId);
                if (evidenciasExistentes.Count >= 6)
                {
                    return BadRequest(new { Mensagem = "Limite máximo de 6 evidências por laudo atingido", Status = "400" });
                }

                // Validar tipo de arquivo (apenas imagens)
                var extensao = Path.GetExtension(arquivo.FileName).ToLower();
                var tiposPermitidos = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                if (!Array.Exists(tiposPermitidos, tipo => tipo == extensao))
                {
                    return BadRequest(new { Mensagem = "Tipo de arquivo não permitido. Apenas imagens são aceitas.", Status = "400" });
                }

                // Validar tamanho (máximo 10MB)
                if (arquivo.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { Mensagem = "Arquivo muito grande. Tamanho máximo permitido: 10MB", Status = "400" });
                }

                // Salvar arquivo usando FileUploadService
                var nomeArquivo = await _fileUploadService.UploadFileAsync(arquivo, "laudos");

                // Criar registro de evidência - APENAS com as colunas que existem na tabela
                var evidencia = new LaudoEvidencia
                {
                    laudo = laudoId,
                    nomearquivo = nomeArquivo,
                    ordem = _negocio.ObterProximaOrdemEvidencia(laudoId)
                };

                Console.WriteLine($"[CONTROLLER] Salvando evidência: laudo={evidencia.laudo}, nomearquivo={evidencia.nomearquivo}, ordem={evidencia.ordem}");
                
                _negocio.SalvarEvidenciaLaudo(evidencia);
                
                Console.WriteLine($"[CONTROLLER] Evidência salva com ID: {evidencia.Id}");

                return Ok(new { 
                    Mensagem = "Evidência enviada com sucesso!", 
                    Status = "200", 
                    Id = evidencia.Id,
                    NomeArquivo = nomeArquivo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao fazer upload da evidência: " + ex.Message, Status = "500" });
            }
        }

        [HttpDelete("[action]/{evidenciaId}", Name = "ExcluirEvidenciaLaudo")]
        public IActionResult ExcluirEvidenciaLaudo(int evidenciaId)
        {
            try
            {
                _negocio.ExcluirEvidenciaLaudo(evidenciaId);
                return Ok(new { Mensagem = "Evidência excluída com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao excluir evidência: " + ex.Message, Status = "500" });
            }
        }

        [HttpPost("[action]", Name = "ReordenarEvidenciasLaudo")]
        public IActionResult ReordenarEvidenciasLaudo([FromBody] ReordenarEvidenciasRequest request)
        {
            try
            {
                if (request == null || request.OrdemEvidencias == null || request.OrdemEvidencias.Count == 0)
                {
                    return BadRequest(new { Mensagem = "Dados para reordenação inválidos", Status = "400" });
                }

                _negocio.ReordenarEvidenciasLaudo(request.LaudoId, request.OrdemEvidencias);
                return Ok(new { Mensagem = "Evidências reordenadas com sucesso!", Status = "200" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao reordenar evidências: " + ex.Message, Status = "500" });
            }
        }

        [HttpGet("[action]/{evidenciaId}", Name = "DownloadEvidenciaLaudo")]
        public async Task<IActionResult> DownloadEvidenciaLaudo(int evidenciaId)
        {
            try
            {
                var evidencia = _negocio.ObterEvidenciaPorId(evidenciaId);
                if (evidencia == null)
                {
                    return NotFound(new { Mensagem = "Evidência não encontrada", Status = "404" });
                }

                var arquivo = await _fileUploadService.DownloadFileAsync(evidencia.nomearquivo, "laudos");
                if (arquivo == null)
                {
                    return NotFound(new { Mensagem = "Arquivo não encontrado no servidor", Status = "404" });
                }

                // Como não temos TipoArquivo e NomeOriginal, usar valores padrão
                var contentType = "application/octet-stream";
                var fileName = evidencia.nomearquivo;
                
                return File(arquivo, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensagem = "Erro ao fazer download da evidência: " + ex.Message, Status = "500" });
            }
        }
        #endregion

        #region POLÍTICAS DE ELEGIBILIDADE
        /***************************************************************************************************/
        /************************************** POLÍTICAS DE ELEGIBILIDADE ********************************/
        /***************************************************************************************************/

        /// <summary>
        /// Lista todas as políticas de elegibilidade de um cliente
        /// </summary>
        [HttpGet("[action]/{cliente}", Name = "ListarPoliticasElegibilidade")]
        public IActionResult ListarPoliticasElegibilidade(int cliente, [FromQuery] string tipoColaborador = null, [FromQuery] int? tipoEquipamentoId = null)
        {
            try
            {
                Console.WriteLine($"[ELEGIBILIDADE API] Listar políticas - Cliente: {cliente}");
                var politicas = _negocio.ListarPoliticasElegibilidade(cliente, tipoColaborador, tipoEquipamentoId);
                return Ok(politicas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao listar políticas de elegibilidade: " + ex.Message, Status = "500" });
            }
        }

        /// <summary>
        /// Busca uma política de elegibilidade específica por ID
        /// </summary>
        [HttpGet("[action]/{id}", Name = "BuscarPoliticaElegibilidade")]
        public IActionResult BuscarPoliticaElegibilidade(int id)
        {
            try
            {
                Console.WriteLine($"[ELEGIBILIDADE API] Buscar política ID: {id}");
                var politica = _negocio.BuscarPoliticaElegibilidadePorId(id);
                
                if (politica == null)
                {
                    return NotFound(new { Mensagem = "Política não encontrada", Status = "404" });
                }
                
                return Ok(politica);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao buscar política: " + ex.Message, Status = "500" });
            }
        }

        /// <summary>
        /// Cria ou atualiza uma política de elegibilidade
        /// </summary>
        [HttpPost("[action]", Name = "SalvarPoliticaElegibilidade")]
        public IActionResult SalvarPoliticaElegibilidade([FromBody] SingleOneAPI.Models.PoliticaElegibilidade politica)
        {
            try
            {
                Console.WriteLine($"[ELEGIBILIDADE API] Salvar política - ID: {politica.Id}");
                
                if (politica == null)
                {
                    return BadRequest(new { Mensagem = "Política não pode ser nula", Status = "400" });
                }

                var resultado = _negocio.SalvarPoliticaElegibilidade(politica);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                var status = resultadoObj.Status?.ToString();
                if (status == "200")
                {
                    return Ok(resultadoObj);
                }
                else if (status == "400" || status == "400.1")
                {
                    return BadRequest(resultadoObj);
                }
                else if (status == "404")
                {
                    return NotFound(resultadoObj);
                }
                else
                {
                    return StatusCode(500, resultadoObj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao salvar política: " + ex.Message, Status = "500" });
            }
        }

        /// <summary>
        /// Exclui (inativa) uma política de elegibilidade
        /// </summary>
        [HttpDelete("[action]/{id}", Name = "ExcluirPoliticaElegibilidade")]
        public IActionResult ExcluirPoliticaElegibilidade(int id)
        {
            try
            {
                Console.WriteLine($"[ELEGIBILIDADE API] Excluir política ID: {id}");
                
                var resultado = _negocio.ExcluirPoliticaElegibilidade(id);
                var resultadoObj = JsonConvert.DeserializeObject<dynamic>(resultado);
                
                var status = resultadoObj.Status?.ToString();
                if (status == "200")
                {
                    return Ok(resultadoObj);
                }
                else if (status == "404")
                {
                    return NotFound(resultadoObj);
                }
                else
                {
                    return StatusCode(500, resultadoObj);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao excluir política: " + ex.Message, Status = "500" });
            }
        }

        /// <summary>
        /// Verifica se um colaborador é elegível para um tipo de equipamento
        /// </summary>
        [HttpGet("[action]/{colaboradorId}/{tipoEquipamentoId}", Name = "VerificarElegibilidade")]
        public IActionResult VerificarElegibilidade(int colaboradorId, int tipoEquipamentoId)
        {
            try
            {
                Console.WriteLine($"[ELEGIBILIDADE API] Verificar elegibilidade - Colaborador: {colaboradorId}, Equipamento: {tipoEquipamentoId}");
                
                var elegivel = _negocio.VerificarElegibilidade(colaboradorId, tipoEquipamentoId);
                
                return Ok(new { 
                    Elegivel = elegivel, 
                    Mensagem = elegivel ? "Colaborador é elegível para este tipo de equipamento" : "Colaborador NÃO é elegível para este tipo de equipamento",
                    Status = "200" 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao verificar elegibilidade: " + ex.Message, Status = "500" });
            }
        }

        /// <summary>
        /// Retorna a lista de tipos de colaborador disponíveis (do banco de dados)
        /// </summary>
        [HttpGet("[action]", Name = "ListarTiposColaborador")]
        public IActionResult ListarTiposColaborador()
        {
            try
            {
                // Buscar tipos distintos de colaboradores ativos no banco
                var tiposDistintos = _negocio.ListarTiposColaboradorDistintos();
                
                Console.WriteLine($"[ELEGIBILIDADE API] Tipos de colaborador encontrados: {tiposDistintos.Count}");
                
                return Ok(tiposDistintos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ELEGIBILIDADE API] ❌ Erro: {ex.Message}");
                return StatusCode(500, new { Mensagem = "Erro ao listar tipos de colaborador: " + ex.Message, Status = "500" });
            }
        }

        #endregion
    }
}
