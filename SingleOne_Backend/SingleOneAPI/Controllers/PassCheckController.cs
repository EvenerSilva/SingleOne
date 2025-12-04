using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models.DTO;
using SingleOne.Negocios;
using SingleOne.Util;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Negocios.Interfaces;
using SingleOne.Models.ViewModels;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOneAPI.Controllers
{
    /// <summary>
    /// Controller para o PassCheck - Portal da Portaria
    /// Acesso público para consulta de colaboradores e equipamentos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PassCheckController : ControllerBase
    {
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Empresa> _empresaRepository;
        private readonly IRepository<Centrocusto> _centroCustoRepository;
        private readonly IRepository<Localidade> _localidadeRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Tipoaquisicao> _tipoAquisicaoRepository;
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly IRepository<Tipoequipamento> _tipoEquipamentoRepository;
        private readonly IRepository<Fabricante> _fabricanteRepository;
        private readonly IRepository<Modelo> _modeloRepository;
        private readonly IRepository<Telefonialinha> _telefonialinhaRepository;
        private readonly IRepository<Telefoniaplano> _telefoniaplanoRepository;
        private readonly IRepository<Telefoniacontrato> _telefoniacontratoRepository;
        private readonly IRepository<Telefoniaoperadora> _telefoniaoperadoraRepository;
        private readonly PatrimonioNegocio _patrimonioNegocio;
        private readonly IRequisicoesNegocio _requisicoesNegocio;
        private readonly ISinalizacaoSuspeitaNegocio _sinalizacaoNegocio;
        private readonly IIpAddressService _ipAddressService;

        public PassCheckController(
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Empresa> empresaRepository,
            IRepository<Centrocusto> centroCustoRepository,
            IRepository<Localidade> localidadeRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Tipoaquisicao> tipoAquisicaoRepository,
            IRepository<Requisico> requisicaoRepository,
            IRepository<Tipoequipamento> tipoEquipamentoRepository,
            IRepository<Fabricante> fabricanteRepository,
            IRepository<Modelo> modeloRepository,
            IRepository<Telefonialinha> telefonialinhaRepository,
            IRepository<Telefoniaplano> telefoniaplanoRepository,
            IRepository<Telefoniacontrato> telefoniacontratoRepository,
            IRepository<Telefoniaoperadora> telefoniaoperadoraRepository,
            PatrimonioNegocio patrimonioNegocio,
            IRequisicoesNegocio requisicoesNegocio,
            ISinalizacaoSuspeitaNegocio sinalizacaoNegocio,
            IIpAddressService ipAddressService)
        {
            _colaboradorRepository = colaboradorRepository;
            _empresaRepository = empresaRepository;
            _centroCustoRepository = centroCustoRepository;
            _localidadeRepository = localidadeRepository;
            _equipamentoRepository = equipamentoRepository;
            _tipoAquisicaoRepository = tipoAquisicaoRepository;
            _requisicaoRepository = requisicaoRepository;
            _tipoEquipamentoRepository = tipoEquipamentoRepository;
            _fabricanteRepository = fabricanteRepository;
            _modeloRepository = modeloRepository;
            _telefonialinhaRepository = telefonialinhaRepository;
            _telefoniaplanoRepository = telefoniaplanoRepository;
            _telefoniacontratoRepository = telefoniacontratoRepository;
            _telefoniaoperadoraRepository = telefoniaoperadoraRepository;
            _patrimonioNegocio = patrimonioNegocio;
            _requisicoesNegocio = requisicoesNegocio;
            _sinalizacaoNegocio = sinalizacaoNegocio;
            _ipAddressService = ipAddressService;
        }

        /// <summary>
        /// Consulta colaborador por CPF (acesso público) - Usando lógica de entregas existente
        /// </summary>
        /// <param name="cpf">CPF do colaborador</param>
        /// <returns>Dados do colaborador e equipamentos em posse</returns>
        [HttpGet("consultar/{cpf}")]
        [AllowAnonymous]
        public async Task<ActionResult<PassCheckResponseDTO>> ConsultarColaborador(string cpf)
        {
            try
            {
                Console.WriteLine($"[PASSCHECK] Iniciando consulta para CPF: {cpf}");
                
                // Validar CPF
                if (string.IsNullOrEmpty(cpf) || cpf.Length < 11)
                {
                    return BadRequest(new PassCheckResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "CPF inválido"
                    });
                }

                // Limpar CPF (remover pontos e hífens)
                cpf = cpf.Replace(".", "").Replace("-", "").Trim();
                
                // Criptografar CPF para busca no banco (CPFs são armazenados em Base64)
                string cpfCriptografado = Cripto.CriptografarDescriptografar(cpf, true);
                
                // Buscar colaborador com relacionamentos
                Console.WriteLine($"[PASSCHECK] Buscando colaborador com CPF criptografado: {cpfCriptografado}");
                
                // Tentar abordagem com contexto direto
                var colaborador = _colaboradorRepository
                    .Query()
                    .Include(x => x.EmpresaNavigation)
                    .Include(x => x.CentrocustoNavigation)
                    .Include(x => x.LocalidadeNavigation)
                    .FirstOrDefault(x => x.Cpf == cpfCriptografado);
                    
                // Se os relacionamentos não foram carregados, tentar carregar manualmente
                if (colaborador != null && (colaborador.EmpresaNavigation == null || colaborador.CentrocustoNavigation == null || colaborador.LocalidadeNavigation == null))
                {
                    Console.WriteLine($"[PASSCHECK] ⚠️ Relacionamentos não carregados automaticamente, tentando carregar manualmente...");
                    
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
                    
                Console.WriteLine($"[PASSCHECK] Colaborador carregado do banco: {colaborador?.Nome ?? "NULL"}");
                
                // Verificar se os relacionamentos foram carregados
                if (colaborador != null)
                {
                    Console.WriteLine($"[PASSCHECK] 🔍 Verificando relacionamentos carregados...");
                    Console.WriteLine($"[PASSCHECK] Empresa ID: {colaborador.Empresa} - Navigation: {colaborador.EmpresaNavigation?.Nome ?? "NULL"}");
                    Console.WriteLine($"[PASSCHECK] Centro Custo ID: {colaborador.Centrocusto} - Navigation: {colaborador.CentrocustoNavigation?.Nome ?? "NULL"}");
                    Console.WriteLine($"[PASSCHECK] Localidade ID: {colaborador.Localidade} - Navigation: {colaborador.LocalidadeNavigation?.Descricao ?? "NULL"}");
                    
                    // Verificar se os IDs são válidos
                    if (colaborador.Empresa <= 0)
                        Console.WriteLine($"[PASSCHECK] ⚠️ AVISO: ID da empresa inválido: {colaborador.Empresa}");
                    if (colaborador.Centrocusto <= 0)
                        Console.WriteLine($"[PASSCHECK] ⚠️ AVISO: ID do centro de custo inválido: {colaborador.Centrocusto}");
                    if (colaborador.Localidade <= 0)
                        Console.WriteLine($"[PASSCHECK] ⚠️ AVISO: ID da localidade inválido: {colaborador.Localidade}");
                }
                if (colaborador == null)
                {
                    Console.WriteLine($"[PASSCHECK] Colaborador não encontrado para CPF: {cpf}");
                    
                    // Log do acesso (mesmo sem sucesso)
                    _patrimonioNegocio.LogarAcesso("passcheck", null, cpf, 
                        _ipAddressService.GetClientIpAddress(Request.HttpContext),
                        Request.Headers["User-Agent"].ToString(), null, false, "Colaborador não encontrado");
                    
                    return NotFound(new PassCheckResponseDTO
                    {
                        Sucesso = false,
                        Mensagem = "Colaborador não encontrado"
                    });
                }

                Console.WriteLine($"[PASSCHECK] Colaborador encontrado: {colaborador.Nome}");
                Console.WriteLine($"[PASSCHECK] Empresa: {colaborador.Empresa} - Nome: {colaborador.EmpresaNavigation?.Nome ?? "NULL"}");
                Console.WriteLine($"[PASSCHECK] Centro Custo: {colaborador.Centrocusto} - Nome: {colaborador.CentrocustoNavigation?.Nome ?? "NULL"}");
                Console.WriteLine($"[PASSCHECK] Localidade: {colaborador.Localidade} - Nome: {colaborador.LocalidadeNavigation?.Descricao ?? "NULL"}");
                
                // Verificar se os relacionamentos foram carregados
                if (colaborador.EmpresaNavigation == null)
                    Console.WriteLine($"[PASSCHECK] ⚠️ AVISO: EmpresaNavigation é NULL para empresa ID: {colaborador.Empresa}");
                if (colaborador.CentrocustoNavigation == null)
                    Console.WriteLine($"[PASSCHECK] ⚠️ AVISO: CentrocustoNavigation é NULL para centro de custo ID: {colaborador.Centrocusto}");
                if (colaborador.LocalidadeNavigation == null)
                    Console.WriteLine($"[PASSCHECK] ⚠️ AVISO: LocalidadeNavigation é NULL para localidade ID: {colaborador.Localidade}");

                // ✅ CORREÇÃO: Usar a mesma lógica do MeuPatrimonioController para incluir TODOS os tipos de aquisição
                Console.WriteLine($"[PASSCHECK] Buscando equipamentos do colaborador usando lógica do MeuPatrimonioController");
                
                // Buscar todos os itens de requisição processadas do colaborador (independente do tipo de aquisição)
                var itensRequisicao = _requisicaoRepository
                    .Buscar(r => r.Colaboradorfinal == colaborador.Id && r.Requisicaostatus == 3)
                    .SelectMany(r => r.Requisicoesitens)
                    .Where(ri => ri.Equipamento != null && ri.Equipamento > 0)
                    .ToList();

                // Buscar linhas telefônicas do colaborador
                var itensLinhasTelefonicas = _requisicaoRepository
                    .Buscar(r => r.Colaboradorfinal == colaborador.Id && r.Requisicaostatus == 3)
                    .SelectMany(r => r.Requisicoesitens)
                    .Where(ri => ri.Linhatelefonica != null && ri.Linhatelefonica > 0)
                    .ToList();

                // Manter apenas o último registro por equipamento (caso exista mais de uma movimentação)
                var ultimosItensPorEquipamento = itensRequisicao
                    .GroupBy(ri => ri.Equipamento)
                    .Select(g => g.OrderByDescending(x => x.Id).First())
                    .ToList();

                // Manter apenas o último registro por linha telefônica
                var ultimosItensPorLinha = itensLinhasTelefonicas
                    .GroupBy(ri => ri.Linhatelefonica)
                    .Select(g => g.OrderByDescending(x => x.Id).First())
                    .ToList();

                Console.WriteLine($"[PASSCHECK] Itens de equipamentos considerados: {ultimosItensPorEquipamento.Count}");
                Console.WriteLine($"[PASSCHECK] Itens de linhas telefônicas considerados: {ultimosItensPorLinha.Count}");
                
                if (ultimosItensPorEquipamento.Count == 0 && ultimosItensPorLinha.Count == 0)
                {
                    Console.WriteLine($"[PASSCHECK] Colaborador não possui entregas ativas: {colaborador.Nome}");
                    
                    // Log do acesso
                    _patrimonioNegocio.LogarAcesso("passcheck", colaborador.Id, cpf,
                        _ipAddressService.GetClientIpAddress(Request.HttpContext),
                        Request.Headers["User-Agent"].ToString(), 
                        new { colaborador_id = colaborador.Id, entregas_count = 0, termo_assinado = true }, 
                        true, "");

                    // Colaborador sem entregas ativas = Liberado
                    var response = new PassCheckResponseDTO
                    {
                        Sucesso = true,
                        Mensagem = "Consulta realizada com sucesso",
                        Colaborador = new PassCheckColaboradorDTO
                        {
                            Id = colaborador.Id,
                            Nome = colaborador.Nome,
                            Cpf = Cripto.CriptografarDescriptografar(colaborador.Cpf, false), // Descriptografar CPF
                            Matricula = colaborador.Matricula,
                            Cargo = colaborador.Cargo,
                            Setor = colaborador.Setor,
                            Empresa = colaborador.Empresa.ToString(),
                            EmpresaNome = colaborador.EmpresaNavigation?.Nome ?? "N/A",
                            CentroCusto = colaborador.Centrocusto.ToString(),
                            CentroCustoNome = colaborador.CentrocustoNavigation?.Nome ?? "N/A",
                            Localidade = colaborador.Localidade.ToString(),
                            LocalidadeNome = colaborador.LocalidadeNavigation?.Descricao ?? "N/A",
                            Situacao = colaborador.Situacao,
                            DtAdmissao = colaborador.Dtadmissao,
                            DtDemissao = colaborador.Dtdemissao,
                            SuperiorImediato = colaborador.Matriculasuperior ?? "",
                            SuperiorImediatoNome = ObterNomeSuperiorImediato(colaborador.Matriculasuperior)
                        },
                        Equipamentos = new List<PassCheckEquipamentoDTO>(),
                        StatusLiberacao = colaborador.Situacao == "Ativo" ? "Liberado" : "Pendências",
                        MotivosPendencia = colaborador.Situacao != "Ativo" ? new List<string> { $"Colaborador com situação: {colaborador.Situacao}" } : new List<string>()
                    };

                    return Ok(response);
                }

                // ✅ CORREÇÃO: Montar lista de equipamentos usando a mesma lógica do MeuPatrimonioController
                Console.WriteLine($"[PASSCHECK] Montando lista de equipamentos para colaborador {colaborador.Nome}");
                var equipamentos = new List<PassCheckEquipamentoDTO>();
                
                // Processar equipamentos
                foreach (var item in ultimosItensPorEquipamento)
                {
                    var equipamento = _equipamentoRepository
                        .Buscar(e => e.Id == item.Equipamento)
                        .FirstOrDefault();

                    if (equipamento == null)
                    {
                        continue;
                    }

                    // Buscar informações de assinatura da requisição
                    var requisicao = _requisicaoRepository
                        .Buscar(r => r.Id == item.Requisicao)
                        .FirstOrDefault();

                    // Se não encontrou pela ID, tentar buscar requisições do colaborador que contenham este equipamento
                    if (requisicao == null)
                    {
                        var requisicoesColaborador = _requisicaoRepository
                            .Buscar(r => r.Colaboradorfinal == colaborador.Id && r.Requisicaostatus == 3)
                            .Where(r => r.Requisicoesitens.Any(ri => ri.Equipamento == equipamento.Id))
                            .OrderByDescending(r => r.Dtprocessamento)
                            .FirstOrDefault();
                        
                        if (requisicoesColaborador != null)
                        {
                            requisicao = requisicoesColaborador;
                        }
                    }

                    // Determina status pelo item (se possui Dtdevolucao)
                    string statusEquipamento = item.Dtdevolucao.HasValue ? "Devolvido" : "Entregue";
                    string dtEntrega = item.Dtentrega?.ToString("yyyy-MM-dd") ?? equipamento.Dtcadastro.ToString("yyyy-MM-dd");
                    
                    // ✅ NOVA LÓGICA: Determinar se é recurso particular (BYOD)
                    bool isRecursoParticular = equipamento.Tipoaquisicao == 2; // TipoAquisicaoEnum.Próprio
                    
                    // ✅ NOVA LÓGICA: Determinar categoria do recurso
                    string categoriaRecurso = "Recursos Corporativos";
                    if (isRecursoParticular)
                    {
                        categoriaRecurso = "BYOD - Recursos Particulares - Podem Sair Livremente";
                    }
                    else if (equipamento.Tipoaquisicao == 1) // Alugado
                    {
                        categoriaRecurso = "Recursos Alugados";
                    }
                    else if (equipamento.Tipoaquisicao == 3) // Corporativo
                    {
                        categoriaRecurso = "Recursos Corporativos";
                    }
                    
                    // ✅ NOVA LÓGICA: Verificar se é histórico (equipamento inativo ou devolvido)
                    bool isHistorico = !equipamento.Ativo || item.Dtdevolucao.HasValue;
                    if (isHistorico)
                    {
                        categoriaRecurso = "Histórico - " + categoriaRecurso;
                    }

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

                    // ✅ NOVA LÓGICA: Recursos particulares (BYOD) sempre têm trânsito livre
                    bool transitoLivre = false;
                    
                    if (isRecursoParticular)
                    {
                        // Recursos particulares sempre têm trânsito livre
                        transitoLivre = true;
                        Console.WriteLine($"[PASSCHECK] ✅ RECURSO PARTICULAR - Trânsito livre automático: {tipoDesc}");
                    }
                    else
                    {
                        // Para outros tipos, verificar configuração do tipo de equipamento
                        var equipamentoCompleto = _equipamentoRepository.Buscar(x => x.Id == equipamento.Id)
                            .Include(e => e.TipoequipamentoNavigation)
                            .AsNoTracking()
                            .FirstOrDefault();
                        
                        if (equipamentoCompleto?.TipoequipamentoNavigation != null)
                        {
                            transitoLivre = equipamentoCompleto.TipoequipamentoNavigation.TransitoLivre;
                        }
                    }

                    Console.WriteLine($"[PASSCHECK] Equipamento {tipoDesc} (ID: {equipamento.Id}) - Trânsito livre: {transitoLivre} - Tipo Aquis: {tipoAquisicaoDesc}");

                    equipamentos.Add(new PassCheckEquipamentoDTO
                    {
                        Id = equipamento.Id,
                        Patrimonio = equipamento.Patrimonio ?? string.Empty,
                        NumeroSerie = equipamento.Numeroserie ?? string.Empty,
                        TipoEquipamento = tipoDesc,
                        TipoEquipamentoTransitoLivre = transitoLivre,
                        Fabricante = fabricanteDesc,
                        Modelo = modeloDesc,
                        Status = statusEquipamento,
                        DtEntrega = DateTime.TryParse(dtEntrega, out var dt) ? dt : DateTime.MinValue,
                        Observacao = equipamento.Descricaobo ?? string.Empty,
                        TipoAquisicao = tipoAquisicaoDesc,
                        CategoriaRecurso = categoriaRecurso, // ✅ NOVO: Categoria do recurso
                        IsHistorico = isHistorico, // ✅ NOVO: Se é histórico
                        IsRecursoParticular = isRecursoParticular // ✅ NOVO: Se é recurso particular
                    });
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

                    // Buscar informações de assinatura da requisição
                    var requisicao = _requisicaoRepository
                        .Buscar(r => r.Id == item.Requisicao)
                        .FirstOrDefault();

                    // Se não encontrou pela ID, tentar buscar requisições do colaborador que contenham esta linha
                    if (requisicao == null)
                    {
                        var requisicoesColaborador = _requisicaoRepository
                            .Buscar(r => r.Colaboradorfinal == colaborador.Id && r.Requisicaostatus == 3)
                            .Where(r => r.Requisicoesitens.Any(ri => ri.Linhatelefonica == linha.Id))
                            .OrderByDescending(r => r.Dtprocessamento)
                            .FirstOrDefault();
                        
                        if (requisicoesColaborador != null)
                        {
                            requisicao = requisicoesColaborador;
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
                    string dtEntrega = item.Dtentrega?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                    
                    // ✅ NOVA LÓGICA: Categorizar linha telefônica
                    string categoriaLinha = "Recursos Corporativos";
                    bool isLinhaHistorico = !linha.Ativo || item.Dtdevolucao.HasValue;
                    if (isLinhaHistorico)
                    {
                        categoriaLinha = "Histórico - " + categoriaLinha;
                    }

                    Console.WriteLine($"[PASSCHECK] Linha telefônica {linha.Numero} (ID: {linha.Id}) - Trânsito livre: true - Tipo Aquis: Corporativo");

                    equipamentos.Add(new PassCheckEquipamentoDTO
                    {
                        Id = linha.Id,
                        Patrimonio = linha.Numero.ToString(),
                        NumeroSerie = linha.Iccid ?? string.Empty,
                        TipoEquipamento = "Linha Telefônica",
                        TipoEquipamentoTransitoLivre = true, // Linhas telefônicas sempre têm trânsito livre
                        Fabricante = operadora?.Nome ?? "N/A",
                        Modelo = plano?.Nome ?? "N/A",
                        Status = statusLinha,
                        DtEntrega = DateTime.TryParse(dtEntrega, out var dt) ? dt : DateTime.MinValue,
                        Observacao = $"Contrato: {contrato?.Nome ?? "N/A"}",
                        TipoAquisicao = "Corporativo",
                        CategoriaRecurso = categoriaLinha, // ✅ NOVO: Categoria do recurso
                        IsHistorico = isLinhaHistorico, // ✅ NOVO: Se é histórico
                        IsRecursoParticular = false // ✅ NOVO: Linhas telefônicas não são particulares
                    });
                }

                // Montar resposta
                var responseFinal = new PassCheckResponseDTO
                {
                    Sucesso = true,
                    Mensagem = "Consulta realizada com sucesso",
                    Colaborador = new PassCheckColaboradorDTO
                    {
                        Id = colaborador.Id,
                        Nome = colaborador.Nome,
                        Cpf = Cripto.CriptografarDescriptografar(colaborador.Cpf, false), // Descriptografar CPF
                        Matricula = colaborador.Matricula,
                        Cargo = colaborador.Cargo,
                        Setor = colaborador.Setor,
                        Empresa = colaborador.Empresa.ToString(),
                        EmpresaNome = colaborador.EmpresaNavigation?.Nome ?? "N/A",
                        CentroCusto = colaborador.Centrocusto.ToString(),
                        CentroCustoNome = colaborador.CentrocustoNavigation?.Nome ?? "N/A",
                        Localidade = colaborador.Localidade.ToString(),
                        LocalidadeNome = colaborador.LocalidadeNavigation?.Descricao ?? "N/A",
                        Situacao = colaborador.Situacao,
                        DtAdmissao = colaborador.Dtadmissao,
                        DtDemissao = colaborador.Dtdemissao,
                        SuperiorImediato = colaborador.Matriculasuperior ?? "",
                        SuperiorImediatoNome = ObterNomeSuperiorImediato(colaborador.Matriculasuperior)
                    },
                    Equipamentos = equipamentos
                };

                // ✅ USAR LÓGICA EXISTENTE: Determinar status baseado na assinatura
                // Considerar apenas recursos em posse (não históricos) para pendências
                var equipamentosEmPosse = equipamentos.Where(e => !e.IsHistorico).ToList();

                // Mapear assinatura por equipamento/linha com base nas requisições dos últimos itens
                var assinaturaPorEquipamento = ultimosItensPorEquipamento
                    .Where(i => i.Equipamento.HasValue && i.Equipamento.Value > 0)
                    .ToDictionary(
                        i => i.Equipamento!.Value,
                        i => _requisicaoRepository
                                .Buscar(r => r.Id == i.Requisicao)
                                .Select(r => (bool?)r.Assinaturaeletronica)
                                .FirstOrDefault() ?? false
                    );

                var assinaturaPorLinha = ultimosItensPorLinha
                    .Where(i => i.Linhatelefonica.HasValue && i.Linhatelefonica.Value > 0)
                    .ToDictionary(
                        i => i.Linhatelefonica!.Value,
                        i => _requisicaoRepository
                                .Buscar(r => r.Id == i.Requisicao)
                                .Select(r => (bool?)r.Assinaturaeletronica)
                                .FirstOrDefault() ?? false
                    );

                bool termoAssinado = equipamentosEmPosse.All(e =>
                {
                    var isLinha = (e.TipoEquipamento ?? string.Empty).ToLower().Contains("linha");
                    if (isLinha)
                    {
                        return assinaturaPorLinha.TryGetValue(e.Id, out var okLinha) ? okLinha : true; // evitar falso positivo
                    }
                    return assinaturaPorEquipamento.TryGetValue(e.Id, out var okEqp) ? okEqp : true; // evitar falso positivo
                });

                responseFinal.StatusLiberacao = DeterminarStatusLiberacao(colaborador, equipamentosEmPosse, termoAssinado);
                responseFinal.MotivosPendencia = ObterMotivosPendencia(colaborador, equipamentosEmPosse, termoAssinado);

                // Log do acesso
                _patrimonioNegocio.LogarAcesso("passcheck", colaborador.Id, cpf,
                    _ipAddressService.GetClientIpAddress(Request.HttpContext),
                    Request.Headers["User-Agent"].ToString(), 
                    new { colaborador_id = colaborador.Id, equipamentos_count = equipamentos.Count, termo_assinado = termoAssinado }, 
                    true, "");

                // ✅ NOVO: Logs detalhados das categorias
                Console.WriteLine($"[PASSCHECK] Resposta final montada com {equipamentos.Count} equipamentos");
                Console.WriteLine($"[PASSCHECK] Equipamentos com trânsito livre: {equipamentos.Count(e => e.TipoEquipamentoTransitoLivre)}");
                Console.WriteLine($"[PASSCHECK] Equipamentos por tipo de aquisição:");
                var equipamentosPorTipo = equipamentos.GroupBy(e => e.TipoAquisicao);
                foreach (var grupo in equipamentosPorTipo)
                {
                    Console.WriteLine($"[PASSCHECK] - {grupo.Key}: {grupo.Count()} equipamentos");
                }
                
                Console.WriteLine($"[PASSCHECK] Equipamentos por categoria:");
                var equipamentosPorCategoria = equipamentos.GroupBy(e => e.CategoriaRecurso);
                foreach (var grupo in equipamentosPorCategoria)
                {
                    Console.WriteLine($"[PASSCHECK] - {grupo.Key}: {grupo.Count()} equipamentos");
                }
                
                // ✅ NOVO: Log dos recursos particulares
                var recursosParticulares = equipamentos.Where(e => e.IsRecursoParticular).ToList();
                Console.WriteLine($"[PASSCHECK] ✅ RECURSOS PARTICULARES (BYOD): {recursosParticulares.Count} equipamentos");
                foreach (var recurso in recursosParticulares)
                {
                    Console.WriteLine($"[PASSCHECK] - {recurso.TipoEquipamento} (Patrimônio: {recurso.Patrimonio}) - Trânsito livre: {recurso.TipoEquipamentoTransitoLivre}");
                }
                
                // ✅ NOVO: Log do histórico
                var recursosHistorico = equipamentos.Where(e => e.IsHistorico).ToList();
                Console.WriteLine($"[PASSCHECK] 📚 RECURSOS HISTÓRICOS: {recursosHistorico.Count} equipamentos");

                Console.WriteLine($"[PASSCHECK] Consulta finalizada com sucesso para: {colaborador.Nome} - Status: {responseFinal.StatusLiberacao}");
                return Ok(responseFinal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PASSCHECK] Erro na consulta: {ex.Message}");
                
                // Log do erro
                _patrimonioNegocio.LogarAcesso("passcheck", null, cpf,
                    _ipAddressService.GetClientIpAddress(Request.HttpContext),
                    Request.Headers["User-Agent"].ToString(), null, false, ex.Message);
                
                return StatusCode(500, new PassCheckResponseDTO
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor"
                });
            }
        }

        /// <summary>
        /// Determina o status de liberação do colaborador baseado na lógica existente
        /// </summary>
        private string DeterminarStatusLiberacao(Colaboradore colaborador, List<PassCheckEquipamentoDTO> equipamentos, bool termoAssinado)
        {
            // ✅ CORREÇÃO: Verificar se colaborador está ativo (A = Ativo)
            if (colaborador.Situacao != "A" && colaborador.Situacao != "Ativo")
            {
                return "Pendências";
            }

            // ✅ NOVA VALIDAÇÃO: Verificar se colaborador tem data de demissão preenchida
            if (colaborador.Dtdemissao.HasValue)
            {
                var hoje = DateTime.Today;
                var dataDemissao = colaborador.Dtdemissao.Value.Date;
                
                // Se data de demissão é hoje ou já passou
                if (dataDemissao <= hoje)
                {
                    return "Reter Recursos";
                }
            }

            // Se não tem entregas ativas = Liberado (independente de assinatura)
            if (!equipamentos.Any())
            {
                return "Liberado";
            }

            // Se tem entregas ativas mas não assinou = Pendências
            if (!termoAssinado)
            {
                return "Pendências";
            }

            // Tem entregas ativas e assinou = Liberado
            return "Liberado";
        }

        /// <summary>
        /// Obtém motivos de pendência baseado na lógica existente
        /// </summary>
        private List<string> ObterMotivosPendencia(Colaboradore colaborador, List<PassCheckEquipamentoDTO> equipamentos, bool termoAssinado)
        {
            var motivos = new List<string>();

            // ✅ CORREÇÃO: Só adicionar motivo de situação se não estiver ativo
            if (colaborador.Situacao != "A" && colaborador.Situacao != "Ativo")
            {
                motivos.Add($"Colaborador com situação: {colaborador.Situacao}");
            }

            // ✅ NOVA VALIDAÇÃO: Adicionar motivo se colaborador tem data de demissão preenchida
            if (colaborador.Dtdemissao.HasValue)
            {
                var hoje = DateTime.Today;
                var dataDemissao = colaborador.Dtdemissao.Value.Date;
                
                // Se data de demissão é hoje ou já passou
                if (dataDemissao <= hoje)
                {
                    motivos.Add($"⚠️ ATENÇÃO: Colaborador com data de demissão em {dataDemissao:dd/MM/yyyy}. RETENHA OS RECURSOS!");
                }
            }

            // Só adicionar motivo de assinatura se tiver equipamentos e não tiver assinado
            if (equipamentos.Any() && !termoAssinado)
            {
                motivos.Add("Termo de compromisso não assinado");
            }

            return motivos;
        }

        /// <summary>
        /// Sinalizar suspeita sobre um colaborador consultado (usado pelo vigilante)
        /// </summary>
        /// <param name="dto">Dados da sinalização de suspeita</param>
        /// <returns>Resultado da sinalização</returns>
        [HttpPost("sinalizar-suspeita")]
        [AllowAnonymous] // Acesso público para vigilantes da portaria
        public async Task<ActionResult<SinalizacaoCriadaDTO>> SinalizarSuspeita([FromBody] CriarSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[PASSCHECK] Sinalizando suspeita para colaborador ID: {dto.ColaboradorId}");
                
                // Capturar dados da requisição
                dto.IpAddress = _ipAddressService.GetClientIpAddress(Request.HttpContext);
                dto.UserAgent = Request.Headers["User-Agent"].ToString();
                
                // Criar sinalização
                var resultado = await _sinalizacaoNegocio.CriarSinalizacaoAsync(dto);
                
                if (resultado.Sucesso)
                {
                    Console.WriteLine($"[PASSCHECK] Suspeita sinalizada com sucesso - Protocolo: {resultado.NumeroProtocolo}");
                    return Ok(resultado);
                }
                else
                {
                    Console.WriteLine($"[PASSCHECK] Erro ao sinalizar suspeita: {resultado.Mensagem}");
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PASSCHECK] Erro interno ao sinalizar suspeita: {ex.Message}");
                return StatusCode(500, new SinalizacaoCriadaDTO
                {
                    Sucesso = false,
                    Mensagem = "Erro interno do servidor"
                });
            }
        }

        /// <summary>
        /// Obter motivos de suspeita disponíveis para sinalização
        /// </summary>
        /// <returns>Lista de motivos de suspeita</returns>
        [HttpGet("motivos-suspeita")]
        [AllowAnonymous] // Acesso público para vigilantes da portaria
        public async Task<ActionResult<List<MotivoSuspeitaDTO>>> ObterMotivosSuspeita()
        {
            try
            {
                Console.WriteLine($"[PASSCHECK] Obtendo motivos de suspeita");
                
                var motivos = await _sinalizacaoNegocio.ObterMotivosSuspeitaAsync();
                return Ok(motivos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PASSCHECK] Erro ao obter motivos de suspeita: {ex.Message}");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        private string ObterNomeSuperiorImediato(string? matriculaSuperior)
        {
            Console.WriteLine($"[PASSCHECK] Buscando superior imediato para matrícula: {matriculaSuperior}");
            
            if (string.IsNullOrEmpty(matriculaSuperior))
            {
                Console.WriteLine($"[PASSCHECK] Matrícula do superior é nula ou vazia");
                return string.Empty;
            }

            try
            {
                var superior = _colaboradorRepository.Buscar(x => x.Matricula == matriculaSuperior)
                    .AsNoTracking()
                    .FirstOrDefault();

                var nomeSuperior = superior?.Nome ?? string.Empty;
                Console.WriteLine($"[PASSCHECK] Superior encontrado: {nomeSuperior}");
                return nomeSuperior;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PASSCHECK] Erro ao buscar nome do superior imediato: {ex.Message}");
                return string.Empty;
            }
        }

    }
}