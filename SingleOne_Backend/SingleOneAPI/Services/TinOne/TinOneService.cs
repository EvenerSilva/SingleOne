using SingleOneAPI.Infra.Contexto;
using SingleOneAPI.DTOs.TinOne;
using SingleOneAPI.Models.TinOne;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace SingleOneAPI.Services.TinOne
{
    /// <summary>
    /// Serviço principal do TinOne - Processamento de perguntas e base de conhecimento
    /// </summary>
    public class TinOneService : ITinOneService
    {
        private readonly SingleOneDbContext _context;
        private readonly ITinOneConfigService _configService;
        private readonly IOllamaService _ollamaService;
        private readonly ILogger<TinOneService> _logger;
        private readonly string _knowledgeBasePath;

        // Cache em memória (pode ser substituído por Redis/MemoryCache depois)
        private Dictionary<string, TinOneCampoInfoDTO>? _camposCache;
        private Dictionary<string, TinOneProcessoDTO>? _processosCache;
        private Dictionary<string, string>? _faqCache;

        public TinOneService(
            SingleOneDbContext context, 
            ITinOneConfigService configService,
            IOllamaService ollamaService,
            ILogger<TinOneService> logger,
            IWebHostEnvironment env)
        {
            _context = context;
            _configService = configService;
            _ollamaService = ollamaService;
            _logger = logger;
            _knowledgeBasePath = Path.Combine(env.ContentRootPath, "KnowledgeBase");
            
            // Garante que o diretório existe
            if (!Directory.Exists(_knowledgeBasePath))
            {
                Directory.CreateDirectory(_knowledgeBasePath);
                _logger.LogWarning($"[TinOne] Diretório de base de conhecimento criado: {_knowledgeBasePath}");
            }
        }

        public async Task<TinOneRespostaDTO> ProcessarPerguntaAsync(TinOnePerguntaDTO pergunta)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation($"[TinOne] Processando pergunta: {pergunta.Pergunta}");

                // Normaliza a pergunta
                var perguntaNormalizada = NormalizarTexto(pergunta.Pergunta);

                // Verifica se a pergunta é sobre temas fora do escopo
                if (VerificarPerguntaForaEscopo(perguntaNormalizada))
                {
                    stopwatch.Stop();
                    var respostaForaEscopo = new TinOneRespostaDTO
                    {
                        Resposta = AdicionarFraseSabedoria("🦉 Desculpe, mas eu sou especializado apenas em ajudar com o sistema SingleOne.\n\n" +
                                  "Não posso responder sobre temas como política, religião, esportes, notícias ou outros assuntos não relacionados ao sistema.\n\n" +
                                  "Como posso ajudá-lo com o SingleOne? Posso explicar sobre:\n" +
                                  "• Requisições e movimentações\n" +
                                  "• Equipamentos e patrimônio\n" +
                                  "• Colaboradores e cadastros\n" +
                                  "• Relatórios e exportações"),
                        Tipo = "texto",
                        Sucesso = true
                    };

                    await RegistrarAnalyticsAsync(
                        pergunta.UsuarioId, pergunta.ClienteId, pergunta.SessaoId,
                        pergunta.PaginaContexto, null, "pergunta_fora_escopo",
                        pergunta.Pergunta, respostaForaEscopo.Resposta, (int)stopwatch.ElapsedMilliseconds
                    );

                    // Salva conversa
                    await SalvarConversaAsync(pergunta, respostaForaEscopo.Resposta);

                    return respostaForaEscopo;
                }

                // 1. Tenta responder com FAQ
                var respostaFaq = await BuscarNaFaqAsync(perguntaNormalizada);
                if (respostaFaq != null)
                {
                    stopwatch.Stop();
                    await RegistrarAnalyticsAsync(
                        pergunta.UsuarioId, pergunta.ClienteId, pergunta.SessaoId,
                        pergunta.PaginaContexto, null, "pergunta_chat",
                        pergunta.Pergunta, respostaFaq.Resposta, (int)stopwatch.ElapsedMilliseconds
                    );

                    // Salva conversa
                    await SalvarConversaAsync(pergunta, respostaFaq.Resposta);

                    return respostaFaq;
                }

                // 2. Verifica se é pergunta sobre processo
                var processo = await IdentificarProcessoAsync(perguntaNormalizada);
                if (processo != null)
                {
                    stopwatch.Stop();
                    var resposta = new TinOneRespostaDTO
                    {
                        Resposta = AdicionarFraseSabedoria($"Encontrei o processo: {processo.Nome}. Posso te guiar passo a passo!"),
                        Tipo = "guia",
                        Dados = processo,
                        Sucesso = true
                    };

                    await RegistrarAnalyticsAsync(
                        pergunta.UsuarioId, pergunta.ClienteId, pergunta.SessaoId,
                        pergunta.PaginaContexto, null, "pergunta_chat",
                        pergunta.Pergunta, resposta.Resposta, (int)stopwatch.ElapsedMilliseconds
                    );

                    // Salva conversa
                    await SalvarConversaAsync(pergunta, resposta.Resposta);

                    return resposta;
                }

                // 3. Tenta usar IA se habilitada (RAG - Retrieval-Augmented Generation)
                var config = _configService.GetConfig(pergunta.ClienteId);
                bool iaHabilitada = config?.IaHabilitada ?? false;

                if (iaHabilitada)
                {
                    _logger.LogInformation("[TinOne] IA habilitada - tentando gerar resposta com Ollama");
                    
                    // Verifica se Ollama está disponível
                    var ollamaDisponivel = await _ollamaService.VerificarDisponibilidadeAsync();
                    
                    if (ollamaDisponivel)
                    {
                        // RAG: Busca contexto relevante na base de conhecimento
                        var contexto = await BuscarContextoRelevanteAsync(perguntaNormalizada);
                        
                        // Gera resposta usando IA + contexto
                        var respostaIA = await _ollamaService.GerarRespostaAsync(pergunta.Pergunta, contexto);
                        
                        if (!string.IsNullOrEmpty(respostaIA))
                        {
                            stopwatch.Stop();
                            var respostaComIA = new TinOneRespostaDTO
                            {
                                Resposta = AdicionarFraseSabedoria(respostaIA + "\n\n_✨ Resposta gerada por IA_"),
                                Tipo = "texto",
                                Sucesso = true
                            };

                            await RegistrarAnalyticsAsync(
                                pergunta.UsuarioId, pergunta.ClienteId, pergunta.SessaoId,
                                pergunta.PaginaContexto, null, "pergunta_ia",
                                pergunta.Pergunta, respostaComIA.Resposta, (int)stopwatch.ElapsedMilliseconds
                            );

                            await SalvarConversaAsync(pergunta, respostaComIA.Resposta);
                            
                            _logger.LogInformation("[TinOne] ✅ Resposta gerada com IA");
                            return respostaComIA;
                        }
                        else
                        {
                            _logger.LogWarning("[TinOne] IA não conseguiu gerar resposta, usando fallback");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("[TinOne] Ollama não disponível, usando resposta genérica");
                    }
                }
                
                // 4. Resposta genérica (fallback)
                stopwatch.Stop();
                
                var respostaGenerica = new TinOneRespostaDTO
                {
                    Resposta = AdicionarFraseSabedoria("Desculpe, ainda não sei responder essa pergunta. Estou aprendendo! 🦉\n\n" +
                              "Você pode tentar:\n" +
                              "• Reformular a pergunta\n" +
                              "• Perguntar sobre processos específicos (ex: 'como criar uma requisição?')\n" +
                              "• Navegar pelo menu para encontrar o que precisa"),
                    Tipo = "texto",
                    Sucesso = true
                };

                // Salva conversa genérica
                await SalvarConversaAsync(pergunta, respostaGenerica.Resposta);

                return respostaGenerica;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TinOne] Erro ao processar pergunta: {pergunta.Pergunta}");
                
                return new TinOneRespostaDTO
                {
                    Resposta = AdicionarFraseSabedoria("Ops! Tive um problema ao processar sua pergunta. Tente novamente em alguns instantes."),
                    Tipo = "erro",
                    Sucesso = false,
                    ErroMensagem = ex.Message
                };
            }
        }

        public async Task<TinOneCampoInfoDTO?> GetCampoInfoAsync(string campoId)
        {
            try
            {
                // Carrega cache se necessário
                if (_camposCache == null)
                {
                    await CarregarCamposAsync();
                }

                if (_camposCache != null && _camposCache.TryGetValue(campoId, out var campo))
                {
                    return campo;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TinOne] Erro ao buscar info do campo: {campoId}");
                return null;
            }
        }

        public async Task<TinOneProcessoDTO?> GetProcessoAsync(string processoId)
        {
            try
            {
                // Carrega cache se necessário
                if (_processosCache == null)
                {
                    await CarregarProcessosAsync();
                }

                if (_processosCache != null && _processosCache.TryGetValue(processoId, out var processo))
                {
                    return processo;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TinOne] Erro ao buscar processo: {processoId}");
                return null;
            }
        }

        public async Task RegistrarAnalyticsAsync(int? usuarioId, int? clienteId, string? sessaoId,
            string? paginaUrl, string? paginaNome, string? acaoTipo,
            string? pergunta, string? resposta, int? tempoRespostaMs)
        {
            try
            {
                // Verifica se analytics está habilitado
                var config = _configService.GetConfig(clienteId);
                if (!config.Analytics)
                    return;

                var analytics = new TinOneAnalytics
                {
                    UsuarioId = usuarioId,
                    ClienteId = clienteId,
                    SessaoId = sessaoId,
                    PaginaUrl = paginaUrl,
                    PaginaNome = paginaNome,
                    AcaoTipo = acaoTipo,
                    Pergunta = pergunta,
                    Resposta = resposta,
                    TempoRespostaMs = tempoRespostaMs,
                    CreatedAt = DateTime.Now
                };

                _context.Set<TinOneAnalytics>().Add(analytics);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Não falha se analytics falhar - apenas loga
                _logger.LogError(ex, "[TinOne] Erro ao registrar analytics");
            }
        }

        public async Task RegistrarFeedbackAsync(TinOneFeedbackDTO feedback)
        {
            try
            {
                if (feedback.AnalyticsId.HasValue)
                {
                    var analytics = await _context.Set<TinOneAnalytics>()
                        .FindAsync(feedback.AnalyticsId.Value);

                    if (analytics != null)
                    {
                        analytics.FoiUtil = feedback.FoiUtil;
                        analytics.FeedbackTexto = feedback.Comentario;
                        analytics.UpdatedAt = DateTime.Now;

                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"[TinOne] Feedback registrado - ID: {feedback.AnalyticsId}, Útil: {feedback.FoiUtil}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao registrar feedback");
            }
        }

        /**
         * Salva a conversa no histórico
         */
        private async Task SalvarConversaAsync(TinOnePerguntaDTO pergunta, string resposta)
        {
            try
            {
                _logger.LogInformation($"[TinOne] Iniciando salvamento de conversa - Usuário: {pergunta.UsuarioId}, Sessão: {pergunta.SessaoId}");
                
                // Salva pergunta do usuário
                var conversaUsuario = new TinOneConversa
                {
                    UsuarioId = pergunta.UsuarioId,
                    SessaoId = pergunta.SessaoId,
                    TipoMensagem = "usuario",
                    Mensagem = pergunta.Pergunta,
                    PaginaContexto = pergunta.PaginaContexto,
                    Metadata = null, // Deixa null por enquanto
                    CreatedAt = DateTime.Now
                };

                _context.Set<TinOneConversa>().Add(conversaUsuario);
                _logger.LogInformation($"[TinOne] Mensagem do usuário adicionada ao contexto");

                // Salva resposta do assistente
                var conversaAssistente = new TinOneConversa
                {
                    UsuarioId = pergunta.UsuarioId,
                    SessaoId = pergunta.SessaoId,
                    TipoMensagem = "assistente",
                    Mensagem = resposta,
                    PaginaContexto = pergunta.PaginaContexto,
                    Metadata = null, // Deixa null por enquanto
                    CreatedAt = DateTime.Now
                };

                _context.Set<TinOneConversa>().Add(conversaAssistente);
                _logger.LogInformation($"[TinOne] Resposta do assistente adicionada ao contexto");

                var resultado = await _context.SaveChangesAsync();
                
                _logger.LogInformation($"[TinOne] ✅ Conversa salva com sucesso! Registros salvos: {resultado}, Usuário: {pergunta.UsuarioId}, Sessão: {pergunta.SessaoId}");
            }
            catch (Exception ex)
            {
                // Não falha se salvar conversa falhar - apenas loga
                _logger.LogError(ex, "[TinOne] ❌ ERRO ao salvar conversa");
                _logger.LogError(ex, $"[TinOne] Detalhes - Mensagem: {ex.Message}, InnerException: {ex.InnerException?.Message}");
            }
        }

        #region Métodos Privados

        private async Task<TinOneRespostaDTO?> BuscarNaFaqAsync(string perguntaNormalizada)
        {
            // Carrega FAQ se necessário
            if (_faqCache == null)
            {
                await CarregarFaqAsync();
            }

            if (_faqCache == null)
                return null;

            // Quebra a pergunta em palavras e busca no dicionário
            // O dicionário já tem as chaves normalizadas
            var palavras = perguntaNormalizada.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            _logger.LogInformation($"[TinOne] Buscando FAQ - Palavras extraídas: {string.Join(", ", palavras)}");
            
            // Busca por combinações de 2 palavras primeiro (mais específico)
            for (int i = 0; i < palavras.Length - 1; i++)
            {
                var combinacao = $"{palavras[i]} {palavras[i + 1]}";
                if (_faqCache.ContainsKey(combinacao))
                {
                    _logger.LogInformation($"[TinOne] ✅ FAQ encontrado por combinação: '{combinacao}'");
                    
                    // Verifica se é uma saudação para responder com horário contextual
                    var resposta = _faqCache[combinacao];
                    if (EhSaudacao(combinacao))
                    {
                        resposta = GerarSaudacaoContextual();
                    }
                    
                    return new TinOneRespostaDTO
                    {
                        Resposta = AdicionarFraseSabedoria(resposta),
                        Tipo = "texto",
                        Sucesso = true
                    };
                }
            }
            
            // Busca por palavras individuais
            foreach (var palavra in palavras)
            {
                if (_faqCache.ContainsKey(palavra))
                {
                    _logger.LogInformation($"[TinOne] ✅ FAQ encontrado por palavra: '{palavra}'");
                    
                    // Verifica se é uma saudação para responder com horário contextual
                    var resposta = _faqCache[palavra];
                    if (EhSaudacao(palavra))
                    {
                        resposta = GerarSaudacaoContextual();
                    }
                    
                    return new TinOneRespostaDTO
                    {
                        Resposta = AdicionarFraseSabedoria(resposta),
                        Tipo = "texto",
                        Sucesso = true
                    };
                }
            }

            _logger.LogWarning($"[TinOne] ❌ Nenhuma FAQ encontrada para: '{perguntaNormalizada}'");
            return null;
        }

        private async Task<TinOneProcessoDTO?> IdentificarProcessoAsync(string perguntaNormalizada)
        {
            // Carrega processos se necessário
            if (_processosCache == null)
            {
                await CarregarProcessosAsync();
            }

            if (_processosCache == null)
                return null;

            // Busca processo por palavras-chave no dicionário
            // Pega todas as palavras da pergunta e procura no cache
            var palavras = perguntaNormalizada.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var palavra in palavras)
            {
                if (_processosCache.ContainsKey(palavra))
                {
                    return _processosCache[palavra];
                }
            }

            return null;
        }

        private async Task CarregarFaqAsync()
        {
            try
            {
                var faqPath = Path.Combine(_knowledgeBasePath, "faq.json");
                if (File.Exists(faqPath))
                {
                    var json = await File.ReadAllTextAsync(faqPath);
                    var faqList = JsonSerializer.Deserialize<List<FaqItem>>(json);
                    
                    // Converte lista para dicionário usando palavras-chave
                    _faqCache = new Dictionary<string, string>();
                    if (faqList != null)
                    {
                        foreach (var item in faqList)
                        {
                            // Adiciona entrada para cada palavra-chave
                            if (item.PalavrasChave != null)
                            {
                                foreach (var palavra in item.PalavrasChave)
                                {
                                    var palavraNormalizada = NormalizarTexto(palavra);
                                    if (!_faqCache.ContainsKey(palavraNormalizada))
                                    {
                                        _faqCache[palavraNormalizada] = item.Resposta;
                                    }
                                }
                            }
                        }
                    }
                    
                    _logger.LogInformation($"[TinOne] FAQ carregada: {_faqCache?.Count ?? 0} entradas");
                }
                else
                {
                    _faqCache = new Dictionary<string, string>();
                    _logger.LogWarning($"[TinOne] Arquivo FAQ não encontrado: {faqPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao carregar FAQ");
                _faqCache = new Dictionary<string, string>();
            }
        }
        
        // Classe auxiliar para deserializar FAQ
        private class FaqItem
        {
            public string Pergunta { get; set; }
            public string Resposta { get; set; }
            
            [System.Text.Json.Serialization.JsonPropertyName("Palavras-chave")]
            public List<string> PalavrasChave { get; set; }
        }

        private async Task CarregarCamposAsync()
        {
            try
            {
                var camposPath = Path.Combine(_knowledgeBasePath, "fields.json");
                if (File.Exists(camposPath))
                {
                    var json = await File.ReadAllTextAsync(camposPath);
                    _camposCache = JsonSerializer.Deserialize<Dictionary<string, TinOneCampoInfoDTO>>(json);
                    _logger.LogInformation($"[TinOne] Campos carregados: {_camposCache?.Count ?? 0}");
                }
                else
                {
                    _camposCache = new Dictionary<string, TinOneCampoInfoDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao carregar campos");
                _camposCache = new Dictionary<string, TinOneCampoInfoDTO>();
            }
        }

        private async Task CarregarProcessosAsync()
        {
            try
            {
                var processosPath = Path.Combine(_knowledgeBasePath, "processes.json");
                if (File.Exists(processosPath))
                {
                    var json = await File.ReadAllTextAsync(processosPath);
                    var processosList = JsonSerializer.Deserialize<List<TinOneProcessoDTO>>(json);
                    
                    // Converte lista para dicionário usando palavras-chave
                    _processosCache = new Dictionary<string, TinOneProcessoDTO>();
                    if (processosList != null)
                    {
                        foreach (var processo in processosList)
                        {
                            // Adiciona entrada para cada palavra-chave
                            if (processo.PalavrasChave != null)
                            {
                                foreach (var palavra in processo.PalavrasChave)
                                {
                                    var palavraNormalizada = NormalizarTexto(palavra);
                                    if (!_processosCache.ContainsKey(palavraNormalizada))
                                    {
                                        _processosCache[palavraNormalizada] = processo;
                                    }
                                }
                            }
                        }
                    }
                    
                    _logger.LogInformation($"[TinOne] Processos carregados: {_processosCache?.Count ?? 0}");
                }
                else
                {
                    _processosCache = new Dictionary<string, TinOneProcessoDTO>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne] Erro ao carregar processos");
                _processosCache = new Dictionary<string, TinOneProcessoDTO>();
            }
        }

        private bool VerificarPerguntaForaEscopo(string perguntaNormalizada)
        {
            // Primeiro, verifica se a pergunta contém palavras DO SISTEMA
            var palavrasDoSistema = new[]
            {
                "requisicao", "requisicoes", "equipamento", "equipamentos", "colaborador", "colaboradores",
                "patrimonio", "relatorio", "relatorios", "movimentacao", "movimentacoes", "entrega", "entregas",
                "devolucao", "devolucoes", "cadastro", "cadastros", "filtro", "filtros", "exportar", "exportacao",
                "garantia", "garantias", "nota fiscal", "numero serie", "cpf", "email", "localidade", "localidades",
                "centro custo", "empresa", "empresas", "tipo equipamento", "fabricante", "fabricantes", "modelo",
                "auditoria", "sinalizacao", "suspeit", "byod", "termo", "termos", "sistema", "singleone",
                "estoque", "minimo", "alerta", "dashboard", "usuario", "usuarios", "senha", "login", "acesso",
                "permissao", "permissoes", "perfil", "perfis", "configuracao", "configuracoes"
            };

            // Se contém palavras do sistema, NÃO bloquear
            foreach (var palavra in palavrasDoSistema)
            {
                if (perguntaNormalizada.Contains(palavra))
                {
                    _logger.LogInformation($"[TinOne] Pergunta válida do sistema detectada - palavra: {palavra}");
                    return false; // É sobre o sistema, não bloquear
                }
            }

            // Lista de palavras-chave que indicam temas fora do escopo do sistema
            var palavrasProibidas = new[]
            {
                // Política
                "politica", "eleicao", "eleicoes", "presidente", "governador", "prefeito", 
                "deputado", "senador", "partido", "esquerda", "direita", "golpe", "ditadura",
                "democracia", "voto", "urna", "plebiscito", "referendo", "congresso",
                "lula", "bolsonaro", "dilma", "temer", "fhc", "collor", "sarney",
                "pt", "psdb", "psl", "mdb", "psol", "pdt", "republicanos",
                "comunismo", "socialismo", "capitalismo", "fascismo", "nazismo",
                "impeachment", "corrupcao", "mensalao", "petrolao", "lava jato",
                
                // Religião
                "religiao", "deus", "jesus", "buda", "alah", "biblia", "corao", "igreja", 
                "templo", "mesquita", "sinagoga", "fe", "oracao", "milagre", "santo", "padre",
                "pastor", "rabino", "muculmano", "catolico", "evangelico", "espirita",
                
                // Guerras e Conflitos
                "guerra", "conflito", "exercito", "militar", "armamento", "bomba", 
                "missil", "tanque", "soldado", "batalha", "invasao", "ataque terrorista",
                "terrorismo", "bombardeio", "genocidio",
                
                // Esportes
                "futebol", "basquete", "volei", "tenis", "formula 1", "f1", "copa do mundo",
                "olimpiadas", "campeonato", "time", "jogador", "gol", "partida", "jogo",
                "brasileirao", "libertadores", "champions", "flamengo", "corinthians", 
                "palmeiras", "sao paulo", "santos",
                
                // Entretenimento
                "novela", "filme", "serie", "ator", "atriz", "cantora", "cantor", "musica",
                "show", "festival", "cinema", "netflix", "streaming", "youtube", "tiktok",
                "instagram", "facebook", "twitter", "big brother", "bbb", "reality show",
                "fama", "celebridade", "artista", "hit", "album", "globo", "record", "sbt",
                
                // Notícias Gerais
                "acidente", "crime", "assalto", "roubo", "assassinato", "homicidio",
                "trafico", "droga", "covid", "pandemia", "vacina", "virus", "doenca",
                "morto", "morte", "faleceu", "obito", "vitima", "policia", "prisao",
                
                // Outros temas pessoais/inapropriados
                "namoro", "casamento", "divorcio", "sexo", "relacionamento", "traicao",
                "piada", "piadinha", "fofoca", "celebridade", "famoso",
                "receita", "culinaria", "comida", "prato", "cozinhar", "ingrediente",
                "horoscopo", "signo", "astrologia", "zodiaco",
                
                // Finanças pessoais/investimentos
                "bitcoin", "criptomoeda", "bolsa de valores", "acoes", "investimento",
                "forex", "dolar", "euro", "cambio", "inflacao", "trader", "criptomoedas",
                
                // Clima/Meteorologia
                "previsao do tempo", "meteorologia", "tempestade", "furacao", "tufao",
                
                // Outros tópicos gerais
                "loteria", "mega sena", "aposta", "jogo de azar", "casino"
            };

            // Agora verifica se contém palavras proibidas
            foreach (var palavra in palavrasProibidas)
            {
                if (perguntaNormalizada.Contains(palavra))
                {
                    _logger.LogWarning($"[TinOne] Pergunta fora do escopo detectada - palavra: {palavra}");
                    return true; // Bloquear
                }
            }

            // Se não tem palavras do sistema nem palavras proibidas, deixa passar
            return false;
        }

        private string NormalizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            // Remove acentos e converte para minúsculas
            return texto.ToLower()
                .Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a")
                .Replace("é", "e").Replace("ê", "e")
                .Replace("í", "i")
                .Replace("ó", "o").Replace("õ", "o").Replace("ô", "o")
                .Replace("ú", "u").Replace("ü", "u")
                .Replace("ç", "c");
        }

        /// <summary>
        /// Verifica se a palavra/frase é uma saudação
        /// </summary>
        private bool EhSaudacao(string palavraNormalizada)
        {
            var saudacoes = new[] { 
                "ola", "oi", "bom dia", "boa tarde", "boa noite", 
                "e ai", "opa", "oi oni", "oni" 
            };
            
            return saudacoes.Any(s => palavraNormalizada.Contains(s) || s.Contains(palavraNormalizada));
        }

        /// <summary>
        /// Gera uma saudação contextual baseada no horário atual (hora de Brasília)
        /// </summary>
        private string GerarSaudacaoContextual()
        {
            // Pega a hora de Brasília (UTC-3)
            var fusoHorarioBrasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var horaAtual = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, fusoHorarioBrasilia);
            var hora = horaAtual.Hour;

            string saudacao;
            
            if (hora >= 6 && hora < 12)
            {
                saudacao = "☀️ Bom dia";
            }
            else if (hora >= 12 && hora < 18)
            {
                saudacao = "🌤️ Boa tarde";
            }
            else
            {
                saudacao = "🌙 Boa noite";
            }

            return $"{saudacao}! Sou o Oni o Sábio, seu assistente virtual do SingleOne! Como posso ajudá-lo?";
        }

        /// <summary>
        /// RAG: Busca contexto relevante na base de conhecimento para a IA
        /// </summary>
        private async Task<string> BuscarContextoRelevanteAsync(string perguntaNormalizada)
        {
            try
            {
                _logger.LogInformation("[TinOne RAG] Buscando contexto relevante");
                
                var contexto = new System.Text.StringBuilder();
                var palavrasPergunta = perguntaNormalizada.Split(new[] { ' ', ',', '.', '!', '?', ':', ';' }, 
                    StringSplitOptions.RemoveEmptyEntries);

                // 1. Busca em FAQ
                await CarregarFaqAsync();
                if (_faqCache != null && _faqCache.Any())
                {
                    var faqsRelevantes = new List<string>();
                    foreach (var palavra in palavrasPergunta)
                    {
                        if (_faqCache.ContainsKey(palavra))
                        {
                            faqsRelevantes.Add($"- {_faqCache[palavra]}");
                        }
                    }

                    if (faqsRelevantes.Any())
                    {
                        contexto.AppendLine("**Informações da Base de Conhecimento (FAQ):**");
                        foreach (var faq in faqsRelevantes.Distinct().Take(3)) // Máximo 3 FAQs
                        {
                            contexto.AppendLine(faq);
                        }
                        contexto.AppendLine();
                    }
                }

                // 2. Busca em Processos
                await CarregarProcessosAsync();
                if (_processosCache != null && _processosCache.Any())
                {
                    var processosRelevantes = new List<TinOneProcessoDTO>();
                    foreach (var palavra in palavrasPergunta)
                    {
                        if (_processosCache.ContainsKey(palavra))
                        {
                            processosRelevantes.Add(_processosCache[palavra]);
                        }
                    }

                    if (processosRelevantes.Any())
                    {
                        contexto.AppendLine("**Processos Disponíveis:**");
                        foreach (var processo in processosRelevantes.Distinct().Take(2)) // Máximo 2 processos
                        {
                            contexto.AppendLine($"- {processo.Nome}: {processo.Descricao}");
                            if (processo.Passos != null && processo.Passos.Any())
                            {
                                contexto.AppendLine("  Passos:");
                                foreach (var passo in processo.Passos.Take(3)) // Primeiros 3 passos
                                {
                                    contexto.AppendLine($"  {passo.Numero}. {passo.Titulo}");
                                }
                            }
                        }
                        contexto.AppendLine();
                    }
                }

                // 3. Busca em Campos (se relevante)
                await CarregarCamposAsync();
                if (_camposCache != null && _camposCache.Any())
                {
                    var camposRelevantes = new List<TinOneCampoInfoDTO>();
                    foreach (var palavra in palavrasPergunta)
                    {
                        if (_camposCache.ContainsKey(palavra))
                        {
                            camposRelevantes.Add(_camposCache[palavra]);
                        }
                    }

                    if (camposRelevantes.Any())
                    {
                        contexto.AppendLine("**Informações sobre Campos do Sistema:**");
                        foreach (var campo in camposRelevantes.Distinct().Take(3)) // Máximo 3 campos
                        {
                            contexto.AppendLine($"- {campo.Nome}: {campo.Descricao}");
                            if (!string.IsNullOrEmpty(campo.Exemplo))
                            {
                                contexto.AppendLine($"  Exemplo: {campo.Exemplo}");
                            }
                        }
                        contexto.AppendLine();
                    }
                }

                var contextoFinal = contexto.ToString().Trim();
                
                if (string.IsNullOrEmpty(contextoFinal))
                {
                    _logger.LogWarning("[TinOne RAG] Nenhum contexto relevante encontrado");
                    return "Nenhum contexto específico encontrado na base de conhecimento.";
                }
                
                _logger.LogInformation($"[TinOne RAG] ✅ Contexto montado com {contextoFinal.Length} caracteres");
                return contextoFinal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TinOne RAG] Erro ao buscar contexto");
                return string.Empty;
            }
        }

        /// <summary>
        /// Adiciona uma frase de sabedoria do ONI ao final da resposta (se ainda não tiver)
        /// </summary>
        private string AdicionarFraseSabedoria(string resposta)
        {
            // Verifica se a resposta já contém uma frase de sabedoria
            if (resposta.Contains("🦉") && (resposta.Contains("ONI ENSINA") || resposta.Contains("SABEDORIA DO ONI")))
            {
                return resposta; // Já tem, não adiciona outra
            }

            // Seleciona uma frase aleatória
            var frase = ObterFraseSabedoria();
            
            // Adiciona ao final da resposta
            return resposta + "\n\n**🦉 ONI ENSINA:**\n> \"" + frase + "\"";
        }

        /// <summary>
        /// Retorna uma frase de sabedoria aleatória do ONI
        /// </summary>
        private string ObterFraseSabedoria()
        {
            var frases = new[]
            {
                "A sabedoria não está em saber tudo, mas em saber onde encontrar.",
                "Cada ação registrada é um elo na corrente da governança.",
                "O controle não é sobre restrição, mas sobre organização e clareza.",
                "Um sistema bem usado é como um jardim bem cuidado: requer atenção constante.",
                "A rastreabilidade é a memória do sistema, preserve-a com cuidado.",
                "Compliance não é burocracia, é proteção para todos.",
                "Cada recurso tem uma história, e cada história importa.",
                "A organização é a base da eficiência.",
                "Documentar é preservar, preservar é governar.",
                "A auditoria não é punição, é garantia de integridade.",
                "Um termo assinado é um compromisso, honre-o sempre.",
                "O estoque vazio é sinal de planejamento ausente.",
                "Cada movimentação conta uma história, escreva-a bem.",
                "A conformidade não limita, ela protege e organiza.",
                "Um recurso bem cadastrado é um recurso bem controlado.",
                "A transparência é a luz que ilumina a governança.",
                "Cada colaborador é responsável, cada responsabilidade importa.",
                "O histórico não se apaga, ele se preserva para sempre.",
                "A organização é a mãe da eficiência.",
                "Um sistema sem controle é como um navio sem leme.",
                "A precisão nos dados é a base da confiança.",
                "Cada processo bem executado fortalece a governança.",
                "A atenção aos detalhes é o que separa o bom do excelente.",
                "Um inventário atualizado é um patrimônio protegido.",
                "A consistência é a chave da confiabilidade.",
                "Cada ação documentada é uma garantia de rastreabilidade.",
                "O cuidado com os dados é cuidado com o futuro.",
                "A disciplina no registro é disciplina na gestão.",
                "Um sistema bem usado é um sistema que serve bem.",
                "A governança começa com o primeiro registro e nunca termina."
            };

            var random = new Random();
            return frases[random.Next(frases.Length)];
        }

        #endregion
    }
}

