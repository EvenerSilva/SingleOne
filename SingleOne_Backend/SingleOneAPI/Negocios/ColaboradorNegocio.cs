using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SingleOne.Enumeradores;
using SingleOne.Models;
using SingleOne.Util;
using SingleOneAPI;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using System.Linq;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace SingleOne.Negocios
{
    public class ColaboradorNegocio : IColaboradorNegocio
    {
        private SendMail mail;
        private readonly EnvironmentApiSettings _environmentApiSettings;
        private readonly IRepository<Colaboradore> _repository;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<Template> _templateRepository;
        private readonly IRepository<Requisico> _requisicaoRepository;
        private readonly IRepository<Empresa> _empresaRepository;
        private readonly IRepository<Descartecargo> _descartecargoRepository;
        private readonly IRepository<CargoConfianca> _cargoConfiancaRepository;
        private readonly IReadOnlyRepository<VwUltimasRequisicaoBYOD> _viewRepositoryBYOD;
        private readonly IReadOnlyRepository<VwUltimasRequisicaoNaoBYOD> _viewRepositoryNaoBYOD;
        private readonly IReadOnlyRepository<Vwnadaconstum> _viewRepositoryVwnadaconstum;
        private readonly IReadOnlyRepository<Termoscolaboradoresvm> _viewRepositoryTermoscolaboradoresvm;
        private readonly IReadOnlyRepository<ColaboradoresVM> _viewRepositoryColaboradoresVM;
        private readonly IRepository<GeolocalizacaoAssinatura> _geolocalizacaoRepository;
        private readonly IEquipamentoNegocio _equipamentoNegocio;
        private readonly IRepository<Requisicoesiten> _requisicaoItensRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<RequisicaoItemCompartilhado> _reqItemCompartilhadoRepository;
        private readonly IRepository<Cliente> _clienteRepository;

        public ColaboradorNegocio(EnvironmentApiSettings environmentApiSettings,
            IRepository<Colaboradore> repository,
            IRepository<Usuario> usuarioRepository,
            IRepository<Template> templateRepository,
            IRepository<Requisico> requisicaoRepository,
            IRepository<Empresa> empresaRepository,
            IRepository<Descartecargo> descartecargoRepository,
            IRepository<CargoConfianca> cargoConfiancaRepository,
            IReadOnlyRepository<VwUltimasRequisicaoBYOD> viewRepositoryBYOD,
            IReadOnlyRepository<VwUltimasRequisicaoNaoBYOD> viewRepositoryNaoBYOD,
            IReadOnlyRepository<Vwnadaconstum> viewRepositoryVwnadaconstum,
            IReadOnlyRepository<Termoscolaboradoresvm> viewRepositoryTermoscolaboradoresvm,
            IReadOnlyRepository<ColaboradoresVM> viewRepositoryColaboradoresVM,
            IRepository<GeolocalizacaoAssinatura> geolocalizacaoRepository,
            IEquipamentoNegocio equipamentoNegocio,
            ISmtpConfigService smtpConfigService,
            IRepository<Requisicoesiten> requisicaoItensRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<RequisicaoItemCompartilhado> reqItemCompartilhadoRepository,
            IRepository<Cliente> clienteRepository
            )
        {
            mail = new SendMail(environmentApiSettings, smtpConfigService);
            _environmentApiSettings = environmentApiSettings;
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _templateRepository = templateRepository;
            _requisicaoRepository = requisicaoRepository;
            _empresaRepository = empresaRepository;
            _descartecargoRepository = descartecargoRepository;
            _cargoConfiancaRepository = cargoConfiancaRepository;
            _viewRepositoryBYOD = viewRepositoryBYOD;
            _viewRepositoryNaoBYOD = viewRepositoryNaoBYOD;
            _viewRepositoryVwnadaconstum = viewRepositoryVwnadaconstum;
            _viewRepositoryTermoscolaboradoresvm = viewRepositoryTermoscolaboradoresvm;
            _viewRepositoryColaboradoresVM = viewRepositoryColaboradoresVM;
            _geolocalizacaoRepository = geolocalizacaoRepository;
            _equipamentoNegocio = equipamentoNegocio;
            _requisicaoItensRepository = requisicaoItensRepository;
            _equipamentoRepository = equipamentoRepository;
            _reqItemCompartilhadoRepository = reqItemCompartilhadoRepository;
            _clienteRepository = clienteRepository;
        }

        public PagedResult<ColaboradoresVM> ListarColaboradores(string pesquisa, int cliente, int pagina)
        {
            // ✅ OTIMIZAÇÃO: Normalizar pesquisa uma vez só
            pesquisa = pesquisa?.Trim().ToLower();
            bool temPesquisa = !string.IsNullOrWhiteSpace(pesquisa) && pesquisa != "null";
            
            // ✅ OTIMIZAÇÃO: Construir query de forma eficiente
            IQueryable<ColaboradoresVM> query = _viewRepositoryColaboradoresVM
                                .Buscar(x => x.Cliente == cliente);
            
            // ✅ OTIMIZAÇÃO: Aplicar filtros de pesquisa de forma otimizada
            if (temPesquisa)
            {
                // Tentar buscar por CPF apenas se parecer um CPF (11 dígitos)
                string cpfCriptografado = null;
                if (pesquisa.Length == 11 && pesquisa.All(char.IsDigit))
                {
                    try
                    {
                        cpfCriptografado = Cripto.CriptografarDescriptografar(pesquisa, true);
                    }
                    catch
                    {
                        // Se falhar na criptografia, ignora busca por CPF
                    }
                }
                
                // Aplicar filtros de pesquisa
                query = query.Where(x => 
                    x.Nome.ToLower().Contains(pesquisa) ||
                    x.Matricula.ToLower().Contains(pesquisa) ||
                    x.Empresa.ToLower().Contains(pesquisa) ||
                    x.CodigoCentroCusto.ToLower().Contains(pesquisa) ||
                    x.NomeCentroCusto.ToLower().Contains(pesquisa) ||
                    (cpfCriptografado != null && x.Cpf.Contains(cpfCriptografado))
                );
            }
            
            // ✅ OTIMIZAÇÃO: Usar AsNoTracking() para melhor performance (read-only)
            query = query.AsNoTracking().OrderBy(x => x.Nome);
            
            var colaboradores = query.GetPaged(pagina, 10);

            // ✅ OTIMIZAÇÃO: Descriptografar apenas os registros retornados (não todos)
            foreach (var col in colaboradores.Results)
            {
                if (!string.IsNullOrEmpty(col.Email))
                    col.Email = Cripto.CriptografarDescriptografar(col.Email, false);
                if (!string.IsNullOrEmpty(col.Cpf))
                    col.Cpf = Cripto.CriptografarDescriptografar(col.Cpf, false);
            }
            return colaboradores;
        }

        public List<Colaboradore> ListarColaboradores(string pesquisa, int cliente)
        {
            pesquisa = pesquisa.ToLower();
            var colaboradores = _repository
                .Include(x => x.EmpresaNavigation)
                .Include(x => x.CentrocustoNavigation)
                .Include(x => x.LocalidadeNavigation)
                .Where(x => x.Cliente == cliente && ((pesquisa != "null" ?
                    x.Nome.ToLower().Contains(pesquisa) ||
                    x.Cpf.ToLower().Contains(pesquisa) ||
                    x.Matricula.ToLower().Contains(pesquisa) ||
                    x.EmpresaNavigation.Nome.ToLower().Contains(pesquisa) ||
                    x.CentrocustoNavigation.Nome.ToLower().Contains(pesquisa) ||
                    x.CentrocustoNavigation.Codigo.ToLower().Contains(pesquisa)
                : 1 == 1)))
                .OrderBy(x => x.Nome)
                .AsNoTracking()
                .ToList();
            foreach (var col in colaboradores)
            {
                col.Email = Cripto.CriptografarDescriptografar(col.Email, false);
            }

            return colaboradores;
        }

        public List<Colaboradore> ListarColaboradoresAtivos(string pesquisa, int cliente)
        {
            DateTime dtNow = TimeZoneMapper.GetDateTimeNow();
            pesquisa = pesquisa.ToLower();
            
            // 🎯 IMPORTANTE: Removido .Take(10) para retornar TODOS os colaboradores ativos
            // Isso é necessário para extrair todas as opções de filtros (empresas, localidades, etc.)
            var colaboradores = _repository
                .Include(x => x.EmpresaNavigation)
                .Include(x => x.CentrocustoNavigation)
                .Include(x => x.LocalidadeNavigation)
                .Include(x => x.Filial) // ✅ Adicionado para filtros de filial
                .Where(x => x.Cliente == cliente &&
                (!x.Dtdemissao.HasValue || (x.Dtdemissao.HasValue && x.Dtdemissao.Value > dtNow)) && 
                (pesquisa == "null" || x.Nome.ToLower().Contains(pesquisa)))
                .OrderBy(x => x.Nome)
                .AsNoTracking() // ✅ Performance
                .ToList();
                
            Console.WriteLine($"[NEGOCIO-ATIVOS] Total de colaboradores ativos: {colaboradores.Count}");
            if (colaboradores.Count > 0)
            {
                Console.WriteLine($"[NEGOCIO-ATIVOS] Primeiro colaborador:");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - ID: {colaboradores[0].Id}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - Nome: {colaboradores[0].Nome}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - Empresa: {colaboradores[0].Empresa}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - EmpresaNavigation: {colaboradores[0].EmpresaNavigation?.Nome}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - Localidade: {colaboradores[0].Localidade}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - LocalidadeNavigation: {colaboradores[0].LocalidadeNavigation?.Descricao}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - CentroCusto: {colaboradores[0].Centrocusto}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - CentrocustoNavigation: {colaboradores[0].CentrocustoNavigation?.Nome}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - FilialId: {colaboradores[0].FilialId}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - Filial: {colaboradores[0].Filial?.Nome}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - Setor: {colaboradores[0].Setor}");
                Console.WriteLine($"[NEGOCIO-ATIVOS]   - Cargo: {colaboradores[0].Cargo}");
            }
            Console.WriteLine($"[NEGOCIO-ATIVOS] ========== FIM ==========");
            
            return colaboradores;
        }

        private bool VerificarSituacaoColaboradorAtivo(Colaboradore colaborador)
        {
            return (!colaborador.Dtdemissao.HasValue || (colaborador.Dtdemissao.HasValue && colaborador.Dtdemissao.Value > TimeZoneMapper.GetDateTimeNow()));
        }

        public ColaboradorEstatisticasDTO ObterEstatisticas(int cliente)
        {
            DateTime dtNow = TimeZoneMapper.GetDateTimeNow();
            
            // ✅ OTIMIZAÇÃO: Usar uma única query com GROUP BY para calcular todas as estatísticas de uma vez
            var query = _repository.Buscar(x => x.Cliente == cliente);
            
            // Calcular totais em uma única passada
            var total = query.Count();
            var funcionarios = query.Count(x => x.Tipocolaborador == 'F');
            var terceiros = query.Count(x => x.Tipocolaborador == 'T');
            var consultores = query.Count(x => x.Tipocolaborador == 'C');
            var ativos = query.Count(x => !x.Dtdemissao.HasValue || (x.Dtdemissao.HasValue && x.Dtdemissao.Value > dtNow));
            var desligados = query.Count(x => x.Dtdemissao.HasValue && x.Dtdemissao.Value <= dtNow);
            
            return new ColaboradorEstatisticasDTO
            {
                Total = total,
                Funcionarios = funcionarios,
                Terceiros = terceiros,
                Consultores = consultores,
                Ativos = ativos,
                Desligados = desligados
            };
        }

        public ColaboradorCompletoDTO ObterColaboradorPorID(int id)
        {
            var colaborador = ObterColaboradorInterno(id);
            if (colaborador == null)
                return null;
                
            // Retornar DTO com todos os campos necessários para o frontend
            return new ColaboradorCompletoDTO
            {
                Id = colaborador.Id,
                Cliente = colaborador.Cliente,
                Usuario = colaborador.Usuario,
                Empresa = colaborador.EmpresaNavigation?.Nome ?? "",
                EmpresaId = colaborador.Empresa,
                NomeCentroCusto = colaborador.CentrocustoNavigation?.Nome ?? "",
                CodigoCentroCusto = colaborador.CentrocustoNavigation?.Codigo ?? "",
                Centrocusto = colaborador.Centrocusto,
                CentrocustoId = colaborador.Centrocusto,
                Localidade = colaborador.LocalidadeNavigation?.Descricao ?? "",
                LocalidadeId = colaborador.Localidade,
                Nome = colaborador.Nome,
                Cpf = colaborador.Cpf,
                Matricula = colaborador.Matricula,
                Email = colaborador.Email,
                Cargo = colaborador.Cargo,
                Setor = colaborador.Setor,
                Dtadmissao = colaborador.Dtadmissao.ToString("yyyy-MM-dd"),
                Dtdemissao = colaborador.Dtdemissao?.ToString("yyyy-MM-dd"),
                Tipocolaborador = colaborador.Tipocolaborador.ToString(),
                Situacao = colaborador.Situacao,
                Matriculasuperior = colaborador.Matriculasuperior,
                FilialId = colaborador.FilialId,
                Dtcadastro = colaborador.Dtcadastro?.ToString("yyyy-MM-dd"),
                Dtatualizacao = colaborador.Dtatualizacao?.ToString("yyyy-MM-dd"),
                Antigaempresa = colaborador.Antigaempresa,
                Antigocentrocusto = colaborador.Antigocentrocusto,
                Antigalocalidade = colaborador.Antigalocalidade,
                Situacaoantiga = colaborador.Situacaoantiga?.ToString(),
                Migrateid = colaborador.Migrateid
            };
        }

        private Colaboradore ObterColaboradorInterno(int id)
        {
            //db = new singleOneContext();
            Console.WriteLine($"[OBTER_COLABORADOR] Buscando colaborador ID: {id}");
            
            var colaborador = _repository
                .Include(x => x.EmpresaNavigation)
                .Include(x => x.CentrocustoNavigation)
                .Include(x => x.LocalidadeNavigation)
                .Where(x => x.Id == id)
                .OrderBy(x => x.Nome).FirstOrDefault();
            
            if (colaborador == null)
            {
                Console.WriteLine($"[OBTER_COLABORADOR] Colaborador ID {id} não encontrado");
            }
            else
            {
                Console.WriteLine($"[OBTER_COLABORADOR] Colaborador encontrado: {colaborador.Nome}");
                Console.WriteLine($"[OBTER_COLABORADOR] Empresa: {colaborador.EmpresaNavigation?.Nome ?? "NULL"}");
                Console.WriteLine($"[OBTER_COLABORADOR] Centro Custo: {colaborador.CentrocustoNavigation?.Nome ?? "NULL"}");
            }
            
            if (colaborador == null)
                return null;
                
            colaborador.Email = Cripto.CriptografarDescriptografar(colaborador.Email, false);
            colaborador.Cpf = Cripto.CriptografarDescriptografar(colaborador.Cpf, false);
            
            return colaborador;
        }
        public string SalvarColaborador(Colaboradore colaborador)
        {
            try
            {
                colaborador.Cpf = colaborador.Cpf.Replace(".", "").Replace("-", "");
                colaborador.Email = Cripto.CriptografarDescriptografar(colaborador.Email, true);
                colaborador.Cpf = Cripto.CriptografarDescriptografar(colaborador.Cpf, true);

                // Validar tipo de colaborador
                if (colaborador.Tipocolaborador != 'F' && colaborador.Tipocolaborador != 'T' && colaborador.Tipocolaborador != 'C')
                {
                    throw new DomainException("Tipo de colaborador deve ser F (Funcionário), T (Terceirizado) ou C (Consultor).");
                }

                // Validar data de demissão para terceirizados e consultores
                if ((colaborador.Tipocolaborador == 'T' || colaborador.Tipocolaborador == 'C') && colaborador.Dtdemissao == null)
                {
                    throw new DomainException("Necessário preencher Data de Termino de Contrato para Terceirizados e Consultores.");
                }

                if (colaborador.Id == 0)
                {
                    ValidarNovoColaborador(colaborador);
                    colaborador.Dtcadastro = TimeZoneMapper.GetDateTimeNow();
                    colaborador.Situacao = "A";
                    // ✅ CORREÇÃO: Sincronizar LocalidadeId com Localidade (campo ativo)
                    colaborador.LocalidadeId = colaborador.Localidade;
                    _repository.Adicionar(colaborador);
                    return "Colaborador salvo com sucesso!";
                }
                else
                {
                    colaborador.Dtatualizacao = TimeZoneMapper.GetDateTimeNow();
                    // ✅ CORREÇÃO: Sincronizar LocalidadeId com Localidade em atualizações também
                    colaborador.LocalidadeId = colaborador.Localidade;
                    _repository.Atualizar(colaborador);
                    return "Colaborador salvo com sucesso!";
                }
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message.Contains("colaboradores_cpf_key"))
                {
                    throw new DomainException("Já existe um colaborador com o cpf informado");
                }
                throw;
            }
        }

        private void ValidarNovoColaborador(Colaboradore colaborador)
        {
            var existe = _repository.Buscar(x => x.Cpf == colaborador.Cpf).Any();
            if(existe) 
                throw new DomainException("Já existe um colaborador com o cpf informado");
            
            existe = _repository.Buscar(x => x.Matricula == colaborador.Matricula).Any();
            if (existe) 
                throw new DomainException("Já existe um colaborador cadastrado com a matrícula informada");
        }

        public void ExcluirColaborador(int id)
        {
            var col = _repository.Buscar(x => x.Id == id).FirstOrDefault();
            try
            {
                _repository.Remover(col);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public byte[] TermoCompromisso(int cliente, int colaborador, int usuarioLogado, bool byod = false)
        {
            try
            {
                Console.WriteLine($"[TERMO_COMPROMISSO] Iniciando geração do termo - Cliente: {cliente}, Colaborador: {colaborador}, BYOD: {byod}");
                
                Colaboradore col = ObterColaboradorInterno(colaborador);
                if (col == null)
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] ERRO: Colaborador {colaborador} não encontrado");
                    throw new Exception($"Colaborador {colaborador} não encontrado");
                }
                
                var usu = _usuarioRepository.ObterPorId(usuarioLogado);
                if (usu == null)
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] ERRO: Usuário {usuarioLogado} não encontrado");
                    throw new Exception($"Usuário {usuarioLogado} não encontrado");
                }
                
                var eqptos = _equipamentoNegocio.EquipamentosDoTermoDeEntrega(cliente, colaborador, byod).ToList();
                string strEquipamentos = FormatarTabelaEquipamentos(eqptos);
                int tipoTermo = byod ? (int)TipoTemplateEnum.TermoCompromissoBYOD : (int)TipoTemplateEnum.TermoCompromisso;
                var template = _templateRepository.Buscar(x => x.Tipo == tipoTermo && x.Cliente == cliente).FirstOrDefault();
                
                if (template == null)
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] ERRO: Template não encontrado - Tipo: {tipoTermo}, Cliente: {cliente}");
                    throw new Exception($"Template não encontrado para tipo {tipoTermo} e cliente {cliente}");
                }
                
                // ✅ VERIFICAR SE EXISTE SNAPSHOT de template assinado
                // Buscar snapshot filtrado pelo tipo de termo (BYOD ou Corporativo)
                var requisicaoAssinada = _requisicaoRepository
                    .Buscar(x => x.Colaboradorfinal == colaborador && 
                                 x.Cliente == cliente && 
                                 x.Assinaturaeletronica == true &&
                                 !string.IsNullOrEmpty(x.ConteudoTemplateAssinado) &&
                                 x.TipoTermoAssinado == tipoTermo) // 🔖 Filtra pelo tipo de termo
                    .OrderByDescending(x => x.Dtassinaturaeletronica)
                    .FirstOrDefault();

                Console.WriteLine($"[TERMO_COMPROMISSO] Buscando snapshot para tipo: {(byod ? "BYOD" : "Corporativo")}");

                if (requisicaoAssinada != null)
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] 📸 Snapshot encontrado! Requisição ID: {requisicaoAssinada.Id}, Data assinatura: {requisicaoAssinada.Dtassinaturaeletronica}, Tipo: {(byod ? "BYOD" : "Corporativo")}");
                    Console.WriteLine($"[TERMO_COMPROMISSO] Usando conteúdo congelado da versão assinada (tamanho: {requisicaoAssinada.ConteudoTemplateAssinado.Length} caracteres)");
                    
                    // Criar template temporário com o snapshot
                    template = new Template
                    {
                        Id = template.Id,
                        Tipo = template.Tipo,
                        Cliente = template.Cliente,
                        Titulo = template.Titulo,
                        Conteudo = requisicaoAssinada.ConteudoTemplateAssinado, // 📸 Usa snapshot
                        Ativo = template.Ativo,
                        Versao = requisicaoAssinada.VersaoTemplateAssinado ?? template.Versao, // 🔢 Usa versão do momento da assinatura
                        DataCriacao = template.DataCriacao,
                        DataAlteracao = requisicaoAssinada.Dtassinaturaeletronica // ✅ Usa data da assinatura para validação correta
                    };
                    
                    Console.WriteLine($"[TERMO_COMPROMISSO] Template ajustado - DataAlteracao setada para data da assinatura: {requisicaoAssinada.Dtassinaturaeletronica}");
                }
                else
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] Nenhum snapshot encontrado para tipo {(byod ? "BYOD" : "Corporativo")}. Usando template ativo atual (Versão: {template.Versao})");
                }
                
                var dataUltimaVersao = template.DataAlteracao.HasValue ? template.DataAlteracao.Value : template.DataCriacao;
                
                // Valores padrão para navegações NULL
                string nomeEmpresa = col.EmpresaNavigation?.Nome ?? "Empresa não informada";
                string cnpjEmpresa = col.EmpresaNavigation?.Cnpj?.ToString() ?? "CNPJ não informado";
                string centroCusto = col.CentrocustoNavigation != null 
                    ? $"{col.CentrocustoNavigation.Codigo}   {col.CentrocustoNavigation.Nome}"
                    : "Centro de custo não informado";
                string nomeColaborador = col.Nome ?? "Nome não informado";
                string cargo = col.Cargo ?? "Cargo não informado";
                string matricula = col.Matricula ?? "Matrícula não informada";
                
                // Adiar substituição do @usuarioLogado para depois de determinar quem entregou

                var file = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "ckeditor.css");
                string css = File.Exists(file) ? File.ReadAllText(file) : string.Empty;
                string footer = string.Empty;
                string publicValidationUrl = string.Empty;

                string selectedHash = null;
                if (byod)
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] Buscando requisição BYOD para colaborador {colaborador} e cliente {cliente}");
                    VwUltimasRequisicaoBYOD req = null;
                    
                    // ✅ CORREÇÃO: Priorizar requisições PENDENTES de assinatura primeiro (BYOD)
                    Console.WriteLine($"[TERMO_COMPROMISSO] Buscando requisições BYOD PENDENTES de assinatura para colaborador {colaborador}");
                    req = _viewRepositoryBYOD
                        .Buscar(x => x.ColaboradorFinal == colaborador && 
                                   x.Cliente == cliente && 
                                   x.DtDevolucao == null &&
                                   x.AssinaturaEletronica == false) // ✅ CORREÇÃO: Filtrar por assinatura pendente
                        .OrderByDescending(x => x.DtSolicitacao) // ✅ CORREÇÃO: Ordenar por data mais recente
                        .FirstOrDefault();
                    
                    if (req != null)
                    {
                        Console.WriteLine($"[TERMO_COMPROMISSO] ✅ Requisição BYOD PENDENTE encontrada - Hash: {req.HashRequisicao}, Assinatura: {req.AssinaturaEletronica}");
                        publicValidationUrl = $"{ObterUrlSite(cliente).TrimEnd('/')}/verificar-termo/{req.HashRequisicao}";
                        selectedHash = req.HashRequisicao;
                        footer = GerarRodape(req.AssinaturaEletronica, req.DtAssinaturaEletronica, template, req.HashRequisicao, publicValidationUrl);
                        Console.WriteLine($"[TERMO_COMPROMISSO] Footer BYOD gerado para hash: {req.HashRequisicao}");
                    }
                    else
                    {
                        Console.WriteLine($"[TERMO_COMPROMISSO] Nenhuma requisição BYOD PENDENTE encontrada, buscando requisições ASSINADAS...");
                        
                        // ✅ CORREÇÃO: Se não encontrar pendente, buscar requisições assinadas (BYOD)
                        req = _viewRepositoryBYOD
                            .Buscar(x => x.ColaboradorFinal == colaborador && 
                                       x.Cliente == cliente && 
                                       x.DtDevolucao == null &&
                                       x.AssinaturaEletronica == true) // ✅ CORREÇÃO: Filtrar por assinatura realizada
                            .OrderByDescending(x => x.DtAssinaturaEletronica) // ✅ CORREÇÃO: Ordenar por data de assinatura
                            .FirstOrDefault();
                        
                        if (req != null)
                        {
                            Console.WriteLine($"[TERMO_COMPROMISSO] ✅ Requisição BYOD ASSINADA encontrada - Hash: {req.HashRequisicao}, Assinatura: {req.AssinaturaEletronica}");
                            publicValidationUrl = $"{ObterUrlSite(cliente).TrimEnd('/')}/verificar-termo/{req.HashRequisicao}";
                            selectedHash = req.HashRequisicao;
                            footer = GerarRodape(req.AssinaturaEletronica, req.DtAssinaturaEletronica, template, req.HashRequisicao, publicValidationUrl);
                            Console.WriteLine($"[TERMO_COMPROMISSO] Footer BYOD gerado para hash: {req.HashRequisicao}");
                        }
                        else
                        {
                            Console.WriteLine($"[TERMO_COMPROMISSO] AVISO: Nenhuma requisição BYOD encontrada na view para colaborador {colaborador}");
                            
                            // ✅ CORREÇÃO: Busca direta na tabela de requisições (sem filtro de assinatura)
                            var requisicaoDireta = _requisicaoRepository.Buscar(x => 
                                x.Colaboradorfinal == colaborador && 
                                x.Cliente == cliente)
                                .OrderByDescending(x => x.Dtassinaturaeletronica)
                                .FirstOrDefault();
                            
                            if (requisicaoDireta != null)
                            {
                                Console.WriteLine($"[TERMO_COMPROMISSO] ✅ Requisição BYOD encontrada via busca direta - Hash: {requisicaoDireta.Hashrequisicao}, Assinatura: {requisicaoDireta.Assinaturaeletronica}");
                                publicValidationUrl = $"{ObterUrlSite(cliente).TrimEnd('/')}/verificar-termo/{requisicaoDireta.Hashrequisicao}";
                                selectedHash = requisicaoDireta.Hashrequisicao;
                                footer = GerarRodape(requisicaoDireta.Assinaturaeletronica, requisicaoDireta.Dtassinaturaeletronica, template, requisicaoDireta.Hashrequisicao, publicValidationUrl);
                                Console.WriteLine($"[TERMO_COMPROMISSO] Footer BYOD gerado via busca direta para hash: {requisicaoDireta.Hashrequisicao}");
                            }
                            else
                            {
                                Console.WriteLine($"[TERMO_COMPROMISSO] ❌ Nenhuma requisição BYOD encontrada para colaborador {colaborador}");
                                footer = "<footer><p>Termo BYOD - Assinatura pendente</p></footer>";
                            }
                        }
                    }
                }
                else
                {
                    // ✅ Determinar status exclusivamente a partir dos hashes corporativos presentes nos itens do termo
                    try
                    {
                        var hashesCorporativos = eqptos
                            .Where(e => !string.IsNullOrWhiteSpace(e.Hashrequisicao))
                            .Select(e => e.Hashrequisicao)
                            .Distinct()
                            .ToList();

                        // ✅ CORREÇÃO CRÍTICA: Incluir LINHAS TELEFÔNICAS além de equipamentos corporativos
                        var reqIdsCorp = _requisicaoItensRepository
                            .Buscar(ri => ri.RequisicaoNavigation != null &&
                                         ri.RequisicaoNavigation.Colaboradorfinal == colaborador &&
                                         ri.RequisicaoNavigation.Cliente == cliente &&
                                         ri.Dtdevolucao == null &&
                                         (
                                             // Equipamentos corporativos (não BYOD)
                                             (ri.EquipamentoNavigation != null && ri.EquipamentoNavigation.Tipoaquisicao != 2) ||
                                             // Linhas telefônicas (identificadas por Linhatelefonica > 0)
                                             (ri.Linhatelefonica.HasValue && ri.Linhatelefonica.Value > 0)
                                         ))
                            .Include(ri => ri.RequisicaoNavigation)
                            .Select(ri => ri.Requisicao)
                            .Distinct()
                            .ToList();

                        var reqsCorp = (reqIdsCorp != null && reqIdsCorp.Count > 0)
                            ? _requisicaoRepository.Buscar(r => reqIdsCorp.Contains(r.Id)).ToList()
                            : _requisicaoRepository.Buscar(r => hashesCorporativos.Contains(r.Hashrequisicao)).ToList();

                        // ✅ CORREÇÃO: Fallback incluindo LINHAS TELEFÔNICAS além de equipamentos
                        // Nota: Linhas telefônicas têm seu número em Numeroserie/Patrimonio no Termoentregavm
                        if (reqsCorp == null || reqsCorp.Count == 0)
                        {
                            var numerosSerie = eqptos.Select(e => e.Numeroserie).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                            var patrimonios = eqptos.Select(e => e.Patrimonio).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

                            var reqIds = _requisicaoItensRepository
                                .Buscar(ri => ri.Dtdevolucao == null &&
                                             (
                                                 // Equipamentos por S/N ou Patrimônio
                                                 (ri.EquipamentoNavigation != null && 
                                                  (numerosSerie.Contains(ri.EquipamentoNavigation.Numeroserie) || 
                                                   patrimonios.Contains(ri.EquipamentoNavigation.Patrimonio))) ||
                                                 // Linhas telefônicas por número (número da linha está em Numeroserie/Patrimonio)
                                                 (ri.Linhatelefonica.HasValue && ri.Linhatelefonica.Value > 0 && 
                                                  ri.LinhatelefonicaNavigation != null && 
                                                  (numerosSerie.Contains(ri.LinhatelefonicaNavigation.Numero.ToString()) || 
                                                   patrimonios.Contains(ri.LinhatelefonicaNavigation.Numero.ToString())))
                                             ))
                                .Include(ri => ri.RequisicaoNavigation)
                                .Include(ri => ri.LinhatelefonicaNavigation)
                                .Select(ri => ri.Requisicao)
                                .Distinct()
                                .ToList();

                            if (reqIds != null && reqIds.Count > 0)
                            {
                                reqsCorp = _requisicaoRepository
                                    .Buscar(r => reqIds.Contains(r.Id))
                                    .ToList();
                            }
                        }

                        // Debug: listar hashes e status
                        try
                        {
                            Console.WriteLine($"[TERMO_COMPROMISSO] [CORP] Hashes no PDF: {string.Join(", ", hashesCorporativos)}");
                            Console.WriteLine($"[TERMO_COMPROMISSO] [CORP] Requisições carregadas: {string.Join(", ", reqsCorp.Select(r => r.Hashrequisicao + ":" + (r.Assinaturaeletronica ? "OK" : "PEND")))}");
                        }
                        catch { }

                        var temRequisicoes = reqsCorp != null && reqsCorp.Count > 0;
                        var todasAssinadas = temRequisicoes && reqsCorp.All(r => r.Assinaturaeletronica);
                        var reqRef = temRequisicoes
                            ? reqsCorp.OrderByDescending(r => r.Dtsolicitacao ?? r.Dtprocessamento ?? r.Dtassinaturaeletronica).FirstOrDefault()
                            : null;

                        if (reqRef != null)
                        {
                            publicValidationUrl = $"{ObterUrlSite(cliente).TrimEnd('/')}/verificar-termo/{reqRef.Hashrequisicao}";
                            selectedHash = reqRef.Hashrequisicao;
                            DateTime? dtAss = null;
                            if (todasAssinadas)
                            {
                                dtAss = reqsCorp.Where(r => r.Assinaturaeletronica)
                                                .Max(r => r.Dtassinaturaeletronica);
                            }
                            footer = GerarRodape(todasAssinadas, dtAss, template, reqRef.Hashrequisicao, publicValidationUrl);
                        }
                        else
                        {
                            footer = "<footer><p>Termo Corporativo - Assinatura pendente</p></footer>";
                        }
                    }
                    catch
                    {
                        footer = "<footer><p>Termo Corporativo - Assinatura pendente</p></footer>";
                    }
                }
                
                // Determinar o usuário de entrega (quem efetivamente entregou) a partir do último item entregue do hash selecionado
                string usuarioEntregaNome = null;
                if (!string.IsNullOrWhiteSpace(selectedHash))
                {
                    try
                    {
                        var reqSel = _requisicaoRepository.Buscar(r => r.Hashrequisicao == selectedHash)
                            .Include(r => r.Requisicoesitens)
                            .FirstOrDefault();
                        var itemEntrega = reqSel?.Requisicoesitens
                            ?.Where(ri => ri.Dtentrega.HasValue && ri.Usuarioentrega.HasValue)
                            ?.OrderByDescending(ri => ri.Dtentrega)
                            ?.FirstOrDefault();
                        if (itemEntrega?.Usuarioentrega != null)
                        {
                            var usuEntrega = _usuarioRepository.ObterPorId(itemEntrega.Usuarioentrega.Value);
                            usuarioEntregaNome = usuEntrega?.Nome;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TERMO_COMPROMISSO] Aviso: falha ao resolver usuário de entrega: {ex.Message}");
                    }
                }

                // Agora aplicar substituições do template com o nome correto do entregador
                var nomeUsuarioParaExibir = !string.IsNullOrWhiteSpace(usuarioEntregaNome) ? usuarioEntregaNome : usu.Nome;

                template.Conteudo = template.Conteudo
                    .Replace("@nomeEmpresa", nomeEmpresa)
                    .Replace("@cnpjEmpresa", cnpjEmpresa)
                    .Replace("@centroCusto", centroCusto)
                    .Replace("@nomeColaborador", nomeColaborador)
                    .Replace("@cargo", cargo)
                    .Replace("@matricula", matricula)
                    .Replace("@equipamentos", strEquipamentos)
                    .Replace("@usuarioLogado", nomeUsuarioParaExibir)
                    .Replace("@dataAtual", TimeZoneMapper.GetDateTimeNow().ToString("dddd, dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
                    .Replace("@dataUltimaAtual", dataUltimaVersao.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
                    .Replace("@versao", $"Versão: {template.Versao.ToString()}") // 🔢 Exibe versão (atual ou do snapshot)
                    .Replace("@tipoColaborador", $"{ObterTipoColaborador(col.Tipocolaborador)}");

                template.Conteudo = template.Conteudo + footer;
                template.Conteudo = template.Conteudo + "<style>" + css + "table{width:100%}</style>";

                Console.WriteLine($"[TERMO_COMPROMISSO] Gerando PDF...");
                var pdf = HtmlToPdfConverter.ConvertHtmlToPdf(template.Conteudo);
                Console.WriteLine($"[TERMO_COMPROMISSO] PDF gerado com sucesso - Tamanho: {pdf?.Length ?? 0} bytes");

                if (pdf == null || pdf.Length == 0)
                {
                    Console.WriteLine($"[TERMO_COMPROMISSO] ERRO: PDF gerado está vazio ou nulo");
                    throw new Exception("Falha ao gerar PDF - arquivo vazio ou nulo");
                }

                return pdf;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TERMO_COMPROMISSO] ERRO: {ex.Message}");
                Console.WriteLine($"[TERMO_COMPROMISSO] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private string ObterTipoColaborador(char tipocolaborador)
        {
            switch (tipocolaborador)
            {
                case 'F': return "Funcionário";
                case 'T': return "Terceirizado";
                case 'C': return "Consultor";
                default: return "Tipo não identificado";
            }
        }

        private string GerarRodape(bool assinaturaeletronica, DateTime? dtAssinaturaEletronica, Template template, string hashRequisicao, string publicValidationUrl = null)
        {
            Console.WriteLine($"[GERAR_RODAPE] Iniciando geração do rodapé - Hash: {hashRequisicao}, Assinatura: {assinaturaeletronica}");
            
            SituacaoTemplateEnum situacao = ValidaVersaoTemplate(template, assinaturaeletronica, dtAssinaturaEletronica);

            // ✅ REGRA: O rodapé reflete apenas o status do hash específico passado (ou conjunto de hashes do termo).
            // Não deve ser influenciado por outras requisições pendentes de tipos diferentes (BYOD vs não-BYOD).
            try
            {
                var reqHash = _requisicaoRepository.Buscar(x => x.Hashrequisicao == hashRequisicao).FirstOrDefault();
                if (reqHash != null)
                {
                    Console.WriteLine($"[GERAR_RODAPE] Requisição encontrada para hash {hashRequisicao}: Assinatura={reqHash.Assinaturaeletronica}");
                    
                    // Se a requisição deste hash não estiver assinada, força pendente
                    if (!reqHash.Assinaturaeletronica)
                    {
                        Console.WriteLine($"[GERAR_RODAPE] Hash {hashRequisicao} não assinado. Rodapé: pendente");
                        situacao = SituacaoTemplateEnum.NAssinado;
                        assinaturaeletronica = false;
                        dtAssinaturaEletronica = null;
                    }
                    else
                    {
                        Console.WriteLine($"[GERAR_RODAPE] Hash {hashRequisicao} assinado. Rodapé: realizada");
                    }
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine($"[GERAR_RODAPE] ERRO ao verificar hash: {ex.Message}");
            }
            // Quando pendente, não exibir o cabeçalho "Manifesto de assinatura"
            string footer = (situacao == SituacaoTemplateEnum.NAssinado)
                ? "<footer>"
                : "<footer><b>Manifesto de assinatura:</b>" + Environment.NewLine;
            switch (situacao)
            {
                case SituacaoTemplateEnum.NAssinado:
                    footer += $"Assinatura deste termo se encontra pendente</footer>";
                    break;
                case SituacaoTemplateEnum.Assinado:
                    footer += $"<p>Termo assinado digitalmente.</p>";
                    footer += $"<p>Token de validação: {hashRequisicao} </p>";
                    footer += $"<p>Data e hora: {dtAssinaturaEletronica.Value.ToString("dd/MM/yyyy HH:mm")} </p>";
                    
                    // Buscar informações de geolocalização da assinatura
                    try
                    {
                        Console.WriteLine($"[GERAR_RODAPE] Buscando geolocalização para hash: {hashRequisicao}");
                        
                        // Buscar o colaborador pelo hash da requisição
                        var requisicao = _requisicaoRepository.Buscar(x => x.Hashrequisicao == hashRequisicao).FirstOrDefault();
                        if (requisicao != null)
                        {
                            Console.WriteLine($"[GERAR_RODAPE] Requisição encontrada - Colaborador: {requisicao.Colaboradorfinal}");
                            
                            // Buscar todos os registros do dia e escolher o mais próximo do horário da assinatura
                            var geosDoDia = _geolocalizacaoRepository.Buscar(x =>
                                x.ColaboradorId == requisicao.Colaboradorfinal &&
                                (x.Acao == "ASSINATURA_TERMO_ELETRONICO" || x.Acao == "ASSINATURA_TERMO_ELETRONICO_BYOD") &&
                                x.TimestampCaptura.Date == dtAssinaturaEletronica.Value.Date)
                                .ToList();

                            var geolocalizacao = geosDoDia
                                .OrderBy(x => Math.Abs((x.TimestampCaptura - dtAssinaturaEletronica.Value).TotalSeconds))
                                .ThenByDescending(x => x.TimestampCaptura)
                                .FirstOrDefault();

                            if (geolocalizacao != null)
                            {
                                Console.WriteLine($"[GERAR_RODAPE] Geolocalização encontrada - IP: {geolocalizacao.IpAddress}, Cidade: {geolocalizacao.City}");
                                
                                footer += $"<p><b>Informações de localização da assinatura:</b></p>";
                                footer += $"<p>• Endereço IP: {geolocalizacao.IpAddress}</p>";
                                
                                if (!string.IsNullOrEmpty(geolocalizacao.City))
                                {
                                    footer += $"<p>• Cidade: {geolocalizacao.City}</p>";
                                }
                                if (!string.IsNullOrEmpty(geolocalizacao.Region))
                                {
                                    footer += $"<p>• Estado/Região: {geolocalizacao.Region}</p>";
                                }
                                if (!string.IsNullOrEmpty(geolocalizacao.Country))
                                {
                                    footer += $"<p>• País: {geolocalizacao.Country}</p>";
                                }
                                
                                if (geolocalizacao.Latitude.HasValue && geolocalizacao.Longitude.HasValue)
                                {
                                    footer += $"<p>• Coordenadas GPS: {geolocalizacao.Latitude:F6}, {geolocalizacao.Longitude:F6}</p>";
                                }
                                
                                if (geolocalizacao.AccuracyMeters.HasValue)
                                {
                                    footer += $"<p>• Precisão: {geolocalizacao.AccuracyMeters:F1} metros</p>";
                                }
                                
                                footer += $"<p>• Timestamp da captura: {geolocalizacao.TimestampCaptura.ToString("dd/MM/yyyy HH:mm:ss")}</p>";
                            }
                            else
                            {
                                Console.WriteLine($"[GERAR_RODAPE] Geolocalização não encontrada para colaborador {requisicao.Colaboradorfinal}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[GERAR_RODAPE] Requisição não encontrada para hash: {hashRequisicao}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GERAR_RODAPE] Erro ao buscar geolocalização: {ex.Message}");
                        // Não falhar o PDF por erro na geolocalização
                    }
                    
                    // Bloco de verificação pública (link + QR)
                    if (!string.IsNullOrWhiteSpace(publicValidationUrl))
                    {
                        try
                        {
                            var qrGenerator = new QRCodeGenerator();
                            var qrData = qrGenerator.CreateQrCode(publicValidationUrl, QRCodeGenerator.ECCLevel.Q);
                            var qrCode = new PngByteQRCode(qrData);
                            var qrBytes = qrCode.GetGraphic(5);
                            string base64Qr = Convert.ToBase64String(qrBytes);

                            footer += $"<div style='margin-top:10px;border-top:1px solid #ccc;padding-top:8px;'>";
                            footer += $"<p><b>Verificação pública:</b> {publicValidationUrl}</p>";
                            footer += $"<img alt='QR Code de verificação' style='width:120px;height:120px' src='data:image/png;base64,{base64Qr}' />";
                            footer += "</div>";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[GERAR_RODAPE] Falha ao gerar QR Code: {ex.Message}");
                        }
                    }

                    footer += "</footer>";
                    break;
                case SituacaoTemplateEnum.Desatualizado:
                    footer += $"<p>Termo assinado digitalmente, uma nova versão do termo foi adicionada.</p>";
                    footer += $"<p>Token de validação: {hashRequisicao} </p>";
                    footer += $"<p>Data e hora: {dtAssinaturaEletronica.Value.ToString("dd/MM/yyyy HH:mm")} </p>";
                    if (!string.IsNullOrWhiteSpace(publicValidationUrl))
                    {
                        try
                        {
                            var qrGenerator = new QRCodeGenerator();
                            var qrData = qrGenerator.CreateQrCode(publicValidationUrl, QRCodeGenerator.ECCLevel.Q);
                            var qrCode = new PngByteQRCode(qrData);
                            var qrBytes = qrCode.GetGraphic(5);
                            string base64Qr = Convert.ToBase64String(qrBytes);

                            footer += $"<div style='margin-top:10px;border-top:1px solid #ccc;padding-top:8px;'>";
                            footer += $"<p><b>Verificação pública:</b> {publicValidationUrl}</p>";
                            footer += $"<img alt='QR Code de verificação' style='width:120px;height:120px' src='data:image/png;base64,{base64Qr}' />";
                            footer += "</div>";
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[GERAR_RODAPE] Falha ao gerar QR Code: {ex.Message}");
                        }
                    }
                    footer += "</footer>";
                    break;
                default:
                    break;
            }

            Console.WriteLine($"[GERAR_RODAPE] Rodapé gerado com sucesso");
            return footer;
        }

        private SituacaoTemplateEnum ValidaVersaoTemplate(Template template, bool assinaturaeletronica, DateTime? dtAssinaturaEletronica)
        {
            if (assinaturaeletronica)
            {
                DateTime dataVerificacao = template.DataAlteracao.HasValue ? template.DataAlteracao.Value : template.DataCriacao;
                if (dataVerificacao <= dtAssinaturaEletronica)
                {
                    return SituacaoTemplateEnum.Assinado;
                }
                return SituacaoTemplateEnum.Desatualizado;
            }

            return SituacaoTemplateEnum.NAssinado;
        }

        private int VerificarTipoTemplate(int tipoAquisicao)
        {
            switch (tipoAquisicao)
            {
                case 'F': 
                    return (int)TipoTemplateEnum.TermoCompromisso;
                case 'T':
                    return (int)TipoTemplateEnum.TermoCompromisso;
                case 'C':
                    return (int)TipoTemplateEnum.TermoCompromissoBYOD;
                default:
                    return (int)TipoTemplateEnum.TermoCompromisso;
            }
        }

        public async Task<string> TermoPorEmail(int cliente, int colaborador, int usuarioLogado, bool byod)
        {
            try
            {
                Console.WriteLine($"[TERMO POR EMAIL] Iniciando - Cliente: {cliente}, Colaborador: {colaborador}, BYOD: {byod}");
                
                var pdf = TermoCompromisso(cliente, colaborador, usuarioLogado, byod);
                Console.WriteLine($"[TERMO POR EMAIL] PDF gerado com sucesso - Tamanho: {pdf?.Length ?? 0} bytes");

                var reqs = _requisicaoRepository.Buscar(x => x.Colaboradorfinal == colaborador && x.Assinaturaeletronica == false).ToList();
                Console.WriteLine($"[TERMO POR EMAIL] Requisições encontradas: {reqs.Count}");
                
                var col = ObterColaboradorInterno(colaborador);
                Console.WriteLine($"[TERMO POR EMAIL] Colaborador: {col.Nome}, Email: {col.Email}");
                
                if (col.Email == "naocadastrado")
                {
                    Console.WriteLine($"[TERMO POR EMAIL] ERRO: E-mail não cadastrado");
                    return JsonConvert.SerializeObject(new { Mensagem = "E-mail do colaborador não está cadastrado.", Status = "200.1" });
                }
                else
                {
                //EquipamentoNegocio equipamentoNegocio = new EquipamentoNegocio(this._config);
                var eqptos = _equipamentoNegocio.EquipamentosDoTermoDeEntrega(cliente, colaborador, byod);
                string strEquipamentos = FormatarTabelaEquipamentos(eqptos);
                var file = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "termoEmail.html");
                
                // ✅ CORREÇÃO: Obter URL correta do servidor (prioriza URL do cliente)
                string siteUrl = ObterUrlSite(cliente);
                Console.WriteLine($"[TERMO POR EMAIL] SiteUrl usado: {siteUrl}");
                
                siteUrl = siteUrl.TrimEnd('/') + "/termos/";
                var template = File.Exists(file) ? File.ReadAllText(file) : string.Empty;
                template = template.Replace("@nome", col.Nome)
                    .Replace("@equipamentos", strEquipamentos)
                    .Replace("@palavraPasse", (eqptos[0].Hashrequisicao))
                    //.Replace("@link", siteUrl + (eqptos[0].Hashrequisicao));
                    .Replace("@link", string.Concat(siteUrl, eqptos[0].Hashrequisicao, "/", byod.ToString().ToLower()));

                var filecss = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "ckeditor.css");
                string css = File.Exists(filecss) ? File.ReadAllText(filecss) : string.Empty;
                template = template + "<style>" + css + "table{width:100%}</style>";

                Console.WriteLine($"[TERMO POR EMAIL] Preparando envio para: {col.Email}");
                Console.WriteLine($"[TERMO POR EMAIL] Template carregado - tamanho: {template.Length} chars");
                Console.WriteLine($"[TERMO POR EMAIL] PDF anexo - tamanho: {pdf?.Length ?? 0} bytes");
                
                // Usar EnviarAsync para carregar configurações SMTP do banco
                await mail.EnviarAsync(col.Email, "Termo eletrônico de entrega de recursos", template, pdf, cliente);
                Console.WriteLine($"[TERMO POR EMAIL] E-mail enviado com sucesso!");

                try
                {
                    // Atualizar todas as requisições de uma vez usando o novo método
                    foreach (var r in reqs)
                    {
                        r.Dtenviotermo = TimeZoneMapper.GetDateTimeNow();
                    }
                    
                    // Atualizar todas as requisições em uma única operação
                    _requisicaoRepository.AtualizarMuitos(reqs);
                }
                catch (Exception)
                {
                    throw;
                }

                Console.WriteLine($"[TERMO POR EMAIL] Processo concluído com sucesso!");
                return JsonConvert.SerializeObject(new { Mensagem = "Termo enviado com sucesso!", Status = "200" });
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TERMO POR EMAIL] ERRO: {ex.Message}");
                Console.WriteLine($"[TERMO POR EMAIL] Stack trace: {ex.StackTrace}");
                return JsonConvert.SerializeObject(new { Mensagem = "Erro ao enviar termo: " + ex.Message, Status = "500" });
            }
        }
        private string FormatarTabelaEquipamentos(List<Termoentregavm> equipamentos)
        {
            // ✅ Ordenar cronologicamente: primeira entrega no topo
            var listaOrdenada = equipamentos
                .OrderBy(e => e.Dtentrega ?? DateTime.MaxValue)
                .ThenBy(e => e.Hashrequisicao)
                .ToList();

            // Separar equipamentos de linhas telefônicas
            var equipamentosNormais = listaOrdenada.Where(e => e.Tipoequipamento == null || !e.Tipoequipamento.ToLower().Contains("linha")).ToList();
            var linhasTelefonicas = listaOrdenada.Where(e => e.Tipoequipamento != null && e.Tipoequipamento.ToLower().Contains("linha")).ToList();

            string retorno = "";

            // ========== TABELA DE EQUIPAMENTOS ==========
            if (equipamentosNormais.Any())
            {
                retorno += "<table style='width:100%;border-collapse:collapse;font-size:11px;margin-bottom:15px'>";
                
                // Cabeçalho equipamentos
                retorno += "<thead><tr style='background-color:#080039;color:white'>";
                retorno += "<th style='padding:6px;text-align:left'>Tipo</th>";
                retorno += "<th style='padding:6px;text-align:left'>Fabricante</th>";
                retorno += "<th style='padding:6px;text-align:left'>Modelo</th>";
                retorno += "<th style='padding:6px;text-align:left'>S/N</th>";
                retorno += "<th style='padding:6px;text-align:left'>Patrimônio</th>";
                retorno += "<th style='padding:6px;text-align:center'>Data Entrega</th>";
                retorno += "</tr></thead><tbody>";

                int i = 0;
                foreach (var vm in equipamentosNormais)
                {
                    string estilo = "";
                    if (i % 2 != 0)
                        estilo = "<tr style='background-color:whitesmoke'>";
                    else
                        estilo = "<tr>";
                    
                    retorno += estilo + "<td style='padding:4px'><strong>" + vm.Tipoequipamento + "</strong></td>"
                        + "<td style='padding:4px'>" + vm.Fabricante + "</td>"
                        + "<td style='padding:4px'>" + vm.Modelo + "</td>"
                        + "<td style='padding:4px'>" + vm.Numeroserie + "</td>"
                        + "<td style='padding:4px'>" + vm.Patrimonio + "</td>"
                        + "<td style='padding:4px;text-align:center'>" + vm.Dtentrega.Value.ToString("dd/MM/yyyy") + "</td></tr>";
                if (!String.IsNullOrEmpty(vm.Observacaoentrega) || vm.Dtprogramadaretorno.HasValue)
                {
                    retorno += "<tr>";
                    if (!String.IsNullOrEmpty(vm.Observacaoentrega))
                    {
                        retorno += "<td style='width:100%;padding:4px;font-size:10px;font-style:italic' colspan='6'>Obs: " + vm.Observacaoentrega + "</td>";
                    }
                    retorno += "</tr>";
                    if (vm.Dtprogramadaretorno.HasValue)
                    {
                        retorno += "<tr><td style='width:100%;padding:4px;font-size:10px;font-style:italic' colspan='6'>Devolução programada: " + vm.Dtprogramadaretorno.Value.ToString("dd/MM/yyyy") + "</td></tr>";
                    }
                }

                // Exibir co-responsáveis (somente não-BYOD e somente para itens de equipamento)
                try
                {
                    bool ehLinha = vm.Tipoequipamento != null && vm.Tipoequipamento.ToLower().Contains("linha");
                    if (!ehLinha && vm.TipoAquisicao != 2) // 2 = BYOD
                    {
                        // Resolver item da requisição via hash + equipamento
                        var equipamento = _equipamentoRepository
                            .Buscar(e => e.Numeroserie == vm.Numeroserie || e.Patrimonio == vm.Patrimonio)
                            .AsNoTracking()
                            .FirstOrDefault();
                        if (equipamento != null)
                        {
                            // 1) Tentar localizar a requisição pelo hash informado no VM
                            var req = !string.IsNullOrWhiteSpace(vm.Hashrequisicao)
                                ? _requisicaoRepository
                                    .Buscar(r => r.Hashrequisicao == vm.Hashrequisicao)
                                    .Include(r => r.Requisicoesitens)
                                    .ThenInclude(ri => ri.EquipamentoNavigation)
                                    .AsNoTracking()
                                    .FirstOrDefault()
                                : null;

                            // 2) Fallback: se não há hash no VM ou não encontrou, buscar a última requisição aberta
                            //    do colaborador contendo este equipamento
                            if (req == null)
                            {
                                var query = _requisicaoRepository
                                    .Buscar(r => r.Colaboradorfinal == (vm.Colaboradorfinal ?? 0));
                                if (vm.Cliente.HasValue)
                                {
                                    query = query.Where(r => r.Cliente == vm.Cliente.Value);
                                }
                                req = query
                                    .Include(r => r.Requisicoesitens)
                                    .ThenInclude(ri => ri.EquipamentoNavigation)
                                    .AsNoTracking()
                                    .ToList() // materializa para poder usar Any com navegação
                                    .OrderByDescending(r => r.Dtsolicitacao)
                                    .FirstOrDefault(r => r.Requisicoesitens != null && r.Requisicoesitens.Any(ri => ri.Dtdevolucao == null && ri.Equipamento == equipamento.Id));
                            }
                            // 1) Tentar casar por número de série/patrimônio
                            var item = req?.Requisicoesitens?.FirstOrDefault(ri =>
                                ri.Dtdevolucao == null &&
                                ri.EquipamentoNavigation != null &&
                                (
                                    (!string.IsNullOrWhiteSpace(vm.Numeroserie) && ri.EquipamentoNavigation.Numeroserie == vm.Numeroserie) ||
                                    (!string.IsNullOrWhiteSpace(vm.Patrimonio) && ri.EquipamentoNavigation.Patrimonio == vm.Patrimonio)
                                ));
                            // 2) Fallback por ID do equipamento se não achou por atributos
                            if (item == null && equipamento != null)
                            {
                                item = req?.Requisicoesitens?.FirstOrDefault(ri => ri.Equipamento == equipamento.Id && ri.Dtdevolucao == null);
                            }
                            // 3) Fallback final: buscar diretamente na tabela de itens (com navegação) caso a requisição não tenha sido localizada
                            if (item == null && equipamento != null)
                            {
                                try
                                {
                                    item = _requisicaoItensRepository
                                        .Buscar(ri => ri.Equipamento == equipamento.Id && ri.Dtdevolucao == null)
                                        .Include(ri => ri.RequisicaoNavigation)
                                        .AsNoTracking()
                                        .OrderByDescending(ri => ri.Dtentrega)
                                        .FirstOrDefault(ri => ri.RequisicaoNavigation != null && ri.RequisicaoNavigation.Colaboradorfinal == (vm.Colaboradorfinal ?? 0));
                                }
                                catch { /* silencioso */ }
                            }
                            if (item != null)
                            {
                                var vinculosAtivos = _reqItemCompartilhadoRepository
                                    .Buscar(v => v.RequisicaoItemId == item.Id && v.Ativo)
                                    .AsNoTracking()
                                    .ToList();
                                if (vinculosAtivos.Any())
                                {
                                    // Buscar nomes dos colaboradores vinculados e combinar com observações do vínculo
                                    var ids = vinculosAtivos.Select(v => v.ColaboradorId).Distinct().ToList();
                                    var mapaColabs = _repository
                                        .Buscar(c => ids.Contains(c.Id))
                                        .Select(c => new { c.Id, c.Nome })
                                        .ToList();

                                    var entradas = vinculosAtivos.Select(v =>
                                    {
                                        var nome = mapaColabs.FirstOrDefault(x => x.Id == v.ColaboradorId)?.Nome ?? v.ColaboradorId.ToString();
                                        if (!string.IsNullOrWhiteSpace(v.Observacao))
                                        {
                                            return $"{nome} (Obs: {v.Observacao})";
                                        }
                                        return nome;
                                    }).ToList();

                                    if (entradas.Any())
                                    {
                                        var lista = string.Join(", ", entradas);
                                        retorno += "<tr><td style='width:100%;padding:4px;font-size:10px;font-style:italic;color:#FF3A0F' colspan='6'>↪ Compartilhado: " + lista + "</td></tr>";
                                    }
                                }
                            }
                        }
                    }
                }
                catch { /* silencioso para não quebrar geração do termo */ }
 
                    i++;
                }
                retorno += "</tbody></table>";
            }

            // ========== TABELA DE LINHAS TELEFÔNICAS ==========
            if (linhasTelefonicas.Any())
            {
                retorno += "<table style='width:100%;border-collapse:collapse;font-size:11px;margin-bottom:15px'>";
                
                // Cabeçalho linhas
                retorno += "<thead><tr style='background-color:#FF3A0F;color:white'>";
                retorno += "<th style='padding:6px;text-align:left'>Tipo</th>";
                retorno += "<th style='padding:6px;text-align:left'>Operadora</th>";
                retorno += "<th style='padding:6px;text-align:left'>Plano</th>";
                retorno += "<th style='padding:6px;text-align:left'>Linha/Número</th>";
                retorno += "<th style='padding:6px;text-align:left'>ICCID</th>";
                retorno += "<th style='padding:6px;text-align:center'>Data Entrega</th>";
                retorno += "</tr></thead><tbody>";

                int j = 0;
                foreach (var vm in linhasTelefonicas)
                {
                    string estilo = "";
                    if (j % 2 != 0)
                        estilo = "<tr style='background-color:#fff5f3'>";
                    else
                        estilo = "<tr>";
                    
                    retorno += estilo + "<td style='padding:4px'><strong>" + vm.Tipoequipamento + "</strong></td>"
                        + "<td style='padding:4px'>" + (string.IsNullOrWhiteSpace(vm.Fabricante) ? "-" : vm.Fabricante) + "</td>"
                        + "<td style='padding:4px'>" + (string.IsNullOrWhiteSpace(vm.Modelo) ? "-" : vm.Modelo) + "</td>"
                        + "<td style='padding:4px'>" + (string.IsNullOrWhiteSpace(vm.Patrimonio) ? "-" : vm.Patrimonio) + "</td>"
                        + "<td style='padding:4px'>" + (string.IsNullOrWhiteSpace(vm.Numeroserie) ? "-" : vm.Numeroserie) + "</td>"
                        + "<td style='padding:4px;text-align:center'>" + vm.Dtentrega.Value.ToString("dd/MM/yyyy") + "</td></tr>";

                    if (!String.IsNullOrEmpty(vm.Observacaoentrega) || vm.Dtprogramadaretorno.HasValue)
                    {
                        retorno += "<tr>";
                        if (!String.IsNullOrEmpty(vm.Observacaoentrega))
                        {
                            retorno += "<td style='width:100%;padding:4px;font-size:10px;font-style:italic' colspan='6'>Obs: " + vm.Observacaoentrega + "</td>";
                        }
                        retorno += "</tr>";
                        if (vm.Dtprogramadaretorno.HasValue)
                        {
                            retorno += "<tr><td style='width:100%;padding:4px;font-size:10px;font-style:italic' colspan='6'>Devolução programada: " + vm.Dtprogramadaretorno.Value.ToString("dd/MM/yyyy") + "</td></tr>";
                        }
                    }
                    
                    j++;
                }
                retorno += "</tbody></table>";
            }

            return retorno;
        }


        public List<Vwnadaconstum> NadaConsta(int colaborador, int cliente)
        {
            return _viewRepositoryVwnadaconstum.Buscar(x => x.Cliente == cliente && x.Id == colaborador).ToList();
        }
        public byte[] TermoNadaConsta(int colaborador, int cliente, int usuarioLogado)
        {
            var col = ObterColaboradorInterno(colaborador);
            var usuario = _usuarioRepository.ObterPorId(usuarioLogado);
            var template = _templateRepository.Buscar(x => x.Tipo == (int)TipoTemplateEnum.NadaConsta && x.Cliente == cliente).FirstOrDefault();
            var dataUltimaVersao = template.DataAlteracao.HasValue ? template.DataAlteracao.Value : template.DataCriacao;

            template.Conteudo = template.Conteudo
                .Replace("@nomeEmpresa", col.EmpresaNavigation.Nome)
                .Replace("@cnpjEmpresa", col.EmpresaNavigation.Cnpj.ToString())
                .Replace("@centroCusto", col.CentrocustoNavigation.Codigo + "   " + col.CentrocustoNavigation.Nome)
                .Replace("@nomeColaborador", col.Nome)
                .Replace("@cargo", col.Cargo)
                .Replace("@matricula", col.Matricula)
                .Replace("@usuarioLogado", usuario.Nome)
                .Replace("@dataAtual", TimeZoneMapper.GetDateTimeNow().ToString("dddd, dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
                .Replace("@dataUltimaAtual", dataUltimaVersao.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
                .Replace("@versao", $"Versão: {template.Versao.ToString()}")
                .Replace("@tipoColaborador", $"{ObterTipoColaborador(col.Tipocolaborador)}");

            var file = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "ckeditor.css");
            string css = File.Exists(file) ? File.ReadAllText(file) : string.Empty;
            template.Conteudo = template.Conteudo + "<style>" + css + "table{width:100%}</style>";

            var pdf = HtmlToPdfConverter.ConvertHtmlToPdf(template.Conteudo);
            var fileName = "TermoEntrega " + col.Nome + ".pdf";

            return pdf;
        }
        public List<Termoscolaboradoresvm> ColaboradoresComTermoPorAssinar(string pesquisa, int cliente, string filtro)
        {
            Console.WriteLine($"[TERMOS] ========== ColaboradoresComTermoPorAssinar ==========");
            Console.WriteLine($"[TERMOS] Pesquisa: '{pesquisa}' | Cliente: {cliente} | Filtro: '{filtro}'");
            
            DateTime dtNow = TimeZoneMapper.GetDateTimeNow();
            pesquisa = pesquisa.ToLower();
            
            // ✅ CORREÇÃO: Buscar apenas colaboradores com recursos ATIVOS (sem devolução)
            // União de equipamentos não-BYOD e BYOD que ainda não foram devolvidos
            var colaboradoresComRecursos = (
                from vwNaoBYOD in _viewRepositoryNaoBYOD.Query()
                where vwNaoBYOD.Cliente == cliente && vwNaoBYOD.DtDevolucao == null
                select new { ColaboradorId = vwNaoBYOD.ColaboradorFinal, ColaboradorNome = vwNaoBYOD.NomeColaboradorFinal }
            ).Union(
                from vwBYOD in _viewRepositoryBYOD.Query()
                where vwBYOD.Cliente == cliente && vwBYOD.DtDevolucao == null
                select new { ColaboradorId = vwBYOD.ColaboradorFinal, ColaboradorNome = vwBYOD.NomeColaboradorFinal }
            ).Distinct().ToList();
            
            Console.WriteLine($"[TERMOS] Total de colaboradores com recursos ativos: {colaboradoresComRecursos.Count}");
            
            // IDs dos colaboradores com recursos
            var idsComRecursos = colaboradoresComRecursos.Select(x => x.ColaboradorId).ToHashSet();
            
            // 🔍 DEBUG: Verificar se Raimundo tem recursos ativos
            var raimundoComRecurso = colaboradoresComRecursos.FirstOrDefault(c => 
                c.ColaboradorNome != null && c.ColaboradorNome.ToLower().Contains("raimundo"));
            if (raimundoComRecurso != null)
            {
                Console.WriteLine($"[TERMOS] 🔍 Raimundo Nonato TEM recursos ativos:");
                Console.WriteLine($"[TERMOS]   - ID: {raimundoComRecurso.ColaboradorId}");
                Console.WriteLine($"[TERMOS]   - Nome: {raimundoComRecurso.ColaboradorNome}");
            }
            else
            {
                Console.WriteLine($"[TERMOS] ⚠️ Raimundo Nonato NÃO tem recursos ativos");
            }
            
            // 🔍 DEBUG: Primeiro buscar TODOS da view (sem filtro de situacao) para ver o que tem
            var todosDaView = (from vm in _viewRepositoryTermoscolaboradoresvm.Query()
                        join c in _repository.Query() on vm.Colaboradorfinalid equals c.Id
                        join e in _empresaRepository.Query() on c.Empresa equals e.Id
                        where e.Cliente == cliente 
                            && (!c.Dtdemissao.HasValue || (c.Dtdemissao.HasValue && c.Dtdemissao.Value > dtNow))
                            && idsComRecursos.Contains(vm.Colaboradorfinalid.Value)
                        select vm).ToList();
            
            Console.WriteLine($"[TERMOS] Total na view (sem filtro de situacao): {todosDaView.Count}");
            
            // Verificar Raimundo ANTES do filtro
            var raimundoNaView = todosDaView.FirstOrDefault(r => 
                r.Colaboradorfinal != null && r.Colaboradorfinal.ToLower().Contains("raimundo"));
            if (raimundoNaView != null)
            {
                Console.WriteLine($"[TERMOS] 🔍 Raimundo na VIEW (ANTES do filtro de situacao):");
                Console.WriteLine($"[TERMOS]   - Nome: {raimundoNaView.Colaboradorfinal}");
                Console.WriteLine($"[TERMOS]   - Situacao: '{raimundoNaView.Situacao}'");
                Console.WriteLine($"[TERMOS]   - Data Envio: {raimundoNaView.Dtenviotermo}");
                Console.WriteLine($"[TERMOS]   - Filtro a aplicar: '{filtro}'");
                Console.WriteLine($"[TERMOS]   - Situacao contém filtro? {(raimundoNaView.Situacao ?? "").ToLower().Contains(filtro.ToLower())}");
            }
            
            // ✅ Agora aplicar o filtro de situacao
            var reqs = todosDaView
                        .Where(vm => vm.Situacao.ToLower().Contains(filtro.ToLower()))
                        .Where(vm => (pesquisa != "null") ? vm.Colaboradorfinal.ToLower().Contains(pesquisa.ToLower()) : true)
                        .OrderByDescending(vm => vm.Situacao)
                        .ThenByDescending(vm => vm.Dtenviotermo)
                        .ToList();

            Console.WriteLine($"[TERMOS] Total retornado após filtros: {reqs.Count}");
            
            // 🔍 DEBUG: Verificar status de Raimundo Nonato
            var raimundo = reqs.FirstOrDefault(r => r.Colaboradorfinal != null && r.Colaboradorfinal.ToLower().Contains("raimundo"));
            if (raimundo != null)
            {
                Console.WriteLine($"[TERMOS] 🔍 DEBUG RAIMUNDO NONATO:");
                Console.WriteLine($"[TERMOS]   - Nome: {raimundo.Colaboradorfinal}");
                Console.WriteLine($"[TERMOS]   - ID: {raimundo.Colaboradorfinalid}");
                Console.WriteLine($"[TERMOS]   - Situacao (view): '{raimundo.Situacao}'");
                Console.WriteLine($"[TERMOS]   - Data Envio: {raimundo.Dtenviotermo}");
                Console.WriteLine($"[TERMOS]   - Filtro aplicado: '{filtro}'");
            }
            
            Console.WriteLine($"[TERMOS] ========== FIM ColaboradoresComTermoPorAssinar ==========");
            
            return reqs;
        }


        public List<string> ListarCargos(int cliente, string pesquisa)
        {
            DateTime dtNow = TimeZoneMapper.GetDateTimeNow();
            pesquisa = pesquisa.ToUpper();
            
            // ✅ Substituído VerificarSituacaoColaboradorAtivo() por expressão inline
            // para permitir tradução para SQL pelo Entity Framework
            var crgs = (from col in _repository.Query()
                        where col.Cliente == cliente 
                            && col.Cargo.ToUpper().Contains(pesquisa) 
                            && (!col.Dtdemissao.HasValue || col.Dtdemissao.Value > dtNow)
                        group col by col.Cargo into cargos
                        orderby cargos.Key
                        select cargos.Key).ToList();
            return crgs;
        }
        public void SalvarCargoDescarte(Descartecargo cargo)
        {
            //db.Add(cargo);
            //db.SaveChanges();
            _descartecargoRepository.Adicionar(cargo);
        }
        public void ExcluirCargoDescarte(int idCargo)
        {
            //var cargo = db.Descartecargos.Where(x => x.Id == idCargo).AsNoTracking().FirstOrDefault();
            //db.Remove(cargo);
            //db.SaveChanges();

            var cargo = _descartecargoRepository.ObterPorId(idCargo);
            if(cargo != null) 
                _descartecargoRepository.Remover(cargo);
        }
        public List<Descartecargo> ListarCargosDeDescarte(int cliente)
        {
            var cargos = _descartecargoRepository.Buscar(x => x.Cliente == cliente).OrderBy(x => x.Cargo).ToList();
            return cargos;
        }

        public void ExportarTermosEmPDF(int cliente)
        {
            var termos = _requisicaoRepository.Buscar(x => x.Cliente == cliente && x.Requisicaostatus == 3 && x.Dtenviotermo != null).ToList();
            var assinados = termos.Where(x => x.Assinaturaeletronica).ToList();
            var naoAssinados = termos.Where(x => !x.Assinaturaeletronica).ToList();

            foreach (var termo in assinados)
            {
                this.TermoCompromissoExport(cliente, termo.Colaboradorfinal.Value, 2, true);
            }
            foreach (var termo in naoAssinados)
            {
                this.TermoCompromissoExport(cliente, termo.Colaboradorfinal.Value, 2, false);
            }
        }
        public void TermoCompromissoExport(int cliente, int colaborador, int usuarioLogado, bool assinado)
        {
            try
            {
                var col = ObterColaboradorInterno(colaborador);
                var usu = _usuarioRepository.ObterPorId(usuarioLogado);
                var eqptos = _equipamentoNegocio.EquipamentosDoTermoDeEntrega(cliente, colaborador);
                string strEquipamentos = FormatarTabelaEquipamentos(eqptos);

                var template = _templateRepository.Buscar(x => x.Tipo == (int)TipoTemplateEnum.TermoCompromisso && x.Cliente == cliente).FirstOrDefault();
                var dataUltimaVersao = template.DataAlteracao.HasValue ? template.DataAlteracao.Value : template.DataCriacao;
                template.Conteudo = template.Conteudo
                    .Replace("@nomeEmpresa", col.EmpresaNavigation.Nome)
                    .Replace("@cnpjEmpresa", col.EmpresaNavigation.Cnpj.ToString())
                    .Replace("@centroCusto", col.CentrocustoNavigation.Codigo + "   " + col.CentrocustoNavigation.Nome)
                    .Replace("@nomeColaborador", col.Nome)
                    .Replace("@cargo", col.Cargo)
                    .Replace("@matricula", col.Matricula)
                    .Replace("@equipamentos", strEquipamentos)
                    .Replace("@usuarioLogado", usu.Nome)
                    .Replace("@dataAtual", TimeZoneMapper.GetDateTimeNow().ToString("dddd, dd MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
                    .Replace("@dataUltimaAtual", dataUltimaVersao.ToString("dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
                    .Replace("@versao", $"Versão: {template.Versao.ToString()}")
                    .Replace("@tipoColaborador", $"{ObterTipoColaborador(col.Tipocolaborador)}");

                var file = Path.Combine(Directory.GetCurrentDirectory(), "Documentos", "ckeditor.css");
                string css = File.Exists(file) ? File.ReadAllText(file) : string.Empty;
                template.Conteudo = template.Conteudo + "<style>" + css + "table{width:100%}</style>";


                //var pdf = _generatePdf.GetPDF(template.Conteudo);
                var pdf = HtmlToPdfConverter.ConvertHtmlToPdf(template.Conteudo);
                var fileName = "TermoEntrega " + col.Nome + ".pdf";

                File.WriteAllBytes((assinado) ? @"C:\SingleOne\Termos\Assinados\" + fileName : @"C:\SingleOne\Termos\Não Assinados\" + fileName, pdf);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RegistrarLocalizacaoAssinatura(LocalizacaoAssinaturaDTO dados)
        {
            try
            {
                var geolocalizacao = new GeolocalizacaoAssinatura
                {
                    ColaboradorId = dados.ColaboradorId,
                    ColaboradorNome = dados.ColaboradorNome,
                    UsuarioLogadoId = dados.UsuarioLogadoId,
                    IpAddress = dados.IP,
                    Country = dados.Country,
                    City = dados.City,
                    Region = dados.Region,
                    Latitude = (decimal?)dados.Latitude,
                    Longitude = (decimal?)dados.Longitude,
                    AccuracyMeters = (decimal?)dados.Accuracy,
                    TimestampCaptura = dados.Timestamp,
                    Acao = dados.Acao,
                    DataCriacao = DateTime.Now
                };

                _geolocalizacaoRepository.Adicionar(geolocalizacao);

                // Log para auditoria
                Console.WriteLine($"[GEOLOCALIZAÇÃO] Dados salvos no banco - ID: {geolocalizacao.Id}, " +
                    $"Colaborador: {dados.ColaboradorNome}, IP: {dados.IP}, " +
                    $"Local: {dados.City}, {dados.Country}, Ação: {dados.Acao}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO_GEOLOCALIZAÇÃO] Falha ao salvar no banco: {ex.Message}");
                throw new Exception($"Erro ao registrar localização: {ex.Message}", ex);
            }
        }

        // ===== CARGOS DE CONFIANÇA =====
        
        public List<string> ListarCargosUnicos(int cliente)
        {
            var cargos = (from col in _repository.Query()
                         where col.Cliente == cliente && 
                               !string.IsNullOrEmpty(col.Cargo) && 
                               VerificarSituacaoColaboradorAtivo(col)
                         group col by col.Cargo into cargosGroup
                         orderby cargosGroup.Key
                         select cargosGroup.Key).ToList();
            return cargos;
        }

        public List<CargoConfianca> ListarCargosConfianca(int cliente)
        {
            var cargos = _cargoConfiancaRepository.Buscar(x => x.Cliente == cliente)
                .OrderByDescending(x => x.Ativo)
                .ThenBy(x => x.Cargo)
                .ToList();
            return cargos;
        }

        public CargoConfianca SalvarCargoConfianca(CargoConfianca cargo)
        {
            // Verificar se já existe um cargo com o mesmo nome e tipo (padrão/exato) para o cliente
            var cargoExistente = _cargoConfiancaRepository
                .Buscar(x => x.Cliente == cargo.Cliente && 
                            x.Cargo.ToUpper().Trim() == cargo.Cargo.ToUpper().Trim() &&
                            x.Usarpadrao == cargo.Usarpadrao &&
                            x.Ativo)
                .FirstOrDefault();

            if (cargoExistente != null)
            {
                string tipo = cargo.Usarpadrao ? "padrão" : "exato";
                throw new DomainException($"Já existe um cargo de confiança '{cargo.Cargo}' (match {tipo}) configurado para este cliente.");
            }

            cargo.Datacriacao = DateTime.Now;
            cargo.Ativo = true;
            _cargoConfiancaRepository.Adicionar(cargo);
            return cargo;
        }

        public CargoConfianca AtualizarCargoConfianca(int id, CargoConfianca cargo)
        {
            var cargoExistente = _cargoConfiancaRepository.ObterPorId(id);
            if (cargoExistente == null)
            {
                throw new DomainException("Cargo de confiança não encontrado");
            }

            // Verificar se já existe outro cargo com o mesmo nome e tipo (exceto o próprio)
            var cargoDuplicado = _cargoConfiancaRepository
                .Buscar(x => x.Cliente == cargo.Cliente && 
                            x.Id != id &&
                            x.Cargo.ToUpper().Trim() == cargo.Cargo.ToUpper().Trim() &&
                            x.Usarpadrao == cargo.Usarpadrao &&
                            x.Ativo)
                .FirstOrDefault();

            if (cargoDuplicado != null)
            {
                string tipo = cargo.Usarpadrao ? "padrão" : "exato";
                throw new DomainException($"Já existe outro cargo de confiança '{cargo.Cargo}' (match {tipo}) configurado para este cliente.");
            }

            cargoExistente.Cargo = cargo.Cargo;
            cargoExistente.Usarpadrao = cargo.Usarpadrao;
            cargoExistente.Nivelcriticidade = cargo.Nivelcriticidade;
            cargoExistente.Obrigarsanitizacao = cargo.Obrigarsanitizacao;
            cargoExistente.Obrigardescaracterizacao = cargo.Obrigardescaracterizacao;
            cargoExistente.Obrigarperfuracaodisco = cargo.Obrigarperfuracaodisco;
            cargoExistente.Obrigarevidencias = cargo.Obrigarevidencias;
            cargoExistente.Ativo = cargo.Ativo;
            cargoExistente.Usuarioalteracao = cargo.Usuarioalteracao ?? cargo.Usuariocriacao;
            cargoExistente.Dataalteracao = DateTime.Now;

            _cargoConfiancaRepository.Atualizar(cargoExistente);
            return cargoExistente;
        }

        public void ExcluirCargoConfianca(int id)
        {
            var cargo = _cargoConfiancaRepository.ObterPorId(id);
            if (cargo != null)
            {
                _cargoConfiancaRepository.Remover(cargo);
            }
        }

        public CargoConfianca VerificarCargoConfianca(string cargo, int cliente)
        {
            if (string.IsNullOrWhiteSpace(cargo))
                return null;

            var cargoUpper = cargo.ToUpper().Trim();
            
            // Buscar todos os cargos de confiança ativos do cliente
            var cargosConfianca = _cargoConfiancaRepository
                .Buscar(x => x.Cliente == cliente && x.Ativo)
                .ToList();

            // Verificar match exato primeiro
            var matchExato = cargosConfianca.FirstOrDefault(x => 
                !x.Usarpadrao && 
                x.Cargo.ToUpper().Trim() == cargoUpper);
            
            if (matchExato != null)
                return matchExato;

            // Verificar padrões (LIKE)
            var matchPadrao = cargosConfianca.FirstOrDefault(x => 
                x.Usarpadrao && 
                cargoUpper.Contains(x.Cargo.ToUpper().Trim()));
            
            return matchPadrao;
        }

        /// <summary>
        /// Obtém a URL do site, priorizando a URL configurada para o cliente específico
        /// </summary>
        /// <param name="clienteId">ID do cliente (opcional). Se fornecido, busca URL específica do cliente.</param>
        private string ObterUrlSite(int? clienteId = null)
        {
            Console.WriteLine($"[OBTER_URL] ========== INÍCIO DETECÇÃO URL ==========");
            if (clienteId.HasValue)
            {
                Console.WriteLine($"[OBTER_URL] Cliente ID: {clienteId.Value}");
            }
            
            // 1. Tentar obter URL específica do cliente (prioridade MÁXIMA)
            if (clienteId.HasValue)
            {
                try
                {
                    var cliente = _clienteRepository.Buscar(x => x.Id == clienteId.Value).FirstOrDefault();
                    if (cliente != null && !string.IsNullOrEmpty(cliente.SiteUrl) && 
                        !cliente.SiteUrl.Contains("localhost") && !cliente.SiteUrl.Contains("SEU_IP"))
                    {
                        Console.WriteLine($"[OBTER_URL] ✅ Usando URL do cliente ({cliente.Razaosocial}): {cliente.SiteUrl}");
                        return cliente.SiteUrl;
                    }
                    else if (cliente != null && !string.IsNullOrEmpty(cliente.SiteUrl))
                    {
                        Console.WriteLine($"[OBTER_URL] ⚠️ URL do cliente configurada mas inválida: {cliente.SiteUrl}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OBTER_URL] ⚠️ Erro ao buscar URL do cliente: {ex.Message}");
                }
            }
            
            // 2. Tentar obter da variável de ambiente
            var envUrl = Environment.GetEnvironmentVariable("SITE_URL");
            if (!string.IsNullOrEmpty(envUrl) && !envUrl.Contains("localhost") && !envUrl.Contains("SEU_IP"))
            {
                Console.WriteLine($"[OBTER_URL] ✅ Usando URL da variável de ambiente: {envUrl}");
                return envUrl;
            }
            else if (!string.IsNullOrEmpty(envUrl))
            {
                Console.WriteLine($"[OBTER_URL] ⚠️ SITE_URL configurada mas inválida: {envUrl}");
            }

            // 3. Tentar usar a URL configurada no EnvironmentApiSettings
            if (!string.IsNullOrEmpty(_environmentApiSettings.SiteUrl) && 
                !_environmentApiSettings.SiteUrl.Contains("localhost") &&
                !_environmentApiSettings.SiteUrl.Contains("SEU_IP"))
            {
                Console.WriteLine($"[OBTER_URL] ✅ Usando URL configurada: {_environmentApiSettings.SiteUrl}");
                return _environmentApiSettings.SiteUrl;
            }
            else if (!string.IsNullOrEmpty(_environmentApiSettings.SiteUrl))
            {
                Console.WriteLine($"[OBTER_URL] ⚠️ SiteUrl configurada mas inválida: {_environmentApiSettings.SiteUrl}");
            }

            // 4. Tentar detectar IP do servidor automaticamente (ANTES de usar localhost)
            string detectedIp = null;
            try
            {
                var hostName = Dns.GetHostName();
                Console.WriteLine($"[OBTER_URL] Hostname do servidor: {hostName}");
                var addresses = Dns.GetHostAddresses(hostName);
                Console.WriteLine($"[OBTER_URL] Endereços encontrados: {addresses.Length}");
                
                foreach (var addr in addresses)
                {
                    Console.WriteLine($"[OBTER_URL]   - {addr} (Family: {addr.AddressFamily})");
                    // Ignorar IPv6 e localhost
                    if (addr.AddressFamily == AddressFamily.InterNetwork && 
                        !addr.ToString().StartsWith("127.") && 
                        !addr.ToString().StartsWith("169.254."))
                    {
                        detectedIp = addr.ToString();
                        Console.WriteLine($"[OBTER_URL] ✅ IP válido detectado: {detectedIp}");
                        break;
                    }
                }
                
                if (!string.IsNullOrEmpty(detectedIp))
                {
                    var detectedUrl = $"http://{detectedIp}";
                    Console.WriteLine($"[OBTER_URL] ⚠️ URL não configurada, usando IP detectado: {detectedUrl}");
                    Console.WriteLine($"[OBTER_URL] ⚠️ Configure SITE_URL no systemd para usar domínio personalizado");
                    Console.WriteLine($"[OBTER_URL] ========== FIM DETECÇÃO URL ==========");
                    return detectedUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OBTER_URL] ❌ Erro ao detectar IP: {ex.Message}");
            }

            // 5. Tentar detectar do ASPNETCORE_URLS (se configurado)
            var aspnetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (!string.IsNullOrEmpty(aspnetUrls))
            {
                Console.WriteLine($"[OBTER_URL] ASPNETCORE_URLS encontrado: {aspnetUrls}");
                // Extrair a primeira URL (pode ter múltiplas separadas por ;)
                var urls = aspnetUrls.Split(';');
                foreach (var url in urls)
                {
                    if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
                    {
                        // Tentar extrair IP da URL
                        var uri = new Uri(url);
                        var host = uri.Host;
                        if (host != "0.0.0.0" && host != "localhost" && host != "127.0.0.1")
                        {
                            var baseUrl = $"http://{host}";
                            Console.WriteLine($"[OBTER_URL] ✅ Detectado de ASPNETCORE_URLS: {baseUrl}");
                            Console.WriteLine($"[OBTER_URL] ========== FIM DETECÇÃO URL ==========");
                            return baseUrl;
                        }
                    }
                }
            }
            
            // 6. Avisar e usar fallback (só se não detectou IP)
            Console.WriteLine($"[OBTER_URL] ❌ ERRO: Não foi possível detectar URL do servidor!");
            Console.WriteLine($"[OBTER_URL] Configure no /etc/systemd/system/singleone-api.service:");
            Console.WriteLine($"[OBTER_URL] Environment=SITE_URL=http://SEU_IP_OU_DOMINIO");
            Console.WriteLine($"[OBTER_URL] Ou execute: sudo bash /opt/SingleOne/SingleOne_Backend/scripts/configurar_site_url.sh");
            Console.WriteLine($"[OBTER_URL] ========== FIM DETECÇÃO URL ==========");

            // Último fallback - mas avisar que está errado
            var fallback = _environmentApiSettings.SiteUrl ?? "http://localhost:4200";
            Console.WriteLine($"[OBTER_URL] ⚠️ USANDO FALLBACK (PODE ESTAR ERRADO): {fallback}");
            return fallback;
        }

    }
}
