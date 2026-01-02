using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOne.Util;
using SingleOneAPI.Models.ViewModels;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequisicoesController : ControllerBase
    {
        private readonly IRequisicoesNegocio _negocio;
        private readonly IIpAddressService _ipAddressService;
        
        public RequisicoesController(IRequisicoesNegocio negocio, IIpAddressService ipAddressService)
        {
            _negocio = negocio;
            _ipAddressService = ipAddressService;
        }


        [HttpGet("[action]/{pesquisa}/{cliente}/{pagina}", Name="ListarRequisicoes")]
        public PagedResult<RequisicaoVM> ListarRequisicoes(string pesquisa, int cliente, int pagina)
        {
            return _negocio.ListarRequisicoes(pesquisa, cliente, pagina);
        }
        [HttpGet("[action]/{id}", Name = "BuscarRequisicaoPorID")]
        public RequisicaoVM BuscarRequisicaoPorID(int id)
        {
            return _negocio.BuscarRequisicaoPorId(id);
        }
        [HttpGet("[action]/{hash}/{byod}", Name = "ListarEquipamentosDaRequisicao")]
        [AllowAnonymous]
        public RequisicaoVM ListarEquipamentosDaRequisicao(string hash, bool byod)
        {
            Console.WriteLine($"[CONTROLLER] ListarEquipamentosDaRequisicao chamado - Hash: {hash}, BYOD: {byod}");
            var resultado = _negocio.ListarEquipamentosDaRequisicao(hash, byod);
            Console.WriteLine($"[CONTROLLER] ListarEquipamentosDaRequisicao retornou {resultado?.EquipamentosRequisicao?.Count ?? 0} recursos");
            return resultado;
        }
        [HttpPost("[action]", Name ="SalvarRequisicao")]
        public string SalvarRequisicao([FromBody] RequisicaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[CONTROLLER] SalvarRequisicao - Dados recebidos:");
                Console.WriteLine($"[CONTROLLER] - ID: {dto?.Id}");
                Console.WriteLine($"[CONTROLLER] - Cliente: {dto?.Cliente}");
                Console.WriteLine($"[CONTROLLER] - Status: {dto?.Requisicaostatus}");
                Console.WriteLine($"[CONTROLLER] - Usuario: {dto?.Usuariorequisicao}");
                Console.WriteLine($"[CONTROLLER] - Tecnico: {dto?.Tecnicoresponsavel}");
                Console.WriteLine($"[CONTROLLER] - Itens: {dto?.Requisicoesitens?.Count ?? 0}");
                
                // ✅ VALIDAÇÃO: Verificar campos obrigatórios
                if (dto.Tecnicoresponsavel <= 0)
                {
                    return "{\"Mensagem\": \"O campo Responsável Provisório é obrigatório.\", \"Status\": \"400\"}";
                }
                
                if (dto.Cliente <= 0)
                {
                    return "{\"Mensagem\": \"Cliente inválido.\", \"Status\": \"400\"}";
                }
                
                if (dto.Usuariorequisicao <= 0)
                {
                    return "{\"Mensagem\": \"Usuário da requisição inválido.\", \"Status\": \"400\"}";
                }
                
                if (dto.Requisicoesitens == null || dto.Requisicoesitens.Count == 0)
                {
                    return "{\"Mensagem\": \"Adicione pelo menos um recurso à requisição.\", \"Status\": \"400\"}";
                }
                
                // ✅ CORREÇÃO: Converter DTO para modelo
                var req = new Requisico
                {
                    Id = dto.Id,
                    Cliente = dto.Cliente,
                    Usuariorequisicao = dto.Usuariorequisicao,
                    Tecnicoresponsavel = dto.Tecnicoresponsavel,
                    Requisicaostatus = dto.Requisicaostatus,
                    Colaboradorfinal = dto.Colaboradorfinal,
                    Dtsolicitacao = dto.Dtsolicitacao ?? DateTime.Now,
                    Dtprocessamento = dto.Dtprocessamento,
                    Assinaturaeletronica = dto.Assinaturaeletronica,
                    Dtassinaturaeletronica = dto.Dtassinaturaeletronica,
                    Dtenviotermo = dto.Dtenviotermo,
                    Hashrequisicao = dto.Hashrequisicao,
                    Migrateid = dto.Migrateid
                };
                
                // ✅ CORREÇÃO: Converter itens
                if (dto.Requisicoesitens != null)
                {
                    foreach (var itemDto in dto.Requisicoesitens)
                    {
                        var item = new Requisicoesiten
                        {
                            Id = itemDto.Id,
                            Requisicao = itemDto.Requisicao,
                            Equipamento = itemDto.Equipamento,
                            Linhatelefonica = itemDto.Linhatelefonica,
                            Usuarioentrega = itemDto.Usuarioentrega,
                            Usuariodevolucao = itemDto.Usuariodevolucao,
                            Dtentrega = itemDto.Dtentrega,
                            Dtdevolucao = itemDto.Dtdevolucao,
                            Observacaoentrega = itemDto.Observacaoentrega,
                            Dtprogramadaretorno = itemDto.Dtprogramadaretorno
                        };
                        req.Requisicoesitens.Add(item);
                    }
                }
                
                var resultado = _negocio.SalvarRequisicao(req);
                Console.WriteLine($"[CONTROLLER] SalvarRequisicao - Resultado: {resultado}");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] ERRO em SalvarRequisicao: {ex.Message}");
                Console.WriteLine($"[CONTROLLER] Stack trace: {ex.StackTrace}");
                return $"{{\"Mensagem\": \"Erro interno do servidor: {ex.Message}\", \"Status\": \"500\"}}";
            }
        }
        [HttpPost("[action]", Name ="AceitarTermoResponsabilidade")]
        [AllowAnonymous]
        public async Task<string> AceitarTermoResponsabilidade(TermoEletronicoVM vm)
        {
            Console.WriteLine($"[CONTROLLER] Dados recebidos no controller:");
            Console.WriteLine($"[CONTROLLER] - IP (frontend): {vm.IpAddress}");
            Console.WriteLine($"[CONTROLLER] - País: {vm.Country}");
            Console.WriteLine($"[CONTROLLER] - Cidade: {vm.City}");
            Console.WriteLine($"[CONTROLLER] - Região: {vm.Region}");
            Console.WriteLine($"[CONTROLLER] - Latitude: {vm.Latitude}");
            Console.WriteLine($"[CONTROLLER] - Longitude: {vm.Longitude}");
            Console.WriteLine($"[CONTROLLER] - Precisão: {vm.Accuracy}");
            Console.WriteLine($"[CONTROLLER] - Timestamp: {vm.Timestamp}");
            
            // ✅ CORREÇÃO: Capturar IP do servidor se o frontend não enviou ou enviou "Não informado"
            if (string.IsNullOrEmpty(vm.IpAddress) || vm.IpAddress == "Não informado" || vm.IpAddress == "0.0.0.0")
            {
                try
                {
                    var serverIp = _ipAddressService.GetClientIpAddress(Request.HttpContext);
                    if (!string.IsNullOrEmpty(serverIp) && serverIp != "0.0.0.0")
                    {
                        vm.IpAddress = serverIp;
                        Console.WriteLine($"[CONTROLLER] ✅ IP capturado do servidor: {vm.IpAddress}");
                    }
                    else
                    {
                        // Fallback: tentar capturar diretamente
                        var remoteIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
                        if (!string.IsNullOrEmpty(remoteIp))
                        {
                            vm.IpAddress = remoteIp;
                            Console.WriteLine($"[CONTROLLER] ✅ IP capturado (fallback): {vm.IpAddress}");
                        }
                        else
                        {
                            Console.WriteLine($"[CONTROLLER] ⚠️ Não foi possível capturar IP do servidor");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CONTROLLER] ⚠️ Erro ao capturar IP do servidor: {ex.Message}");
                }
            }
            
            // ✅ CORREÇÃO: Garantir que timestamp seja preenchido se não foi enviado
            if (!vm.Timestamp.HasValue)
            {
                vm.Timestamp = DateTime.Now;
                Console.WriteLine($"[CONTROLLER] ✅ Timestamp preenchido automaticamente: {vm.Timestamp}");
            }
            
            Console.WriteLine($"[CONTROLLER] Dados finais que serão salvos:");
            Console.WriteLine($"[CONTROLLER] - IP: {vm.IpAddress}");
            Console.WriteLine($"[CONTROLLER] - País: {vm.Country ?? "Brasil"}");
            Console.WriteLine($"[CONTROLLER] - Cidade: {vm.City ?? "Não informado"}");
            Console.WriteLine($"[CONTROLLER] - Região: {vm.Region ?? "Não informado"}");
            Console.WriteLine($"[CONTROLLER] - Latitude: {vm.Latitude?.ToString() ?? "null"}");
            Console.WriteLine($"[CONTROLLER] - Longitude: {vm.Longitude?.ToString() ?? "null"}");
            Console.WriteLine($"[CONTROLLER] - Precisão: {vm.Accuracy?.ToString() ?? "null"}");
            Console.WriteLine($"[CONTROLLER] - Timestamp: {vm.Timestamp}");
            
            return await _negocio.AceitarTermoEntrega(vm);
        }

        [HttpGet("[action]/{cliente}", Name ="ListarEntregasDisponiveis")]
        public List<RequisicaoVM> ListarEntregasDisponiveis(int cliente)
        {
            return _negocio.ListarEntregasDisponiveis(cliente);
        }
        [HttpGet("[action]/{id}", Name = "BuscarEntregaPorID")]
        public Requisico BuscarEntregaPorID(int id)
        {
            return _negocio.BuscarEntregasDisponiveisPorID(id);
        }
        [HttpPost("[action]", Name ="RealizarEntrega")]
        public void RealizarEntrega(Requisico req)
        {
            _negocio.RealizarEntrega(req);
        }

        // Nova rota para entrega com co-responsáveis (somente NÃO-BYOD), recebendo DTO
        [HttpPost("[action]", Name ="RealizarEntregaComCompartilhados")]
        public IActionResult RealizarEntregaComCompartilhados([FromBody] RequisicaoDTO dto)
        {
            _negocio.RealizarEntregaComCompartilhados(dto);
            return Ok();
        }
        [HttpPost("[action]", Name = "RealizarEntregaMobile")]
        public string RealizarEntregaMobile(Requisico req)
        {
            return _negocio.RealizarEntregaMobile(req);
        }
        [HttpPost("[action]", Name = "TransferenciaEquipamento")]
        public void TransferenciaEquipamento(TransferenciaEqpVM vm)
        {
            _negocio.TransferenciaEquipamento(vm);
        }

        // ===================== COMPARTILHADOS POR ITEM (CO-RESPONSÁVEIS) =====================
        [HttpGet("[action]/{requisicaoItemId}")]
        public ActionResult<List<SingleOneAPI.Models.RequisicaoItemCompartilhado>> ListarCompartilhadosItem(int requisicaoItemId)
        {
            var lista = _negocio.ListarCompartilhadosItem(requisicaoItemId);
            return Ok(lista);
        }

        [HttpPost("[action]/{requisicaoItemId}")]
        public ActionResult<SingleOneAPI.Models.RequisicaoItemCompartilhado> AdicionarCompartilhadoItem(int requisicaoItemId, [FromBody] SingleOneAPI.Models.RequisicaoItemCompartilhado vinculo)
        {
            var usuarioIdClaim = User?.FindFirst("id")?.Value ?? User?.FindFirst("userid")?.Value ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int usuarioId = 0;
            int.TryParse(usuarioIdClaim, out usuarioId);
            var criado = _negocio.AdicionarCompartilhadoItem(requisicaoItemId, vinculo, usuarioId);
            return Ok(criado);
        }

        [HttpPut("[action]/{vinculoId}")]
        public ActionResult<SingleOneAPI.Models.RequisicaoItemCompartilhado> AtualizarCompartilhadoItem(int vinculoId, [FromBody] SingleOneAPI.Models.RequisicaoItemCompartilhado vinculo)
        {
            var atualizado = _negocio.AtualizarCompartilhadoItem(vinculoId, vinculo);
            return Ok(atualizado);
        }

        [HttpPatch("[action]/{vinculoId}/encerrar")]
        public IActionResult EncerrarCompartilhadoItem(int vinculoId)
        {
            var usuarioIdClaim = User?.FindFirst("id")?.Value ?? User?.FindFirst("userid")?.Value ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int usuarioId = 0;
            int.TryParse(usuarioIdClaim, out usuarioId);
            _negocio.EncerrarCompartilhadoItem(vinculoId, usuarioId);
            return Ok();
        }
        // ======================================================================================


        [HttpGet("[action]/{pesquisa}/{cliente}/{pagina}/{byod}", Name ="ListarDevolucoesDisponiveis")]
        public ActionResult<PagedResult<EntregaAtivaVM>> ListarDevolucoesDisponiveis(string pesquisa, int cliente, int pagina, bool byod)
        {
            try
            {
                var result = _negocio.ListarDevolucoesDisponiveis(pesquisa, cliente, pagina, byod);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("[action]", Name ="AtualizarItemRequisicao")]
        public void AtualizarItemRequisicao(Requisicaoequipamentosvm ri)
        {
            _negocio.AtualizarItemRequisicao(ri);
        }

        [HttpPost("[action]", Name = "AdicionarObservacaoEquipamentoVM")]
        public void AdicionarObservacaoEquipamentoVM(EquipamentoRequisicaoVM equipamento)
        {
            _negocio.AdicionarObservacaoEquipamentoVM(equipamento);
        }

        [HttpPost("[action]", Name = "AdicionarAgendamentoEquipamentoVM")]
        public void AdicionarAgendamentoEquipamentoVM(EquipamentoRequisicaoVM equipamento)
        {
            _negocio.AdicionarAgendamentoEquipamentoVM(equipamento);
        }

        [HttpPost("[action]", Name ="RealizarDevolucaoEquipamento")]
        public void RealizarDevolucaoEquipamento(EquipamentoRequisicaoVM equipamento)
        {
            _negocio.RealizarDevolucaoEquipamento(equipamento);
        }
        [HttpPost("[action]/{byod}", Name = "RealizarDevolucoesDoColaborador")]
        public void RealizarDevolucoesDoColaborador(RequisicaoVM vm, bool byod)
        {
            _negocio.RealizarDevolucoesDoColaborador(vm.ColaboradorId, vm.UsuarioDevolucaoId, byod);
        }

    }
}
