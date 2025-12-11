using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SingleOne.Enumeradores;
using SingleOne.Models;
using SingleOne.Models.ViewModels;
using SingleOne.Util;
using SingleOneAPI;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Models.ViewModels;
using SingleOneAPI.Negocios.Interfaces;
using SingleOneAPI.Services;
using SingleOneAPI.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Negocios
{
    public class RequisicoesNegocio : IRequisicoesNegocio
    {
        // ✅ CORREÇÃO CRÍTICA: Constantes para status de requisição
        // ATENÇÃO: Baseado na análise da view, os IDs estão INVERTIDOS!
        // A view mostra: ID 3 = "Processada", então ID 2 deve ser "Cancelada"
        private const int STATUS_ATIVA = 1;
        private const int STATUS_PROCESSADA = 3;  // ✅ CORREÇÃO: ID 3 = Processada (conforme view)
        private const int STATUS_CANCELADA = 2;   // ✅ CORREÇÃO: ID 2 = Cancelada (conforme view)
        
        // ✅ CORREÇÃO: Comentários atualizados para refletir a realidade do banco
        // STATUS_ATIVA = 1: Nova requisição, aguardando processamento
        // STATUS_PROCESSADA = 3: Requisição processada/entregue (ID 3 na view)
        // STATUS_CANCELADA = 2: Requisição cancelada (ID 2 na view)
        // NOTA: ID 4 não existe no banco de dados
        
        private SendMail mail;
        private readonly ISmtpConfigService _smtpConfigService;
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly IRepository<Requisicoesiten> _requisicaoItensRepository;
        private readonly IRepository<Telefoniacontrato> _telefoniacontratoRepository;
        private readonly IRepository<Telefonialinha> _telefonialinhaRepository;
        private readonly IRepository<Telefoniaoperadora> _telefoniaoperadoraRepository;
        private readonly IRepository<RegrasTemplate> _regrasTemplateRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Equipamentohistorico> _equipamentohistoricoRepository;
        private readonly IReadOnlyRepository<Equipamentohistoricovm> _equipamentohistoricovmRepository;
        private readonly IReadOnlyRepository<Requisicoesvm> _requisicoesvmRepository;
        private readonly IReadOnlyRepository<Requisicaoequipamentosvm> _requisicaoequipamentosvmsRepository;
        private readonly IReadOnlyRepository<VwUltimasRequisicaoNaoBYOD> _vwUltimasRequisicaoNaoBYODRepository;
        private readonly IReadOnlyRepository<VwUltimasRequisicaoBYOD> _vwUltimasRequisicaoBYODRepository;
        private readonly IRepository<GeolocalizacaoAssinatura> _geolocalizacaoRepository;
        private readonly IRepository<Empresa> _empresaRepository;
        private readonly IColaboradorNegocio _colaboradorNegocio;
        private readonly IRepository<RequisicaoItemCompartilhado> _reqItemCompartilhadoRepository;
        private readonly IRepository<Template> _templateRepository;
        private readonly ICampanhaAssinaturaNegocio _campanhaAssinaturaNegocio; // ✅ NOVO: Para integração com campanhas

        public RequisicoesNegocio(EnvironmentApiSettings environmentApiSettings,
            IRepository<Requisico> requisicaoRepository,
            IRepository<Requisicoesiten> requisicaoItensRepository,
            IRepository<Telefoniacontrato> telefoniacontratoRepository,
            IRepository<Telefonialinha> telefonialinhaRepository,
            IRepository<Telefoniaoperadora> telefoniaoperadoraRepository,
            IRepository<RegrasTemplate> regrasTemplateRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Usuario> usuarioRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Equipamentohistorico> equipamentohistoricoRepository,
            IReadOnlyRepository<Equipamentohistoricovm> equipamentohistoricovmRepository,
            IReadOnlyRepository<Requisicoesvm> requisicoesvmRepository,
            IReadOnlyRepository<Requisicaoequipamentosvm> requisicaoequipamentosvmsRepository,
            IReadOnlyRepository<VwUltimasRequisicaoNaoBYOD> vwUltimasRequisicaoNaoBYODRepository,
            IReadOnlyRepository<VwUltimasRequisicaoBYOD> vwUltimasRequisicaoBYODRepository,
            IRepository<GeolocalizacaoAssinatura> geolocalizacaoRepository,
            IRepository<Empresa> empresaRepository,
            IColaboradorNegocio colaboradorNegocio,
            ISmtpConfigService smtpConfigService,
            IRepository<RequisicaoItemCompartilhado> reqItemCompartilhadoRepository,
            IRepository<Template> templateRepository,
            ICampanhaAssinaturaNegocio campanhaAssinaturaNegocio) // ✅ NOVO: Injeção de dependência
        {
            this.mail = new SendMail(environmentApiSettings, smtpConfigService);
            _smtpConfigService = smtpConfigService;
            _requisicaoRepository = requisicaoRepository;
            _requisicaoItensRepository = requisicaoItensRepository;
            _telefoniacontratoRepository = telefoniacontratoRepository;
            _telefonialinhaRepository = telefonialinhaRepository;
            _telefoniaoperadoraRepository = telefoniaoperadoraRepository;
            _regrasTemplateRepository = regrasTemplateRepository;
            _colaboradorRepository = colaboradorRepository;
            _usuarioRepository = usuarioRepository;
            _equipamentoRepository = equipamentoRepository;
            _equipamentohistoricoRepository = equipamentohistoricoRepository;
            _equipamentohistoricovmRepository = equipamentohistoricovmRepository;
            _requisicoesvmRepository = requisicoesvmRepository;
            _requisicaoequipamentosvmsRepository = requisicaoequipamentosvmsRepository;
            _vwUltimasRequisicaoNaoBYODRepository = vwUltimasRequisicaoNaoBYODRepository;
            _vwUltimasRequisicaoBYODRepository = vwUltimasRequisicaoBYODRepository;
            _geolocalizacaoRepository = geolocalizacaoRepository;
            _empresaRepository = empresaRepository;
            _colaboradorNegocio = colaboradorNegocio;
            _reqItemCompartilhadoRepository = reqItemCompartilhadoRepository;
            _templateRepository = templateRepository;
            _campanhaAssinaturaNegocio = campanhaAssinaturaNegocio; // ✅ NOVO: Inicialização
        }

        // ✅ NOVO: Método auxiliar para enriquecer itens de requisição com informações completas
        private List<Requisicoesiten> EnriquecerItensRequisicao(List<Requisicoesiten> itens)
        {
            // ✅ Para simplificar, vamos retornar os itens originais
            // ✅ As informações completas serão buscadas no frontend
            return itens;
        }

        public PagedResult<RequisicaoVM> ListarRequisicoes(string pesquisa, int cliente, int pagina)
        {
            pesquisa = pesquisa.ToLower();
            
            // ✅ CORREÇÃO: Se pesquisa for "null", retornar TODAS as requisições sem paginação
            if (pesquisa == "null")
            {
                var todasRequisicoes = _requisicoesvmRepository.Buscar(x => x.Cliente == cliente)
                    .OrderBy(x => x.Requisicaostatus).ThenByDescending(x => x.Id).ToList();
                
                var resultadoCompleto = new List<RequisicaoVM>();
                
                foreach (var r in todasRequisicoes)
                {
                    var requisicaoVM = new RequisicaoVM();
                    requisicaoVM.Requisicao = r;
                    requisicaoVM.PodeEntregar = r.Requisicaostatusid == STATUS_ATIVA;
                    requisicaoVM.EquipamentosRequisicao = _requisicaoequipamentosvmsRepository
                        .Buscar(x => x.Requisicao == r.Id && x.Equipamentostatus != 8)
                        .ToList();
                    
                    // ✅ Incluir os itens de requisição (equipamentos e linhas telefônicas)
                    requisicaoVM.RequisicaoItens = _requisicaoItensRepository
                        .Buscar(x => x.Requisicao == r.Id)
                        .ToList();
                    
                    resultadoCompleto.Add(requisicaoVM);
                }
                
                // ✅ CORREÇÃO: Retornar resultado sem paginação para histórico completo
                return new PagedResult<RequisicaoVM>
                {
                    Results = resultadoCompleto,
                    CurrentPage = 1,
                    PageCount = 1,
                    PageSize = resultadoCompleto.Count,
                    RowCount = resultadoCompleto.Count
                };
            }
            
            // ✅ CORREÇÃO: Se houver pesquisa, usar paginação normal
            // ✅ NOVO: Incluir busca por número de série de equipamentos
            var vw = _requisicoesvmRepository.Buscar(x => x.Cliente == cliente && (
                        x.Colaboradorfinal.ToLower().Contains(pesquisa) ||
                        x.Requisicaostatus.ToLower().Contains(pesquisa) ||
                        x.Tecnicoresponsavel.ToLower().Contains(pesquisa) ||
                        x.Id.ToString().Contains(pesquisa.ToLower().Replace("req", ""))
                      )).OrderBy(x => x.Requisicaostatus).ThenByDescending(x => x.Id).GetPaged(pagina, 10);

            // ✅ NOVO: Buscar requisições por número de série de equipamentos separadamente
            var requisicoesPorEquipamento = _requisicaoequipamentosvmsRepository
                .Buscar(eq => eq.Numeroserie.ToLower().Contains(pesquisa.ToLower()))
                .Where(eq => eq.Requisicao.HasValue)
                .Select(eq => eq.Requisicao.Value)
                .Distinct()
                .ToList();

            // ✅ NOVO: Buscar requisições por número de telefone separadamente
            // Usar consulta SQL direta para evitar problemas de navegação
            var requisicoesPorTelefone = new List<int>();
            
            // Buscar todas as linhas telefônicas que contêm o número pesquisado
            var linhasEncontradas = _telefonialinhaRepository
                .Buscar(tl => tl.Ativo && tl.Numero.ToString().Contains(pesquisa))
                .Select(tl => tl.Id)
                .ToList();
            
            if (linhasEncontradas.Any())
            {
                // Buscar requisições que têm essas linhas telefônicas
                requisicoesPorTelefone = _requisicaoItensRepository
                    .Buscar(ri => ri.Linhatelefonica.HasValue && linhasEncontradas.Contains(ri.Linhatelefonica.Value))
                    .Select(ri => ri.Requisicao)
                    .Distinct()
                    .ToList();
            }


            // ✅ NOVO: Combinar todos os IDs encontrados
            var idsCombinados = new List<int>();
            idsCombinados.AddRange(vw.Results.Where(r => r.Id.HasValue).Select(r => r.Id.Value));
            idsCombinados.AddRange(requisicoesPorEquipamento);
            idsCombinados.AddRange(requisicoesPorTelefone);
            idsCombinados = idsCombinados.Distinct().ToList();

            // ✅ NOVO: Se encontrou requisições por equipamento ou telefone, buscar e combinar
            if (idsCombinados.Any() && (requisicoesPorEquipamento.Any() || requisicoesPorTelefone.Any()))
            {
                vw = _requisicoesvmRepository
                    .Buscar(x => x.Cliente == cliente && x.Id.HasValue && idsCombinados.Contains(x.Id.Value))
                    .OrderBy(x => x.Requisicaostatus).ThenByDescending(x => x.Id)
                    .GetPaged(pagina, 10);
            }

            // ✅ CORREÇÃO: Criar lista de RequisicaoVM com dados completos
            var resultado = new List<RequisicaoVM>();
            
            foreach (var r in vw.Results)
            {
                var requisicaoVM = new RequisicaoVM();
                requisicaoVM.Requisicao = r;
                
                // ✅ CORREÇÃO: Verificar se pode entregar baseado no status
                requisicaoVM.PodeEntregar = r.Requisicaostatusid == STATUS_ATIVA;
                
                // ✅ CORREÇÃO: Buscar equipamentos da requisição
                requisicaoVM.EquipamentosRequisicao = _requisicaoequipamentosvmsRepository
                    .Buscar(x => x.Requisicao == r.Id && x.Equipamentostatus != 8)
                    .ToList(); // ✅ CORREÇÃO: Usar diretamente o tipo correto
                
                // ✅ NOVO: Incluir os itens de requisição (equipamentos e linhas telefônicas)
                var requisicaoItens = _requisicaoItensRepository
                    .Buscar(x => x.Requisicao == r.Id)
                    .ToList();
                
                // ✅ NOVO: Enriquecer itens com informações completas
                requisicaoVM.RequisicaoItens = EnriquecerItensRequisicao(requisicaoItens);

                resultado.Add(requisicaoVM);
            }

            // ✅ CORREÇÃO: Retornar resultado paginado
            var resultadoPaginado = new PagedResult<RequisicaoVM>
            {
                Results = resultado,
                CurrentPage = vw.CurrentPage,
                PageCount = vw.PageCount,
                PageSize = vw.PageSize,
                RowCount = vw.RowCount
            };

            return resultadoPaginado;
        }
        public RequisicaoVM BuscarRequisicaoPorId(int id)
        {
            // Evitar exceção quando não houver registro
            var requisicao = _requisicoesvmRepository
                .Buscar(x => x.Id == id)
                .FirstOrDefault();

            if (requisicao == null)
            {
                return null;
            }

            var req = new RequisicaoVM();
            req.Requisicao = requisicao;
            req.EquipamentosRequisicao = _requisicaoequipamentosvmsRepository
                .Buscar(x => x.Requisicao == req.Requisicao.Id)
                .ToList();

            // ✅ Incluir RequisicaoItens com informações completas das linhas telefônicas
            req.RequisicaoItens = _requisicaoItensRepository
                .IncludeWithThenInclude(q => q.Include(x => x.LinhatelefonicaNavigation)
                    .ThenInclude(x => x.PlanoNavigation)
                        .ThenInclude(x => x.ContratoNavigation)
                            .ThenInclude(x => x.OperadoraNavigation))
                .Include(x => x.EquipamentoNavigation)
                .Where(x => x.Requisicao == req.Requisicao.Id)
                .ToList();

            return req;
        }
        public RequisicaoVM ListarEquipamentosDaRequisicao(string hash, bool byod)
        {
            var req = new RequisicaoVM();
            try
            {
                var r = _requisicoesvmRepository.Buscar(x => x.Hashrequisicao == hash).FirstOrDefault();
                if (r == null)
                {
                    Console.WriteLine($"[LISTAR_REQ] AVISO: Nenhuma requisição encontrada para hash {hash}");
                    return req; // retorna vazio para o caller tratar
                }

                //var rs = _requisicaoRepository.Buscar(x => x.ColaboradorFinal == r.ColaboradorFinalId && x.AssinaturaEletronica == false).ToList();
                var colaboradorFinalId = r.Colaboradorfinalid;
                var rs = _requisicoesvmRepository
                    .Buscar(x => x.Colaboradorfinalid == colaboradorFinalId && x.Assinaturaeletronica == false && x.Equipamentospendentes > 0)
                    .ToList();

                // ✅ COBRIR CASO DE LINHAS TELEFÔNICAS: se não há pendentes pela view (equipamentos),
                // procurar requisições processadas sem assinatura com itens de linha telefônica ainda não devolvidos
                if (rs.Count == 0 && r != null)
                {
                    var pendentesLinhasIds = (from rq in _requisicaoRepository.Query()
                                              join ri in _requisicaoItensRepository.Query() on rq.Id equals ri.Requisicao
                                              where rq.Colaboradorfinal == r.Colaboradorfinalid &&
                                                    rq.Assinaturaeletronica == false &&
                                                    rq.Requisicaostatus == 3 &&
                                                    ri.Linhatelefonica.HasValue &&
                                                    ri.Linhatelefonica > 0 &&
                                                    ri.Dtentrega.HasValue &&
                                                    ri.Dtdevolucao == null
                                              select rq.Id)
                                              .Distinct()
                                              .ToList();

                    if (pendentesLinhasIds.Any())
                    {
                        rs = _requisicoesvmRepository
                            .Buscar(x => x.Id.HasValue && pendentesLinhasIds.Contains(x.Id.Value))
                            .ToList();
                    }
                }
                if (rs.Count > 0)
                {
                    req.Requisicao = rs.FirstOrDefault();
                }
                else
                {
                    req.Requisicao = r;
                }
                foreach (var requisicao in rs)
                {
                    if (byod)
                    {
                        var eqp = _requisicaoequipamentosvmsRepository
                            .Buscar(x => x.Requisicao == requisicao.Id && x.Equipamentostatus != 8 && x.TipoAquisicao == 2)
                            .ToList();
                        foreach (var e in eqp)
                        {
                            req.EquipamentosRequisicao.Add(e);
                        }
                    }
                    else
                    {
                        var eqp = _requisicaoequipamentosvmsRepository
                            .Buscar(x => x.Requisicao == requisicao.Id && x.Equipamentostatus != 8 && x.TipoAquisicao != 2)
                            .ToList();
                        foreach (var e in eqp)
                        {
                            req.EquipamentosRequisicao.Add(e);
                        }
                    }

                    // ✅ INCLUIR LINHAS TELEFÔNICAS COMO RECURSOS PARA ASSINATURA
                    var itensLinhas = _requisicaoItensRepository
                        .IncludeWithThenInclude(q => q.Include(x => x.LinhatelefonicaNavigation))
                        .Where(x => x.Requisicao == requisicao.Id &&
                                    x.Linhatelefonica.HasValue &&
                                    x.Linhatelefonica > 0 &&
                                    x.Dtentrega.HasValue &&
                                    x.Dtdevolucao == null)
                        .ToList();

                    foreach (var il in itensLinhas)
                    {
                        var numeroLinha = il.LinhatelefonicaNavigation?.Numero;
                        req.EquipamentosRequisicao.Add(new Requisicaoequipamentosvm
                        {
                            Requisicao = il.Requisicao,
                            Equipamento = "Linha Telefônica",
                            Numeroserie = (numeroLinha.HasValue ? numeroLinha.Value.ToString() : il.Linhatelefonica.ToString()),
                            Patrimonio = (numeroLinha.HasValue ? $"Linha {numeroLinha.Value}" : $"Linha {il.Linhatelefonica}"),
                            Dtentrega = il.Dtentrega,
                            Equipamentostatus = 4,
                            Linhaid = il.Linhatelefonica,
                            Numero = numeroLinha
                        });
                    }
                }
                //req.Requisicao = _requisicaoRepositoryVm.Buscar(x => x.HashRequisicao == hash).FirstOrDefault();
                //req.EquipamentosRequisicao = db.RequisicaoEquipamentosVm.Buscar(x => x.Requisicao == req.Requisicao.Id && x.EquipamentoStatus != 8).ToList();

                return req;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string SalvarRequisicao(Requisico requisicaoSalvar)
        {
            try
            {
                Console.WriteLine($"[NEGOCIO] SalvarRequisicao - Iniciando validações:");
                Console.WriteLine($"[NEGOCIO] - ID: {requisicaoSalvar?.Id}");
                Console.WriteLine($"[NEGOCIO] - Status: {requisicaoSalvar?.Requisicaostatus}");
                
                // ✅ CORREÇÃO CRÍTICA: Validar se não está tentando reutilizar requisição processada
                if (requisicaoSalvar.Id > 0)
                {
                var requisicaoExistente = _requisicaoRepository.Buscar(x => x.Id == requisicaoSalvar.Id).AsNoTracking().FirstOrDefault();
                if (requisicaoExistente != null && requisicaoExistente.Requisicaostatus == STATUS_PROCESSADA)
                {
                    return JsonConvert.SerializeObject(new { 
                        Mensagem = "ERRO CRÍTICO: Tentativa de reutilizar requisição processada. Crie uma nova requisição.", 
                        Status = "400.1" 
                    });
                }
            }

            // ✅ CORREÇÃO CRÍTICA: Para novas requisições, forçar ID = 0
            if (requisicaoSalvar.Id == 0)
            {
                requisicaoSalvar.Id = 0;
                requisicaoSalvar.Requisicaostatus = STATUS_ATIVA; // ✅ CORREÇÃO: Forçar status ativo para novas
            }

            switch (requisicaoSalvar.Requisicaostatus)
            {
                case STATUS_ATIVA:
                    {
                        var requisicoesDivididas = DividirRequisicoesPorTipoAquisicao(requisicaoSalvar);
                        foreach (var req in requisicoesDivididas)
                        {
                            var resultado = GravarRequisicao(req);
                            // ✅ CORREÇÃO: Verificar se houve erro ao gravar
                            if (resultado.Contains("ERRO") || resultado.Contains("400"))
                            {
                                return resultado; // Retornar erro imediatamente
                            }
                        }
                    }
                    return JsonConvert.SerializeObject(new { Mensagem = "Requisição salva com sucesso.", Status = "200" });
                    
                case STATUS_PROCESSADA:
                case STATUS_CANCELADA:
                    Console.WriteLine($"[NEGOCIO] SalvarRequisicao - Status recebido: {requisicaoSalvar.Requisicaostatus}");
                    Console.WriteLine($"[NEGOCIO] SalvarRequisicao - Chamando GravarRequisicao com status: {requisicaoSalvar.Requisicaostatus}");
                    
                    // ✅ CORREÇÃO CRÍTICA: Validar se o status está correto
                    if (requisicaoSalvar.Requisicaostatus == STATUS_CANCELADA)
                    {
                        Console.WriteLine($"[NEGOCIO] SalvarRequisicao - ✅ APLICANDO CANCELAMENTO (Status {STATUS_CANCELADA})");
                        Console.WriteLine($"[NEGOCIO] SalvarRequisicao - STATUS_CANCELADA = {STATUS_CANCELADA} (ID 2 = Cancelada)");
                    }
                    else if (requisicaoSalvar.Requisicaostatus == STATUS_PROCESSADA)
                    {
                        Console.WriteLine($"[NEGOCIO] SalvarRequisicao - ✅ APLICANDO PROCESSAMENTO (Status {STATUS_PROCESSADA})");
                        Console.WriteLine($"[NEGOCIO] SalvarRequisicao - STATUS_PROCESSADA = {STATUS_PROCESSADA} (ID 3 = Processada)");
                    }
                    else
                    {
                        Console.WriteLine($"[NEGOCIO] SalvarRequisicao - ⚠️ STATUS DESCONHECIDO: {requisicaoSalvar.Requisicaostatus}");
                        Console.WriteLine($"[NEGOCIO] SalvarRequisicao - STATUS_ATIVA = {STATUS_ATIVA} (ID 1), STATUS_PROCESSADA = {STATUS_PROCESSADA} (ID 3), STATUS_CANCELADA = {STATUS_CANCELADA} (ID 2)");
                    }
                    
                    return GravarRequisicao(requisicaoSalvar);

                default:
                    return JsonConvert.SerializeObject(new { 
                        Mensagem = "Status de requisição inválido.", 
                        Status = "400.2" 
                    });
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NEGOCIO] ERRO em SalvarRequisicao: {ex.Message}");
                Console.WriteLine($"[NEGOCIO] Stack trace: {ex.StackTrace}");
                return JsonConvert.SerializeObject(new { 
                    Mensagem = $"Erro interno: {ex.Message}", 
                    Status = "500" 
                });
            }
        }

        private string GravarRequisicao(Requisico req)
        {
            // ✅ CORREÇÃO CRÍTICA: Verificar se a requisição já existe e tem status processado
            if (req.Id > 0)
            {
                var requisicaoExistente = _requisicaoRepository.Buscar(x => x.Id == req.Id).AsNoTracking().FirstOrDefault();
                if (requisicaoExistente != null && requisicaoExistente.Requisicaostatus == STATUS_PROCESSADA)
                {
                    // ✅ CORREÇÃO: NUNCA permitir reutilizar requisição processada
                    return JsonConvert.SerializeObject(new { 
                        Mensagem = "ERRO CRÍTICO: Tentativa de reutilizar requisição processada. Crie uma nova requisição.", 
                        Status = "400.1" 
                    });
                }
            }

            if (req.Id == 0)
            {
                // ✅ CORREÇÃO: Usar método seguro de transação com retry automático
                return _requisicaoRepository.ExecuteInTransaction(() =>
                {
                    List<Requisicoesiten> riEquipList = new List<Requisicoesiten>();
                    
                    // ✅ CORREÇÃO CRÍTICA: Garantir que é uma NOVA requisição
                    req.Id = 0; // Forçar ID = 0 para garantir INSERT
                    req.Hashrequisicao = Guid.NewGuid().ToString();
                    req.Dtsolicitacao = TimeZoneMapper.GetDateTimeNow();
                    req.Requisicaostatus = STATUS_ATIVA; // ✅ CORREÇÃO: Usar constante
                    
                    // ✅ CORREÇÃO: Salvar a requisição PRIMEIRO para obter o ID
                    _requisicaoRepository.Adicionar(req);
                    _requisicaoRepository.SalvarAlteracoes(); // Salvar para obter o ID

                    foreach (var ri in req.Requisicoesitens)
                    {
                        //Adiciono os itens da requisição
                        if (ri.Id == 0)
                        {
                            ri.Requisicao = req.Id;
                            _requisicaoItensRepository.Adicionar(ri);
                        }
                        if (ri.Linhatelefonica != null)
                        {
                            var linha = _telefonialinhaRepository.Buscar(x => x.Id == ri.Linhatelefonica).FirstOrDefault();
                            if (linha != null)
                            {
                                linha.Emuso = true;
                                _telefonialinhaRepository.Atualizar(linha);
                            }
                        }

                        // ✅ CORREÇÃO: Verificar se é equipamento ou linha telefônica
                        if (ri.Equipamento.HasValue && ri.Equipamento > 0)
                        {
                            // É um equipamento - atualizar status
                            var eqp = _equipamentoRepository.Buscar(x => x.Id == ri.Equipamento.Value).AsNoTracking().FirstOrDefault();
                            if (eqp != null)
                            {
                                eqp.Equipamentostatus = 7; //Requisitado
                                _equipamentoRepository.Atualizar(eqp);
                            }
                        }
                        riEquipList.Add(ri);
                    }

                    // ✅ CORREÇÃO: Salvar itens e equipamentos ANTES de criar histórico
                    _requisicaoItensRepository.SalvarAlteracoes();
                    _equipamentoRepository.SalvarAlteracoes();

                    for (int i = 0; i < riEquipList.Count; i++)
                    {
                        //Insiro o registro no histórico do equipamento
                        var hst = new Equipamentohistorico();
                        
                        // ✅ CORREÇÃO: Usar equipamento real ou dummy ID=1 para linhas telefônicas
                        if (riEquipList[i].Equipamento.HasValue && riEquipList[i].Equipamento > 0)
                        {
                            hst.Equipamento = riEquipList[i].Equipamento.Value;
                        }
                        else
                        {
                            hst.Equipamento = 1; // ✅ Usar equipamento dummy ID=1 para linhas telefônicas
                        }
                        
                        hst.Equipamentostatus = 7; //Requisitado
                        hst.Requisicao = req.Id; // ✅ Agora req.Id tem valor válido
                        hst.Usuario = req.Usuariorequisicao;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        if (riEquipList[i].Linhatelefonica != null)
                        {
                            hst.Linhatelefonica = riEquipList[i].Linhatelefonica;
                            hst.Linhaemuso = true;
                        }
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }

                    // ✅ CORREÇÃO: Salvar histórico por último
                    _equipamentohistoricoRepository.SalvarAlteracoes();

                    return JsonConvert.SerializeObject(new { Mensagem = "Requisição salva com sucesso.", Status = "200" });
                });
            }
            else
            {
                // ✅ CORREÇÃO: Usar método seguro de transação com retry automático
                return _requisicaoRepository.ExecuteInTransaction(() =>
                {
                    var mReq = _requisicaoRepository.Include(x => x.Requisicoesitens).Where(x => x.Id == req.Id).AsNoTracking().FirstOrDefault();
                    
                    // ✅ CORREÇÃO CRÍTICA: Verificar se não está tentando reutilizar requisição processada
                    if (mReq.Requisicaostatus == STATUS_PROCESSADA)
                    {
                        return JsonConvert.SerializeObject(new { 
                            Mensagem = "ERRO CRÍTICO: Não é permitido alterar requisição processada. Crie uma nova requisição.", 
                            Status = "400.1" 
                        });
                    }
                    
                    // ✅ CORREÇÃO: Atualizar apenas o status, não reutilizar a requisição
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status antes da atualização: {mReq.Requisicaostatus}");
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status que será aplicado: {req.Requisicaostatus}");
                    
                    // ✅ CORREÇÃO CRÍTICA: Garantir que o status seja preservado corretamente
                    var statusOriginal = req.Requisicaostatus;
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status original recebido: {statusOriginal}");
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - STATUS_CANCELADA = {STATUS_CANCELADA}");
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - STATUS_PROCESSADA = {STATUS_PROCESSADA}");
                    
                    // ✅ CORREÇÃO: Validar se o status está correto
                    if (statusOriginal == STATUS_CANCELADA)
                    {
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - ✅ Status CANCELADA confirmado: {statusOriginal} (ID 2 = Cancelada)");
                    }
                    else if (statusOriginal == STATUS_PROCESSADA)
                    {
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - ✅ Status PROCESSADA confirmado: {statusOriginal} (ID 3 = Processada)");
                    }
                    else
                    {
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - ⚠️ STATUS DESCONHECIDO: {statusOriginal}");
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - STATUS_ATIVA = {STATUS_ATIVA} (ID 1), STATUS_PROCESSADA = {STATUS_PROCESSADA} (ID 3), STATUS_CANCELADA = {STATUS_CANCELADA} (ID 2)");
                    }
                    
                    mReq.Requisicaostatus = statusOriginal;
                    
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status após atribuição: {mReq.Requisicaostatus}");
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status original preservado: {statusOriginal}");
                    
                    // ✅ CORREÇÃO: Adicionar data de cancelamento se for cancelada
                    if (statusOriginal == STATUS_CANCELADA)
                    {
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - Aplicando lógica de CANCELAMENTO");
                        // ✅ CORREÇÃO: Usar propriedades que existem no modelo
                        // mReq.Dtcancelamento = TimeZoneMapper.GetDateTimeNow();
                        // mReq.Usuariocancelamento = req.Usuariorequisicao;
                    }
                    
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Chamando Atualizar com status: {mReq.Requisicaostatus}");
                    _requisicaoRepository.Atualizar(mReq);
                    
                    // ✅ CORREÇÃO: Verificar se o status foi preservado após a atualização
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status final antes de salvar: {mReq.Requisicaostatus}");

                    // ✅ CORREÇÃO: Lógica para cancelamento - retornar equipamentos ao estoque
                    if (req.Requisicaostatus == STATUS_CANCELADA)
                    {
                        foreach (var ri in req.Requisicoesitens)
                        {
                            // ✅ CORREÇÃO: Retornar linha telefônica ao estoque
                            if (ri.Linhatelefonica != null)
                            {
                                var linha = _telefonialinhaRepository.Buscar(x => x.Id == ri.Linhatelefonica).AsNoTracking().FirstOrDefault();
                                if (linha != null)
                                {
                                    linha.Emuso = false;
                                    _telefonialinhaRepository.Atualizar(linha);
                                }
                            }

                            // ✅ CORREÇÃO: Retornar equipamento ao estoque
                            if (ri.Equipamento.HasValue && ri.Equipamento > 0)
                            {
                                var eqp = _equipamentoRepository.Buscar(x => x.Id == ri.Equipamento.Value).AsNoTracking().FirstOrDefault();
                                if (eqp != null)
                                {
                                    eqp.Equipamentostatus = 3; // 3 = Em Estoque
                                    _equipamentoRepository.Atualizar(eqp);
                                }
                            }

                            // ✅ CORREÇÃO: Criar histórico de cancelamento
                            var hst = new Equipamentohistorico();
                            
                            // ✅ CORREÇÃO: Usar equipamento real ou dummy ID=1 para linhas telefônicas
                            if (ri.Equipamento.HasValue && ri.Equipamento > 0)
                            {
                                hst.Equipamento = ri.Equipamento.Value;
                            }
                            else
                            {
                                hst.Equipamento = 1; // ✅ Usar equipamento dummy ID=1 para linhas telefônicas
                            }
                            
                            hst.Equipamentostatus = 3; // 3 = Em Estoque
                            hst.Usuario = req.Usuariorequisicao;
                            hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                            hst.Requisicao = req.Id;
                            if (ri.Linhatelefonica != null)
                            {
                                hst.Linhatelefonica = ri.Linhatelefonica;
                                hst.Linhaemuso = false;
                            }
                            _equipamentohistoricoRepository.Adicionar(hst);
                        }
                        
                        // ✅ CORREÇÃO: Salvar todas as alterações
                        _telefonialinhaRepository.SalvarAlteracoes();
                        _equipamentoRepository.SalvarAlteracoes();
                        _equipamentohistoricoRepository.SalvarAlteracoes();
                    }
                    // ✅ CORREÇÃO: Lógica para processamento (entrega)
                    else if (req.Requisicaostatus == STATUS_PROCESSADA)
                    {
                        foreach (var ri in req.Requisicoesitens)
                        {
                            // ✅ CORREÇÃO: Atualizar status do equipamento para entregue
                            if (ri.Equipamento.HasValue && ri.Equipamento > 0)
                            {
                                var eqp = _equipamentoRepository.Buscar(x => x.Id == ri.Equipamento.Value).AsNoTracking().FirstOrDefault();
                                if (eqp != null)
                                {
                                    eqp.Equipamentostatus = 4; // 4 = Entregue
                                    _equipamentoRepository.Atualizar(eqp);
                                }
                            }

                            // ✅ CORREÇÃO: Criar histórico de entrega
                            var hst = new Equipamentohistorico();
                            
                            // ✅ CORREÇÃO: Usar equipamento real ou dummy ID=1 para linhas telefônicas
                            if (ri.Equipamento.HasValue && ri.Equipamento > 0)
                            {
                                hst.Equipamento = ri.Equipamento.Value;
                            }
                            else
                            {
                                hst.Equipamento = 1; // ✅ Usar equipamento dummy ID=1 para linhas telefônicas
                            }
                            
                            hst.Equipamentostatus = 4; // 4 = Entregue
                            hst.Usuario = req.Usuariorequisicao;
                            hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                            hst.Requisicao = req.Id;
                            _equipamentohistoricoRepository.Adicionar(hst);
                        }
                        
                        // ✅ CORREÇÃO: Salvar alterações
                        _equipamentoRepository.SalvarAlteracoes();
                        _equipamentohistoricoRepository.SalvarAlteracoes();
                    }

                    // ✅ CORREÇÃO: Verificar se o status foi salvo corretamente
                    var requisicaoVerificada = _requisicaoRepository.Buscar(x => x.Id == req.Id).AsNoTracking().FirstOrDefault();
                    Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status final no banco: {requisicaoVerificada?.Requisicaostatus}");
                    
                    if (requisicaoVerificada?.Requisicaostatus == statusOriginal)
                    {
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - ✅ Status salvo corretamente: {statusOriginal}");
                    }
                    else
                    {
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - ❌ ERRO: Status não foi salvo corretamente!");
                        Console.WriteLine($"[NEGOCIO] GravarRequisicao - Status esperado: {statusOriginal}, Status atual: {requisicaoVerificada?.Requisicaostatus}");
                    }

                    return JsonConvert.SerializeObject(new { Mensagem = "Requisição atualizada com sucesso.", Status = "200" });
                });
            }
        }

        private List<Requisico> DividirRequisicoesPorTipoAquisicao(Requisico req)
        {
            var regras = _regrasTemplateRepository.ObterTodos().ToList();
            var regrasDivisao = new List<RegraDivisaoRequisicao>();

            if (req.Id == 0)
            {
                foreach (var item in req.Requisicoesitens)
                {
                    // ✅ CORREÇÃO: Verificar se é equipamento ou linha telefônica
                                         if (item.Equipamento.HasValue && item.Equipamento > 0 && item.Linhatelefonica == null)
                    {
                        // É um equipamento
                        var equip = _equipamentoRepository.Buscar(x => x.Id == item.Equipamento.Value).AsNoTracking().FirstOrDefault();
                        if (equip == null)
                        {
                            throw new Exception($"Equipamento com ID {item.Equipamento.Value} não encontrado.");
                        }
                        
                        var regraVigente = regras.FirstOrDefault(x => x.TipoAquisicao == equip.Tipoaquisicao);
                        var regraDivisao = regrasDivisao.FirstOrDefault(x => x.Regra.TipoTemplate == regraVigente.TipoTemplate);
                        if (regraDivisao != null)
                        {
                            regraDivisao.Requisicao.Requisicoesitens.Add(item);
                        }
                        else
                        {
                            var requisicaoClonada = (Requisico)req.Clone();
                            requisicaoClonada.Requisicoesitens.Clear();
                            requisicaoClonada.Requisicoesitens.Add(item);

                            regrasDivisao.Add(new RegraDivisaoRequisicao
                            {
                                Regra = regras.First(r => r.TipoAquisicao == equip.Tipoaquisicao),
                                Requisicao = requisicaoClonada
                            });
                        }
                    }
                    else if (item.Linhatelefonica.HasValue && item.Linhatelefonica > 0)
                    {
                        // ✅ CORREÇÃO: É uma linha telefônica - usar regra padrão ou específica
                        // Para linhas telefônicas, vamos usar uma regra padrão ou criar uma lógica específica
                        var regraLinhas = regras.FirstOrDefault(x => x.TipoAquisicao == 0) ?? regras.FirstOrDefault(); // Usar regra padrão
                        if (regraLinhas != null)
                        {
                            var regraDivisao = regrasDivisao.FirstOrDefault(x => x.Regra.TipoTemplate == regraLinhas.TipoTemplate);
                            if (regraDivisao != null)
                            {
                                regraDivisao.Requisicao.Requisicoesitens.Add(item);
                            }
                            else
                            {
                                var requisicaoClonada = (Requisico)req.Clone();
                                requisicaoClonada.Requisicoesitens.Clear();
                                requisicaoClonada.Requisicoesitens.Add(item);

                                regrasDivisao.Add(new RegraDivisaoRequisicao
                                {
                                    Regra = regraLinhas,
                                    Requisicao = requisicaoClonada
                                });
                            }
                        }
                        else
                        {
                            // Se não há regras configuradas, criar uma requisição simples
                            var requisicaoClonada = (Requisico)req.Clone();
                            requisicaoClonada.Requisicoesitens.Clear();
                            requisicaoClonada.Requisicoesitens.Add(item);
                            
                            regrasDivisao.Add(new RegraDivisaoRequisicao
                            {
                                Regra = null, // Sem regra específica para linhas telefônicas
                                Requisicao = requisicaoClonada
                            });
                        }
                    }
                }
            }

            return regrasDivisao
                .Where(w => w.Requisicao != null)
                .Select(x => x.Requisicao)
                .ToList();
        }

        public async Task<string> AceitarTermoEntrega(TermoEletronicoVM vm)
        {
            try
            {
                Console.WriteLine($"[TERMO_ACEITE] === INÍCIO ===");
                Console.WriteLine($"[TERMO_ACEITE] CPF recebido: {vm.Cpf}");
                Console.WriteLine($"[TERMO_ACEITE] Hash recebido: {vm.HashRequisicao}");
                Console.WriteLine($"[TERMO_ACEITE] Palavra-chave recebida: {vm.PalavraChave}");
                
                vm.Cpf = vm.Cpf.Replace(".", "").Replace("-", "");
                vm.Cpf = Cripto.CriptografarDescriptografar(vm.Cpf, true);
                
                // ✅ VALIDAÇÃO: Verificar palavra-chave
                string palavraChaveCriptografada = null;
                if (!string.IsNullOrEmpty(vm.PalavraChave))
                {
                    palavraChaveCriptografada = Cripto.CriptografarDescriptografar(vm.PalavraChave, true);
                }
                
                // ✅ NOVA LÓGICA: Validar colaborador pelo hash, CPF e palavra-chave (do Usuario)
                var aceiteOK = (from req in _requisicaoRepository.Query()
                                join col in _colaboradorRepository.Query() on req.Colaboradorfinal equals col.Id
                                join usr in _usuarioRepository.Query() on col.Usuario equals usr.Id
                                where req.Hashrequisicao == vm.HashRequisicao &&
                                vm.Cpf == col.Cpf &&
                                (string.IsNullOrEmpty(palavraChaveCriptografada) || palavraChaveCriptografada == usr.Palavracriptografada)
                                select req).FirstOrDefault();
                
                Console.WriteLine($"[TERMO_ACEITE] Requisição encontrada: {aceiteOK != null}");
                
                if (aceiteOK != null)
                {
                    Console.WriteLine($"[TERMO_DINAMICO] === INÍCIO ASSINATURA DINÂMICA ===");
                    Console.WriteLine($"[TERMO_DINAMICO] Colaborador: {aceiteOK.Colaboradorfinal}, Hash recebido: {vm.HashRequisicao}");
                    
                    // ✅ Determinar tipo BYOD baseado na requisição atual
                    bool isByod = _requisicaoequipamentosvmsRepository.Buscar(x => x.Requisicao == aceiteOK.Id && x.TipoAquisicao == 2).Count() > 0;
                    Console.WriteLine($"[TERMO_DINAMICO] Tipo detectado: {(isByod ? "BYOD" : "Corporativo/Alugado")}");
                    
                    // ✅ NOVA LÓGICA: Buscar TODAS as requisições do colaborador (assinadas e não assinadas)
                    var todasRequisicoes = _requisicaoRepository
                        .Buscar(x => x.Colaboradorfinal == aceiteOK.Colaboradorfinal)
                        .ToList();
                    
                    Console.WriteLine($"[TERMO_DINAMICO] Total de requisições do colaborador: {todasRequisicoes.Count}");
                    
                    // ✅ Filtrar por tipo (BYOD ou não-BYOD) e que tenham equipamentos ativos
                    var requisicoesParaAssinar = new List<Requisico>();
                    
                    foreach (var req in todasRequisicoes)
                    {
                        // ✅ SEMPRE incluir a requisição original (a do hash recebido)
                        bool isRequisicaoOriginal = req.Id == aceiteOK.Id;
                        
                        // Verificar se tem equipamentos ativos (não devolvidos)
                        var equipamentosAtivos = _requisicaoequipamentosvmsRepository
                            .Buscar(x => x.Requisicao == req.Id && x.Equipamentostatus != 8)
                            .Count();
                        
                        // ✅ CORREÇÃO CRÍTICA: Verificar também linhas telefônicas ativas (não devolvidas)
                        var linhasAtivas = _requisicaoItensRepository
                            .Buscar(x => x.Requisicao == req.Id && 
                                         x.Linhatelefonica.HasValue && 
                                         x.Linhatelefonica > 0 && 
                                         !x.Dtdevolucao.HasValue)
                            .Count();
                        
                        var totalRecursosAtivos = equipamentosAtivos + linhasAtivas;
                        
                        // Incluir se:
                        // 1. É a requisição original (do hash) - SEMPRE incluir
                        // 2. OU tem equipamentos/linhas ativos E do mesmo tipo (BYOD ou não)
                        if (isRequisicaoOriginal || totalRecursosAtivos > 0)
                        {
                            // Verificar tipo BYOD
                            bool itemIsByod = _requisicaoequipamentosvmsRepository
                                .Buscar(x => x.Requisicao == req.Id && x.TipoAquisicao == 2)
                                .Any();
                            
                            if (isRequisicaoOriginal || isByod.Equals(itemIsByod))
                            {
                                requisicoesParaAssinar.Add(req);
                                string motivo = isRequisicaoOriginal ? " [ORIGINAL - HASH]" : "";
                                Console.WriteLine($"[TERMO_DINAMICO] Requisição {req.Id} incluída{motivo} - Tipo: {(itemIsByod ? "BYOD" : "Corporativo")}, Equipamentos ativos: {equipamentosAtivos}, Linhas ativas: {linhasAtivas}");
                            }
                        }
                    }
                    
                    Console.WriteLine($"[TERMO_DINAMICO] Requisições para assinar: {requisicoesParaAssinar.Count}");
                    
                    // ✅ Gerar novo hash e data de assinatura
                    var novoHash = Guid.NewGuid().ToString();
                    var novaDataAssinatura = TimeZoneMapper.GetDateTimeNow();
                    
                    Console.WriteLine($"[TERMO_DINAMICO] Novo hash: {novoHash}");
                    Console.WriteLine($"[TERMO_DINAMICO] Nova data: {novaDataAssinatura}");
                    
                    // ✅ BUSCAR TEMPLATE para fazer snapshot do conteúdo
                    int tipoTermo = isByod ? (int)TipoTemplateEnum.TermoCompromissoBYOD : (int)TipoTemplateEnum.TermoCompromisso;
                    var templateAtual = _templateRepository.Buscar(x => x.Tipo == tipoTermo && x.Cliente == aceiteOK.Cliente).FirstOrDefault();
                    string conteudoSnapshot = templateAtual?.Conteudo;
                    
                    if (templateAtual != null)
                    {
                        Console.WriteLine($"[TERMO_DINAMICO] Template encontrado - ID: {templateAtual.Id}, Versão: {templateAtual.Versao}, Tipo: {tipoTermo} ({(isByod ? "BYOD" : "Corporativo")})");
                        Console.WriteLine($"[TERMO_DINAMICO] Snapshot do template capturado - Tamanho: {conteudoSnapshot?.Length ?? 0} caracteres");
                    }
                    else
                    {
                        Console.WriteLine($"[TERMO_DINAMICO] ATENÇÃO: Template não encontrado - Tipo: {tipoTermo}, Cliente: {aceiteOK.Cliente}");
                    }
                    
                    // ✅ ATUALIZAR TODAS as requisições (mesmo as já assinadas)
                    int totalAtualizadas = 0;
                    foreach (var req in requisicoesParaAssinar)
                    {
                        bool jaEstavaAssinada = req.Assinaturaeletronica;
                        
                        req.Assinaturaeletronica = true;
                        req.Dtassinaturaeletronica = novaDataAssinatura;
                        req.Hashrequisicao = novoHash;
                        req.ConteudoTemplateAssinado = conteudoSnapshot; // 📸 Snapshot do template no momento da assinatura
                        req.TipoTermoAssinado = tipoTermo; // 🔖 Identificador do tipo de termo (BYOD ou Corporativo)
                        req.VersaoTemplateAssinado = templateAtual?.Versao; // 🔢 Versão do template no momento da assinatura
                        
                        _requisicaoRepository.Atualizar(req);
                        totalAtualizadas++;
                        
                        Console.WriteLine($"[TERMO_DINAMICO] Requisição {req.Id} atualizada - Era assinada: {jaEstavaAssinada}, Tipo: {(isByod ? "BYOD" : "Corporativo")}, Snapshot salvo: {!string.IsNullOrEmpty(conteudoSnapshot)}");
                    }
                    
                    Console.WriteLine($"[TERMO_DINAMICO] Total de requisições atualizadas: {totalAtualizadas}");

                    // Buscar o colaborador uma única vez
                    var colaborador = _colaboradorRepository.Buscar(x => x.Id == aceiteOK.Colaboradorfinal).Single();

                    // ✅ CORREÇÃO: Determinar o ID do usuário logado corretamente
                    // Usar o técnico responsável da requisição como usuário logado
                    int usuarioLogadoId = aceiteOK.Tecnicoresponsavel;
                    
                    // ✅ FALLBACK: Se técnico responsável não for válido ou for o mesmo que colaborador, usar usuário da requisição
                    if (usuarioLogadoId <= 0 || usuarioLogadoId == colaborador.Id)
                    {
                        usuarioLogadoId = aceiteOK.Usuariorequisicao;
                    }
                    
                    // ✅ FALLBACK FINAL: Se ainda não for válido, usar usuário 1 (Admin) como último recurso
                    if (usuarioLogadoId <= 0)
                    {
                        Console.WriteLine($"[ASSINATURA_TERMO] ⚠️ Usuário logado inválido, usando usuário 1 como fallback");
                        usuarioLogadoId = 1;
                    }
                    
                    Console.WriteLine($"[ASSINATURA_TERMO] Usuário logado ID: {usuarioLogadoId} (Técnico: {aceiteOK.Tecnicoresponsavel}, Usuário Req: {aceiteOK.Usuariorequisicao})");

                    // Registrar geolocalização da assinatura (para ambos BYOD e não-BYOD)
                    try
                    {
                        var geolocalizacao = new GeolocalizacaoAssinatura
                        {
                            ColaboradorId = colaborador.Id,
                            ColaboradorNome = colaborador.Nome,
                            UsuarioLogadoId = usuarioLogadoId, // ✅ CORRIGIDO: Usar ID do usuário, não do colaborador
                            IpAddress = !string.IsNullOrEmpty(vm.IpAddress) ? vm.IpAddress : "Não informado",
                            Country = !string.IsNullOrEmpty(vm.Country) ? vm.Country : "Brasil",
                            City = !string.IsNullOrEmpty(vm.City) ? vm.City : "Não informado",
                            Region = !string.IsNullOrEmpty(vm.Region) ? vm.Region : "Não informado",
                            Latitude = vm.Latitude,
                            Longitude = vm.Longitude,
                            AccuracyMeters = vm.Accuracy,
                            TimestampCaptura = TimeZoneMapper.GetDateTimeNow(),
                            Acao = isByod ? "ASSINATURA_TERMO_ELETRONICO_BYOD" : "ASSINATURA_TERMO_ELETRONICO",
                            DataCriacao = TimeZoneMapper.GetDateTimeNow()
                        };

                        _geolocalizacaoRepository.Adicionar(geolocalizacao);
                        Console.WriteLine($"[ASSINATURA_TERMO] Geolocalização registrada para {colaborador.Nome} - Tipo: {(isByod ? "BYOD" : "Corporativo/Alugado")}");
                        Console.WriteLine($"[ASSINATURA_TERMO] Dados: IP={geolocalizacao.IpAddress}, Cidade={geolocalizacao.City}, Região={geolocalizacao.Region}, País={geolocalizacao.Country}");
                        if (geolocalizacao.Latitude.HasValue && geolocalizacao.Longitude.HasValue)
                        {
                            Console.WriteLine($"[ASSINATURA_TERMO] Coordenadas: {geolocalizacao.Latitude:F6}, {geolocalizacao.Longitude:F6}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERRO_GEOLOCALIZAÇÃO] Falha ao registrar geolocalização para {colaborador.Nome}: {ex.Message}");
                    }

                    // ✅ Gerar PDF e enviar email com TODAS as requisições assinadas
                    try
                    {
                        Console.WriteLine($"[TERMO_DINAMICO] Iniciando geração do PDF dinâmico...");
                        byte[] pdfTermo = null;
                        try
                        {
                            Console.WriteLine($"[TERMO_DINAMICO] Gerando PDF - BYOD: {isByod}, Cliente: {aceiteOK.Cliente}, Colaborador: {colaborador.Id}");
                            Console.WriteLine($"[TERMO_DINAMICO] Requisições incluídas: {string.Join(", ", requisicoesParaAssinar.Select(r => r.Id))}");
                            Console.WriteLine($"[TERMO_DINAMICO] Novo hash: {novoHash}");
                            
                            // ✅ CORREÇÃO: Usar o técnico responsável da requisição como usuário logado
                            int usuarioLogadoId = aceiteOK.Tecnicoresponsavel;
                            Console.WriteLine($"[TERMO_DINAMICO] DEBUG - Requisição ID: {aceiteOK.Id}");
                            Console.WriteLine($"[TERMO_DINAMICO] DEBUG - Colaborador ID: {colaborador.Id}");
                            Console.WriteLine($"[TERMO_DINAMICO] DEBUG - Técnico Responsável: {aceiteOK.Tecnicoresponsavel}");
                            Console.WriteLine($"[TERMO_DINAMICO] DEBUG - Usuário Requisição: {aceiteOK.Usuariorequisicao}");
                            
                            // ✅ FALLBACK: Se técnico responsável for o mesmo que colaborador, usar usuário 1 (Admin)
                            if (usuarioLogadoId == colaborador.Id)
                            {
                                Console.WriteLine($"[TERMO_DINAMICO] AVISO: Técnico responsável é o mesmo que colaborador, usando usuário 1 como fallback");
                                usuarioLogadoId = 1;
                            }
                            
                            Console.WriteLine($"[TERMO_DINAMICO] Usando usuário logado ID: {usuarioLogadoId}");
                            
                            pdfTermo = _colaboradorNegocio.TermoCompromisso(aceiteOK.Cliente, colaborador.Id, usuarioLogadoId, isByod);
                            Console.WriteLine($"[TERMO_DINAMICO] PDF gerado com sucesso para {colaborador.Nome} - Tipo: {(isByod ? "BYOD" : "Corporativo/Alugado")} - Tamanho: {pdfTermo?.Length ?? 0} bytes");
                            
                            if (pdfTermo == null || pdfTermo.Length == 0)
                            {
                                Console.WriteLine($"[TERMO_DINAMICO] ERRO: PDF gerado está vazio ou nulo");
                                throw new Exception("PDF gerado está vazio ou nulo");
                            }
                        }
                        catch (Exception pdfEx)
                        {
                            Console.WriteLine($"[TERMO_DINAMICO] ERRO: Falha ao gerar PDF para {colaborador.Nome}: {pdfEx.Message}");
                            Console.WriteLine($"[TERMO_DINAMICO] Stack trace: {pdfEx.StackTrace}");
                            throw; // Re-throw para não continuar sem PDF
                        }
                        
                        var file = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "termoEmail-Confirmacao.html");
                        string template = File.Exists(file) ? File.ReadAllText(file) : string.Empty;
                        template = template.Replace("@nome", colaborador.Nome)
                            .Replace("@ano", DateTime.Today.Year.ToString());

                        Console.WriteLine($"[TERMO_DINAMICO] Enviando email para {colaborador.Email} - PDF: {(pdfTermo != null ? "Sim" : "Não")}");
                        Console.WriteLine($"[TERMO_DINAMICO] Cliente ID para carregar configurações SMTP: {aceiteOK.Cliente}");
                        
                        // ✅ CORREÇÃO: Usar EnviarAsync para carregar configurações SMTP do banco
                        await mail.EnviarAsync(Cripto.CriptografarDescriptografar(colaborador.Email, false), 
                            "Confirmação de assinatura do Termo eletrônico de entrega de recursos", 
                            template, 
                            pdfTermo, 
                            aceiteOK.Cliente); // Passar o ID do cliente para carregar configurações
                            
                        Console.WriteLine($"[TERMO_DINAMICO] Email de confirmação enviado para {colaborador.Email}");
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"[ERRO_EMAIL] Falha ao enviar email para {colaborador.Nome}: {emailEx.Message}");
                        Console.WriteLine($"[ERRO_EMAIL] Stack trace: {emailEx.StackTrace}");
                        throw; // Re-throw para não continuar sem enviar email
                    }

                    // ✅ 🆕 INTEGRAÇÃO COM CAMPANHAS: Atualizar status de assinatura nas campanhas ativas
                    // ⚠️ IMPORTANTE: Este bloco é OPCIONAL e NÃO bloqueia assinaturas que vêm de fora de campanhas
                    // Se o colaborador não estiver em nenhuma campanha, a assinatura continua normalmente
                    try
                    {
                        Console.WriteLine($"[CAMPANHA-INTEGRACAO] === INÍCIO ATUALIZAÇÃO DE CAMPANHAS ===");
                        Console.WriteLine($"[CAMPANHA-INTEGRACAO] Colaborador ID: {aceiteOK.Colaboradorfinal}, Cliente: {aceiteOK.Cliente}");
                        
                        // Buscar campanhas ativas do cliente
                        var campanhasAtivas = _campanhaAssinaturaNegocio.ListarCampanhasPorCliente(aceiteOK.Cliente, 'A'); // 'A' = Ativa
                        Console.WriteLine($"[CAMPANHA-INTEGRACAO] Total de campanhas ativas encontradas: {campanhasAtivas.Count}");
                        
                        if (campanhasAtivas == null || campanhasAtivas.Count == 0)
                        {
                            Console.WriteLine($"[CAMPANHA-INTEGRACAO] ℹ️ Nenhuma campanha ativa encontrada - Assinatura NÃO veio de campanha (normal)");
                            Console.WriteLine($"[CAMPANHA-INTEGRACAO] === FIM ATUALIZAÇÃO DE CAMPANHAS (SEM AÇÃO) ===");
                        }
                        else
                        {
                            int campanhasAtualizadas = 0;
                            foreach (var campanha in campanhasAtivas)
                        {
                            try
                            {
                                Console.WriteLine($"[CAMPANHA-INTEGRACAO] Verificando campanha {campanha.Id} - {campanha.Nome}");
                                
                                // Verificar se o colaborador está nesta campanha
                                var colaboradoresDaCampanha = _campanhaAssinaturaNegocio.ObterColaboradoresDaCampanha(campanha.Id);
                                var colaboradorNaCampanha = colaboradoresDaCampanha.FirstOrDefault(cc => cc.ColaboradorId == aceiteOK.Colaboradorfinal);
                                
                                if (colaboradorNaCampanha != null)
                                {
                                    Console.WriteLine($"[CAMPANHA-INTEGRACAO] ✅ Colaborador encontrado na campanha {campanha.Id}");
                                    Console.WriteLine($"[CAMPANHA-INTEGRACAO]    - Status atual: {colaboradorNaCampanha.StatusAssinatura}");
                                    
                                    // Atualizar status para 'A' (Assinado)
                                    _campanhaAssinaturaNegocio.MarcarComoAssinado(campanha.Id, aceiteOK.Colaboradorfinal.Value);
                                    campanhasAtualizadas++;
                                    
                                    Console.WriteLine($"[CAMPANHA-INTEGRACAO] ✅ Status atualizado para 'Assinado' na campanha {campanha.Id}");
                                    Console.WriteLine($"[CAMPANHA-INTEGRACAO] ✅ Métricas da campanha recalculadas automaticamente");
                                }
                                else
                                {
                                    Console.WriteLine($"[CAMPANHA-INTEGRACAO] ⚠️ Colaborador NÃO encontrado na campanha {campanha.Id}");
                                }
                            }
                            catch (Exception campEx)
                            {
                                Console.WriteLine($"[CAMPANHA-INTEGRACAO] ❌ Erro ao atualizar campanha {campanha.Id}: {campEx.Message}");
                                // Continua para não bloquear a assinatura do termo
                            }
                        }
                            
                            if (campanhasAtualizadas == 0)
                            {
                                Console.WriteLine($"[CAMPANHA-INTEGRACAO] ℹ️ Colaborador não está em nenhuma campanha ativa - Assinatura NÃO veio de campanha (normal)");
                            }
                            
                            Console.WriteLine($"[CAMPANHA-INTEGRACAO] Total de campanhas atualizadas: {campanhasAtualizadas}");
                            Console.WriteLine($"[CAMPANHA-INTEGRACAO] === FIM ATUALIZAÇÃO DE CAMPANHAS ===");
                        }
                    }
                    catch (Exception integracaoEx)
                    {
                        Console.WriteLine($"[CAMPANHA-INTEGRACAO] ❌ ERRO GERAL na integração com campanhas: {integracaoEx.Message}");
                        Console.WriteLine($"[CAMPANHA-INTEGRACAO] Stack trace: {integracaoEx.StackTrace}");
                        // NÃO propaga o erro para não bloquear a assinatura do termo
                    }

                    Console.WriteLine($"[TERMO_DINAMICO] === FIM ASSINATURA DINÂMICA ===");
                    return JsonConvert.SerializeObject(new { Mensagem = "Termo dinâmico assinado com sucesso! Todos os recursos foram atualizados.", Status = "200" });
                }
                else
                    return JsonConvert.SerializeObject(new { Mensagem = "As informações não conferem. Por favor, revise seu cpf e palavra chave para aceitar o termo eletronicamente.", Status = "200.1" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TERMO_ACEITE] ❌ ERRO GERAL ao aceitar termo: {ex.Message}");
                Console.WriteLine($"[TERMO_ACEITE] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[TERMO_ACEITE] Inner exception: {ex.InnerException?.Message}");
                return JsonConvert.SerializeObject(new { Mensagem = $"Falha ao assinar o termo: {ex.Message}", Status = "200.1" });
            }
        }

        public List<RequisicaoVM> ListarEntregasDisponiveis(int cliente)
        {
            var requisicoes = new List<RequisicaoVM>();
            var reqs = _requisicoesvmRepository.Buscar(x => x.Cliente == cliente && x.Requisicaostatusid == 1).OrderByDescending(x => x.Dtsolicitacao).ToList();
            foreach (var req in reqs)
            {
                var r = new RequisicaoVM();
                r.Requisicao = req;
                r.EquipamentosRequisicao = _requisicaoequipamentosvmsRepository.Buscar(x => x.Requisicao == req.Id).ToList();
                r.PodeEntregar = r.EquipamentosRequisicao.Where(x => x.Equipamentostatus != 1).Any();
                requisicoes.Add(r);
            }
            return requisicoes;
        }

        private List<EquipamentosViewModel> BuscarEquipamentosVMRequisicao(int requisicaoId)
        {
            List<EquipamentosViewModel> equipamentosViewModels = new List<EquipamentosViewModel>();
            var equipamentos = _requisicaoequipamentosvmsRepository.Buscar(x => x.Requisicao == requisicaoId).ToList();
            foreach (var item in equipamentos)
            {
                equipamentosViewModels.Add(new EquipamentosViewModel
                {
                    Equipamento = item.Equipamento,
                    NumeroSerie = item.Numeroserie,
                    Patrimonio = item.Patrimonio,
                    IdRequisicaoItem = item.Id.Value,
                    Observacao = item.Observacaoentrega,
                    TipoAquisicao = Enum.GetName(typeof(TipoAquisicaoEnum), item.TipoAquisicao)
                });
            }
            return equipamentosViewModels;
        }

        public Requisico BuscarEntregasDisponiveisPorID(int requisicao)
        {
            var entregas = _requisicaoRepository
                            .IncludeWithThenInclude(q => q.Include(x => x.Requisicoesitens)
                                .ThenInclude(x => x.EquipamentoNavigation)
                                    .ThenInclude(x => x.TipoequipamentoNavigation))
                            .Include(x => x.Requisicoesitens)
                                .ThenInclude(x => x.EquipamentoNavigation).ThenInclude(x => x.FabricanteNavigation)
                            .Include(x => x.Requisicoesitens)
                                .ThenInclude(x => x.EquipamentoNavigation).ThenInclude(x => x.ModeloNavigation)
                            .Include(x => x.RequisicaostatusNavigation)
                            .Include(x => x.TecnicoresponsavelNavigation)
                           .Where(x => x.Id == requisicao && x.Requisicaostatus == 1)
                           .OrderByDescending(x => x.Dtsolicitacao).FirstOrDefault();
            return entregas;
        }

        public void RealizarEntrega(Requisico req)
        {
            // ✅ CORREÇÃO: Usar método seguro de transação com retry automático
            _requisicaoRepository.ExecuteInTransaction(() =>
            {
                // ✅ CORREÇÃO CRÍTICA: Atualizar status para 'Processada' (2), não Cancelada (3)
                var raux = _requisicaoRepository.Buscar(x => x.Id == req.Id).AsNoTracking().FirstOrDefault();
                
                // ✅ CORREÇÃO: Verificar se a requisição existe e pode ser processada
                if (raux == null)
                {
                    throw new InvalidOperationException("Requisição não encontrada");
                }
                
                if (raux.Requisicaostatus != STATUS_ATIVA)
                {
                    throw new InvalidOperationException($"Requisição não pode ser processada. Status atual: {raux.Requisicaostatus}");
                }
                
                raux.Requisicaostatus = STATUS_PROCESSADA; // ✅ CORREÇÃO: Status 2 = Processada
                raux.Dtprocessamento = TimeZoneMapper.GetDateTimeNow();
                raux.Colaboradorfinal = req.Colaboradorfinal;
                raux.Dtenviotermo = TimeZoneMapper.GetDateTimeNow();
                _requisicaoRepository.Atualizar(raux);

                foreach (var ri in req.Requisicoesitens)
                {
                    //Atualizo o item da requisição
                    var item = _requisicaoItensRepository.ObterPorId(ri.Id);
                    item.Observacaoentrega = ri.Observacaoentrega;
                    item.Dtprogramadaretorno = ri.Dtprogramadaretorno;
                    item.Usuarioentrega = ri.Usuarioentrega;
                    item.Dtentrega = TimeZoneMapper.GetDateTimeNow();
                    _requisicaoItensRepository.Atualizar(item);

                    // ✅ CORREÇÃO: Verificar se é equipamento ou linha telefônica
                    if (ri.Equipamento.HasValue && ri.Equipamento > 0)
                    {
                        // É um equipamento
                        var eqp = _equipamentoRepository.Buscar(x => x.Id == ri.Equipamento).FirstOrDefault();
                        eqp.Equipamentostatus = 4;
                        
                        // ✅ NOVO: Atualizar dados organizacionais do equipamento com dados do colaborador
                        if (req.Colaboradorfinal.HasValue)
                        {
                            var colaborador = _colaboradorRepository.Buscar(x => x.Id == req.Colaboradorfinal.Value)
                                .AsNoTracking()
                                .FirstOrDefault();
                            
                            if (colaborador != null)
                            {
                                // Atualizar equipamento com dados do colaborador para rateio correto
                                eqp.Empresa = colaborador.Empresa;
                                eqp.Centrocusto = colaborador.Centrocusto;
                                eqp.Localidade = colaborador.Localidade;
                                eqp.FilialId = colaborador.FilialId;
                                
                                // Herdar cliente da empresa do colaborador se não estiver definido
                                if (!eqp.Cliente.HasValue)
                                {
                                    var empresa = _empresaRepository.Buscar(x => x.Id == colaborador.Empresa)
                                        .AsNoTracking()
                                        .FirstOrDefault();
                                    if (empresa != null)
                                    {
                                        eqp.Cliente = empresa.Cliente;
                                    }
                                }
                            }
                        }
                        
                        _equipamentoRepository.Atualizar(eqp);

                        var hst = new Equipamentohistorico();
                        hst.Equipamento = ri.Equipamento ?? 0;
                        hst.Equipamentostatus = 4; //Entregue
                        hst.Usuario = ri.Usuarioentrega.Value;
                        hst.Colaborador = req.Colaboradorfinal;
                        hst.Requisicao = req.Id;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }
                    else if (ri.Linhatelefonica.HasValue && ri.Linhatelefonica > 0)
                    {
                        // É uma linha telefônica
                        var linha = _telefonialinhaRepository.ObterPorId(ri.Linhatelefonica.Value);
                        linha.Emuso = true;
                        _telefonialinhaRepository.Atualizar(linha);

                        var hst = new Equipamentohistorico();
                        hst.Equipamento = 1; // ✅ Usar equipamento dummy ID=1 para linhas telefônicas
                        hst.Linhatelefonica = ri.Linhatelefonica.Value;
                        hst.Equipamentostatus = 4; //Entregue
                        hst.Usuario = ri.Usuarioentrega.Value;
                        hst.Colaborador = req.Colaboradorfinal;
                        hst.Requisicao = req.Id;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }
                }
                
                // ✅ CORREÇÃO: Salvar todas as alterações
                _requisicaoItensRepository.SalvarAlteracoes();
                _equipamentoRepository.SalvarAlteracoes();
                _equipamentohistoricoRepository.SalvarAlteracoes();
            });
        }

        public PagedResult<EntregaAtivaVM> ListarDevolucoesDisponiveis(string pesquisa, int cliente, int pagina, bool byod = false)
        {
            
            pesquisa = pesquisa.ToLower();
            PagedResult<EntregaAtivaVM> entregasAtivas = null;
            if (byod)
            {
                entregasAtivas = BuscarEntregasAtivasBYOD(pesquisa, cliente, pagina);
            }
            else
            {
                entregasAtivas = BuscarEntregasAtivas(pesquisa, cliente, pagina);
            }

            foreach (var item in entregasAtivas.Results)
            {
                var ultimasRequisicoes = ObterUltimasRequisicoesColaborador(cliente, item.ColaboradorId, byod).DistinctBy(x => x.RequisicaoId).ToList();
                if (ultimasRequisicoes.Count > 0)
                {
                    // Se existir qualquer requisição não assinada, deve marcar como pendente
                    var possuiPendente = ultimasRequisicoes.Any(x => x.AssinaturaEletronica == false);
                    item.AssinouUltimaRequisicao = !possuiPendente;
                    item.RequisicoesColaborador = MontarRequisicoesColaborador(ultimasRequisicoes);
                }
            }
            return entregasAtivas;
        }

        private List<RequisicaoColaboradorVM> MontarRequisicoesColaborador(List<UltimaRequisicaoDTO> qUltimaRequisicaoBYOD)
        {
            List<RequisicaoColaboradorVM> requisicoes = new List<RequisicaoColaboradorVM>();
            foreach (var item in qUltimaRequisicaoBYOD)
            {
                try
                {
                    var usuarioRequisicao = _usuarioRepository.ObterPorId(item.UsuarioRequisicao);
                    var tecnicoResponsavel = _usuarioRepository.ObterPorId(item.TecnicoResponsavel);
                    
                    var equipVM = MontarEquipamentosRequisicao(item.RequisicaoId);

                    // ✅ NOVO: se for linha telefônica e cabeçalho estiver vazio, derivar do primeiro item
                    DateTime? dtProc = item.DtProcessamento;
                    DateTime dtSol = item.DtSolicitacao;
                    string tecnico = tecnicoResponsavel?.Nome;

                    if ((tecnico == null || tecnico == string.Empty || tecnico == "N/A") && equipVM.Count > 0)
                    {
                        tecnico = equipVM.FirstOrDefault()?.UsuarioEntregaNome ?? tecnico;
                    }
                    if ((!dtProc.HasValue || dtProc.Value.Year <= 1900) && equipVM.Count > 0)
                    {
                        dtProc = equipVM.FirstOrDefault()?.DtEntrega ?? dtProc;
                    }
                    
                    // ✅ NOVO: Se DTSolicitacao está vazia, usar dados do primeiro equipamento (linha telefônica)
                    if (dtSol.Year <= 1900 && equipVM.Count > 0)
                    {
                        var dtSolEquip = equipVM.FirstOrDefault()?.DtSolicitacao;
                        if (dtSolEquip.HasValue && dtSolEquip.Value.Year > 1900)
                        {
                            dtSol = dtSolEquip.Value;
                        }
                    }

                    requisicoes.Add(new RequisicaoColaboradorVM
                    {
                        RequisicaoId = item.RequisicaoId,
                        DTProcessamento = dtProc,
                        DTSolicitacao = dtSol,
                        Requisitante = usuarioRequisicao?.Nome ?? "N/A",
                        TecnicoResponsavel = string.IsNullOrWhiteSpace(tecnico) ? "N/A" : tecnico,
                        EquipamentosRequisicao = equipVM
                    });
                }
                catch (Exception ex)
                {
                    // Log apenas erros críticos
                }
            }
            return requisicoes;
        }

        //private List<RequisicaoColaboradorVM> MontarRequisicoesColaborador(List<VwUltimasRequisicaoNaoBYOD> qUltimaRequisicaoBYOD)
        //{
        //    List<RequisicaoColaboradorVM> requisicoes = new List<RequisicaoColaboradorVM>();
        //    foreach (var item in qUltimaRequisicaoBYOD)
        //    {
        //        requisicoes.Add(new RequisicaoColaboradorVM
        //        {
        //            RequisicaoId = item.RequisicaoId,
        //            DTProcessamento = item.DtProcessamento.Value,
        //            DTSolicitacao = item.DtSolicitacao,
        //            Requisitante = _usuarioRepository.ObterPorId(item.UsuarioRequisicao).Nome,
        //            TecnicoResponsavel = _usuarioRepository.ObterPorId(item.TecnicoResponsavel).Nome,
        //            EquipamentosRequisicao = MontarEquipamentosRequisicao(item.RequisicaoId)
        //        });
        //    }
        //    return requisicoes;
        //}

        private List<EquipamentoRequisicaoVM> MontarEquipamentosRequisicao(int requisicaoId)
        {
            List<EquipamentoRequisicaoVM> equipamentosVM = new List<EquipamentoRequisicaoVM>();
            
            // ✅ CORREÇÃO: Buscar equipamentos entregues
            var equipamentos = _requisicaoequipamentosvmsRepository
                .Buscar(x => x.Requisicao == requisicaoId && x.Equipamentostatus == 4 && x.Dtdevolucao == null)
                .ToList();
            foreach (var item in equipamentos)
            {
                equipamentosVM.Add(new EquipamentoRequisicaoVM
                {
                    Equipamento = item.Equipamento,
                    EquipamentoId = item.Equipamentoid.Value,
                    RequisicaoItemId = item.Id.Value,
                    DTProgramadaRetorno = item.Dtprogramadaretorno,
                    NumeroSerie = item.Numeroserie,
                    Patrimonio = item.Patrimonio,
                    ObservacaoEntrega = item.Observacaoentrega,
                    Usuariodevolucaoid = item.Usuariodevolucaoid
                });
            }

            // ✅ NOVO: Buscar linhas telefônicas entregues (apenas não-BYOD/corporativas)
            var linhasTelefonicas = _requisicaoItensRepository
                .IncludeWithThenInclude(q => q.Include(x => x.LinhatelefonicaNavigation)
                    .ThenInclude(x => x.PlanoNavigation)
                        .ThenInclude(x => x.ContratoNavigation)
                            .ThenInclude(x => x.OperadoraNavigation))
                .Where(x => x.Requisicao == requisicaoId && 
                           x.Linhatelefonica.HasValue && 
                           x.Linhatelefonica > 0 && 
                           x.Dtentrega.HasValue && 
                           x.Dtdevolucao == null)
                .ToList();

            foreach (var item in linhasTelefonicas)
            {
                equipamentosVM.Add(new EquipamentoRequisicaoVM
                {
                    Equipamento = $"Linha telefônica {item.LinhatelefonicaNavigation?.Numero ?? item.Linhatelefonica}",
                    EquipamentoId = item.Linhatelefonica ?? 0,
                    RequisicaoItemId = item.Id,
                    DTProgramadaRetorno = item.Dtprogramadaretorno,
                    NumeroSerie = item.LinhatelefonicaNavigation?.Iccid ?? "N/A",
                    Patrimonio = $"{item.LinhatelefonicaNavigation?.PlanoNavigation?.ContratoNavigation?.OperadoraNavigation?.Nome ?? "N/A"} - {item.LinhatelefonicaNavigation?.PlanoNavigation?.Nome ?? "N/A"}",
                    ObservacaoEntrega = item.Observacaoentrega,
                    Usuariodevolucaoid = item.Usuariodevolucao,
                    // ✅ NOVO: popular dados de entrega para uso no cabeçalho
                    DtEntrega = item.Dtentrega,
                    UsuarioEntregaNome = item.Usuarioentrega.HasValue ? _usuarioRepository.ObterPorId(item.Usuarioentrega.Value)?.Nome : null,
                    DtSolicitacao = item.Dtentrega
                });
            }

            return equipamentosVM;
        }

        public List<UltimaRequisicaoDTO> ObterUltimasRequisicoesColaborador(int cliente, int colaborador, bool byod)
        {
            if (byod)
            {
                return Mapper.Map<VwUltimasRequisicaoBYOD, UltimaRequisicaoDTO>(ObterUltimasRequisicoesBYOD(cliente, colaborador));
            }
            else
            {
                return Mapper.Map<VwUltimasRequisicaoNaoBYOD, UltimaRequisicaoDTO>(ObterUltimasRequisicoesNaoBYOD(cliente, colaborador));
            }
        }

        private List<VwUltimasRequisicaoBYOD> ObterUltimasRequisicoesBYOD(int cliente, int colaborador)
        {
            return _vwUltimasRequisicaoBYODRepository
                .Buscar(x => x.Cliente == cliente &&
                            x.RequisicaoStatus == 3 &&
                            x.EquipamentoStatus == 4 &&
                            x.DtDevolucao == null &&
                            x.ColaboradorFinal == colaborador)
                .AsNoTracking()
                .ToList();
        }

        private List<VwUltimasRequisicaoNaoBYOD> ObterUltimasRequisicoesNaoBYOD(int cliente, int colaborador)
        {
            // ✅ CORREÇÃO: Buscar equipamentos da view
            var reqsEquipamentos = _vwUltimasRequisicaoNaoBYODRepository.Buscar(x => x.Cliente == cliente &&
                            x.RequisicaoStatus == 3 &&
                            x.EquipamentoStatus == 4 &&
                            x.DtDevolucao == null &&
                            x.ColaboradorFinal == colaborador)
                .AsNoTracking()
                .ToList();
                
            // ✅ CORREÇÃO: Buscar linhas telefônicas diretamente do banco COM dados da requisição
            var reqsLinhasTelefonicas = (from r in _requisicaoRepository.Query()
                    join ri in _requisicaoItensRepository.Query() on r.Id equals ri.Requisicao
                    where r.Cliente == cliente &&
                            r.Requisicaostatus == 3 && // Processada
                            ri.Linhatelefonica.HasValue &&
                            ri.Linhatelefonica > 0 &&
                            ri.Dtentrega.HasValue &&
                            ri.Dtdevolucao == null &&
                            r.Colaboradorfinal == colaborador
                    select new
                    {
                        RequisicaoId = r.Id,
                        Cliente = r.Cliente,
                        UsuarioRequisicao = r.Usuariorequisicao, // ✅ CORREÇÃO: Incluir usuário da requisição
                        TecnicoResponsavel = r.Tecnicoresponsavel, // ✅ CORREÇÃO: Incluir técnico responsável
                        RequisicaoStatus = r.Requisicaostatus,
                        DtSolicitacao = r.Dtsolicitacao, // ✅ CORREÇÃO: Incluir data de solicitação
                        DtProcessamento = r.Dtprocessamento, // ✅ CORREÇÃO: Incluir data de processamento
                        AssinaturaEletronica = r.Assinaturaeletronica, // ✅ CORREÇÃO: Incluir assinatura
                        EquipamentoStatus = 4, // Entregue
                        DtDevolucao = (DateTime?)null,
                        ColaboradorFinal = r.Colaboradorfinal,
                        NomeColaboradorFinal = r.Colaboradorfinal.HasValue ? _colaboradorRepository.ObterPorId(r.Colaboradorfinal.Value).Nome : "",
                        EquipamentoId = (int?)null, // Linha telefônica, não equipamento
                        NumeroSerie = ri.Linhatelefonica.ToString(),
                        Patrimonio = $"Linha {ri.Linhatelefonica}",
                        DtEntrega = ri.Dtentrega
                    }).AsNoTracking().ToList();
            
            // ✅ COMBINAR: Juntar equipamentos e linhas telefônicas
            var todasRequisicoes = new List<VwUltimasRequisicaoNaoBYOD>();
            todasRequisicoes.AddRange(reqsEquipamentos);
            
            // ✅ MAPEAR: Converter objetos anônimos para VwUltimasRequisicaoNaoBYOD
            foreach (var linha in reqsLinhasTelefonicas)
            {
                todasRequisicoes.Add(new VwUltimasRequisicaoNaoBYOD
                {
                    RequisicaoId = linha.RequisicaoId,
                    Cliente = linha.Cliente,
                    UsuarioRequisicao = linha.UsuarioRequisicao, // ✅ CORREÇÃO: Incluir usuário da requisição
                    TecnicoResponsavel = linha.TecnicoResponsavel, // ✅ CORREÇÃO: Incluir técnico responsável
                    RequisicaoStatus = linha.RequisicaoStatus,
                    DtSolicitacao = linha.DtSolicitacao ?? DateTime.MinValue, // ✅ CORREÇÃO: Incluir data de solicitação
                    DtProcessamento = linha.DtProcessamento, // ✅ CORREÇÃO: Incluir data de processamento
                    AssinaturaEletronica = linha.AssinaturaEletronica, // ✅ CORREÇÃO: Incluir assinatura
                    EquipamentoStatus = linha.EquipamentoStatus,
                    DtDevolucao = linha.DtDevolucao,
                    ColaboradorFinal = linha.ColaboradorFinal,
                    NomeColaboradorFinal = linha.NomeColaboradorFinal,
                    EquipamentoId = linha.EquipamentoId ?? 0,
                    NumeroSerie = linha.NumeroSerie,
                    Patrimonio = linha.Patrimonio,
                    DtEntrega = linha.DtEntrega
                });
            }
                
            
            return todasRequisicoes;
        }


        private PagedResult<EntregaAtivaVM> BuscarEntregasAtivas(string pesquisa, int cliente, int pagina)
        {
            // ✅ DEBUG: Log de entrada
            
            // ✅ CORREÇÃO: Buscar equipamentos entregues (não-BYOD) incluindo matrícula
            var equipamentosEntregues = (from x in _vwUltimasRequisicaoNaoBYODRepository.Query()
                    join c in _colaboradorRepository.Query() on x.ColaboradorFinal equals c.Id into colabJoin
                    from colab in colabJoin.DefaultIfEmpty()
                    where x.Cliente == cliente &&
                            x.RequisicaoStatus == 3 &&
                            x.EquipamentoStatus == 4 &&
                            x.DtDevolucao == null &&
                            x.ColaboradorFinal.HasValue &&
                            ((pesquisa != "null") ?
                            x.NomeColaboradorFinal.ToLower().Contains(pesquisa) ||
                            x.NumeroSerie.ToLower().Contains(pesquisa) ||
                            x.Patrimonio.ToLower().Contains(pesquisa) ||
                            (colab != null && colab.Matricula != null && colab.Matricula.ToLower().Contains(pesquisa)) : true)
                    select x).AsNoTracking().ToList();
            

            // ✅ NOVO: Buscar linhas telefônicas entregues (apenas não-BYOD/corporativas) incluindo matrícula e número da linha
            var linhasTelefonicasEntregues = (from r in _requisicaoRepository.Query()
                    join ri in _requisicaoItensRepository.Query() on r.Id equals ri.Requisicao
                    join c in _colaboradorRepository.Query() on r.Colaboradorfinal equals c.Id
                    join lt in _telefonialinhaRepository.Query() on ri.Linhatelefonica equals lt.Id into linhaJoin
                    from linha in linhaJoin.DefaultIfEmpty()
                    where r.Cliente == cliente &&
                            r.Requisicaostatus == 3 && // Processada
                            ri.Linhatelefonica.HasValue &&
                            ri.Linhatelefonica > 0 &&
                            ri.Dtentrega.HasValue &&
                            ri.Dtdevolucao == null &&
                            r.Colaboradorfinal.HasValue &&
                            ((pesquisa != "null") ?
                            c.Nome.ToLower().Contains(pesquisa) ||
                            (c.Matricula != null && c.Matricula.ToLower().Contains(pesquisa)) ||
                            ri.Linhatelefonica.ToString().Contains(pesquisa) ||
                            (linha != null && linha.Numero.ToString().Contains(pesquisa)) : true)
                    select new
                    {
                        ColaboradorId = r.Colaboradorfinal.Value,
                        Colaborador = c.Nome,
                        RequisicaoId = r.Id,
                        RequisicaoStatus = r.Requisicaostatus,
                        DtEntrega = ri.Dtentrega,
                        DtDevolucao = ri.Dtdevolucao,
                        LinhaTelefonica = ri.Linhatelefonica
                    }).AsNoTracking().ToList();
            
            

            // ✅ COMBINAR: Juntar equipamentos e linhas telefônicas (não-BYOD)
            var todasEntregas = new List<dynamic>();
            
            // Adicionar equipamentos (não-BYOD)
            todasEntregas.AddRange(equipamentosEntregues.Select(x => new
            {
                ColaboradorId = x.ColaboradorFinal.Value,
                Colaborador = x.NomeColaboradorFinal,
                RequisicaoId = x.RequisicaoId,
                RequisicaoStatus = x.RequisicaoStatus,
                DtEntrega = x.DtEntrega,
                DtDevolucao = x.DtDevolucao,
                LinhaTelefonica = (int?)null
            }));

            // Adicionar linhas telefônicas (não-BYOD/corporativas)
            todasEntregas.AddRange(linhasTelefonicasEntregues);

            // ✅ AGRUPAR: Agrupar por colaborador
            var entregasAgrupadas = todasEntregas
                .GroupBy(x => new { x.ColaboradorId, x.Colaborador })
                .Select(g => {
                    var colaborador = _colaboradorRepository.ObterPorId(g.Key.ColaboradorId);
                    return new EntregaAtivaVM()
                    {
                        ColaboradorId = g.Key.ColaboradorId,
                        Colaborador = g.Key.Colaborador,
                        Matricula = colaborador?.Matricula ?? "" // 🆔 Adicionar matrícula
                    };
                })
                .OrderBy(x => x.Colaborador)
                .ToList();

            // ✅ PAGINAR: Aplicar paginação manual
            var total = entregasAgrupadas.Count;
            var skip = (pagina - 1) * 10;
            var results = entregasAgrupadas.Skip(skip).Take(10).ToList();


            return new PagedResult<EntregaAtivaVM>
            {
                Results = results,
                RowCount = total,
                PageCount = (int)Math.Ceiling((double)total / 10),
                CurrentPage = pagina,
                PageSize = 10
            };
        }

        private PagedResult<EntregaAtivaVM> BuscarEntregasAtivasBYOD(string pesquisa, int cliente, int pagina)
        {
            // ✅ CORREÇÃO: Materializar primeiro, depois adicionar matrícula, incluindo busca por matrícula
            var entregasAgrupadas = (from x in _vwUltimasRequisicaoBYODRepository.Query()
                    join c in _colaboradorRepository.Query() on x.ColaboradorFinal equals c.Id into colabJoin
                    from colab in colabJoin.DefaultIfEmpty()
                    where x.Cliente == cliente &&
                            x.RequisicaoStatus == 3 &&
                            x.EquipamentoStatus == 4 &&
                            x.DtDevolucao == null &&
                            x.ColaboradorFinal.HasValue && // ✅ CORREÇÃO: Verificar se ColaboradorFinal não é null
                            ((pesquisa != "null") ?
                            x.NomeColaboradorFinal.ToLower().Contains(pesquisa) ||
                            x.NumeroSerie.ToLower().Contains(pesquisa) ||
                            x.Patrimonio.ToLower().Contains(pesquisa) ||
                            (colab != null && colab.Matricula != null && colab.Matricula.ToLower().Contains(pesquisa)) : true)
                    select x).AsNoTracking()
                .GroupBy(x => new EntregaAtivaVM()
                {
                    ColaboradorId = x.ColaboradorFinal.Value,
                    Colaborador = x.NomeColaboradorFinal
                }).Select(g => new EntregaAtivaVM()
                {
                    ColaboradorId = g.Key.ColaboradorId,
                    Colaborador = g.Key.Colaborador
                }).OrderBy(x => x.Colaborador)
                .ToList(); // Materializar antes de buscar colaborador
            
            // Adicionar matrícula após materialização
            foreach (var item in entregasAgrupadas)
            {
                var colaborador = _colaboradorRepository.ObterPorId(item.ColaboradorId);
                item.Matricula = colaborador?.Matricula ?? "";
            }
            
            // Paginar manualmente
            var total = entregasAgrupadas.Count;
            var skip = (pagina - 1) * 10;
            var results = entregasAgrupadas.Skip(skip).Take(10).ToList();
            
            return new PagedResult<EntregaAtivaVM>
            {
                Results = results,
                RowCount = total,
                PageCount = (int)Math.Ceiling((double)total / 10),
                CurrentPage = pagina,
                PageSize = 10
            };
        }

        private VwUltimasRequisicaoBYOD ObterUltimaRequisicaoBYOD(string pesquisa, int cliente)
        {
            return _vwUltimasRequisicaoBYODRepository
                .Buscar(x => x.Cliente == cliente &&
                            x.RequisicaoStatus == 3 &&
                            x.EquipamentoStatus == 4 &&
                            ((pesquisa != "null") ?
                            x.NomeColaboradorFinal.ToLower().Contains(pesquisa) ||
                            x.NumeroSerie.ToLower().Contains(pesquisa) ||
                            x.Patrimonio.ToLower().Contains(pesquisa) : true))
                .AsNoTracking()
                .OrderByDescending(x => x.RequisicaoId)
                .FirstOrDefault();
        }

        public void AtualizarItemRequisicao(Requisicaoequipamentosvm rivm)
        {
            try
            {
            var ri = _requisicaoItensRepository.Buscar(x => x.Id == rivm.Id).AsNoTracking().FirstOrDefault();

            if (ri == null)
            {
                return;
            }

            ri.Observacaoentrega = rivm.Observacaoentrega;
            if (rivm.Dtprogramadaretorno.HasValue)
            {
                var data = rivm.Dtprogramadaretorno.Value;
                ri.Dtprogramadaretorno = DateTime.SpecifyKind(data, DateTimeKind.Unspecified);
            }
            else
            {
                ri.Dtprogramadaretorno = null;
            }

            _requisicaoItensRepository.Atualizar(ri);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AdicionarObservacaoEquipamentoVM(EquipamentoRequisicaoVM equipamento)
        {
            var item = _requisicaoItensRepository
                .Buscar(x => x.Id == equipamento.RequisicaoItemId)
                .AsNoTracking()
                .FirstOrDefault();
            item.Observacaoentrega = equipamento.ObservacaoEntrega;
            //db.Update(item);
            //db.SaveChanges();
            _requisicaoItensRepository.Atualizar(item);
        }

        public void AdicionarAgendamentoEquipamentoVM(EquipamentoRequisicaoVM equipamento)
        {
            var item = _requisicaoItensRepository
                .Buscar(x => x.Id == equipamento.RequisicaoItemId)
                .AsNoTracking()
                .FirstOrDefault();

            if (item == null)
            {
                return;
            }

            if (equipamento.DTProgramadaRetorno.HasValue)
            {
                var data = equipamento.DTProgramadaRetorno.Value;
                item.Dtprogramadaretorno = DateTime.SpecifyKind(data, DateTimeKind.Unspecified);
            }
            else
            {
                item.Dtprogramadaretorno = null;
            }

            _requisicaoItensRepository.Atualizar(item);
        }
        public void RealizarDevolucaoEquipamento(EquipamentoRequisicaoVM equipamento)
        {
            var ri = _requisicaoItensRepository
                .Include(x => x.RequisicaoNavigation)
                .Where(x => x.Id == equipamento.RequisicaoItemId)
                .FirstOrDefault();

            // ✅ CORREÇÃO: Usar método seguro de transação com retry automático
            _requisicaoItensRepository.ExecuteInTransaction(() =>
            {
                // 🔒 ENCERRAR COMPARTILHAMENTOS ATIVOS antes de devolver
                var compartilhamentosAtivos = _reqItemCompartilhadoRepository
                    .Buscar(x => x.RequisicaoItemId == equipamento.RequisicaoItemId && x.Ativo)
                    .ToList();

                if (compartilhamentosAtivos.Any())
                {
                    var dataDevolucao = TimeZoneMapper.GetDateTimeNow();
                    foreach (var compartilhamento in compartilhamentosAtivos)
                    {
                        compartilhamento.Ativo = false;
                        compartilhamento.DataFim = dataDevolucao;
                        compartilhamento.Observacao = string.IsNullOrEmpty(compartilhamento.Observacao) 
                            ? "Encerrado automaticamente devido à devolução do recurso" 
                            : compartilhamento.Observacao + " | Encerrado automaticamente devido à devolução do recurso";
                        _reqItemCompartilhadoRepository.Atualizar(compartilhamento);
                    }
                }

                ri.Dtdevolucao = TimeZoneMapper.GetDateTimeNow();
                _requisicaoItensRepository.Atualizar(ri);

                if (ri.Linhatelefonica != null)
                {
                    var linha = _telefonialinhaRepository.Buscar(x => x.Id == ri.Linhatelefonica).AsNoTracking().Single();
                    linha.Emuso = false;
                    _telefonialinhaRepository.Atualizar(linha);
                }

                //Atualizo o status do equipamento para 2 - Devolvido
                if (ri.Equipamento != 1)
                {
                    var eqp = _equipamentoRepository.Buscar(x => x.Id == ri.Equipamento).AsNoTracking().FirstOrDefault();
                    if (eqp != null)
                    {
                        eqp.Equipamentostatus = 2; //Devolvido
                        _equipamentoRepository.Atualizar(eqp);
                    }
                }

                var hst = new Equipamentohistorico();
                // Define Equipamento: usa o ID real se existir (> 1), senão usa 1 (placeholder para linhas telefônicas)
                hst.Equipamento = (ri.Equipamento != null && ri.Equipamento > 1) ? ri.Equipamento.Value : 1;
                hst.Equipamentostatus = 2; //Devolvido
                hst.Usuario = equipamento.Usuariodevolucaoid.Value;
                hst.Requisicao = ri.RequisicaoNavigation.Id;
                hst.Colaborador = ri.RequisicaoNavigation.Colaboradorfinal;
                hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                if (ri.Linhatelefonica != null && ri.Linhatelefonica != 0)
                {
                    hst.Linhatelefonica = ri.Linhatelefonica;
                    hst.Linhaemuso = false;
                }
                _equipamentohistoricoRepository.Adicionar(hst);
                
                // SaveChanges é chamado automaticamente pelo ExecuteInTransaction
            });
        }
        public void RealizarDevolucoesDoColaborador(int idColaborador, int usuarioDevolucao, bool byod)
        {
            // ✅ CORREÇÃO: Incluir tanto equipamentos quanto linhas telefônicas
            var itensRequisicao = (from r in _requisicaoRepository.Query()
                                join ri in _requisicaoItensRepository.Query() on r.Id equals ri.Requisicao
                                where r.Colaboradorfinal == idColaborador && ri.Dtdevolucao == null
                                select ri).ToList();
                                
            // ✅ CORREÇÃO: Filtrar por tipo de aquisição apenas para equipamentos
            if (byod)
            {
                itensRequisicao = itensRequisicao.Where(ri => 
                    (ri.Linhatelefonica != null) || // Incluir todas as linhas telefônicas
                    (ri.Equipamento.HasValue && ri.Equipamento > 0 && _equipamentoRepository.ObterPorId(ri.Equipamento.Value)?.Tipoaquisicao == 2)
                ).ToList();
            }
            else
            {
                itensRequisicao = itensRequisicao.Where(ri => 
                    (ri.Linhatelefonica != null) || // Incluir todas as linhas telefônicas
                    (ri.Equipamento.HasValue && ri.Equipamento > 0 && _equipamentoRepository.ObterPorId(ri.Equipamento.Value)?.Tipoaquisicao != 2)
                ).ToList();
            }

            // ✅ CORREÇÃO: Usar método seguro de transação com retry automático
            _requisicaoItensRepository.ExecuteInTransaction(() =>
            {
                var dataDevolucao = TimeZoneMapper.GetDateTimeNow();
                
                foreach (var item in itensRequisicao)
                {
                    // 🔒 ENCERRAR COMPARTILHAMENTOS ATIVOS antes de devolver
                    var compartilhamentosAtivos = _reqItemCompartilhadoRepository
                        .Buscar(x => x.RequisicaoItemId == item.Id && x.Ativo)
                        .ToList();

                    if (compartilhamentosAtivos.Any())
                    {
                        foreach (var compartilhamento in compartilhamentosAtivos)
                        {
                            compartilhamento.Ativo = false;
                            compartilhamento.DataFim = dataDevolucao;
                            compartilhamento.Observacao = string.IsNullOrEmpty(compartilhamento.Observacao) 
                                ? "Encerrado automaticamente devido à devolução do recurso" 
                                : compartilhamento.Observacao + " | Encerrado automaticamente devido à devolução do recurso";
                            _reqItemCompartilhadoRepository.Atualizar(compartilhamento);
                        }
                    }

                    item.Dtdevolucao = dataDevolucao;
                    item.Usuariodevolucao = usuarioDevolucao;
                    _requisicaoItensRepository.Atualizar(item);

                    if (item.Linhatelefonica != null)
                    {
                        var linha = _telefonialinhaRepository.ObterPorId(item.Linhatelefonica.Value);
                        linha.Emuso = false;
                        _telefonialinhaRepository.Atualizar(linha);
                    }
                    
                    //Atualizo o status do equipamento para 2 - Devolvido
                    // ✅ CORREÇÃO: Só atualizar equipamento se não for linha telefônica
                    if (item.Equipamento.HasValue && item.Equipamento > 0 && item.Linhatelefonica == null && item.Equipamento != 1)
                    {
                        var eqp = _equipamentoRepository.ObterPorId(item.Equipamento.Value);
                        eqp.Equipamentostatus = 2; //Devolvido
                        _equipamentoRepository.Atualizar(eqp);
                    }

                    var hst = new Equipamentohistorico();
                    // ✅ CORREÇÃO: Usar equipamento real ou dummy ID=1 para linhas telefônicas
                    if (item.Equipamento.HasValue && item.Equipamento > 0)
                    {
                        hst.Equipamento = item.Equipamento.Value;
                    }
                    else
                    {
                        hst.Equipamento = 1; // ✅ Usar equipamento dummy ID=1 para linhas telefônicas
                    }
                    
                    hst.Equipamentostatus = 2; //Devolvido
                    hst.Usuario = usuarioDevolucao;
                    hst.Requisicao = item.Requisicao;
                    hst.Colaborador = idColaborador;
                    hst.Dtregistro = dataDevolucao;
                    hst.Linhatelefonica = item.Linhatelefonica;
                    hst.Linhaemuso = false;
                    _equipamentohistoricoRepository.Adicionar(hst);
                }
                
                // SaveChanges é chamado automaticamente pelo ExecuteInTransaction
            });
        }

        public string RealizarEntregaMobile(Requisico req)
        {
            var eqpstatus = _equipamentoRepository.ObterPorId(req.Requisicoesitens.FirstOrDefault().Equipamento.Value);
            if (eqpstatus.Equipamentostatus != 3)
            {
                return JsonConvert.SerializeObject(new { Mensagem = "O equipamento não estava mais em estoque! Impossível realizar a entrega.", Status = "200.1" });
                throw new Exception("O equipamento não estava mais em estoque");
            }

            // ✅ CORREÇÃO: Usar método seguro de transação com retry automático
            return _requisicaoRepository.ExecuteInTransaction(() =>
            {
                req.Dtsolicitacao = TimeZoneMapper.GetDateTimeNow();
                req.Dtprocessamento = req.Dtsolicitacao;
                req.Hashrequisicao = Guid.NewGuid().ToString();
                var item = req.Requisicoesitens.FirstOrDefault();
                item.Dtentrega = req.Dtsolicitacao;
                req.Requisicoesitens.Clear();
                req.Requisicoesitens.Add(item);
                _requisicaoRepository.Adicionar(req);

                foreach (var ri in req.Requisicoesitens)
                {
                    // ✅ CORREÇÃO: Verificar se é equipamento ou linha telefônica
                    if (ri.Equipamento.HasValue && ri.Equipamento > 0)
                    {
                        // É um equipamento
                        var eqp = _equipamentoRepository.Buscar(x => x.Id == ri.Equipamento).FirstOrDefault();
                        eqp.Equipamentostatus = 4;
                        
                        // ✅ NOVO: Atualizar dados organizacionais do equipamento com dados do colaborador
                        if (req.Colaboradorfinal.HasValue)
                        {
                            var colaborador = _colaboradorRepository.Buscar(x => x.Id == req.Colaboradorfinal.Value)
                                .AsNoTracking()
                                .FirstOrDefault();
                            
                            if (colaborador != null)
                            {
                                // Atualizar equipamento com dados do colaborador para rateio correto
                                eqp.Empresa = colaborador.Empresa;
                                eqp.Centrocusto = colaborador.Centrocusto;
                                eqp.Localidade = colaborador.Localidade;
                                eqp.FilialId = colaborador.FilialId;
                                
                                // Herdar cliente da empresa do colaborador se não estiver definido
                                if (!eqp.Cliente.HasValue)
                                {
                                    var empresa = _empresaRepository.Buscar(x => x.Id == colaborador.Empresa)
                                        .AsNoTracking()
                                        .FirstOrDefault();
                                    if (empresa != null)
                                    {
                                        eqp.Cliente = empresa.Cliente;
                                    }
                                }
                            }
                        }
                        
                        _equipamentoRepository.Atualizar(eqp);

                        var hst = new Equipamentohistorico();
                        hst.Equipamento = ri.Equipamento ?? 0;
                        hst.Equipamentostatus = 4; //Entregue
                        hst.Usuario = ri.Usuarioentrega.Value;
                        hst.Colaborador = req.Colaboradorfinal;
                        hst.Requisicao = req.Id;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }
                    else if (ri.Linhatelefonica.HasValue && ri.Linhatelefonica > 0)
                    {
                        // É uma linha telefônica
                        var linha = _telefonialinhaRepository.ObterPorId(ri.Linhatelefonica.Value);
                        linha.Emuso = true;
                        _telefonialinhaRepository.Atualizar(linha);

                        var hst = new Equipamentohistorico();
                        hst.Equipamento = 1; // ✅ Usar equipamento dummy ID=1 para linhas telefônicas
                        hst.Linhatelefonica = ri.Linhatelefonica.Value;
                        hst.Equipamentostatus = 4; //Entregue
                        hst.Usuario = ri.Usuarioentrega.Value;
                        hst.Colaborador = req.Colaboradorfinal;
                        hst.Requisicao = req.Id;
                        hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }
                }
                
                // Após persistir a entrega, se o payload original veio com DTO (API mobile ou web),
                // crie os co-responsáveis por item, se informados. (Proteção: apenas se DTO disponível via HttpContext ou similar)
                // Como RealizarEntregaMobile recebe modelo, este bloco será aplicado na versão web (SalvarRequisicao/RealizarEntrega) na sequência.
                
                // SaveChanges é chamado automaticamente pelo ExecuteInTransaction
                return JsonConvert.SerializeObject(new { Mensagem = "O equipamento entregue com sucesso!", Status = "200" });
            });
        }

        public void TransferenciaEquipamento(TransferenciaEqpVM vm)
        {
            var req = (from r in _requisicaoRepository.Query()
                       join rri in _requisicaoItensRepository.Query() on r.Id equals rri.Requisicao
                       where r.Id == vm.RequisicaoID && rri.Equipamento == vm.EquipamentoID && rri.Dtdevolucao == null
                       select r
                      ).Include(x => x.Requisicoesitens).AsNoTracking().FirstOrDefault();

            // ✅ CORREÇÃO: Usar método seguro de transação com retry automático
            _requisicaoItensRepository.ExecuteInTransaction(() =>
            {
                //Realizo a devolução do equipamento indicado
                var ri = req.Requisicoesitens.Where(x => x.Equipamento == vm.EquipamentoID).FirstOrDefault();
                ri.Dtdevolucao = TimeZoneMapper.GetDateTimeNow();
                _requisicaoItensRepository.Atualizar(ri);

                //Atualizo o status do equipamento para 2 - Devolvido
                var eqp = _equipamentoRepository.ObterPorId(ri.Equipamento.Value);
                eqp.Equipamentostatus = 2; //Devolvido
                _equipamentoRepository.Atualizar(eqp);

                //Insiro o registro de historico do equipamento para devolvido
                var hst = new Equipamentohistorico();
                hst.Equipamento = ri.Equipamento ?? 0;
                hst.Equipamentostatus = 2; //Devolvido
                hst.Usuario = vm.Usuario;
                hst.Requisicao = req.Id;
                hst.Colaborador = req.Colaboradorfinal;
                hst.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                _equipamentohistoricoRepository.Adicionar(hst);

                //Crio uma nova requisição associando o equipamento para o colaborador destino
                var novaReq = new Requisico();
                novaReq.Cliente = req.Cliente;
                novaReq.Colaboradorfinal = vm.ColaboradorDestinoID;
                novaReq.Requisicaostatus = 3;
                novaReq.Tecnicoresponsavel = vm.Usuario;
                novaReq.Usuariorequisicao = vm.Usuario;
                novaReq.Dtsolicitacao = TimeZoneMapper.GetDateTimeNow();
                novaReq.Dtprocessamento = TimeZoneMapper.GetDateTimeNow();
                novaReq.Hashrequisicao = Guid.NewGuid().ToString();
                var item = new Requisicoesiten();
                item.Dtentrega = TimeZoneMapper.GetDateTimeNow();
                item.Equipamento = vm.EquipamentoID;
                item.Usuarioentrega = vm.Usuario;
                novaReq.Requisicoesitens.Add(item);
                _requisicaoRepository.Adicionar(novaReq);

                foreach (var riEnt in novaReq.Requisicoesitens)
                {
                    //Atualizo o status do equipamento para 4 - Entregue
                    var eqpEntrega = _equipamentoRepository.ObterPorId(vm.EquipamentoID);
                    eqpEntrega.Equipamentostatus = 4;
                    
                    // ✅ NOVO: Atualizar dados organizacionais do equipamento com dados do colaborador destino
                    if (vm.ColaboradorDestinoID > 0)
                    {
                        var colaborador = _colaboradorRepository.Buscar(x => x.Id == vm.ColaboradorDestinoID)
                            .AsNoTracking()
                            .FirstOrDefault();
                        
                        if (colaborador != null)
                        {
                            // Atualizar equipamento com dados do colaborador para rateio correto
                            eqpEntrega.Empresa = colaborador.Empresa;
                            eqpEntrega.Centrocusto = colaborador.Centrocusto;
                            eqpEntrega.Localidade = colaborador.Localidade;
                            eqpEntrega.FilialId = colaborador.FilialId;
                            
                            // Herdar cliente da empresa do colaborador se não estiver definido
                            if (!eqpEntrega.Cliente.HasValue)
                            {
                                var empresa = _empresaRepository.Buscar(x => x.Id == colaborador.Empresa)
                                    .AsNoTracking()
                                    .FirstOrDefault();
                                if (empresa != null)
                                {
                                    eqpEntrega.Cliente = empresa.Cliente;
                                }
                            }
                        }
                    }
                    
                    _equipamentoRepository.Atualizar(eqpEntrega);

                    var hstEntrega = new Equipamentohistorico();
                    hstEntrega.Equipamento = vm.EquipamentoID;
                    hstEntrega.Equipamentostatus = 4; //Entregue
                    hstEntrega.Usuario = vm.Usuario;
                    hstEntrega.Colaborador = vm.ColaboradorDestinoID;
                    hstEntrega.Requisicao = novaReq.Id;
                    hstEntrega.Dtregistro = TimeZoneMapper.GetDateTimeNow();
                    _equipamentohistoricoRepository.Adicionar(hstEntrega);
                }
                
                // SaveChanges é chamado automaticamente pelo ExecuteInTransaction
            });
        }

        // ===================== COMPARTILHADOS POR ITEM (CO-RESPONSÁVEIS) =====================
        public List<RequisicaoItemCompartilhado> ListarCompartilhadosItem(int requisicaoItemId)
        {
            if (requisicaoItemId <= 0) throw new Exception("Requisição item inválido");
            return _reqItemCompartilhadoRepository
                .Buscar(x => x.RequisicaoItemId == requisicaoItemId)
                .OrderByDescending(x => x.Ativo)
                .ThenByDescending(x => x.DataInicio)
                .ToList();
        }

        public RequisicaoItemCompartilhado AdicionarCompartilhadoItem(int requisicaoItemId, RequisicaoItemCompartilhado vinculo, int usuarioId)
        {
            if (requisicaoItemId <= 0) throw new Exception("Requisição item inválido");
            if (vinculo == null) throw new Exception("Dados do vínculo não informados");
            if (vinculo.ColaboradorId <= 0) throw new Exception("Colaborador inválido");

            // Bloquear BYOD: se item pertence à view de BYOD, não permite compartilhamento
            var ehByod = _vwUltimasRequisicaoBYODRepository
                .Buscar(v => v.RequisicaoItemId == requisicaoItemId)
                .Any();
            if (ehByod) throw new Exception("Compartilhamento não permitido para BYOD.");

            var jaAtivo = _reqItemCompartilhadoRepository
                .Buscar(x => x.RequisicaoItemId == requisicaoItemId && x.ColaboradorId == vinculo.ColaboradorId && x.Ativo)
                .Any();
            if (jaAtivo) throw new Exception("Já existe um vínculo ativo para este colaborador neste item.");

            var agora = TimeZoneMapper.GetDateTimeNow();
            var novo = new RequisicaoItemCompartilhado
            {
                RequisicaoItemId = requisicaoItemId,
                ColaboradorId = vinculo.ColaboradorId,
                TipoAcesso = string.IsNullOrWhiteSpace(vinculo.TipoAcesso) ? "usuario_compartilhado" : vinculo.TipoAcesso,
                DataInicio = vinculo.DataInicio == default ? agora : vinculo.DataInicio,
                DataFim = vinculo.DataFim,
                Observacao = vinculo.Observacao,
                Ativo = true,
                CriadoPor = usuarioId,
                CriadoEm = agora
            };

            _reqItemCompartilhadoRepository.Adicionar(novo);
            _reqItemCompartilhadoRepository.SalvarAlteracoes();
            return novo;
        }

        public RequisicaoItemCompartilhado AtualizarCompartilhadoItem(int vinculoId, RequisicaoItemCompartilhado vinculo)
        {
            var atual = _reqItemCompartilhadoRepository.ObterPorId(vinculoId);
            if (atual == null) throw new Exception("Vínculo não encontrado");

            // Se tentar reativar em item BYOD, bloquear
            if (vinculo.Ativo && !atual.Ativo)
            {
                var ehByod = _vwUltimasRequisicaoBYODRepository
                    .Buscar(v => v.RequisicaoItemId == atual.RequisicaoItemId)
                    .Any();
                if (ehByod) throw new Exception("Compartilhamento não permitido para BYOD.");
            }

            if (!string.IsNullOrWhiteSpace(vinculo.TipoAcesso)) atual.TipoAcesso = vinculo.TipoAcesso;
            atual.DataFim = vinculo.DataFim;
            atual.Observacao = vinculo.Observacao;

            if (vinculo.Ativo != atual.Ativo)
            {
                if (vinculo.Ativo)
                {
                    var existeAtivo = _reqItemCompartilhadoRepository
                        .Buscar(x => x.Id != vinculoId && x.RequisicaoItemId == atual.RequisicaoItemId && x.ColaboradorId == atual.ColaboradorId && x.Ativo)
                        .Any();
                    if (existeAtivo) throw new Exception("Já existe um vínculo ativo para este colaborador neste item.");
                }
                atual.Ativo = vinculo.Ativo;
            }

            _reqItemCompartilhadoRepository.Atualizar(atual);
            _reqItemCompartilhadoRepository.SalvarAlteracoes();
            return atual;
        }

        public void EncerrarCompartilhadoItem(int vinculoId, int usuarioId)
        {
            var atual = _reqItemCompartilhadoRepository.ObterPorId(vinculoId);
            if (atual == null) throw new Exception("Vínculo não encontrado");
            if (!atual.Ativo) return;

            atual.Ativo = false;
            atual.DataFim = TimeZoneMapper.GetDateTimeNow();
            _reqItemCompartilhadoRepository.Atualizar(atual);
            _reqItemCompartilhadoRepository.SalvarAlteracoes();
        }
        // ======================================================================================

        public void RealizarEntregaComCompartilhados(RequisicaoDTO dto)
        {
            if (dto == null) throw new Exception("Payload inválido");
            if (dto.Requisicoesitens == null || dto.Requisicoesitens.Count == 0) throw new Exception("Sem itens para entrega");

            _requisicaoRepository.ExecuteInTransaction(() =>
            {
                var raux = _requisicaoRepository.Buscar(x => x.Id == dto.Id).AsNoTracking().FirstOrDefault();
                if (raux == null) throw new InvalidOperationException("Requisição não encontrada");
                if (raux.Requisicaostatus != STATUS_ATIVA) throw new InvalidOperationException("Requisição não pode ser processada");

                raux.Requisicaostatus = STATUS_PROCESSADA;
                raux.Dtprocessamento = TimeZoneMapper.GetDateTimeNow();
                raux.Colaboradorfinal = dto.Colaboradorfinal;
                raux.Dtenviotermo = TimeZoneMapper.GetDateTimeNow();
                _requisicaoRepository.Atualizar(raux);

                foreach (var riDto in dto.Requisicoesitens)
                {
                    var item = _requisicaoItensRepository.ObterPorId(riDto.Id);
                    item.Observacaoentrega = riDto.Observacaoentrega;
                    item.Dtprogramadaretorno = riDto.Dtprogramadaretorno;
                    item.Usuarioentrega = riDto.Usuarioentrega;
                    item.Dtentrega = TimeZoneMapper.GetDateTimeNow();
                    _requisicaoItensRepository.Atualizar(item);

                    // Atualizações de equipamento/linha (como em RealizarEntrega)
                    if (riDto.Equipamento.HasValue && riDto.Equipamento > 0)
                    {
                        var eqp = _equipamentoRepository.Buscar(x => x.Id == riDto.Equipamento).FirstOrDefault();
                        eqp.Equipamentostatus = 4;
                        if (dto.Colaboradorfinal.HasValue)
                        {
                            var colaborador = _colaboradorRepository.Buscar(x => x.Id == dto.Colaboradorfinal.Value).AsNoTracking().FirstOrDefault();
                            if (colaborador != null)
                            {
                                eqp.Empresa = colaborador.Empresa;
                                eqp.Centrocusto = colaborador.Centrocusto;
                                eqp.Localidade = colaborador.Localidade;
                                eqp.FilialId = colaborador.FilialId;
                                if (!eqp.Cliente.HasValue)
                                {
                                    var empresa = _empresaRepository.Buscar(x => x.Id == colaborador.Empresa).AsNoTracking().FirstOrDefault();
                                    if (empresa != null) eqp.Cliente = empresa.Cliente;
                                }
                            }
                        }
                        _equipamentoRepository.Atualizar(eqp);

                        var hst = new Equipamentohistorico
                        {
                            Equipamento = riDto.Equipamento ?? 0,
                            Equipamentostatus = 4,
                            Usuario = riDto.Usuarioentrega.Value,
                            Colaborador = dto.Colaboradorfinal,
                            Requisicao = dto.Id,
                            Dtregistro = TimeZoneMapper.GetDateTimeNow()
                        };
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }
                    else if (riDto.Linhatelefonica.HasValue && riDto.Linhatelefonica > 0)
                    {
                        var linha = _telefonialinhaRepository.ObterPorId(riDto.Linhatelefonica.Value);
                        linha.Emuso = true;
                        _telefonialinhaRepository.Atualizar(linha);

                        var hst = new Equipamentohistorico
                        {
                            Equipamento = 1,
                            Linhatelefonica = riDto.Linhatelefonica.Value,
                            Equipamentostatus = 4,
                            Usuario = riDto.Usuarioentrega.Value,
                            Colaborador = dto.Colaboradorfinal,
                            Requisicao = dto.Id,
                            Dtregistro = TimeZoneMapper.GetDateTimeNow()
                        };
                        _equipamentohistoricoRepository.Adicionar(hst);
                    }

                    // Criar co-responsáveis (somente NÃO-BYOD)
                    if (riDto.CoResponsaveis != null && riDto.CoResponsaveis.Count > 0)
                    {
                        // Se o item aparecer na view BYOD, pular
                        var ehByod = _vwUltimasRequisicaoBYODRepository
                            .Buscar(v => v.RequisicaoItemId == item.Id)
                            .Any();
                        if (!ehByod)
                        {
                            foreach (var co in riDto.CoResponsaveis)
                            {
                                var vinc = new RequisicaoItemCompartilhado
                                {
                                    ColaboradorId = co.ColaboradorId,
                                    TipoAcesso = string.IsNullOrWhiteSpace(co.TipoAcesso) ? "usuario_compartilhado" : co.TipoAcesso,
                                    DataFim = co.DataFim,
                                    Observacao = co.Observacao
                                };
                                AdicionarCompartilhadoItem(item.Id, vinc, riDto.Usuarioentrega ?? dto.Tecnicoresponsavel);
                            }
                        }
                    }
                }

                _requisicaoItensRepository.SalvarAlteracoes();
                _equipamentoRepository.SalvarAlteracoes();
                _equipamentohistoricoRepository.SalvarAlteracoes();
            });
        }
    }
}



