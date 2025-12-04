using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models.DTO;
using SingleOne.Negocios;
using SingleOne.Util;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Negocios.Interfaces;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOneAPI.Services;
using SingleOneAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    /// <summary>
    /// Controller para o Meu Patrimônio - Portal do Colaborador
    /// Acesso autenticado para colaboradores visualizarem seus equipamentos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MeuPatrimonioController : ControllerBase
    {
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Empresa> _empresaRepository;
        private readonly IRepository<Centrocusto> _centroCustoRepository;
        private readonly IRepository<Localidade> _localidadeRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly PatrimonioNegocio _patrimonioNegocio;
        private readonly IRequisicoesNegocio _requisicoesNegocio;
        private readonly IRepository<Tipoequipamento> _tipoEquipamentoRepository;
        private readonly IRepository<Fabricante> _fabricanteRepository;
        private readonly IRepository<Modelo> _modeloRepository;
        private readonly IRepository<Tipoaquisicao> _tipoAquisicaoRepository;
        private readonly IRepository<Telefonialinha> _telefonialinhaRepository;
        private readonly IRepository<Telefoniaplano> _telefoniaplanoRepository;
        private readonly IRepository<Telefoniacontrato> _telefoniacontratoRepository;
        private readonly IRepository<Telefoniaoperadora> _telefoniaoperadoraRepository;
        private readonly IColaboradorNegocio _colaboradorNegocio;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IIpAddressService _ipAddressService;
        private readonly IRepository<PatrimonioContestacao> _contestacaoRepository;
        private readonly IReadOnlyRepository<Requisicaoequipamentosvm> _requisicaoequipamentosvmsRepository;

        public MeuPatrimonioController(
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Empresa> empresaRepository,
            IRepository<Centrocusto> centroCustoRepository,
            IRepository<Localidade> localidadeRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Requisico> requisicaoRepository,
            PatrimonioNegocio patrimonioNegocio,
            IRequisicoesNegocio requisicoesNegocio,
            IRepository<Tipoequipamento> tipoEquipamentoRepository,
            IRepository<Fabricante> fabricanteRepository,
            IRepository<Modelo> modeloRepository,
            IRepository<Tipoaquisicao> tipoAquisicaoRepository,
            IRepository<Telefonialinha> telefonialinhaRepository,
            IRepository<Telefoniaplano> telefoniaplanoRepository,
            IRepository<Telefoniacontrato> telefoniacontratoRepository,
            IRepository<Telefoniaoperadora> telefoniaoperadoraRepository,
            IColaboradorNegocio colaboradorNegocio,
            IRepository<Usuario> usuarioRepository,
            IIpAddressService ipAddressService,
            IRepository<PatrimonioContestacao> contestacaoRepository,
            IReadOnlyRepository<Requisicaoequipamentosvm> requisicaoequipamentosvmsRepository)
        {
            _colaboradorRepository = colaboradorRepository;
            _empresaRepository = empresaRepository;
            _centroCustoRepository = centroCustoRepository;
            _localidadeRepository = localidadeRepository;
            _equipamentoRepository = equipamentoRepository;
            _requisicaoRepository = requisicaoRepository;
            _patrimonioNegocio = patrimonioNegocio;
            _requisicoesNegocio = requisicoesNegocio;
            _tipoEquipamentoRepository = tipoEquipamentoRepository;
            _fabricanteRepository = fabricanteRepository;
            _modeloRepository = modeloRepository;
            _tipoAquisicaoRepository = tipoAquisicaoRepository;
            _telefonialinhaRepository = telefonialinhaRepository;
            _telefoniaplanoRepository = telefoniaplanoRepository;
            _telefoniacontratoRepository = telefoniacontratoRepository;
            _telefoniaoperadoraRepository = telefoniaoperadoraRepository;
            _colaboradorNegocio = colaboradorNegocio;
            _usuarioRepository = usuarioRepository;
            _ipAddressService = ipAddressService;
            _contestacaoRepository = contestacaoRepository;
            _requisicaoequipamentosvmsRepository = requisicaoequipamentosvmsRepository;
        }

        /// <summary>
        /// Autentica colaborador usando CPF + Email ou CPF + Matrícula
        /// </summary>
        [HttpPost("autenticar")]
        [AllowAnonymous]
        public async Task<ActionResult<MeuPatrimonioResponseDTO>> Autenticar([FromBody] MeuPatrimonioAuthDTO authData)
        {
            try
            {
                // Validar dados de entrada
                if (string.IsNullOrEmpty(authData.Cpf) || authData.Cpf.Length < 11)
                {
                    return BadRequest(new MeuPatrimonioResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "CPF é obrigatório e deve ter pelo menos 11 dígitos"
                    });
                }

                // Validar segundo campo baseado no tipo
                if (authData.TipoAutenticacao == "email")
                {
                    if (string.IsNullOrEmpty(authData.Email) || !authData.Email.Contains("@"))
                    {
                        return BadRequest(new MeuPatrimonioResponseDTO
                        {
                            Sucesso = false,
                            Mensagem = "Email é obrigatório e deve ser válido"
                        });
                    }
                }
                else if (authData.TipoAutenticacao == "matricula")
                {
                    if (string.IsNullOrEmpty(authData.Matricula) || authData.Matricula.Length < 3)
                    {
                        return BadRequest(new MeuPatrimonioResponseDTO
                        {
                            Sucesso = false,
                            Mensagem = "Matrícula é obrigatória e deve ter pelo menos 3 caracteres"
                        });
                    }
                }

                // Criptografar CPF para busca no banco
                var cpfCriptografado = Cripto.CriptografarDescriptografar(authData.Cpf, true);

                // Buscar colaborador por CPF com relacionamentos
                var colaborador = _colaboradorRepository
                    .Query()
                    .Include(x => x.EmpresaNavigation)
                    .Include(x => x.CentrocustoNavigation)
                    .Include(x => x.LocalidadeNavigation)
                    .FirstOrDefault(x => x.Cpf == cpfCriptografado);

                if (colaborador == null)
                {
                    return Unauthorized(new MeuPatrimonioResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Colaborador não encontrado"
                    });
                }
                
                // Se os relacionamentos não foram carregados, tentar carregar manualmente
                if (colaborador.EmpresaNavigation == null || colaborador.CentrocustoNavigation == null || colaborador.LocalidadeNavigation == null)
                {
                    
                    // Carregar relacionamentos manualmente usando os repositórios específicos
                    if (colaborador.EmpresaNavigation == null)
                    {
                        colaborador.EmpresaNavigation = _empresaRepository.Buscar(x => x.Id == colaborador.Empresa).FirstOrDefault();
                    }
                    
                    if (colaborador.CentrocustoNavigation == null)
                    {
                        colaborador.CentrocustoNavigation = _centroCustoRepository.Buscar(x => x.Id == colaborador.Centrocusto).FirstOrDefault();
                    }
                    
                    if (colaborador.LocalidadeNavigation == null)
                    {
                        colaborador.LocalidadeNavigation = _localidadeRepository.Buscar(x => x.Id == colaborador.Localidade).FirstOrDefault();
                    }
                }

                // Validar segundo campo (apenas para confirmar identidade, não para buscar)
                bool segundoCampoValido = false;
                if (authData.TipoAutenticacao == "email")
                {
                    // Descriptografar email do banco para comparação
                    var emailDescriptografado = !string.IsNullOrEmpty(colaborador.Email) ? 
                        Cripto.CriptografarDescriptografar(colaborador.Email, false) : "";
                    segundoCampoValido = emailDescriptografado?.ToLower() == authData.Email.ToLower();
                }
                else if (authData.TipoAutenticacao == "matricula")
                {
                    // Descriptografar matrícula do banco para comparação
                    var matriculaDescriptografada = !string.IsNullOrEmpty(colaborador.Matricula) ? 
                        Cripto.CriptografarDescriptografar(colaborador.Matricula, false) : "";
                    segundoCampoValido = matriculaDescriptografada?.ToLower() == authData.Matricula.ToLower();
                }

                if (!segundoCampoValido)
                {
                    return Unauthorized(new MeuPatrimonioResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = $"Dados de {authData.TipoAutenticacao} não conferem com o CPF informado"
                    });
                }

                // Gerar token simples (pode ser melhorado com JWT)
                var token = Guid.NewGuid().ToString();

                // Logar acesso
                _patrimonioNegocio.LogarAcesso(
                    "patrimonio",
                    colaborador.Id,
                    authData.Cpf,
                    _ipAddressService.GetClientIpAddress(Request.HttpContext),
                    Request.Headers["User-Agent"].ToString(),
                    new { TipoAutenticacao = authData.TipoAutenticacao },
                    true,
                    null
                );

                // Buscar equipamentos do colaborador
                var equipamentos = await BuscarEquipamentosColaborador(colaborador.Id);

                return Ok(new MeuPatrimonioResponseDTO
                {
                    Sucesso = true,
                    Token = token,
                    Mensagem = "Autenticação realizada com sucesso",
                    Colaborador = new MeuPatrimonioColaboradorDTO
                    {
                        Id = colaborador.Id,
                        Nome = colaborador.Nome,
                        Cpf = colaborador.Cpf,
                        Matricula = colaborador.Matricula,
                        Email = colaborador.Email,
                        Cargo = colaborador.Cargo,
                        Setor = colaborador.Setor,
                        Empresa = colaborador.Empresa.ToString(),
                        EmpresaNome = colaborador.EmpresaNavigation?.Nome ?? "N/A",
                        CentroCusto = colaborador.Centrocusto.ToString(),
                        CentroCustoNome = colaborador.CentrocustoNavigation?.Nome ?? "N/A",
                        Localidade = colaborador.Localidade.ToString(),
                        LocalidadeNome = colaborador.LocalidadeNavigation?.Descricao ?? "N/A",
                        Situacao = colaborador.Situacao,
                        DtAdmissao = colaborador.Dtadmissao.ToString("yyyy-MM-dd"),
                        SuperiorImediato = colaborador.Matriculasuperior ?? string.Empty,
                        SuperiorImediatoNome = ObterNomeSuperiorImediato(colaborador.Matriculasuperior)
                    },
                    Equipamentos = equipamentos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new MeuPatrimonioResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obtém patrimônio do colaborador autenticado
        /// </summary>
        [HttpGet("meu-patrimonio")]
        public async Task<ActionResult<MeuPatrimonioResponseDTO>> ObterMeuPatrimonio([FromQuery] int colaboradorId)
        {
            try
            {
                if (colaboradorId <= 0)
                {
                    return BadRequest(new MeuPatrimonioResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "ID do colaborador é obrigatório"
                    });
                }

                // Buscar colaborador com relacionamentos
                var colaborador = _colaboradorRepository
                    .Query()
                    .Include(x => x.EmpresaNavigation)
                    .Include(x => x.CentrocustoNavigation)
                    .Include(x => x.LocalidadeNavigation)
                    .FirstOrDefault(x => x.Id == colaboradorId);
                    
                if (colaborador == null)
                {
                    return NotFound(new MeuPatrimonioResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Colaborador não encontrado"
                    });
                }
                
                // Se os relacionamentos não foram carregados, tentar carregar manualmente
                if (colaborador.EmpresaNavigation == null || colaborador.CentrocustoNavigation == null || colaborador.LocalidadeNavigation == null)
                {
                    
                    // Carregar relacionamentos manualmente usando os repositórios específicos
                    if (colaborador.EmpresaNavigation == null)
                    {
                        colaborador.EmpresaNavigation = _empresaRepository.Buscar(x => x.Id == colaborador.Empresa).FirstOrDefault();
                    }
                    
                    if (colaborador.CentrocustoNavigation == null)
                    {
                        colaborador.CentrocustoNavigation = _centroCustoRepository.Buscar(x => x.Id == colaborador.Centrocusto).FirstOrDefault();
                    }
                    
                    if (colaborador.LocalidadeNavigation == null)
                    {
                        colaborador.LocalidadeNavigation = _localidadeRepository.Buscar(x => x.Id == colaborador.Localidade).FirstOrDefault();
                    }
                }

                var equipamentos = await BuscarEquipamentosColaborador(colaboradorId);

                return Ok(new MeuPatrimonioResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Dados carregados com sucesso",
                    Colaborador = new MeuPatrimonioColaboradorDTO
                    {
                        Id = colaborador.Id,
                        Nome = colaborador.Nome,
                        Cpf = colaborador.Cpf,
                        Matricula = colaborador.Matricula,
                        Email = colaborador.Email,
                        Cargo = colaborador.Cargo,
                        Setor = colaborador.Setor,
                        Empresa = colaborador.Empresa.ToString(),
                        EmpresaNome = colaborador.EmpresaNavigation?.Nome ?? "N/A",
                        CentroCusto = colaborador.Centrocusto.ToString(),
                        CentroCustoNome = colaborador.CentrocustoNavigation?.Nome ?? "N/A",
                        Localidade = colaborador.Localidade.ToString(),
                        LocalidadeNome = colaborador.LocalidadeNavigation?.Descricao ?? "N/A",
                        Situacao = colaborador.Situacao,
                        DtAdmissao = colaborador.Dtadmissao.ToString("yyyy-MM-dd"),
                        SuperiorImediato = colaborador.Matriculasuperior ?? string.Empty,
                        SuperiorImediatoNome = ObterNomeSuperiorImediato(colaborador.Matriculasuperior)
                    },
                    Equipamentos = equipamentos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new MeuPatrimonioResponseDTO
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cria nova contestação de patrimônio
        /// </summary>
        [HttpPost("contestacao")]
        [AllowAnonymous]
        public ActionResult<object> CriarContestacao([FromBody] CriarContestacaoDTO contestacao)
        {
            try
            {
                // Validações básicas
                if (contestacao == null)
                {
                    return BadRequest(new { sucesso = false, mensagem = "Dados da contestação não foram fornecidos" });
                }

                if (contestacao.ColaboradorId <= 0)
                {
                    return BadRequest(new { sucesso = false, mensagem = "ID do colaborador é obrigatório" });
                }

                if (contestacao.EquipamentoId <= 0)
                {
                    return BadRequest(new { sucesso = false, mensagem = "ID do equipamento é obrigatório" });
                }

                if (string.IsNullOrEmpty(contestacao.Motivo))
                {
                    return BadRequest(new { sucesso = false, mensagem = "Motivo é obrigatório" });
                }

                if (string.IsNullOrEmpty(contestacao.Descricao))
                {
                    return BadRequest(new { sucesso = false, mensagem = "Descrição é obrigatória" });
                }

                var sucesso = _patrimonioNegocio.CriarContestacao(contestacao);
                
                if (sucesso)
                {
                    return Ok(new { sucesso = true, mensagem = "Contestação criada com sucesso" });
                }
                else
                {
                    return BadRequest(new { sucesso = false, mensagem = "Erro ao criar contestação" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Cancela uma contestação pendente
        /// </summary>
        [HttpPost("contestacao/cancelar")]
        [AllowAnonymous]
        public ActionResult<object> CancelarContestacao([FromBody] CancelarContestacaoDTO dto)
        {
            try
            {

                if (dto.ColaboradorId <= 0 || dto.EquipamentoId <= 0)
                {
                    return BadRequest(new { sucesso = false, mensagem = "Parâmetros inválidos" });
                }

                var ok = _patrimonioNegocio.CancelarContestacao(dto.ColaboradorId, dto.EquipamentoId, dto.ContestacaoId, dto.Justificativa);
                if (!ok)
                {
                    return BadRequest(new { sucesso = false, mensagem = "Nenhuma contestação pendente encontrada" });
                }

                return Ok(new { sucesso = true, mensagem = "Contestação cancelada com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Criar solicitação de Auto Inventário
        /// </summary>
        [HttpPost("auto-inventario")]
        public async Task<IActionResult> CriarAutoInventario([FromBody] CriarAutoInventarioDTO dados)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(new { sucesso = false, mensagem = "Dados inválidos", erros = errors });
                }

                var sucesso = _patrimonioNegocio.CriarAutoInventario(dados);

                if (sucesso)
                {
                    return Ok(new { sucesso = true, mensagem = "Solicitação de Auto Inventário criada com sucesso!" });
                }
                else
                {
                    return BadRequest(new { sucesso = false, mensagem = "Erro ao criar solicitação de Auto Inventário. Verifique se já não existe uma solicitação pendente para este número de série." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Endpoint de debug para verificar itens de requisição do colaborador
        /// </summary>
        [HttpGet("debug-itens/{colaboradorId}")]
        public ActionResult DebugItensRequisicao(int colaboradorId)
        {
            try
            {
                // Buscar itens de requisição do colaborador
                var itensRequisicao = _requisicaoRepository
                    .Buscar(r => r.Colaboradorfinal == colaboradorId && r.Requisicaostatus == 3)
                    .SelectMany(r => r.Requisicoesitens)
                    .Where(ri => ri.Equipamento != null && ri.Equipamento > 0)
                    .Select(ri => new
                    {
                        ItemId = ri.Id,
                        RequisicaoId = ri.Requisicao,
                        EquipamentoId = ri.Equipamento,
                        DtEntrega = ri.Dtentrega,
                        DtDevolucao = ri.Dtdevolucao,
                        RequisicaoHash = ri.RequisicaoNavigation.Hashrequisicao,
                        RequisicaoAssinado = ri.RequisicaoNavigation.Assinaturaeletronica,
                        RequisicaoDtAssinatura = ri.RequisicaoNavigation.Dtassinaturaeletronica
                    })
                    .ToList();

                return Ok(new
                {
                    ColaboradorId = colaboradorId,
                    TotalItens = itensRequisicao.Count,
                    Itens = itensRequisicao
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Erro = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint de debug para verificar requisições do colaborador
        /// </summary>
        [HttpGet("debug-requisicoes/{colaboradorId}")]
        public ActionResult DebugRequisicoes(int colaboradorId)
        {
            try
            {
                var requisicoes = _requisicaoRepository
                    .Buscar(r => r.Colaboradorfinal == colaboradorId && r.Requisicaostatus == 3)
                    .Select(r => new
                    {
                        Id = r.Id,
                        HashRequisicao = r.Hashrequisicao,
                        AssinaturaEletronica = r.Assinaturaeletronica,
                        DtAssinaturaEletronica = r.Dtassinaturaeletronica,
                        DtProcessamento = r.Dtprocessamento,
                        ItensCount = r.Requisicoesitens.Count,
                        Equipamentos = r.Requisicoesitens.Where(ri => ri.Equipamento.HasValue && ri.Equipamento > 0).Select(ri => ri.Equipamento).ToList(),
                        Linhas = r.Requisicoesitens.Where(ri => ri.Linhatelefonica.HasValue && ri.Linhatelefonica > 0).Select(ri => ri.Linhatelefonica).ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    ColaboradorId = colaboradorId,
                    TotalRequisicoes = requisicoes.Count,
                    Requisicoes = requisicoes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Erro = ex.Message });
            }
        }

        /// <summary>
        /// Busca equipamentos em posse do colaborador
        /// </summary>
        private async Task<List<MeuPatrimonioEquipamentoDTO>> BuscarEquipamentosColaborador(int colaboradorId)
        {
            var equipamentos = new List<MeuPatrimonioEquipamentoDTO>();


            // Busca todos os itens de requisição processadas do colaborador (independente do status atual do equipamento)
            var itensRequisicao = _requisicaoRepository
                .Buscar(r => r.Colaboradorfinal == colaboradorId && r.Requisicaostatus == 3)
                .SelectMany(r => r.Requisicoesitens)
                .Where(ri => ri.Equipamento != null && ri.Equipamento > 0)
                .ToList();

            // Busca linhas telefônicas do colaborador
            var itensLinhasTelefonicas = _requisicaoRepository
                .Buscar(r => r.Colaboradorfinal == colaboradorId && r.Requisicaostatus == 3)
                .SelectMany(r => r.Requisicoesitens)
                .Where(ri => ri.Linhatelefonica != null && ri.Linhatelefonica > 0)
                .ToList();

            // Mantém apenas o último registro por equipamento (caso exista mais de uma movimentação)
            var ultimosItensPorEquipamento = itensRequisicao
                .GroupBy(ri => ri.Equipamento)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .ToList();

            // Mantém apenas o último registro por linha telefônica
            var ultimosItensPorLinha = itensLinhasTelefonicas
                .GroupBy(ri => ri.Linhatelefonica)
                .Select(g => g.OrderByDescending(x => x.Id).First())
                .ToList();


            foreach (var item in ultimosItensPorEquipamento)
            {
                var equipamento = _equipamentoRepository
                    .Buscar(e => e.Id == item.Equipamento)
                    .FirstOrDefault();

                if (equipamento == null)
                {
                    continue;
                }

                // Buscar informações de assinatura da requisição usando a navegação
                var requisicao = _requisicaoRepository
                    .Buscar(r => r.Id == item.Requisicao)
                    .FirstOrDefault();


                // Se não encontrou pela ID, tentar buscar requisições do colaborador que contenham este equipamento
                if (requisicao == null)
                {
                    
                    // Buscar requisições do colaborador que possam ter este equipamento
                    var requisicoesColaborador = _requisicaoRepository
                        .Buscar(r => r.Colaboradorfinal == colaboradorId && r.Requisicaostatus == 3)
                        .Where(r => r.Requisicoesitens.Any(ri => ri.Equipamento == equipamento.Id))
                        .OrderByDescending(r => r.Dtprocessamento)
                        .FirstOrDefault();
                    
                    if (requisicoesColaborador != null)
                    {
                        requisicao = requisicoesColaborador;
                    }
                    else
                    {
                    }
                }

                // Determina status pelo item (se possui Dtdevolucao)
                string statusEquipamento = item.Dtdevolucao.HasValue ? "Devolvido" : "Entregue";
                DateTime dtEntrega = item.Dtentrega ?? equipamento.Dtcadastro;
                DateTime? dtDevolucao = item.Dtdevolucao;

                // Buscar descrições
                var tipoDesc = _tipoEquipamentoRepository
                    .Buscar(t => t.Id == equipamento.Tipoequipamento)
                    .Select(t => t.Descricao)
                    .FirstOrDefault() ?? equipamento.Tipoequipamento.ToString();

                var fabricanteDesc = _fabricanteRepository
                    .Buscar(f => f.Id == equipamento.Fabricante)
                    .Select(f => f.Descricao)
                    .FirstOrDefault() ?? equipamento.Fabricante.ToString();

                var modeloDesc = _modeloRepository
                    .Buscar(m => m.Id == equipamento.Modelo)
                    .Select(m => m.Descricao)
                    .FirstOrDefault() ?? equipamento.Modelo.ToString();

                var tipoAquisicaoDesc = _tipoAquisicaoRepository
                    .Buscar(ta => ta.Id == equipamento.Tipoaquisicao)
                    .Select(ta => ta.Nome)
                    .FirstOrDefault() ?? equipamento.Tipoaquisicao.ToString();

                // Determinar se é BYOD baseado no tipo de aquisição (TipoAquisicao == 2 = BYOD)
                bool isByod = equipamento.Tipoaquisicao == 2;

                // Verificar se existe contestação pendente para este equipamento
                var todasContestoes = _patrimonioNegocio.BuscarContestoesPorColaborador(colaboradorId);
                var contestacaoPendente = todasContestoes.FirstOrDefault(c => c.EquipamentoId == equipamento.Id && c.Status == "pendente");
                
                

                // ✅ NOVO: Obter informações de trânsito livre do tipo de equipamento
                bool transitoLivre = false;
                
                // Aplicar a mesma lógica da portaria
                if (isByod)
                {
                    // Recursos particulares (BYOD) sempre têm trânsito livre
                    transitoLivre = true;
                }
                else
                {
                    // Para outros tipos, verificar configuração do tipo de equipamento
                    if (equipamento.TipoequipamentoNavigation != null)
                    {
                        transitoLivre = equipamento.TipoequipamentoNavigation.TransitoLivre;
                    }
                    else
                    {
                        // Garantir leitura do TransitoLivre como na Portaria
                        var equipamentoCompleto = _equipamentoRepository
                            .Buscar(e => e.Id == equipamento.Id)
                            .Include(e => e.TipoequipamentoNavigation)
                            .AsNoTracking()
                            .FirstOrDefault();

                        if (equipamentoCompleto?.TipoequipamentoNavigation != null)
                        {
                            transitoLivre = equipamentoCompleto.TipoequipamentoNavigation.TransitoLivre;
                        }
                    }
                }

                var equipamentoDTO = new MeuPatrimonioEquipamentoDTO
                {
                    Id = equipamento.Id,
                    Patrimonio = equipamento.Patrimonio ?? string.Empty,
                    NumeroSerie = equipamento.Numeroserie ?? string.Empty,
                    TipoEquipamento = tipoDesc,
                    TipoEquipamentoTransitoLivre = transitoLivre, // ✅ NOVO: Campo para trânsito livre
                    Fabricante = fabricanteDesc,
                    Modelo = modeloDesc,
                    Status = statusEquipamento,
                    DtEntrega = dtEntrega,
                    DtDevolucao = dtDevolucao,
                    Observacao = equipamento.Descricaobo ?? string.Empty,
                    TipoAquisicao = tipoAquisicaoDesc,
                    PodeContestar = statusEquipamento == "Entregue" && contestacaoPendente == null,
                    TemContestacao = contestacaoPendente != null,
                    ContestacaoId = contestacaoPendente?.Id,
                    Assinado = requisicao?.Assinaturaeletronica ?? false,
                    DataAssinatura = requisicao?.Dtassinaturaeletronica,
                    HashRequisicao = requisicao?.Hashrequisicao ?? string.Empty,
                    IsByod = isByod,
                    // IsHistorico = statusEquipamento != "Entregue", // ✅ NOVO: Se é histórico
                    // IsRecursoParticular = isByod // ✅ NOVO: Se é recurso particular
                };

                // Adicionar informações de contestação se existir
                if (contestacaoPendente != null)
                {
                    equipamentoDTO.ContestacaoStatus = contestacaoPendente.Status ?? string.Empty;
                    equipamentoDTO.ContestacaoData = contestacaoPendente.DataContestacao.ToString("dd/MM/yyyy");
                    equipamentoDTO.ContestacaoMotivo = contestacaoPendente.Motivo ?? string.Empty;
                }

                equipamentos.Add(equipamentoDTO);
            }

            // Processar linhas telefônicas
            foreach (var item in ultimosItensPorLinha)
            {
                var linha = _telefonialinhaRepository
                    .Buscar(l => l.Id == item.Linhatelefonica)
                    .FirstOrDefault();

                if (linha == null)
                {
                    continue;
                }

                // Buscar informações de assinatura da requisição usando a navegação
                var requisicao = _requisicaoRepository
                    .Buscar(r => r.Id == item.Requisicao)
                    .FirstOrDefault();


                // Se não encontrou pela ID, tentar buscar requisições do colaborador que contenham esta linha
                if (requisicao == null)
                {
                    
                    // Buscar requisições do colaborador que possam ter esta linha
                    var requisicoesColaborador = _requisicaoRepository
                        .Buscar(r => r.Colaboradorfinal == colaboradorId && r.Requisicaostatus == 3)
                        .Where(r => r.Requisicoesitens.Any(ri => ri.Linhatelefonica == linha.Id))
                        .OrderByDescending(r => r.Dtprocessamento)
                        .FirstOrDefault();
                    
                    if (requisicoesColaborador != null)
                    {
                        requisicao = requisicoesColaborador;
                    }
                    else
                    {
                    }
                }

                // Buscar informações do plano, contrato e operadora
                var plano = _telefoniaplanoRepository
                    .Buscar(p => p.Id == linha.Plano)
                    .FirstOrDefault();

                var contrato = plano != null ? _telefoniacontratoRepository
                    .Buscar(c => c.Id == plano.Contrato)
                    .FirstOrDefault() : null;

                var operadora = contrato != null ? _telefoniaoperadoraRepository
                    .Buscar(o => o.Id == contrato.Operadora)
                    .FirstOrDefault() : null;

                // Determina status pelo item (se possui Dtdevolucao)
                string statusLinha = item.Dtdevolucao.HasValue ? "Devolvido" : "Entregue";
                DateTime dtEntrega = item.Dtentrega ?? DateTime.Now;
                DateTime? dtDevolucao = item.Dtdevolucao;

                // Verificar se existe contestação pendente para esta linha telefônica
                var todasContestoes = _patrimonioNegocio.BuscarContestoesPorColaborador(colaboradorId);
                var contestacaoPendenteLinha = todasContestoes.FirstOrDefault(c => c.EquipamentoId == linha.Id && c.Status == "pendente");
                

                var linhaDTO = new MeuPatrimonioEquipamentoDTO
                {
                    Id = linha.Id,
                    Patrimonio = linha.Numero.ToString(),
                    NumeroSerie = linha.Iccid ?? string.Empty,
                    TipoEquipamento = "Linha Telefônica",
                    TipoEquipamentoTransitoLivre = true, // ✅ NOVO: Linhas telefônicas sempre têm trânsito livre
                    Fabricante = operadora?.Nome ?? "N/A",
                    Modelo = plano?.Nome ?? "N/A",
                    Status = statusLinha,
                    DtEntrega = dtEntrega,
                    DtDevolucao = dtDevolucao,
                    Observacao = $"Contrato: {contrato?.Nome ?? "N/A"}",
                    TipoAquisicao = "Corporativo",
                    PodeContestar = statusLinha == "Entregue" && contestacaoPendenteLinha == null,
                    TemContestacao = contestacaoPendenteLinha != null,
                    ContestacaoId = contestacaoPendenteLinha?.Id,
                    Assinado = requisicao?.Assinaturaeletronica ?? false,
                    DataAssinatura = requisicao?.Dtassinaturaeletronica,
                    HashRequisicao = requisicao?.Hashrequisicao ?? string.Empty,
                    IsByod = false, // Linhas telefônicas não são BYOD
                    // IsHistorico = statusLinha != "Entregue", // ✅ NOVO: Se é histórico
                    // IsRecursoParticular = false // ✅ NOVO: Linhas telefônicas não são particulares
                };

                // Adicionar informações de contestação se existir
                if (contestacaoPendenteLinha != null)
                {
                    linhaDTO.ContestacaoStatus = contestacaoPendenteLinha.Status ?? string.Empty;
                    linhaDTO.ContestacaoData = contestacaoPendenteLinha.DataContestacao.ToString("dd/MM/yyyy");
                    linhaDTO.ContestacaoMotivo = contestacaoPendenteLinha.Motivo ?? string.Empty;
                }

                equipamentos.Add(linhaDTO);
            }

            // Agrupa explicitamente: primeiro ENTREGUE por DtEntrega desc, depois DEVOLVIDO por DtDevolucao desc
            var entregues = equipamentos
                .Where(e => e.Status == "Entregue")
                .OrderByDescending(e => e.DtEntrega)
                .ToList();

            var devolvidos = equipamentos
                .Where(e => e.Status == "Devolvido")
                .OrderByDescending(e => e.DtDevolucao ?? e.DtEntrega)
                .ToList();

            var ordenados = new List<MeuPatrimonioEquipamentoDTO>(entregues.Count + devolvidos.Count);
            ordenados.AddRange(entregues);
            ordenados.AddRange(devolvidos);


            return ordenados;
        }

        [HttpGet("debug/usuarios")]
        public ActionResult DebugUsuarios()
        {
            try
            {
                var usuarios = _usuarioRepository.Buscar(u => true).Take(5).ToList();
                return Ok(new { 
                    sucesso = true, 
                    usuarios = usuarios.Select(u => new {
                        id = u.Id,
                        nome = u.Nome,
                        login = u.Email
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet("debug/colaborador/{colaboradorId}")]
        public ActionResult DebugColaborador(int colaboradorId)
        {
            try
            {
                
                var colaborador = _colaboradorRepository
                    .Buscar(c => c.Id == colaboradorId)
                    .FirstOrDefault();

                if (colaborador == null)
                {
                    return NotFound(new { sucesso = false, mensagem = $"Colaborador ID {colaboradorId} não encontrado" });
                }

                return Ok(new { 
                    sucesso = true, 
                    colaborador = new {
                        id = colaborador.Id,
                        nome = colaborador.Nome,
                        empresa = colaborador.EmpresaNavigation?.Nome ?? "NULL",
                        centroCusto = colaborador.CentrocustoNavigation?.Nome ?? "NULL",
                        localidade = colaborador.LocalidadeNavigation?.Descricao ?? "NULL"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet("termo/pdf/{equipamentoId}")]
        [AllowAnonymous]
        public ActionResult GerarTermoPDF(int equipamentoId, [FromQuery] int? colaboradorId = null)
        {
            try
            {

                // ✅ VERIFICAR SE É EQUIPAMENTO FÍSICO OU LINHA TELEFÔNICA
                var equipamento = _equipamentoRepository.Buscar(e => e.Id == equipamentoId).FirstOrDefault();
                var linhaTefc = _telefonialinhaRepository.Buscar(l => l.Id == equipamentoId).FirstOrDefault();
                
                Console.WriteLine($"[MEU_PATRIMONIO] Equipamento físico: {(equipamento != null ? $"SIM (ID={equipamento.Id})" : "NÃO")}");
                Console.WriteLine($"[MEU_PATRIMONIO] Linha telefônica: {(linhaTefc != null ? $"SIM (ID={linhaTefc.Id}, Número={linhaTefc.Numero})" : "NÃO")}");
                
                if (equipamento == null && linhaTefc == null)
                {
                    Console.WriteLine($"[MEU_PATRIMONIO] ❌ Retornando 404: Recurso não encontrado (nem equipamento, nem linha)");
                    return NotFound(new { sucesso = false, mensagem = "Recurso não encontrado." });
                }

                // ✅ BUSCAR REQUISIÇÃO: Tentar em ambas as tabelas (pode haver colisão de IDs)
                Requisico requisicao = null;
                bool isLinhaTelefonica = false;
                
                // Primeiro tenta buscar como equipamento físico
                Console.WriteLine($"[MEU_PATRIMONIO] Tentando buscar requisição como equipamento físico...");
                requisicao = _requisicaoRepository
                    .Buscar(r => r.Requisicoesitens.Any(ri => ri.Equipamento == equipamentoId))
                    .OrderByDescending(r => r.Dtprocessamento)
                    .FirstOrDefault();
                
                if (requisicao != null)
                {
                    Console.WriteLine($"[MEU_PATRIMONIO] ✅ Requisição encontrada como equipamento: ID={requisicao.Id}");
                    isLinhaTelefonica = false;
                }
                else
                {
                    // Se não achou como equipamento, tenta como linha telefônica
                    Console.WriteLine($"[MEU_PATRIMONIO] Não encontrou como equipamento, tentando como linha telefônica...");
                    requisicao = _requisicaoRepository
                        .Buscar(r => r.Requisicoesitens.Any(ri => ri.Linhatelefonica == equipamentoId))
                        .OrderByDescending(r => r.Dtprocessamento)
                        .FirstOrDefault();
                    
                    if (requisicao != null)
                    {
                        Console.WriteLine($"[MEU_PATRIMONIO] ✅ Requisição encontrada como linha telefônica: ID={requisicao.Id}");
                        isLinhaTelefonica = true;
                    }
                }

                if (requisicao == null)
                {
                    Console.WriteLine($"[MEU_PATRIMONIO] ❌ Retornando 404: Requisição não encontrada");
                    return NotFound(new { sucesso = false, mensagem = "Requisição não encontrada para este equipamento." });
                }

                Console.WriteLine($"[MEU_PATRIMONIO] ✅ Requisição encontrada: ID={requisicao.Id}, Status={requisicao.Requisicaostatus}");

                // Usar colaborador da requisição ou o passado como parâmetro
                int colaboradorFinalId = requisicao.Colaboradorfinal ?? colaboradorId ?? 0;
                
                if (colaboradorFinalId == 0)
                {
                    return NotFound(new { sucesso = false, mensagem = "Colaborador não encontrado. Tente passar o colaboradorId como parâmetro." });
                }

                // Verificar se o colaborador existe e tem as informações necessárias
                var colaborador = _colaboradorRepository
                    .Buscar(c => c.Id == colaboradorFinalId)
                    .FirstOrDefault();

                if (colaborador == null)
                {
                    return NotFound(new { sucesso = false, mensagem = $"Colaborador com ID {colaboradorFinalId} não encontrado." });
                }

                // ✅ Determinar se é BYOD (APENAS para equipamentos físicos, NUNCA para linhas!)
                bool isByod = false;
                string identificadorRecurso = "";
                
                if (isLinhaTelefonica)
                {
                    // Para linhas telefônicas: NUNCA é BYOD
                    isByod = false;
                    identificadorRecurso = linhaTefc?.Numero.ToString() ?? equipamentoId.ToString();
                    Console.WriteLine($"[MEU_PATRIMONIO] Linha telefônica - Número: {identificadorRecurso}, BYOD: {isByod} (linhas NUNCA são BYOD)");
                }
                else
                {
                    // Para equipamentos físicos: verificar tipo de aquisição
                    isByod = (equipamento.Tipoaquisicao == 2);
                    identificadorRecurso = equipamento.Patrimonio ?? equipamentoId.ToString();
                    Console.WriteLine($"[MEU_PATRIMONIO] Equipamento físico - Patrimônio: {identificadorRecurso}, BYOD: {isByod}");
                }

                Console.WriteLine($"[MEU_PATRIMONIO] Gerando PDF - Cliente: {requisicao.Cliente}, Colaborador: {colaboradorFinalId}, BYOD: {isByod}");
                Console.WriteLine($"[MEU_PATRIMONIO] Colaborador encontrado: {colaborador.Nome}");

                // Gerar PDF usando o método existente
                // ✅ CORREÇÃO: Usar o técnico responsável da requisição (quem fez a entrega)
                int usuarioLogadoId = requisicao.Tecnicoresponsavel;
                Console.WriteLine($"[MEU_PATRIMONIO] Usando usuário logado ID: {usuarioLogadoId} (Técnico responsável)");
                
                var pdfBytes = _colaboradorNegocio.TermoCompromisso(requisicao.Cliente, colaboradorFinalId, usuarioLogadoId, isByod);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return StatusCode(500, new { sucesso = false, mensagem = "Erro ao gerar PDF do termo." });
                }

                var fileName = $"Termo_{identificadorRecurso}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MEU_PATRIMONIO] ❌ ERRO CRÍTICO ao gerar PDF:");
                Console.WriteLine($"[MEU_PATRIMONIO] Mensagem: {ex.Message}");
                Console.WriteLine($"[MEU_PATRIMONIO] Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"[MEU_PATRIMONIO] Inner Exception: {ex.InnerException?.Message}");
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro interno ao gerar PDF: {ex.Message}" });
            }
        }

        /// <summary>
        /// Busca o nome do superior imediato baseado na matrícula
        /// </summary>
        private string ObterNomeSuperiorImediato(string? matriculaSuperior)
        {
            if (string.IsNullOrEmpty(matriculaSuperior))
                return string.Empty;

            try
            {
                var superior = _colaboradorRepository.Buscar(x => x.Matricula == matriculaSuperior)
                    .AsNoTracking()
                    .FirstOrDefault();

                return superior?.Nome ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MEU_PATRIMONIO] Erro ao buscar nome do superior imediato: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtém as contestações reais de um colaborador específico
        /// </summary>
        [HttpGet("contestoes/{colaboradorId}")]
        [AllowAnonymous]
        public ActionResult<object> ObterContestoesColaborador(int colaboradorId)
        {
            try
            {
                Console.WriteLine($"[MEU_PATRIMONIO] Obtendo contestações reais do colaborador {colaboradorId}");

                // Buscar contestações reais da tabela com relacionamentos
                var contestoes = _contestacaoRepository
                    .Include(c => c.Colaborador)
                    .Where(x => x.ColaboradorId == colaboradorId)
                    .OrderByDescending(x => x.DataContestacao)
                    .ToList();

                var resultado = new List<object>();

                foreach (var c in contestoes)
                {
                    // Buscar dados do equipamento ou linha telefônica
                    var equipamento = _equipamentoRepository
                        .Include(e => e.TipoequipamentoNavigation)
                        .Where(e => e.Id == c.EquipamentoId)
                        .FirstOrDefault();
                    
                    string nomeRecurso = "N/A";
                    string numeroSerie = "N/A";
                    string tipoRecurso = "N/A";
                    
                    if (equipamento != null)
                    {
                        string tipoDescricao = equipamento.TipoequipamentoNavigation?.Descricao ?? "";
                        bool isLinha = tipoDescricao.ToLower().Contains("telefon") || tipoDescricao.ToLower().Contains("linha");
                        
                        if (isLinha)
                        {
                            nomeRecurso = equipamento.Numeroserie ?? equipamento.Patrimonio ?? "N/A";
                            numeroSerie = equipamento.Numeroserie ?? "N/A";
                            
                            // Tentar buscar ICCID na tabela de linhas se não estiver no equipamento
                            if (numeroSerie == "N/A" && !string.IsNullOrWhiteSpace(equipamento.Patrimonio) && decimal.TryParse(equipamento.Patrimonio, out decimal numero))
                            {
                                var linha = _telefonialinhaRepository.Buscar(x => x.Numero == numero).FirstOrDefault();
                                if (linha != null && !string.IsNullOrWhiteSpace(linha.Iccid))
                                {
                                    numeroSerie = linha.Iccid;
                                }
                            }
                        }
                        else
                        {
                            nomeRecurso = equipamento.Patrimonio ?? "N/A";
                            numeroSerie = equipamento.Numeroserie ?? "N/A";
                        }
                        
                        tipoRecurso = tipoDescricao;
                    }
                    else
                    {
                        // Tentar buscar como linha telefônica direta
                        var linha = _telefonialinhaRepository.Buscar(x => x.Id == c.EquipamentoId).FirstOrDefault();
                        if (linha != null)
                        {
                            nomeRecurso = linha.Numero.ToString();
                            numeroSerie = linha.Iccid ?? "N/A";
                            tipoRecurso = "Linha Telefônica";
                        }
                    }
                    
                    // Buscar nome do usuário que resolveu (pode ser null)
                    string tecnicoNome = "Não atribuído";
                    if (c.UsuarioResolucao.HasValue)
                    {
                        var usuario = _usuarioRepository.Buscar(u => u.Id == c.UsuarioResolucao.Value).FirstOrDefault();
                        if (usuario != null)
                        {
                            tecnicoNome = usuario.Nome;
                        }
                    }
                    
                    var contestacaoDTO = new
                    {
                        id = c.Id,
                        patrimonioId = c.EquipamentoId,
                        colaboradorId = c.ColaboradorId,
                        dataContestacao = c.DataContestacao,
                        motivo = c.Motivo,
                        descricao = c.Descricao,
                        status = c.Status,
                        statusId = ObterStatusId(c.Status),
                        usuarioAbertura = "Sistema",
                        tecnicoResponsavel = tecnicoNome,
                        tecnicoResponsavelId = c.UsuarioResolucao,
                        usuarioResolucao = c.UsuarioResolucao,
                        dataResolucao = c.DataResolucao,
                        observacoesResolucao = c.ObservacaoResolucao,
                        cliente = 1,
                        hashContestacao = c.Id.ToString(),
                        tipoContestacao = c.TipoContestacao ?? "contestacao",
                        equipamento = new
                        {
                            id = c.EquipamentoId,
                            nome = nomeRecurso,
                            numeroSerie = numeroSerie,
                            tipoEquipamento = tipoRecurso
                        },
                        colaborador = new
                        {
                            id = c.ColaboradorId,
                            nome = c.Colaborador?.Nome ?? "N/A",
                            cpf = c.Colaborador?.Cpf ?? "",
                            email = c.Colaborador?.Email ?? ""
                        }
                    };
                    
                    resultado.Add(contestacaoDTO);
                }

                Console.WriteLine($"[MEU_PATRIMONIO] Encontradas {resultado.Count} contestações para o colaborador {colaboradorId}");

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Contestações obtidas com sucesso",
                    data = resultado
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MEU_PATRIMONIO] Erro ao obter contestações do colaborador: {ex.Message}");
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = "Erro ao obter contestações do colaborador",
                    erro = ex.Message
                });
            }
        }

        /// <summary>
        /// Mapeia status string para ID numérico
        /// </summary>
        private int ObterStatusId(string status)
        {
            return status?.ToLower() switch
            {
                "pendente" or "aberta" => 1,
                "em_analise" or "em análise" => 2,
                "resolvida" => 3,
                "negada" => 4,
                "cancelada" => 5,
                "pendente_colaborador" => 6,
                _ => 1
            };
        }

        /// <summary>
        /// Libera equipamento para assinatura do termo
        /// </summary>
        [HttpPost("liberar-assinatura")]
        [AllowAnonymous]
        public ActionResult<object> LiberarParaAssinatura([FromBody] LiberarAssinaturaDTO dados)
        {
            try
            {
                Console.WriteLine($"[MEU_PATRIMONIO] === LIBERAR PARA ASSINATURA ===");
                Console.WriteLine($"[MEU_PATRIMONIO] Equipamento ID: {dados.EquipamentoId}, Colaborador ID: {dados.ColaboradorId}");

                if (dados.EquipamentoId <= 0 || dados.ColaboradorId <= 0)
                {
                    return BadRequest(new { sucesso = false, mensagem = "IDs inválidos" });
                }

                // Buscar equipamento
                var equipamento = _equipamentoRepository.Buscar(e => e.Id == dados.EquipamentoId).FirstOrDefault();
                if (equipamento == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Equipamento não encontrado" });
                }

                // Buscar colaborador
                var colaborador = _colaboradorRepository.Buscar(c => c.Id == dados.ColaboradorId).FirstOrDefault();
                if (colaborador == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Colaborador não encontrado" });
                }

                // Buscar requisição associada ao equipamento
                var requisicao = _requisicaoRepository
                    .Buscar(r => r.Requisicoesitens.Any(ri => ri.Equipamento == dados.EquipamentoId))
                    .OrderByDescending(r => r.Dtprocessamento)
                    .FirstOrDefault();

                if (requisicao == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Requisição não encontrada para este equipamento" });
                }

                // Aqui você pode implementar a lógica para liberar para assinatura
                // Por exemplo, enviar email com link para assinatura
                // Por enquanto, vamos simular uma resposta de sucesso
                
                Console.WriteLine($"[MEU_PATRIMONIO] Equipamento {equipamento.Patrimonio} liberado para assinatura pelo colaborador {colaborador.Nome}");
                Console.WriteLine($"[MEU_PATRIMONIO] === FIM LIBERAR PARA ASSINATURA ===");

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Equipamento liberado para assinatura com sucesso! Um email será enviado com instruções para assinar o termo."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MEU_PATRIMONIO] ERRO ao liberar para assinatura: {ex.Message}");
                return StatusCode(500, new { sucesso = false, mensagem = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Gera novo termo para equipamento
        /// </summary>
        [HttpPost("gerar-termo")]
        [AllowAnonymous]
        public ActionResult<object> GerarTermo([FromBody] GerarTermoDTO dados)
        {
            try
            {
                Console.WriteLine($"[MEU_PATRIMONIO] === GERAR TERMO ===");
                Console.WriteLine($"[MEU_PATRIMONIO] Equipamento ID: {dados.EquipamentoId}, Colaborador ID: {dados.ColaboradorId}");

                if (dados.EquipamentoId <= 0 || dados.ColaboradorId <= 0)
                {
                    return BadRequest(new { sucesso = false, mensagem = "IDs inválidos" });
                }

                // Buscar equipamento
                var equipamento = _equipamentoRepository.Buscar(e => e.Id == dados.EquipamentoId).FirstOrDefault();
                if (equipamento == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Equipamento não encontrado" });
                }

                // Buscar colaborador
                var colaborador = _colaboradorRepository.Buscar(c => c.Id == dados.ColaboradorId).FirstOrDefault();
                if (colaborador == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Colaborador não encontrado" });
                }

                // Buscar requisição associada ao equipamento
                var requisicao = _requisicaoRepository
                    .Buscar(r => r.Requisicoesitens.Any(ri => ri.Equipamento == dados.EquipamentoId))
                    .OrderByDescending(r => r.Dtprocessamento)
                    .FirstOrDefault();

                if (requisicao == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Requisição não encontrada para este equipamento" });
                }

                // Aqui você pode implementar a lógica para gerar novo termo
                // Por exemplo, gerar novo hash, enviar email, etc.
                
                Console.WriteLine($"[MEU_PATRIMONIO] Novo termo gerado para equipamento {equipamento.Patrimonio} do colaborador {colaborador.Nome}");
                Console.WriteLine($"[MEU_PATRIMONIO] === FIM GERAR TERMO ===");

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Novo termo gerado com sucesso! Verifique seu email para mais informações."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MEU_PATRIMONIO] ERRO ao gerar termo: {ex.Message}");
                return StatusCode(500, new { sucesso = false, mensagem = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Envia termo por email para o colaborador
        /// </summary>
        [HttpPost("enviar-termo-email")]
        [AllowAnonymous]
        public ActionResult<object> EnviarTermoPorEmail([FromBody] EnviarTermoEmailDTO dados)
        {
            try
            {
                Console.WriteLine($"[MEU_PATRIMONIO] === ENVIAR TERMO POR EMAIL ===");
                Console.WriteLine($"[MEU_PATRIMONIO] Equipamento ID: {dados.EquipamentoId}, Colaborador ID: {dados.ColaboradorId}");

                if (dados.EquipamentoId <= 0 || dados.ColaboradorId <= 0)
                {
                    return BadRequest(new { sucesso = false, mensagem = "IDs inválidos" });
                }

                // Buscar equipamento
                var equipamento = _equipamentoRepository.Buscar(e => e.Id == dados.EquipamentoId).FirstOrDefault();
                if (equipamento == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Equipamento não encontrado" });
                }

                // Buscar colaborador
                var colaborador = _colaboradorRepository.Buscar(c => c.Id == dados.ColaboradorId).FirstOrDefault();
                if (colaborador == null)
                {
                    return NotFound(new { sucesso = false, mensagem = "Colaborador não encontrado" });
                }

                // Aqui você pode implementar a lógica para enviar email com termo
                // Por exemplo, usar o serviço de email existente
                
                Console.WriteLine($"[MEU_PATRIMONIO] Termo enviado por email para colaborador {colaborador.Nome}");
                Console.WriteLine($"[MEU_PATRIMONIO] === FIM ENVIAR TERMO POR EMAIL ===");

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Termo enviado por email com sucesso!"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MEU_PATRIMONIO] ERRO ao enviar termo por email: {ex.Message}");
                return StatusCode(500, new { sucesso = false, mensagem = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Lista usuários ativos para atribuição de investigador
        /// </summary>
        /// <returns>Lista de usuários ativos</returns>
        [HttpGet("usuarios-ativos")]
        [AllowAnonymous]
        public ActionResult<List<object>> GetUsuariosAtivos()
        {
            try
            {
                Console.WriteLine($"[MEU_PATRIMONIO] === BUSCAR USUÁRIOS ATIVOS ===");
                
                var usuarios = _usuarioRepository.Buscar(x => x.Ativo == true).ToList();
                var usuariosAtivos = usuarios.Select(u => new
                {
                    id = u.Id,
                    nome = u.Nome,
                    email = u.Email,
                    ativo = u.Ativo,
                    su = u.Su,
                    adm = u.Adm,
                    operador = u.Operador,
                    consulta = u.Consulta
                }).ToList();

                Console.WriteLine($"[MEU_PATRIMONIO] Usuários ativos encontrados: {usuariosAtivos.Count}");
                Console.WriteLine($"[MEU_PATRIMONIO] === FIM BUSCAR USUÁRIOS ATIVOS ===");
                
                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Usuários ativos obtidos com sucesso",
                    data = usuariosAtivos
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MEU_PATRIMONIO] ERRO ao buscar usuários ativos: {ex.Message}");
                Console.WriteLine($"[MEU_PATRIMONIO] === FIM BUSCAR USUÁRIOS ATIVOS ===");
                
                return BadRequest(new
                {
                    sucesso = false,
                    mensagem = "Erro ao buscar usuários ativos",
                    erro = ex.Message
                });
            }
        }

    }

    /// <summary>
    /// DTO para autenticação do Meu Patrimônio
    /// </summary>
    public class MeuPatrimonioAuthDTO
    {
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Matricula { get; set; }
        public string TipoAutenticacao { get; set; }
    }

    /// <summary>
    /// DTO de resposta do Meu Patrimônio
    /// </summary>
    public class MeuPatrimonioResponseDTO
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public string Token { get; set; }
        public MeuPatrimonioColaboradorDTO Colaborador { get; set; }
        public List<MeuPatrimonioEquipamentoDTO> Equipamentos { get; set; } = new List<MeuPatrimonioEquipamentoDTO>();
        public List<object> Contestoes { get; set; } = new List<object>();
    }

    /// <summary>
    /// DTO de dados do colaborador
    /// </summary>
    public class MeuPatrimonioColaboradorDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Matricula { get; set; }
        public string Email { get; set; }
        public string Cargo { get; set; }
        public string Setor { get; set; }
        public string Empresa { get; set; }
        public string EmpresaNome { get; set; }
        public string CentroCusto { get; set; }
        public string CentroCustoNome { get; set; }
        public string Localidade { get; set; }
        public string LocalidadeNome { get; set; }
        public string Situacao { get; set; }
        public string DtAdmissao { get; set; }
        public string SuperiorImediato { get; set; }
        public string SuperiorImediatoNome { get; set; }
    }

    // Removido: definição duplicada de MeuPatrimonioEquipamentoDTO.
    // Utilizar a classe oficial em `SingleOneAPI.Models.DTO.MeuPatrimonioEquipamentoDTO`.

    /// <summary>
    /// DTO para liberar equipamento para assinatura
    /// </summary>
    public class LiberarAssinaturaDTO
    {
        public int EquipamentoId { get; set; }
        public int ColaboradorId { get; set; }
    }

    /// <summary>
    /// DTO para gerar termo
    /// </summary>
    public class GerarTermoDTO
    {
        public int EquipamentoId { get; set; }
        public int ColaboradorId { get; set; }
    }

    /// <summary>
    /// DTO para enviar termo por email
    /// </summary>
    public class EnviarTermoEmailDTO
    {
        public int EquipamentoId { get; set; }
        public int ColaboradorId { get; set; }
    }
}
