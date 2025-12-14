using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Infra.Repositorio;
using SingleOne.Util;
using SingleOne.Enumeradores;
using SingleOne.Models;
using SingleOneAPI;
using SingleOneAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContestacoesController : ControllerBase
    {
        private readonly IRepository<PatrimonioContestacao> _contestacaoRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Telefonialinha> _linhaRepository;
        private readonly IRepository<Usuario> _usuarioRepository;
        private readonly IRepository<Template> _templateRepository;
        private readonly IRepository<Empresa> _empresaRepository;
        private readonly IRepository<Cliente> _clienteRepository;
        private readonly SendMail _mail;

        public ContestacoesController(
            IRepository<PatrimonioContestacao> contestacaoRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Telefonialinha> linhaRepository,
            IRepository<Usuario> usuarioRepository,
            IRepository<Template> templateRepository,
            IRepository<Empresa> empresaRepository,
            IRepository<Cliente> clienteRepository,
            EnvironmentApiSettings environmentApiSettings,
            ISmtpConfigService smtpConfigService)
        {
            _contestacaoRepository = contestacaoRepository;
            _colaboradorRepository = colaboradorRepository;
            _equipamentoRepository = equipamentoRepository;
            _linhaRepository = linhaRepository;
            _usuarioRepository = usuarioRepository;
            _clienteRepository = clienteRepository;
            _templateRepository = templateRepository;
            _empresaRepository = empresaRepository;
            _mail = new SendMail(environmentApiSettings, smtpConfigService);
        }

        /// <summary>
        /// Lista contestações com filtros opcionais
        /// </summary>
        [HttpGet("[action]/{filtro}/{cliente}/{pagina}", Name = "ListarContestacoes")]
        public ActionResult<object> ListarContestacoes(string filtro, int cliente, int pagina)
        {
            try
            {
                Console.WriteLine($"[CONTESTACOES] Listando contestações - Filtro: {filtro}, Cliente: {cliente}, Página: {pagina}");

                // Iniciar query com Include do colaborador
                IQueryable<PatrimonioContestacao> query = _contestacaoRepository
                    .Buscar(x => true)
                    .Include(x => x.Colaborador);

                // Filtrar por cliente
                query = query.Where(x => x.Colaborador.Cliente == cliente);
                
                Console.WriteLine($"[CONTESTACOES] 🔍 Total de contestações do cliente {cliente}: {query.Count()}");

                // Aplicar filtro de busca se não for "null"
                if (!string.IsNullOrEmpty(filtro) && filtro != "null")
                {
                    Console.WriteLine($"[CONTESTACOES] 🔍 Aplicando filtro de busca: '{filtro}'");
                    
                    query = query.Where(x => 
                        x.Motivo.Contains(filtro) || 
                        x.Descricao.Contains(filtro) ||
                        x.Status.Contains(filtro) ||
                        (x.Colaborador != null && x.Colaborador.Nome.Contains(filtro))); // 🔍 Busca pelo nome do colaborador
                    
                    Console.WriteLine($"[CONTESTACOES] 🔍 Total após filtro de busca: {query.Count()}");
                }

                // Ordenar por data de contestação (mais recentes primeiro)
                var contestoes = query
                    .OrderByDescending(x => x.DataContestacao)
                    .ToList();

                // 📊 LOG: Mostrar distribuição por tipo
                var distribuicaoPorTipo = contestoes.GroupBy(x => x.TipoContestacao)
                    .Select(g => new { Tipo = g.Key, Count = g.Count() })
                    .ToList();
                
                Console.WriteLine($"[CONTESTACOES] 📊 Distribuição por tipo:");
                foreach (var item in distribuicaoPorTipo)
                {
                    Console.WriteLine($"  - {item.Tipo}: {item.Count}");
                }

                // Aplicar paginação
                var pageSize = 10;
                var totalCount = contestoes.Count;
                var skip = (pagina - 1) * pageSize;
                var pagedContestoes = contestoes.Skip(skip).Take(pageSize).ToList();

                // Enriquecer dados com informações de colaborador e equipamento
                var contestoesEnriquecidas = pagedContestoes.Select(c => new
                {
                    id = c.Id,
                    status = c.Status,
                    statusId = ObterStatusId(c.Status),
                    tipo_contestacao = c.TipoContestacao,
                    colaborador = new
                    {
                        id = c.ColaboradorId,
                        nome = ObterNomeColaborador(c.ColaboradorId)
                    },
                    equipamento = new
                    {
                        id = c.EquipamentoId,
                        nome = ObterNomeEquipamento(c.EquipamentoId),
                        numeroSerie = ObterNumeroSerieEquipamento(c.EquipamentoId)
                    },
                    dataContestacao = c.DataContestacao.ToString("yyyy-MM-dd HH:mm:ss"),
                    tecnicoResponsavel = ObterNomeUsuario(c.UsuarioResolucao),
                    motivo = c.Motivo,
                    descricao = c.Descricao,
                    observacaoResolucao = c.ObservacaoResolucao,
                    dataResolucao = c.DataResolucao?.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                var resultado = new
                {
                    results = contestoesEnriquecidas,
                    currentPage = pagina,
                    pageSize = pageSize,
                    rowCount = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                Console.WriteLine($"[CONTESTACOES] Retornando {contestoesEnriquecidas.Count} contestações de {totalCount} total");
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] Erro ao listar contestações: {ex.Message}");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém uma contestação específica por ID
        /// </summary>
        [HttpGet("{id}", Name = "ObterContestacao")]
        public ActionResult<object> ObterContestacao(int id)
        {
            try
            {
                var contestacao = _contestacaoRepository.Buscar(x => x.Id == id).FirstOrDefault();
                
                if (contestacao == null)
                {
                    return NotFound(new { error = "Contestação não encontrada" });
                }

                var resultado = new
                {
                    id = contestacao.Id,
                    status = contestacao.Status,
                    statusId = ObterStatusId(contestacao.Status),
                    colaborador = new
                    {
                        id = contestacao.ColaboradorId,
                        nome = ObterNomeColaborador(contestacao.ColaboradorId)
                    },
                    equipamento = new
                    {
                        id = contestacao.EquipamentoId,
                        nome = ObterNomeEquipamento(contestacao.EquipamentoId),
                        numeroSerie = ObterNumeroSerieEquipamento(contestacao.EquipamentoId)
                    },
                    dataContestacao = contestacao.DataContestacao.ToString("yyyy-MM-dd HH:mm:ss"),
                    tecnicoResponsavel = ObterNomeUsuario(contestacao.UsuarioResolucao),
                    motivo = contestacao.Motivo,
                    descricao = contestacao.Descricao,
                    observacaoResolucao = contestacao.ObservacaoResolucao,
                    dataResolucao = contestacao.DataResolucao?.ToString("yyyy-MM-dd HH:mm:ss"),
                    evidenciaUrl = contestacao.EvidenciaUrl
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] Erro ao obter contestação: {ex.Message}");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Cria uma nova contestação
        /// </summary>
        [HttpPost(Name = "CriarContestacao")]
        public ActionResult<object> CriarContestacao([FromBody] CriarContestacaoDTO contestacao)
        {
            try
            {
                if (contestacao == null)
                {
                    return BadRequest(new { error = "Dados da contestação não fornecidos" });
                }

                // Validar se já existe contestação pendente para este equipamento
                var contestacaoExistente = _contestacaoRepository.Buscar(x => 
                    x.ColaboradorId == contestacao.ColaboradorId && 
                    x.EquipamentoId == contestacao.EquipamentoId && 
                    x.Status == "pendente").FirstOrDefault();

                if (contestacaoExistente != null)
                {
                    return BadRequest(new { error = "Já existe uma contestação pendente para este equipamento" });
                }

                var novaContestacao = new PatrimonioContestacao
                {
                    ColaboradorId = contestacao.ColaboradorId,
                    EquipamentoId = contestacao.EquipamentoId,
                    Motivo = contestacao.Motivo,
                    Descricao = contestacao.Descricao,
                    Status = "pendente",
                    EvidenciaUrl = contestacao.EvidenciaUrl ?? "",
                    DataContestacao = TimeZoneMapper.GetDateTimeNow(),
                    CreatedAt = TimeZoneMapper.GetDateTimeNow(),
                    UpdatedAt = TimeZoneMapper.GetDateTimeNow()
                };

                _contestacaoRepository.Adicionar(novaContestacao);

                Console.WriteLine($"[CONTESTACOES] Nova contestação criada: ID {novaContestacao.Id}");
                return Ok(new { id = novaContestacao.Id, message = "Contestação criada com sucesso" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] Erro ao criar contestação: {ex.Message}");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Atualiza uma contestação existente
        /// </summary>
        [HttpPut("{id}", Name = "AtualizarContestacao")]
        public ActionResult<object> AtualizarContestacao(int id, [FromBody] AtualizarContestacaoDTO contestacao)
        {
            try
            {
                Console.WriteLine($"[CONTESTACOES] Atualizando contestação {id}");
                Console.WriteLine($"[CONTESTACOES] - Status: {contestacao?.Status}");
                Console.WriteLine($"[CONTESTACOES] - ObservacaoResolucao: {contestacao?.ObservacaoResolucao}");
                Console.WriteLine($"[CONTESTACOES] - UsuarioResolucao: {contestacao?.UsuarioResolucao}");
                Console.WriteLine($"[CONTESTACOES] - UsuarioResolucao.HasValue: {contestacao?.UsuarioResolucao.HasValue}");
                
                var contestacaoExistente = _contestacaoRepository.Buscar(x => x.Id == id).FirstOrDefault();
                
                if (contestacaoExistente == null)
                {
                    Console.WriteLine($"[CONTESTACOES] Contestação {id} não encontrada");
                    return NotFound(new { error = "Contestação não encontrada" });
                }

                Console.WriteLine($"[CONTESTACOES] Contestação encontrada - Status atual: {contestacaoExistente.Status}");

                // Atualizar campos
                if (!string.IsNullOrEmpty(contestacao.Status))
                {
                    contestacaoExistente.Status = contestacao.Status.ToLower();
                    Console.WriteLine($"[CONTESTACOES] Status atualizado para: {contestacaoExistente.Status}");
                }
                
                if (!string.IsNullOrEmpty(contestacao.ObservacaoResolucao))
                {
                    contestacaoExistente.ObservacaoResolucao = contestacao.ObservacaoResolucao;
                    Console.WriteLine($"[CONTESTACOES] Observação de resolução atualizada");
                }

                if (contestacao.UsuarioResolucao.HasValue)
                {
                    // Salvar ID sem validação de FK (pode ser usuário ou colaborador)
                    contestacaoExistente.UsuarioResolucao = contestacao.UsuarioResolucao;
                    Console.WriteLine($"[CONTESTACOES] Usuário/Colaborador resolução definido: {contestacao.UsuarioResolucao}");
                }

                // Se o status foi alterado para resolvido/cancelado/negada, definir data de resolução
                var statusLower = contestacao.Status?.ToLower() ?? "";
                if (statusLower == "resolvida" || statusLower == "cancelada" || statusLower == "negada")
                {
                    contestacaoExistente.DataResolucao = TimeZoneMapper.GetDateTimeNow();
                    Console.WriteLine($"[CONTESTACOES] Data de resolução definida: {contestacaoExistente.DataResolucao}");
                }

                contestacaoExistente.UpdatedAt = TimeZoneMapper.GetDateTimeNow();

                _contestacaoRepository.Atualizar(contestacaoExistente);

                Console.WriteLine($"[CONTESTACOES] Contestação {id} atualizada com sucesso");
                return Ok(new { message = "Contestação atualizada com sucesso" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] ERRO ao atualizar contestação {id}: {ex.Message}");
                Console.WriteLine($"[CONTESTACOES] Stack trace: {ex.StackTrace}");
                
                // Log detalhado de inner exceptions
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    Console.WriteLine($"[CONTESTACOES] Inner Exception: {innerEx.Message}");
                    Console.WriteLine($"[CONTESTACOES] Inner Stack trace: {innerEx.StackTrace}");
                    innerEx = innerEx.InnerException;
                }
                
                return StatusCode(500, new { 
                    error = $"Erro interno do servidor: {ex.Message}",
                    innerError = ex.InnerException?.Message,
                    details = ex.ToString()
                });
            }
        }

        /// <summary>
        /// Exclui uma contestação
        /// </summary>
        [HttpDelete("{id}", Name = "ExcluirContestacao")]
        public ActionResult<object> ExcluirContestacao(int id)
        {
            try
            {
                var contestacao = _contestacaoRepository.Buscar(x => x.Id == id).FirstOrDefault();
                
                if (contestacao == null)
                {
                    return NotFound(new { error = "Contestação não encontrada" });
                }

                _contestacaoRepository.Remover(contestacao);

                Console.WriteLine($"[CONTESTACOES] Contestação excluída: ID {id}");
                return Ok(new { message = "Contestação excluída com sucesso" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] Erro ao excluir contestação: {ex.Message}");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        #region Métodos Auxiliares


        private string ObterNomeColaborador(int colaboradorId)
        {
            try
            {
                var colaborador = _colaboradorRepository.Buscar(x => x.Id == colaboradorId).FirstOrDefault();
                return colaborador?.Nome ?? "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private string ObterNomeEquipamento(int? equipamentoId)
        {
            try
            {
                if (equipamentoId == null)
                {
                    return "N/A";
                }

                // Primeiro tenta buscar como equipamento
                var equipamento = _equipamentoRepository
                    .Include(e => e.TipoequipamentoNavigation)
                    .Where(x => x.Id == equipamentoId.Value)
                    .FirstOrDefault();
                
                if (equipamento != null)
                {
                    string tipoDescricao = equipamento.TipoequipamentoNavigation?.Descricao ?? "";
                    
                    // Verifica se é uma linha telefônica (tipo contém "telefon" ou "linha")
                    string tipoLower = tipoDescricao.ToLower();
                    if (tipoLower.Contains("telefon") || tipoLower.Contains("linha"))
                    {
                        // Para linhas, o número de série pode conter o número de telefone
                        if (!string.IsNullOrWhiteSpace(equipamento.Numeroserie))
                        {
                            return equipamento.Numeroserie;
                        }
                    }
                    
                    return equipamento.Patrimonio ?? "N/A";
                }

                // Se não encontrou como equipamento, tenta buscar na tabela de linhas telefônicas
                var linha = _linhaRepository
                    .Include(l => l.PlanoNavigation)
                    .Where(x => x.Id == equipamentoId.Value)
                    .FirstOrDefault();
                
                if (linha != null)
                {
                    return linha.Numero.ToString();
                }

                return "N/A";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] Erro ao obter nome do equipamento/linha {equipamentoId}: {ex.Message}");
                return "N/A";
            }
        }

        private string ObterNumeroSerieEquipamento(int? equipamentoId)
        {
            try
            {
                if (equipamentoId == null)
                {
                    return "N/A";
                }

                // Primeiro tenta buscar como equipamento
                var equipamento = _equipamentoRepository
                    .Include(e => e.TipoequipamentoNavigation)
                    .Where(x => x.Id == equipamentoId.Value)
                    .FirstOrDefault();
                
                if (equipamento != null)
                {
                    string tipoDescricao = equipamento.TipoequipamentoNavigation?.Descricao ?? "";
                    
                    // Verifica se é uma linha telefônica
                    string tipoLower = tipoDescricao.ToLower();
                    if (tipoLower.Contains("telefon") || tipoLower.Contains("linha"))
                    {
                        // Para linhas telefônicas, o numeroSerie já contém o ICCID
                        if (!string.IsNullOrWhiteSpace(equipamento.Numeroserie))
                        {
                            return equipamento.Numeroserie;
                        }
                        
                        // Se não tem numeroSerie, tenta buscar na tabela de linhas pelo número de telefone armazenado no patrimônio
                        if (!string.IsNullOrWhiteSpace(equipamento.Patrimonio) && decimal.TryParse(equipamento.Patrimonio, out decimal numero))
                        {
                            var linha = _linhaRepository.Buscar(x => x.Numero == numero).FirstOrDefault();
                            if (linha != null && !string.IsNullOrWhiteSpace(linha.Iccid))
                            {
                                return linha.Iccid;
                            }
                        }
                        
                        return "N/A";
                    }
                    
                    return equipamento.Numeroserie ?? "N/A";
                }

                // Se não encontrou como equipamento, tenta buscar na tabela de linhas telefônicas diretamente
                var linhaDireta = _linhaRepository.Buscar(x => x.Id == equipamentoId.Value).FirstOrDefault();
                if (linhaDireta != null)
                {
                    return !string.IsNullOrWhiteSpace(linhaDireta.Iccid) ? linhaDireta.Iccid : "N/A";
                }

                return "N/A";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] Erro ao obter número de série do equipamento/linha {equipamentoId}: {ex.Message}");
                return "N/A";
            }
        }

        private string ObterNomeUsuario(int? usuarioId)
        {
            try
            {
                if (!usuarioId.HasValue) return "N/A";
                
                // Primeiro tenta buscar como usuário (equipe técnica)
                var usuario = _usuarioRepository.Buscar(x => x.Id == usuarioId.Value).FirstOrDefault();
                if (usuario != null)
                {
                    return usuario.Nome;
                }
                
                // Se não encontrou como usuário, tenta buscar como colaborador (autoatendimento)
                var colaborador = _colaboradorRepository.Buscar(x => x.Id == usuarioId.Value).FirstOrDefault();
                if (colaborador != null)
                {
                    return $"{colaborador.Nome} (Colaborador)";
                }
                
                return "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        #endregion

        /// <summary>
        /// Obtém as contestações de um colaborador específico
        /// </summary>
        [HttpGet("colaborador/{colaboradorId}")]
        [AllowAnonymous] // Temporariamente para teste
        public ActionResult<object> ObterContestoesColaborador(int colaboradorId)
        {
            try
            {
                Console.WriteLine($"[CONTESTACOES] Obtendo contestações do colaborador {colaboradorId}");

                // Dados mockados para teste
                var dadosMockados = new List<object>
                {
                    new
                    {
                        id = 1,
                        patrimonioId = 123,
                        colaboradorId = colaboradorId,
                        dataContestacao = DateTime.Now.AddDays(-5),
                        motivo = "Não reconheço este recurso",
                        descricao = "Este equipamento não foi entregue para mim",
                        status = "pendente",
                        statusId = 1,
                        usuarioAbertura = "Sistema",
                        tecnicoResponsavel = "Não atribuído",
                        tecnicoResponsavelId = (int?)null,
                        usuarioResolucao = (int?)null,
                        dataResolucao = (DateTime?)null,
                        observacoesResolucao = "",
                        cliente = 1,
                        hashContestacao = "1",
                        tipoContestacao = "contestacao",
                        equipamento = new
                        {
                            id = 123,
                            nome = "Notebook Dell",
                            numeroSerie = "DL123456789",
                            tipoEquipamento = "Notebook"
                        },
                        colaborador = new
                        {
                            id = colaboradorId,
                            nome = "Colaborador Teste",
                            cpf = "12345678901",
                            email = "teste@empresa.com"
                        }
                    },
                    new
                    {
                        id = 2,
                        patrimonioId = 456,
                        colaboradorId = colaboradorId,
                        dataContestacao = DateTime.Now.AddDays(-10),
                        motivo = "Recurso está danificado",
                        descricao = "O equipamento chegou com defeito",
                        status = "resolvida",
                        statusId = 3,
                        usuarioAbertura = "Sistema",
                        tecnicoResponsavel = "João Silva",
                        tecnicoResponsavelId = 1,
                        usuarioResolucao = 1,
                        dataResolucao = DateTime.Now.AddDays(-2),
                        observacoesResolucao = "Equipamento substituído por um novo",
                        cliente = 1,
                        hashContestacao = "2",
                        tipoContestacao = "contestacao",
                        equipamento = new
                        {
                            id = 456,
                            nome = "Monitor Samsung",
                            numeroSerie = "SM987654321",
                            tipoEquipamento = "Monitor"
                        },
                        colaborador = new
                        {
                            id = colaboradorId,
                            nome = "Colaborador Teste",
                            cpf = "12345678901",
                            email = "teste@empresa.com"
                        }
                    }
                };

                return Ok(new
                {
                    sucesso = true,
                    mensagem = "Contestações obtidas com sucesso (dados mockados)",
                    data = dadosMockados
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] Erro ao obter contestações do colaborador: {ex.Message}");
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
        /// 📝 Cria registro(s) de inventário forçado
        /// Usado quando TI identifica colaboradores sem recursos e precisa fazer levantamento
        /// </summary>
        [HttpPost("[action]", Name = "CriarInventarioForcado")]
        public async Task<ActionResult<object>> CriarInventarioForcado([FromBody] CriarInventarioForcadoDTO dto)
        {
            try
            {
                Console.WriteLine($"[CONTESTACOES] 📝 Criando inventário forçado para {dto.ColaboradorIds?.Count ?? 1} colaborador(es)");

                var colaboradorIds = dto.ColaboradorIds != null && dto.ColaboradorIds.Any() 
                    ? dto.ColaboradorIds 
                    : new List<int> { dto.ColaboradorId };

                var inventariosCriados = new List<PatrimonioContestacao>();

                foreach (var colaboradorId in colaboradorIds)
                {
                    // Verificar se colaborador existe
                    var colaborador = _colaboradorRepository.Buscar(c => c.Id == colaboradorId).FirstOrDefault();
                    if (colaborador == null)
                    {
                        Console.WriteLine($"[CONTESTACOES] ⚠️ Colaborador {colaboradorId} não encontrado, pulando...");
                        continue;
                    }

                    // Criar registro de inventário forçado
                    var inventario = new PatrimonioContestacao
                    {
                        ColaboradorId = colaboradorId,
                        EquipamentoId = 0, // Inventário forçado não tem equipamento específico ainda
                        Motivo = dto.Motivo,
                        Descricao = dto.Descricao,
                        Status = "pendente",
                        TipoContestacao = "inventario_forcado", // 🎯 Tipo específico
                        DataContestacao = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UsuarioResolucao = dto.UsuarioId // Quem criou (TI)
                    };

                    _contestacaoRepository.Adicionar(inventario);
                    inventariosCriados.Add(inventario);

                    Console.WriteLine($"[CONTESTACOES] ✅ Inventário forçado criado para colaborador {colaboradorId} (ID: {inventario.Id})");
                }

                if (inventariosCriados.Count == 0)
                {
                    return BadRequest(new { 
                        mensagem = "Nenhum inventário forçado foi criado. Verifique os IDs dos colaboradores.", 
                        status = "400" 
                    });
                }

                // 📧 ENVIAR E-MAILS DE NOTIFICAÇÃO (apenas se solicitado)
                if (dto.EnviarEmail)
                {
                    Console.WriteLine($"[CONTESTACOES] 📧 Iniciando envio de e-mails para {inventariosCriados.Count} colaborador(es)");
                    
                    try
                    {
                        // Buscar template de notificação de inventário forçado
                        var template = _templateRepository
                            .Buscar(t => t.Tipo == (int)TipoTemplateEnum.NotificacaoInventarioForcado && t.Cliente == dto.ClienteId)
                            .FirstOrDefault();

                        if (template == null)
                        {
                            Console.WriteLine("[CONTESTACOES] ⚠️ Template de notificação de inventário forçado não encontrado. E-mails não serão enviados.");
                        }
                        else
                        {
                        // Buscar usuário que forçou o inventário
                        var usuarioQueForcou = _usuarioRepository.Buscar(u => u.Id == dto.UsuarioId).FirstOrDefault();
                        var nomeUsuarioQueForcou = usuarioQueForcou?.Nome ?? "Sistema";

                        int emailsEnviados = 0;
                        int emailsFalhados = 0;

                        foreach (var inventario in inventariosCriados)
                        {
                            try
                            {
                                // Buscar colaborador com relacionamentos
                                var colaborador = _colaboradorRepository
                                    .Include(c => c.EmpresaNavigation)
                                    .Include(c => c.CentrocustoNavigation)
                                    .Where(c => c.Id == inventario.ColaboradorId)
                                    .FirstOrDefault();

                                if (colaborador == null || string.IsNullOrEmpty(colaborador.Email))
                                {
                                    Console.WriteLine($"[CONTESTACOES] ⚠️ Colaborador {inventario.ColaboradorId} ({colaborador?.Nome ?? "N/A"}) não encontrado ou sem e-mail cadastrado. Pulando envio.");
                                    emailsFalhados++;
                                    continue;
                                }

                                // Descriptografar e validar email
                                string emailColaborador = Cripto.CriptografarDescriptografar(colaborador.Email, false)?.Trim();
                                
                                // Validar email após descriptografia
                                if (string.IsNullOrWhiteSpace(emailColaborador))
                                {
                                    Console.WriteLine($"[CONTESTACOES] ⚠️ Colaborador {inventario.ColaboradorId} ({colaborador.Nome}) possui e-mail inválido após descriptografia. Pulando envio.");
                                    emailsFalhados++;
                                    continue;
                                }

                                // Calcular prazo (exemplo: 5 dias úteis)
                                int prazoDias = 5;
                                DateTime dataLimite = DateTime.Now.AddDays(prazoDias);
                                string prazoCalculado = $"{prazoDias} dias úteis";

                                // Buscar empresa principal do cliente
                                var empresaPrincipal = _empresaRepository
                                    .Buscar(e => e.Cliente == dto.ClienteId)
                                    .OrderBy(e => e.Id)
                                    .FirstOrDefault();

                                // Buscar URL do cliente (site_url da tabela clientes)
                                var cliente = _clienteRepository.Buscar(c => c.Id == dto.ClienteId).FirstOrDefault();
                                string urlSistema = cliente?.SiteUrl ?? "http://localhost:4200";
                                Console.WriteLine($"[CONTESTACOES] URL do sistema para e-mail: {urlSistema}");

                                // Substituir variáveis do template
                                string conteudoEmail = template.Conteudo
                                    .Replace("@nomeColaborador", colaborador.Nome ?? "")
                                    .Replace("@cpf", Cripto.CriptografarDescriptografar(colaborador.Cpf ?? "", false))
                                    .Replace("@matricula", colaborador.Matricula ?? "")
                                    .Replace("@cargo", colaborador.Cargo ?? "N/A")
                                    .Replace("@empresa", colaborador.EmpresaNavigation?.Nome ?? "N/A")
                                    .Replace("@dataLimite", dataLimite.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR")))
                                    .Replace("@prazoCalculado", prazoCalculado)
                                    .Replace("@dataForcado", DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR")))
                                    .Replace("@nomeEquipe", "TI/Patrimônio")
                                    .Replace("@emailEquipe", "ti@empresa.com") // TODO: Parametrizar
                                    .Replace("@telefoneEquipe", "Ramal XXXX") // TODO: Parametrizar
                                    .Replace("@usuarioQueForcou", nomeUsuarioQueForcou)
                                    .Replace("@mensagemAdicional", !string.IsNullOrWhiteSpace(dto.MensagemAdicional) ? $"<p><strong>Mensagem da equipe:</strong></p><p>{dto.MensagemAdicional}</p>" : "")
                                    .Replace("@nomeEmpresa", empresaPrincipal?.Nome ?? "Empresa")
                                    .Replace("@urlSistema", urlSistema); // ✅ Novo marcador para URL do sistema

                                // Enviar e-mail
                                await _mail.EnviarAsync(
                                    emailColaborador,
                                    template.Titulo ?? "Levantamento de Recursos de TI",
                                    conteudoEmail,
                                    null,
                                    dto.ClienteId
                                );

                                Console.WriteLine($"[CONTESTACOES] ✅ E-mail enviado para {colaborador.Nome} ({emailColaborador})");
                                emailsEnviados++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[CONTESTACOES] ❌ Erro ao enviar e-mail para colaborador {inventario.ColaboradorId}: {ex.Message}");
                                emailsFalhados++;
                            }
                        }

                            Console.WriteLine($"[CONTESTACOES] 📊 Resumo de envios: {emailsEnviados} enviados, {emailsFalhados} falhados");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CONTESTACOES] ❌ Erro geral no envio de e-mails: {ex.Message}");
                        // Não falha a operação se houver erro no envio de emails
                    }
                }
                else
                {
                    Console.WriteLine("[CONTESTACOES] 📧 Envio de email não solicitado. Pulando etapa de notificação.");
                }

                return Ok(new
                {
                    mensagem = $"Inventário forçado criado com sucesso para {inventariosCriados.Count} colaborador(es)",
                    status = "200",
                    data = inventariosCriados.Select(i => new
                    {
                        id = i.Id,
                        colaboradorId = i.ColaboradorId,
                        status = i.Status,
                        dataContestacao = i.DataContestacao
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTESTACOES] ❌ Erro ao criar inventário forçado: {ex.Message}");
                Console.WriteLine($"[CONTESTACOES] ❌ StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { 
                    mensagem = $"Erro ao criar inventário forçado: {ex.Message}", 
                    status = "500" 
                });
            }
        }
    }

    /// <summary>
    /// DTO para atualizar contestação
    /// </summary>
    public class AtualizarContestacaoDTO
    {
        public string Status { get; set; }
        
        public string ObservacaoResolucao { get; set; }
        
        public int? UsuarioResolucao { get; set; }
    }

    /// <summary>
    /// DTO para criar inventário forçado
    /// </summary>
    public class CriarInventarioForcadoDTO
    {
        public int ColaboradorId { get; set; }
        public List<int> ColaboradorIds { get; set; }
        public string Motivo { get; set; }
        public string Descricao { get; set; }
        public int UsuarioId { get; set; }
        public int ClienteId { get; set; }
        public bool EnviarEmail { get; set; } // 📧 Se deve enviar email de notificação
        public string MensagemAdicional { get; set; } // 💬 Mensagem adicional opcional para o email
    }
}
