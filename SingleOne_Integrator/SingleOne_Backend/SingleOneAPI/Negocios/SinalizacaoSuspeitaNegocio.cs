using Microsoft.EntityFrameworkCore;
using SingleOneAPI.Infra.Repositorio;
using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Negocios.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace SingleOneAPI.Negocios
{
    /// <summary>
    /// Lógica de negócio para sinalizações de suspeitas
    /// </summary>
    public class SinalizacaoSuspeitaNegocio : ISinalizacaoSuspeitaNegocio
    {
        private readonly IRepository<SinalizacaoSuspeita> _sinalizacaoRepository;
        private readonly IRepository<HistoricoInvestigacao> _historicoRepository;
        private readonly IRepository<MotivoSuspeita> _motivoRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Usuario> _usuarioRepository;

        public SinalizacaoSuspeitaNegocio(
            IRepository<SinalizacaoSuspeita> sinalizacaoRepository,
            IRepository<HistoricoInvestigacao> historicoRepository,
            IRepository<MotivoSuspeita> motivoRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Usuario> usuarioRepository)
        {
            _sinalizacaoRepository = sinalizacaoRepository;
            _historicoRepository = historicoRepository;
            _motivoRepository = motivoRepository;
            _colaboradorRepository = colaboradorRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<SinalizacaoCriadaDTO> CriarSinalizacaoAsync(CriarSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Criando sinalização para colaborador ID: {dto.ColaboradorId}");

                // Validar se colaborador existe
                var colaborador = await _colaboradorRepository.Buscar(x => x.Id == dto.ColaboradorId).FirstOrDefaultAsync();
                if (colaborador == null)
                {
                    return new SinalizacaoCriadaDTO
                    {
                        Sucesso = false,
                        Mensagem = "Colaborador não encontrado"
                    };
                }

                // Validar motivo de suspeita
                var motivo = await _motivoRepository.Buscar(x => x.Codigo == dto.MotivoSuspeita && x.Ativo).FirstOrDefaultAsync();
                if (motivo == null)
                {
                    return new SinalizacaoCriadaDTO
                    {
                        Sucesso = false,
                        Mensagem = "Motivo de suspeita inválido"
                    };
                }

                // Criar sinalização
                var agora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                var sinalizacao = new SinalizacaoSuspeita
                {
                    ColaboradorId = dto.ColaboradorId,
                    CpfConsultado = dto.CpfConsultado,
                    MotivoSuspeita = dto.MotivoSuspeita,
                    DescricaoDetalhada = dto.DescricaoDetalhada,
                    NomeVigilante = dto.NomeVigilante,
                    ObservacoesVigilante = dto.ObservacoesVigilante,
                    Prioridade = dto.Prioridade,
                    DadosConsulta = dto.DadosConsulta,
                    IpAddress = dto.IpAddress,
                    UserAgent = dto.UserAgent,
                    Status = "pendente",
                    DataSinalizacao = agora,
                    CreatedAt = agora,
                    UpdatedAt = agora
                };

                _sinalizacaoRepository.Adicionar(sinalizacao);
                _sinalizacaoRepository.SalvarAlteracoes();

                // Gerar número de protocolo
                var numeroProtocolo = $"SS{DateTime.Now:yyyyMMdd}{sinalizacao.Id:D6}";
                
                // Salvar o protocolo no banco
                sinalizacao.NumeroProtocolo = numeroProtocolo;
                _sinalizacaoRepository.Atualizar(sinalizacao);
                _sinalizacaoRepository.SalvarAlteracoes();

                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Sinalização criada com sucesso - ID: {sinalizacao.Id}, Protocolo: {numeroProtocolo}");

                return new SinalizacaoCriadaDTO
                {
                    Sucesso = true,
                    Mensagem = "Sinalização criada com sucesso",
                    SinalizacaoId = sinalizacao.Id,
                    NumeroProtocolo = numeroProtocolo
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao criar sinalização: {ex.Message}");
                return new SinalizacaoCriadaDTO
                {
                    Sucesso = false,
                    Mensagem = "Erro interno ao criar sinalização"
                };
            }
        }

        public async Task<SinalizacoesPaginadasDTO> ListarSinalizacoesAsync(FiltroSinalizacoesDTO filtros)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Listando sinalizações com filtros");

                var query = _sinalizacaoRepository.Buscar(x => true)
                    .Include(s => s.Colaborador)
                    .Include(s => s.Vigilante)
                    .Include(s => s.Investigador)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(filtros.Status))
                {
                    query = query.Where(s => s.Status == filtros.Status);
                }

                if (!string.IsNullOrEmpty(filtros.Prioridade))
                {
                    query = query.Where(s => s.Prioridade == filtros.Prioridade);
                }

                if (!string.IsNullOrEmpty(filtros.MotivoSuspeita))
                {
                    query = query.Where(s => s.MotivoSuspeita == filtros.MotivoSuspeita);
                }

                if (filtros.InvestigadorId.HasValue)
                {
                    query = query.Where(s => s.InvestigadorId == filtros.InvestigadorId.Value);
                }

                if (filtros.DataInicio.HasValue)
                {
                    // Início do dia (00:00:00)
                    var dataInicio = filtros.DataInicio.Value.Date;
                    query = query.Where(s => s.DataSinalizacao >= dataInicio);
                }

                if (filtros.DataFim.HasValue)
                {
                    // Final do dia (23:59:59.999) - adiciona 1 dia e compara com <
                    var dataFim = filtros.DataFim.Value.Date.AddDays(1);
                    query = query.Where(s => s.DataSinalizacao < dataFim);
                }

                if (!string.IsNullOrEmpty(filtros.ColaboradorNome))
                {
                    query = query.Where(s => s.Colaborador != null && s.Colaborador.Nome.Contains(filtros.ColaboradorNome));
                }

                if (!string.IsNullOrEmpty(filtros.CpfConsultado))
                {
                    query = query.Where(s => s.CpfConsultado.Contains(filtros.CpfConsultado));
                }

                // Contar total de registros
                var totalRegistros = await query.CountAsync();

                // Aplicar paginação
                var sinalizacoes = await query
                    .OrderByDescending(s => s.DataSinalizacao)
                    .Skip((filtros.Pagina - 1) * filtros.TamanhoPagina)
                    .Take(filtros.TamanhoPagina)
                    .Select(s => new SinalizacaoListaDTO
                    {
                        Id = s.Id,
                        NumeroProtocolo = s.NumeroProtocolo,
                        ColaboradorId = s.ColaboradorId,
                        ColaboradorNome = s.Colaborador != null ? s.Colaborador.Nome : "N/A",
                        CpfConsultado = s.CpfConsultado,
                        MotivoSuspeita = s.MotivoSuspeita,
                        DescricaoDetalhada = s.DescricaoDetalhada,
                        ObservacoesVigilante = s.ObservacoesVigilante,
                        Status = s.Status,
                        Prioridade = s.Prioridade,
                        DataSinalizacao = s.DataSinalizacao,
                        DataInvestigacao = s.DataInvestigacao,
                        DataResolucao = s.DataResolucao,
                        VigilanteId = s.VigilanteId,
                        InvestigadorId = s.InvestigadorId,
                        InvestigadorNome = s.Investigador != null ? s.Investigador.Nome : null,
                        VigilanteNome = s.Vigilante != null ? s.Vigilante.Nome : null,
                        ResultadoInvestigacao = s.ResultadoInvestigacao,
                        AcoesTomadas = s.AcoesTomadas,
                        ObservacoesFinais = s.ObservacoesFinais
                    })
                    .ToListAsync();

                // Obter descrições dos motivos
                var motivos = await _motivoRepository.Buscar(x => x.Ativo).ToListAsync();
                var motivosDict = motivos.ToDictionary(m => m.Codigo, m => m.Descricao);

                foreach (var sinalizacao in sinalizacoes)
                {
                    if (motivosDict.ContainsKey(sinalizacao.MotivoSuspeita))
                    {
                        sinalizacao.MotivoSuspeitaDescricao = motivosDict[sinalizacao.MotivoSuspeita];
                    }
                }

                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / filtros.TamanhoPagina);

                return new SinalizacoesPaginadasDTO
                {
                    Sinalizacoes = sinalizacoes,
                    TotalRegistros = totalRegistros,
                    PaginaAtual = filtros.Pagina,
                    TotalPaginas = totalPaginas,
                    TamanhoPagina = filtros.TamanhoPagina
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao listar sinalizações: {ex.Message}");
                throw;
            }
        }

        public async Task<SinalizacaoDetalhesDTO?> ObterDetalhesAsync(int id)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Obtendo detalhes da sinalização ID: {id}");

                var sinalizacao = await _sinalizacaoRepository.Buscar(x => x.Id == id)
                    .Include(s => s.Colaborador)
                    .Include(s => s.Vigilante)
                    .Include(s => s.Investigador)
                    .Include(s => s.Historico)
                        .ThenInclude(h => h.Usuario)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sinalizacao == null)
                {
                    return null;
                }

                // Obter descrição do motivo
                var motivo = await _motivoRepository.Buscar(x => x.Codigo == sinalizacao.MotivoSuspeita && x.Ativo)
                    .FirstOrDefaultAsync();

                var detalhes = new SinalizacaoDetalhesDTO
                {
                    Id = sinalizacao.Id,
                    ColaboradorId = sinalizacao.ColaboradorId,
                    ColaboradorNome = sinalizacao.Colaborador?.Nome ?? "N/A",
                    ColaboradorCpf = sinalizacao.Colaborador?.Cpf ?? "",
                    ColaboradorMatricula = sinalizacao.Colaborador?.Matricula ?? "",
                    ColaboradorCargo = sinalizacao.Colaborador?.Cargo ?? "",
                    ColaboradorSetor = sinalizacao.Colaborador?.Setor ?? "",
                    CpfConsultado = sinalizacao.CpfConsultado,
                    MotivoSuspeita = sinalizacao.MotivoSuspeita,
                    MotivoSuspeitaDescricao = motivo?.Descricao ?? sinalizacao.MotivoSuspeita,
                    DescricaoDetalhada = sinalizacao.DescricaoDetalhada,
                    NomeVigilante = sinalizacao.NomeVigilante,
                    NumeroProtocolo = sinalizacao.NumeroProtocolo,
                    ObservacoesVigilante = sinalizacao.ObservacoesVigilante,
                    Status = sinalizacao.Status,
                    Prioridade = sinalizacao.Prioridade,
                    DadosConsulta = sinalizacao.DadosConsulta,
                    IpAddress = sinalizacao.IpAddress,
                    UserAgent = sinalizacao.UserAgent,
                    DataSinalizacao = sinalizacao.DataSinalizacao,
                    DataInvestigacao = sinalizacao.DataInvestigacao,
                    DataResolucao = sinalizacao.DataResolucao,
                    InvestigadorId = sinalizacao.InvestigadorId,
                    InvestigadorNome = sinalizacao.Investigador?.Nome,
                    VigilanteId = sinalizacao.VigilanteId,
                    VigilanteNome = sinalizacao.Vigilante?.Nome,
                    ResultadoInvestigacao = sinalizacao.ResultadoInvestigacao,
                    AcoesTomadas = sinalizacao.AcoesTomadas,
                    ObservacoesFinais = sinalizacao.ObservacoesFinais,
                    Historico = sinalizacao.Historico.Select(h => new HistoricoInvestigacaoDTO
                    {
                        Id = h.Id,
                        UsuarioId = h.UsuarioId,
                        UsuarioNome = h.Usuario?.Nome ?? "Sistema",
                        Acao = h.Acao,
                        Descricao = h.Descricao,
                        DadosAntes = h.DadosAntes,
                        DadosDepois = h.DadosDepois,
                        CreatedAt = h.CreatedAt
                    }).OrderBy(h => h.CreatedAt).ToList()
                };

                return detalhes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao obter detalhes: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AtualizarStatusAsync(AtualizarStatusSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Atualizando status da sinalização ID: {dto.SinalizacaoId}");

                var sinalizacao = await _sinalizacaoRepository.Buscar(x => x.Id == dto.SinalizacaoId)
                    .FirstOrDefaultAsync();

                if (sinalizacao == null)
                {
                    return false;
                }

                var statusAnterior = sinalizacao.Status;
                var agora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                sinalizacao.Status = dto.Status;
                sinalizacao.UpdatedAt = agora;

                if (dto.Status == "em_investigacao" && !sinalizacao.DataInvestigacao.HasValue)
                {
                    sinalizacao.DataInvestigacao = agora;
                }

                if (dto.Status == "resolvida")
                {
                    sinalizacao.DataResolucao = agora;
                    
                    // Extrair dados da observação se vier do frontend
                    if (!string.IsNullOrEmpty(dto.Observacoes))
                    {
                        var obs = dto.Observacoes;
                        
                        // Parse das observações que vêm no formato:
                        // "RESULTADO: xxx\n\nAÇÕES TOMADAS: yyy\n\nOBSERVAÇÕES: zzz"
                        if (obs.Contains("RESULTADO:"))
                        {
                            var partes = obs.Split(new[] { "\n\n" }, StringSplitOptions.None);
                            
                            foreach (var parte in partes)
                            {
                                if (parte.StartsWith("RESULTADO:"))
                                {
                                    sinalizacao.ResultadoInvestigacao = parte.Replace("RESULTADO:", "").Trim();
                                }
                                else if (parte.StartsWith("AÇÕES TOMADAS:"))
                                {
                                    sinalizacao.AcoesTomadas = parte.Replace("AÇÕES TOMADAS:", "").Trim();
                                }
                                else if (parte.StartsWith("OBSERVAÇÕES:"))
                                {
                                    var obs_final = parte.Replace("OBSERVAÇÕES:", "").Trim();
                                    if (obs_final != "Nenhuma")
                                    {
                                        sinalizacao.ObservacoesFinais = obs_final;
                                    }
                                }
                            }
                        }
                    }
                }

                if (dto.Status == "arquivada")
                {
                    // Extrair dados da observação se vier do frontend  
                    if (!string.IsNullOrEmpty(dto.Observacoes))
                    {
                        var obs = dto.Observacoes;
                        
                        // Parse das observações que vêm no formato:
                        // "MOTIVO: xxx\n\nOBSERVAÇÕES: yyy"
                        if (obs.Contains("MOTIVO:"))
                        {
                            var partes = obs.Split(new[] { "\n\n" }, StringSplitOptions.None);
                            
                            foreach (var parte in partes)
                            {
                                if (parte.StartsWith("MOTIVO:"))
                                {
                                    sinalizacao.ObservacoesFinais = parte.Replace("MOTIVO:", "").Trim();
                                }
                                else if (parte.StartsWith("OBSERVAÇÕES:"))
                                {
                                    var obs_adicional = parte.Replace("OBSERVAÇÕES:", "").Trim();
                                    if (obs_adicional != "Nenhuma")
                                    {
                                        sinalizacao.ObservacoesFinais += "\n\n" + obs_adicional;
                                    }
                                }
                            }
                        }
                    }
                }

                if (dto.InvestigadorId.HasValue)
                {
                    sinalizacao.InvestigadorId = dto.InvestigadorId.Value;
                }

                _sinalizacaoRepository.Atualizar(sinalizacao);
                _sinalizacaoRepository.SalvarAlteracoes();

                Console.WriteLine($"[SINALIZACAO_NEGOCIO] ✅ Status atualizado com sucesso de '{statusAnterior}' para '{dto.Status}'");

                // NÃO criar histórico por enquanto (evitar erro de FK com usuarioId = 0)
                // await CriarHistoricoAsync(dto.SinalizacaoId, 0, "status_alterado", 
                //     $"Status alterado de {statusAnterior} para {dto.Status}", dto.Observacoes);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao atualizar status: {ex.Message}");
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> ResolverSinalizacaoAsync(ResolverSinalizacaoDTO dto)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Resolvendo sinalização ID: {dto.SinalizacaoId}");

                var sinalizacao = await _sinalizacaoRepository.Buscar(x => x.Id == dto.SinalizacaoId)
                    .FirstOrDefaultAsync();

                if (sinalizacao == null)
                {
                    return false;
                }

                var agora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                sinalizacao.Status = "resolvida";
                sinalizacao.DataResolucao = agora;
                sinalizacao.ResultadoInvestigacao = dto.ResultadoInvestigacao;
                sinalizacao.AcoesTomadas = dto.AcoesTomadas;
                sinalizacao.ObservacoesFinais = dto.ObservacoesFinais;
                sinalizacao.UpdatedAt = agora;

                _sinalizacaoRepository.Atualizar(sinalizacao);
                _sinalizacaoRepository.SalvarAlteracoes();

                // NÃO criar histórico por enquanto (evitar erro de FK com usuarioId = 0)
                // await CriarHistoricoAsync(dto.SinalizacaoId, 0, "resolvida", 
                //     "Sinalização resolvida", dto.ResultadoInvestigacao);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao resolver sinalização: {ex.Message}");
                return false;
            }
        }

        public async Task<List<MotivoSuspeitaDTO>> ObterMotivosSuspeitaAsync()
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Obtendo motivos de suspeita");

                var motivos = await _motivoRepository.Buscar(x => x.Ativo)
                    .OrderBy(m => m.Descricao)
                    .Select(m => new MotivoSuspeitaDTO
                    {
                        Id = m.Id,
                        Codigo = m.Codigo,
                        Descricao = m.Descricao,
                        DescricaoDetalhada = m.DescricaoDetalhada,
                        PrioridadePadrao = m.PrioridadePadrao,
                        Ativo = m.Ativo
                    })
                    .ToListAsync();

                return motivos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao obter motivos: {ex.Message}");
                throw;
            }
        }

        public async Task<EstatisticasSinalizacoesDTO> ObterEstatisticasAsync()
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Obtendo estatísticas");

                var sinalizacoes = await _sinalizacaoRepository.Buscar(x => true).ToListAsync();

                var estatisticas = new EstatisticasSinalizacoesDTO
                {
                    TotalSinalizacoes = sinalizacoes.Count,
                    Pendentes = sinalizacoes.Count(s => s.Status == "pendente"),
                    EmInvestigacao = sinalizacoes.Count(s => s.Status == "em_investigacao"),
                    Resolvidas = sinalizacoes.Count(s => s.Status == "resolvida"),
                    Arquivadas = sinalizacoes.Count(s => s.Status == "arquivada"),
                    Criticas = sinalizacoes.Count(s => s.Prioridade == "critica"),
                    Altas = sinalizacoes.Count(s => s.Prioridade == "alta"),
                    Medias = sinalizacoes.Count(s => s.Prioridade == "media"),
                    Baixas = sinalizacoes.Count(s => s.Prioridade == "baixa")
                };

                // Estatísticas por motivo
                estatisticas.PorMotivo = sinalizacoes
                    .GroupBy(s => s.MotivoSuspeita)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Estatísticas por status
                estatisticas.PorStatus = sinalizacoes
                    .GroupBy(s => s.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Estatísticas por prioridade
                estatisticas.PorPrioridade = sinalizacoes
                    .GroupBy(s => s.Prioridade)
                    .ToDictionary(g => g.Key, g => g.Count());

                return estatisticas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao obter estatísticas: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AtribuirInvestigadorAsync(int sinalizacaoId, int investigadorId)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Atribuindo sinalização ID: {sinalizacaoId} para investigador ID: {investigadorId}");

                var sinalizacao = await _sinalizacaoRepository.Buscar(x => x.Id == sinalizacaoId)
                    .FirstOrDefaultAsync();

                if (sinalizacao == null)
                {
                    Console.WriteLine($"[SINALIZACAO_NEGOCIO] ERRO: Sinalização {sinalizacaoId} não encontrada");
                    return false;
                }

                // Usar DateTime sem especificar Kind (Unspecified) para compatibilidade com PostgreSQL timestamp without time zone
                var agora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

                sinalizacao.InvestigadorId = investigadorId;
                sinalizacao.Status = "em_investigacao";
                sinalizacao.DataInvestigacao = agora;
                sinalizacao.UpdatedAt = agora;

                _sinalizacaoRepository.Atualizar(sinalizacao);
                _sinalizacaoRepository.SalvarAlteracoes();

                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Investigador {investigadorId} atribuído com sucesso à sinalização {sinalizacaoId}");

                // NÃO criar histórico por enquanto (evitar erro de FK com usuarioId = 0)
                // await CriarHistoricoAsync(sinalizacaoId, investigadorId, "atribuida", 
                //     "Sinalização atribuída para investigação");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao atribuir investigador: {ex.Message}");
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SINALIZACAO_NEGOCIO] Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> ArquivarSinalizacaoAsync(int id)
        {
            try
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Arquivando sinalização ID: {id}");

                var sinalizacao = await _sinalizacaoRepository.Buscar(x => x.Id == id)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (sinalizacao == null)
                {
                    return false;
                }

                var agora = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                
                sinalizacao.Status = "arquivada";
                sinalizacao.UpdatedAt = agora;

                _sinalizacaoRepository.Atualizar(sinalizacao);
                _sinalizacaoRepository.SalvarAlteracoes();

                // NÃO criar histórico por enquanto (evitar erro de FK com usuarioId = 0)
                // await CriarHistoricoAsync(id, 0, "arquivada", "Sinalização arquivada");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao arquivar: {ex.Message}");
                return false;
            }
        }

        private async Task CriarHistoricoAsync(int sinalizacaoId, int usuarioId, string acao, string descricao, string? observacoes = null)
        {
            try
            {
                var historico = new HistoricoInvestigacao
                {
                    SinalizacaoId = sinalizacaoId,
                    UsuarioId = usuarioId,
                    Acao = acao,
                    Descricao = observacoes != null ? $"{descricao} - {observacoes}" : descricao,
                    CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };

                _historicoRepository.Adicionar(historico);
                _historicoRepository.SalvarAlteracoes();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SINALIZACAO_NEGOCIO] Erro ao criar histórico: {ex.Message}");
            }
        }
    }
}
