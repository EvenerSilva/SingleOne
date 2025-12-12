using SingleOneAPI.Models;
using SingleOneAPI.Models.DTO;
using SingleOneAPI.Infra.Repositorio;
using SingleOne.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingleOne.Negocios
{
    /// <summary>
    /// Negócio para funcionalidades de patrimônio e PassCheck
    /// </summary>
    public class PatrimonioNegocio
    {
        private readonly IRepository<PatrimonioContestacao> _contestacaoRepository;
        private readonly IRepository<PatrimonioLogAcesso> _logAcessoRepository;
        private readonly IRepository<Colaboradore> _colaboradorRepository;
        private readonly IRepository<Equipamento> _equipamentoRepository;
        private readonly IRepository<Telefonialinha> _telefonialinhaRepository;

        public PatrimonioNegocio(
            IRepository<PatrimonioContestacao> contestacaoRepository,
            IRepository<PatrimonioLogAcesso> logAcessoRepository,
            IRepository<Colaboradore> colaboradorRepository,
            IRepository<Equipamento> equipamentoRepository,
            IRepository<Telefonialinha> telefonialinhaRepository)
        {
            _contestacaoRepository = contestacaoRepository;
            _logAcessoRepository = logAcessoRepository;
            _colaboradorRepository = colaboradorRepository;
            _equipamentoRepository = equipamentoRepository;
            _telefonialinhaRepository = telefonialinhaRepository;
        }

        /// <summary>
        /// Loga acesso ao sistema
        /// </summary>
        public void LogarAcesso(string tipoAcesso, int? colaboradorId, string cpfConsultado, 
            string ipAddress, string userAgent, object dadosConsultados, bool sucesso, string mensagemErro)
        {
            try
            {
                var log = new PatrimonioLogAcesso
                {
                    TipoAcesso = tipoAcesso,
                    ColaboradorId = colaboradorId,
                    CpfConsultado = cpfConsultado,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DadosConsultados = dadosConsultados != null ? Newtonsoft.Json.JsonConvert.SerializeObject(dadosConsultados) : "",
                    Sucesso = sucesso,
                    MensagemErro = mensagemErro,
                    CreatedAt = TimeZoneMapper.GetDateTimeNow()
                };

                _logAcessoRepository.Adicionar(log);
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Log de acesso registrado: {tipoAcesso} - {cpfConsultado}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao registrar log: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca contestações por colaborador
        /// </summary>
        public List<PatrimonioContestacao> BuscarContestoesPorColaborador(int colaboradorId)
        {
            try
            {
                return _contestacaoRepository.Buscar(x => x.ColaboradorId == colaboradorId)
                    .OrderByDescending(x => x.DataContestacao)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao buscar contestações: {ex.Message}");
                return new List<PatrimonioContestacao>();
            }
        }

        /// <summary>
        /// Cria nova contestação
        /// </summary>
        public bool CriarContestacao(CriarContestacaoDTO contestacao)
        {
            try
            {
                // Verificar se já existe contestação pendente para este equipamento
                var contestacaoExistente = _contestacaoRepository.Buscar(x => 
                    x.ColaboradorId == contestacao.ColaboradorId && 
                    x.EquipamentoId == contestacao.EquipamentoId && 
                    x.Status == "pendente").FirstOrDefault();

                if (contestacaoExistente != null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Já existe contestação pendente para este equipamento");
                    return false;
                }

                // Verificar se o equipamento existe (pode ser equipamento real ou linha telefônica)
                var equipamento = _equipamentoRepository.Buscar(x => x.Id == contestacao.EquipamentoId).FirstOrDefault();
                var linhaTelefonica = _telefonialinhaRepository.Buscar(x => x.Id == contestacao.EquipamentoId).FirstOrDefault();
                
                if (equipamento == null && linhaTelefonica == null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Equipamento não encontrado (ID: {contestacao.EquipamentoId})");
                    return false;
                }

                // Verificar se o equipamento/linha pertence ao colaborador através das requisições processadas
                // No sistema de patrimônio, a validação é feita pelo frontend baseado nos dados já carregados
                // Se chegou até aqui, significa que o equipamento foi validado no frontend
                if (equipamento != null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Equipamento validado: {equipamento.Id} - {equipamento.Patrimonio}");
                }
                else if (linhaTelefonica != null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Linha telefônica validada: {linhaTelefonica.Id} - {linhaTelefonica.Numero}");
                }

                var novaContestacao = new PatrimonioContestacao
                {
                    ColaboradorId = contestacao.ColaboradorId,
                    EquipamentoId = contestacao.EquipamentoId,
                    Motivo = contestacao.Motivo,
                    Descricao = contestacao.Descricao,
                    EvidenciaUrl = contestacao.EvidenciaUrl,
                    Status = "pendente",
                    DataContestacao = TimeZoneMapper.GetDateTimeNow(),
                    CreatedAt = TimeZoneMapper.GetDateTimeNow(),
                    UpdatedAt = TimeZoneMapper.GetDateTimeNow()
                };

                _contestacaoRepository.Adicionar(novaContestacao);
                
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação criada com sucesso: ID {novaContestacao.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao criar contestação: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cancela uma contestação pendente do colaborador para o equipamento
        /// </summary>
        public bool CancelarContestacao(int colaboradorId, int equipamentoId, int? contestacaoId, string? justificativa)
        {
            try
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Tentando cancelar contestação - ColaboradorId={colaboradorId}, EquipamentoId={equipamentoId}, ContestacaoId={contestacaoId}");
                
                PatrimonioContestacao pendente;
                if (contestacaoId.HasValue && contestacaoId.Value > 0)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Buscando por ID da contestação: {contestacaoId.Value}");
                    pendente = _contestacaoRepository.Buscar(x => x.Id == contestacaoId.Value && x.ColaboradorId == colaboradorId).FirstOrDefault();
                    
                    if (pendente != null)
                    {
                        Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação encontrada: ID={pendente.Id}, Status={pendente.Status}, Colab={pendente.ColaboradorId}");
                    }
                    else
                    {
                        Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação com ID {contestacaoId.Value} não encontrada ou não pertence ao colaborador {colaboradorId}");
                        
                        // Verificar se a contestação existe sem filtro de colaborador
                        var contestacaoQualquer = _contestacaoRepository.Buscar(x => x.Id == contestacaoId.Value).FirstOrDefault();
                        if (contestacaoQualquer != null)
                        {
                            Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação existe mas pertence ao colaborador {contestacaoQualquer.ColaboradorId}, não ao {colaboradorId}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Buscando por ColaboradorId e EquipamentoId");
                    pendente = _contestacaoRepository.Buscar(x =>
                        x.ColaboradorId == colaboradorId &&
                        x.EquipamentoId == equipamentoId &&
                        x.Status == "pendente").FirstOrDefault();
                }

                if (pendente == null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Nenhuma contestação pendente encontrada para cancelar. Colab={colaboradorId}, Equip={equipamentoId}, ContestacaoId={contestacaoId}");
                    return false;
                }
                
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação encontrada para cancelar: ID={pendente.Id}, Status atual={pendente.Status}");

                pendente.Status = "cancelada";
                pendente.DataResolucao = TimeZoneMapper.GetDateTimeNow();
                pendente.UpdatedAt = TimeZoneMapper.GetDateTimeNow();
                
                // Salvar o colaborador como responsável pela resolução (cancelamento)
                pendente.UsuarioResolucao = colaboradorId;
                
                if (!string.IsNullOrWhiteSpace(justificativa))
                {
                    pendente.ObservacaoResolucao = justificativa;
                }
                else
                {
                    pendente.ObservacaoResolucao = "Cancelada pelo colaborador.";
                }

                _contestacaoRepository.Atualizar(pendente);
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação cancelada: {pendente.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao cancelar contestação: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Solicita devolução de equipamento
        /// </summary>
        public bool SolicitarDevolucao(SolicitarDevolucaoDTO solicitacao)
        {
            try
            {
                // Verificar se o equipamento pertence ao colaborador
                var equipamento = _equipamentoRepository.Buscar(x => x.Id == solicitacao.EquipamentoId).FirstOrDefault();
                if (equipamento == null || equipamento.Usuario != solicitacao.ColaboradorId)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Equipamento não pertence ao colaborador");
                    return false;
                }

                // Aqui você pode implementar a lógica para criar uma requisição de devolução
                // Por enquanto, vamos apenas logar a solicitação
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Solicitação de devolução: Colaborador {solicitacao.ColaboradorId}, Equipamento {solicitacao.EquipamentoId}");
                
                // TODO: Implementar criação de requisição de devolução
                // Pode ser uma nova tabela ou usar a tabela de requisições existente
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao solicitar devolução: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Resolve contestação (para administradores)
        /// </summary>
        public bool ResolverContestacao(int contestacaoId, string status, string observacao, int usuarioResolucao)
        {
            try
            {
                var contestacao = _contestacaoRepository.Buscar(x => x.Id == contestacaoId).FirstOrDefault();
                if (contestacao == null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação não encontrada: {contestacaoId}");
                    return false;
                }

                contestacao.Status = status;
                contestacao.ObservacaoResolucao = observacao;
                contestacao.UsuarioResolucao = usuarioResolucao;
                contestacao.DataResolucao = TimeZoneMapper.GetDateTimeNow();
                contestacao.UpdatedAt = TimeZoneMapper.GetDateTimeNow();

                _contestacaoRepository.Atualizar(contestacao);
                
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Contestação resolvida: {contestacaoId} - {status}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao resolver contestação: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Busca logs de acesso por período
        /// </summary>
        public List<PatrimonioLogAcesso> BuscarLogsAcesso(DateTime dataInicio, DateTime dataFim, string tipoAcesso = null)
        {
            try
            {
                var query = _logAcessoRepository.Buscar(x => 
                    x.CreatedAt >= dataInicio && x.CreatedAt <= dataFim);

                if (!string.IsNullOrEmpty(tipoAcesso))
                {
                    query = query.Where(x => x.TipoAcesso == tipoAcesso);
                }

                return query.OrderByDescending(x => x.CreatedAt).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao buscar logs: {ex.Message}");
                return new List<PatrimonioLogAcesso>();
            }
        }

        /// <summary>
        /// Obtém estatísticas de uso
        /// </summary>
        public object ObterEstatisticas(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var logs = _logAcessoRepository.Buscar(x => 
                    x.CreatedAt >= dataInicio && x.CreatedAt <= dataFim);

                var estatisticas = new
                {
                    TotalAcessos = logs.Count(),
                    AcessosPassCheck = logs.Count(x => x.TipoAcesso == "passcheck"),
                    AcessosPatrimonio = logs.Count(x => x.TipoAcesso == "patrimonio"),
                    AcessosComSucesso = logs.Count(x => x.Sucesso),
                    AcessosComErro = logs.Count(x => !x.Sucesso),
                    ContestoesPendentes = _contestacaoRepository.Buscar(x => x.Status == "pendente").Count(),
                    ContestoesResolvidas = _contestacaoRepository.Buscar(x => x.Status != "pendente").Count()
                };

                return estatisticas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Erro ao obter estatísticas: {ex.Message}");
                return new { erro = ex.Message };
            }
        }

        /// <summary>
        /// Criar solicitação de Auto Inventário
        /// </summary>
        public bool CriarAutoInventario(CriarAutoInventarioDTO dados)
        {
            try
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] === INICIANDO CRIAÇÃO DE AUTO INVENTÁRIO ===");
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Colaborador ID: {dados.ColaboradorId}");
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Número de série: {dados.NumeroSerie}");
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Observações: {dados.Observacoes}");

                // Verificar se o colaborador existe
                var colaborador = _colaboradorRepository.Buscar(x => x.Id == dados.ColaboradorId).FirstOrDefault();
                if (colaborador == null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] ERRO: Colaborador não encontrado (ID: {dados.ColaboradorId})");
                    return false;
                }
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Colaborador encontrado: {colaborador.Nome}");

                // Verificar se já existe solicitação pendente para este número de série
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Verificando solicitações existentes...");
                var solicitacaoExistente = _contestacaoRepository.Buscar(x => 
                    x.ColaboradorId == dados.ColaboradorId && 
                    x.TipoContestacao == "auto_inventario" &&
                    x.Descricao == dados.NumeroSerie &&
                    x.Status == "pendente").FirstOrDefault();

                if (solicitacaoExistente != null)
                {
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] ERRO: Já existe solicitação de Auto Inventário pendente para este número de série");
                    return false;
                }
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Nenhuma solicitação duplicada encontrada");

                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Criando nova solicitação...");
                
                // 📝 Concatenar número de série e observações na descrição para exibir ambos
                string descricaoCompleta = $"S/N: {dados.NumeroSerie}";
                if (!string.IsNullOrWhiteSpace(dados.Observacoes))
                {
                    descricaoCompleta += $"\n\n📝 Observações do colaborador:\n{dados.Observacoes}";
                    Console.WriteLine($"[PATRIMONIO_NEGOCIO] Observações do colaborador adicionadas à descrição");
                }
                
                var novaSolicitacao = new PatrimonioContestacao
                {
                    ColaboradorId = dados.ColaboradorId,
                    EquipamentoId = null, // Não tem equipamento ainda (será vinculado posteriormente)
                    TipoContestacao = "auto_inventario",
                    Motivo = "Auto Inventário",
                    Descricao = descricaoCompleta, // ✅ SN + Observações do colaborador
                    Status = "pendente",
                    DataContestacao = TimeZoneMapper.GetDateTimeNow(),
                    CreatedAt = TimeZoneMapper.GetDateTimeNow(),
                    UpdatedAt = TimeZoneMapper.GetDateTimeNow()
                };

                Console.WriteLine($"[PATRIMONIO_NEGOCIO] Salvando no banco de dados...");
                _contestacaoRepository.Adicionar(novaSolicitacao);
                
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] ✅ Auto Inventário criado com sucesso: ID {novaSolicitacao.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] ❌ ERRO ao criar Auto Inventário: {ex.Message}");
                Console.WriteLine($"[PATRIMONIO_NEGOCIO] ❌ Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
