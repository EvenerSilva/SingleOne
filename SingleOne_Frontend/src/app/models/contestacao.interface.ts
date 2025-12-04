export interface Contestacao {
  id: number;
  patrimonioId: number;
  colaboradorId: number;
  dataContestacao: Date;
  motivo: string;
  descricao: string;
  status: ContestacaoStatus;
  statusId: number;
  usuarioAbertura: string;
  tecnicoResponsavel?: string;
  tecnicoResponsavelId?: number;
  usuarioResolucao?: number;
  dataResolucao?: Date;
  observacoesResolucao?: string;
  cliente: number;
  hashContestacao: string;
  tipoContestacao?: string;
  equipamento?: {
    id: number;
    nome: string;
    numeroSerie: string;
    tipoEquipamento: string;
  };
  colaborador?: {
    id: number;
    nome: string;
    cpf: string;
    email: string;
  };
  anexos?: ContestacaoAnexo[];
  historico?: ContestacaoHistorico[];
}

export interface ContestacaoAnexo {
  id: number;
  contestacaoId: number;
  nomeArquivo: string;
  caminhoArquivo: string;
  tipoArquivo: string;
  tamanhoArquivo: number;
  dataUpload: Date;
  usuarioUpload: string;
}

export interface ContestacaoHistorico {
  id: number;
  contestacaoId: number;
  acao: string;
  descricao: string;
  dataAcao: Date;
  usuarioAcao: string;
  dadosAnteriores?: any;
  dadosNovos?: any;
}

export enum ContestacaoStatus {
  ABERTA = 'Aberta',
  EM_ANALISE = 'Em Análise',
  RESOLVIDA = 'Resolvida',
  CANCELADA = 'Cancelada',
  NEGADA = 'Negada',
  PENDENTE_COLABORADOR = 'Pendente Colaborador'
}

export interface ContestacaoFiltros {
  status?: string;
  dataInicio?: Date;
  dataFim?: Date;
  colaboradorId?: number;
  tecnicoResponsavelId?: number;
  patrimonioId?: number;
  busca?: string;
}

export interface ContestacaoEstatisticas {
  total: number;
  abertas: number;
  emAnalise: number;
  resolvidas: number;
  canceladas: number;
  negadas: number;
  pendentesColaborador: number;
  resolvidasHoje: number;
  pendentesUrgentes: number;
}
