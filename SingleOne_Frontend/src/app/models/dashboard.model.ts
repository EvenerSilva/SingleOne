// ========== INTERFACES PARA DASHBOARD EXPANDIDO ==========

export interface KPIPrincipal {
  valor: number;
  valorAnterior: number;
  variacao: number;
  tipoVariacao: string; // "percentual" ou "absoluto"
  tendencia: string; // "alta", "baixa" ou "estavel"
  sparklineUltimos7Dias: number[];
}

export interface MetricasSinalizacoes {
  // Totais por status
  total: number;
  pendentes: number;
  emInvestigacao: number;
  resolvidas: number;
  resolvidasHoje: number;
  arquivadas: number;
  
  // Totais por prioridade
  criticas: number;
  altas: number;
  medias: number;
  baixas: number;
  
  // Histórico
  ultimos7Dias: number[];
  ultimos30Dias: number[];
  
  // Alertas
  pendentesHaMaisDe7Dias: number;
  criticasNaoAtendidas: number;
}

export interface UsuarioAtivo {
  nome: string;
  acessos: number;
}

export interface MetricasAuditoria {
  acessosHoje: number;
  acessosOntem: number;
  tentativasFalhas: number;
  usuariosAtivosHoje: number;
  consultasCPFHoje: number;
  acessosUltimos7Dias: number[];
  topUsuariosAtivos: UsuarioAtivo[];
}

export interface NotificacaoDashboard {
  id: number;
  tipo: string; // "critico", "atencao", "info"
  titulo: string;
  mensagem: string;
  dataHora: Date;
  lida: boolean;
  link: string;
  icone: string;
}

export interface DistribuicaoMovimentacoes {
  entregas: number;
  devolucoes: number;
  outros: number;
}

export interface MetricasRequisicoes {
  totalRequisitados: number;
  urgentes: number;
  pendentes: number;
}

export interface MetricasDevolvidas {
  totalDevolvidos: number;
  vencidos: number;
  proximos: number;
}

export interface DashboardData {
  // Dados existentes
  devolucoesProgramadas: any[];
  equipamentosComColaboradorDesligado: any[];
  equipamentosPorStatus: any[];
  adesaoTermoResponsabilidade: {
    assinados: number;
    naoAssinados: number;
  };
  qtdeAtivosDescartados: number;
  qtdeAtivosRoubados: number;
  qtdeAtivosMovimentadoDia: number;
  qtdeAtivosMovimentadoDiaAnterior: number; // NOVO: Para comparação
  qtdeContestacoesPendentes: number;
  ultimosUsuariosQueMovimentaram: { [key: string]: number };
  usuarios?: any[];
  contestacoesPendentesLista: ContestacaoPendenteResumo[];
  
  // Novos dados
  ultimaAtualizacao: Date;
  totalRecursos: KPIPrincipal;
  totalColaboradores: KPIPrincipal;
  sinalizacoesPendentes: KPIPrincipal;
  devolucoesPendentes: KPIPrincipal;
  taxaAdesaoTermo: KPIPrincipal;
  contestacoesPendentes: KPIPrincipal;
  naoConformidadeElegibilidade: KPIPrincipal;
  garantiasCriticas: KPIPrincipal;
  recursosDescartados: KPIPrincipal;
  recursosPerdidos: KPIPrincipal;
  administradoresAtivos: KPIPrincipal;
  sinalizacoes: MetricasSinalizacoes;
  auditoria: MetricasAuditoria;
  notificacoes: NotificacaoDashboard[];
  
  // Novos dados de métricas reais
  distribuicaoMovimentacoes: DistribuicaoMovimentacoes;
  metricasRequisicoes: MetricasRequisicoes;
  metricasDevolvidas: MetricasDevolvidas;

  // KPIs de associações colaborador-recurso
  recursosAssociados: KPIPrincipal;
  colaboradoresComRecursos: KPIPrincipal;
  colaboradoresSemRecurso: KPIPrincipal;

  // Métricas de desligados
  totalColaboradoresDesligados: number;
  totalDesligadosComRecursos: number;
  totalRecursosDesligados: number;
}

export interface ContestacaoPendenteResumo {
  id: number;
  tipoContestacao: 'contestacao' | 'auto_inventario';
  status: string;
  motivo: string;
  descricao: string;
  dataContestacao: Date;
}

