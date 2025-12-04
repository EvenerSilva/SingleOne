using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SingleOneAPI.Models.ViewModels;

namespace SingleOne.Models.ViewModels
{
    public class DashboardWebVM
    {
        public DashboardWebVM()
        {
            this.AdesaoTermoResponsabilidade = new AdesaoTermoVM();
            this.TotalRecursos = new KPIPrincipal();
            this.SinalizacoesPendentes = new KPIPrincipal();
            this.DevolucoesPendentes = new KPIPrincipal();
            this.TaxaAdesaoTermo = new KPIPrincipal();
            this.ContestacoesPendentes = new KPIPrincipal();
            this.NaoConformidadeElegibilidade = new KPIPrincipal();
            this.GarantiasCriticas = new KPIPrincipal();
            this.RecursosDescartados = new KPIPrincipal();
            this.RecursosPerdidos = new KPIPrincipal();
            this.AdministradoresAtivos = new KPIPrincipal();
            this.ContratosVencidos = new KPIPrincipal();
            this.LaudosEncerrados = new KPIPrincipal();
            this.Sinalizacoes = new MetricasSinalizacoes();
            this.Auditoria = new MetricasAuditoria();
            this.Notificacoes = new List<NotificacaoDashboard>();
            this.ContestacoesPendentesLista = new List<ContestacaoPendenteResumo>();
            this.MetricasCampanhas = new MetricasCampanhasVM() { UltimasCampanhas = new List<CampanhaResumoSimples>() };
        }
        
        // ========== DADOS EXISTENTES ==========
        public List<Vwdevolucaoprogramadum> DevolucoesProgramadas { get; set; }
        public List<Vwequipamentoscomcolaboradoresdesligado> EquipamentosComColaboradorDesligado { get; set; }
        public List<Vwequipamentosstatus> EquipamentosPorStatus { get; set; }
        public AdesaoTermoVM AdesaoTermoResponsabilidade { get; set; }
        public int QtdeAtivosDescartados { get; set; }
        public int QtdeAtivosRoubados { get; set; }
        public int QtdeAtivosMovimentadoDia { get; set; }
        public int QtdeContestacoesPendentes { get; set; }
        public Dictionary<string, int> UltimosUsuariosQueMovimentaram { get; set; }
        
        /// <summary>
        /// Lista resumida das contestações pendentes (inclui auto inventário)
        /// </summary>
        public List<ContestacaoPendenteResumo> ContestacoesPendentesLista { get; set; }

        // ========== NOVOS DADOS ==========
        
        /// <summary>
        /// Timestamp da última atualização dos dados
        /// </summary>
        public DateTime UltimaAtualizacao { get; set; }
        
        /// <summary>
        /// KPI: Total de Recursos com comparação
        /// </summary>
        public KPIPrincipal TotalRecursos { get; set; }
        
        /// <summary>
        /// KPI: Sinalizações Pendentes com comparação
        /// </summary>
        public KPIPrincipal SinalizacoesPendentes { get; set; }
        
        /// <summary>
        /// KPI: Devoluções Pendentes com comparação
        /// </summary>
        public KPIPrincipal DevolucoesPendentes { get; set; }
        
        /// <summary>
        /// KPI: Taxa de Adesão ao Termo com comparação
        /// </summary>
        public KPIPrincipal TaxaAdesaoTermo { get; set; }
        
        /// <summary>
        /// KPI: Contestações e Auto Inventário Pendentes com comparação
        /// </summary>
        public KPIPrincipal ContestacoesPendentes { get; set; }
        
        /// <summary>
        /// KPI: Não Conformidades de Elegibilidade com comparação
        /// </summary>
        public KPIPrincipal NaoConformidadeElegibilidade { get; set; }
        
        /// <summary>
        /// KPI: Garantias Críticas (sem data + expiradas) com comparação
        /// </summary>
        public KPIPrincipal GarantiasCriticas { get; set; }
        
        /// <summary>
        /// KPI: Recursos Descartados com comparação
        /// </summary>
        public KPIPrincipal RecursosDescartados { get; set; }
        
        /// <summary>
        /// KPI: Recursos Perdidos (Roubados/Extraviados) com comparação
        /// </summary>
        public KPIPrincipal RecursosPerdidos { get; set; }
        
        /// <summary>
        /// KPI: Administradores Ativos com comparação (limite recomendado: máx. 5)
        /// </summary>
        public KPIPrincipal AdministradoresAtivos { get; set; }
        
        /// <summary>
        /// KPI: Contratos Vencidos com comparação
        /// </summary>
        public KPIPrincipal ContratosVencidos { get; set; }
        
        /// <summary>
        /// Total de Contratos (simples)
        /// </summary>
        public int TotalContratos { get; set; }
        
        /// <summary>
        /// KPI: Laudos (Sinistros) Encerrados com comparação
        /// </summary>
        public KPIPrincipal LaudosEncerrados { get; set; }
        
        /// <summary>
        /// Total de Laudos/Sinistros (simples)
        /// </summary>
        public int TotalLaudos { get; set; }
        
        /// <summary>
        /// Laudos em Análise (sem Dtlaudo)
        /// </summary>
        public int LaudosEmAnalise { get; set; }
        
        /// <summary>
        /// Métricas consolidadas das Sinalizações de Suspeitas
        /// </summary>
        public MetricasSinalizacoes Sinalizacoes { get; set; }
        
        /// <summary>
        /// Métricas consolidadas da Auditoria de Acessos
        /// </summary>
        public MetricasAuditoria Auditoria { get; set; }
        
        /// <summary>
        /// Lista de notificações/alertas do dashboard
        /// </summary>
        public List<NotificacaoDashboard> Notificacoes { get; set; }
        
        // ========== MÉTRICAS DE MOVIMENTAÇÕES ==========
        
        /// <summary>
        /// Quantidade de ativos movimentados ontem (para comparação)
        /// </summary>
        public int QtdeAtivosMovimentadoDiaAnterior { get; set; }
        
        /// <summary>
        /// Distribuição de movimentações por tipo
        /// </summary>
        public DistribuicaoMovimentacoesVM DistribuicaoMovimentacoes { get; set; }
        
        /// <summary>
        /// Métricas de requisições
        /// </summary>
        public MetricasRequisicoesVM MetricasRequisicoes { get; set; }
        
        /// <summary>
        /// Métricas de devoluções
        /// </summary>
        public MetricasDevolvidasVM MetricasDevolvidas { get; set; }

        // ========= MÉTRICAS DE DESLIGADOS ==========
        /// <summary>
        /// Quantidade total de colaboradores com dtdemissao <= hoje (independente de terem recursos)
        /// </summary>
        public int TotalColaboradoresDesligados { get; set; }

        /// <summary>
        /// Quantidade de ex-colaboradores que ainda possuem ao menos 1 recurso ativo (entregue e não devolvido)
        /// </summary>
        public int TotalDesligadosComRecursos { get; set; }

        /// <summary>
        /// Soma total de recursos ativos (equipamentos + linhas) alocados a todos os desligados
        /// </summary>
        public int TotalRecursosDesligados { get; set; }

        // ========= KPIs DE ASSOCIAÇÕES COLABORADOR-RECURSO ==========
        /// <summary>
        /// Quantidade total de recursos atualmente associados a colaboradores (equipamentos + linhas)
        /// </summary>
        public KPIPrincipal RecursosAssociados { get; set; }

        /// <summary>
        /// Quantidade de colaboradores ativos que possuem pelo menos um recurso associado
        /// </summary>
        public KPIPrincipal ColaboradoresComRecursos { get; set; }

        /// <summary>
        /// Quantidade de colaboradores ativos sem qualquer recurso associado
        /// </summary>
        public KPIPrincipal ColaboradoresSemRecurso { get; set; }
        
        // ========= MÉTRICAS DE CAMPANHAS ==========
        /// <summary>
        /// Métricas consolidadas de Campanhas de Assinatura
        /// </summary>
        public MetricasCampanhasVM MetricasCampanhas { get; set; }
    }
    
    // ========== CLASSES AUXILIARES ==========
    
    /// <summary>
    /// Resumo de ação pendente de contestação (ou auto inventário)
    /// </summary>
    public class ContestacaoPendenteResumo
    {
        public int Id { get; set; }
        public string TipoContestacao { get; set; } = "contestacao"; // "contestacao" | "auto_inventario"
        public string Status { get; set; } = "pendente";
        public string Motivo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataContestacao { get; set; }
    }
    
    /// <summary>
    /// Distribuição de movimentações por tipo
    /// </summary>
    public class DistribuicaoMovimentacoesVM
    {
        public int Entregas { get; set; }
        public int Devolucoes { get; set; }
        public int Outros { get; set; }
    }
    
    /// <summary>
    /// Métricas de requisições
    /// </summary>
    public class MetricasRequisicoesVM
    {
        public int TotalRequisitados { get; set; }
        public int Urgentes { get; set; }
        public int Pendentes { get; set; }
    }
    
    /// <summary>
    /// Métricas de devoluções
    /// </summary>
    public class MetricasDevolvidasVM
    {
        public int TotalDevolvidos { get; set; }
        public int Vencidos { get; set; }
        public int Proximos { get; set; }
    }
    
    /// <summary>
    /// Métricas consolidadas de Campanhas de Assinatura
    /// </summary>
    public class MetricasCampanhasVM
    {
        /// <summary>
        /// Quantidade de campanhas abertas/ativas
        /// </summary>
        public int CampanhasAbertas { get; set; }
        
        /// <summary>
        /// Quantidade de campanhas encerradas/concluídas
        /// </summary>
        public int CampanhasEncerradas { get; set; }
        
        /// <summary>
        /// Quantidade de campanhas agendadas
        /// </summary>
        public int CampanhasAgendadas { get; set; }
        
        /// <summary>
        /// Total de colaboradores incluídos em campanhas ativas
        /// </summary>
        public int TotalColaboradoresEmCampanhas { get; set; }
        
        /// <summary>
        /// Total de assinaturas realizadas em campanhas
        /// </summary>
        public int TotalAssinaturasRealizadas { get; set; }
        
        /// <summary>
        /// Total de assinaturas pendentes em campanhas
        /// </summary>
        public int TotalAssinaturasPendentes { get; set; }
        
        /// <summary>
        /// Taxa de adesão média das campanhas (percentual)
        /// </summary>
        public decimal TaxaAdesaoMedia { get; set; }
        
        /// <summary>
        /// Lista das últimas campanhas (últimas 5)
        /// </summary>
        public List<CampanhaResumoSimples> UltimasCampanhas { get; set; }
    }
    
    /// <summary>
    /// Resumo simples de uma campanha para o dashboard
    /// </summary>
    public class CampanhaResumoSimples
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public char Status { get; set; }
        public string StatusDescricao { get; set; }
        public int TotalColaboradores { get; set; }
        public int TotalAssinados { get; set; }
        public int TotalPendentes { get; set; }
        public decimal? PercentualAdesao { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
