export interface ProtocoloDescarte {
  id?: number;
  protocolo: string;
  cliente: number;
  nomeCliente?: string;
  tipoDescarte: string;
  tipoDescarteDescricao?: string;
  motivoDescarte?: string;
  destinoFinal?: string;
  empresaDestinoFinal?: string;
  cnpjDestinoFinal?: string;
  certificadoDescarte?: string;
  
  // Campos MTR (Manifesto de Transporte de Resíduos)
  mtrObrigatorio?: boolean;
  mtrNumero?: string;
  mtrEmitidoPor?: string;
  mtrDataEmissao?: Date;
  mtrValidade?: Date;
  mtrArquivo?: string;
  mtrEmpresaTransportadora?: string;
  mtrCnpjTransportadora?: string;
  mtrPlacaVeiculo?: string;
  mtrMotorista?: string;
  mtrCpfMotorista?: string;
  responsavelProtocolo: number;
  nomeResponsavel?: string;
  dataCriacao: Date;
  dataConclusao?: Date;
  status: string;
  statusDescricao?: string;
  valorTotalEstimado?: number;
  documentoGerado: boolean;
  caminhoDocumento?: string;
  observacoes?: string;
  ativo: boolean;
  itens?: ProtocoloDescarteItem[];
  quantidadeEquipamentos?: number;
  quantidadeConcluidos?: number;
  percentualConclusao?: number;
  podeFinalizar?: boolean;
}

export interface ProtocoloDescarteItem {
  id?: number;
  protocoloId: number;
  equipamento: number;
  equipamentoNavigation?: any; // EquipamentoVM
  
  // Indicadores de processos obrigatórios
  processosObrigatorios?: boolean;
  obrigarSanitizacao?: boolean;
  obrigarDescaracterizacao?: boolean;
  obrigarPerfuracaoDisco?: boolean;
  evidenciasObrigatorias?: boolean;
  
  // Indicadores de execução de processos
  processoSanitizacao: boolean;
  processoDescaracterizacao: boolean;
  processoPerfuracaoDisco: boolean;
  evidenciasExecutadas: boolean;
  
  // Campos de sanitização (Política de Sanitização)
  metodoSanitizacao?: string;
  ferramentaUtilizada?: string;
  observacoesSanitizacao?: string;
  
  quantidadeEvidencias?: number;
  evidencias?: DescarteEvidencia[];
  valorEstimado?: number;
  observacoesItem?: string;
  dataProcessoIniciado?: Date;
  dataProcessoConcluido?: Date;
  statusItem: string;
  statusItemDescricao?: string;
  ativo: boolean;
  prontoParaConclusao?: boolean;
}

export interface DescarteEvidencia {
  id?: number;
  equipamento: number;
  protocoloId?: number;
  descricao?: string;
  tipoProcesso: string;
  nomeArquivo: string;
  caminhoArquivo: string;
  tipoArquivo?: string;
  tamanhoArquivo?: number;
  usuarioUpload: number;
  dataUpload: Date;
  ativo: boolean;
}

export interface EquipamentoDisponivel {
  id: number;
  numeroSerie: string;
  patrimonio: string;
  descricao: string;
  fabricante: string;
  modelo: string;
  status: string;
  tipoEquipamento?: string;
  tipoAquisicao?: string;
  jaEstaEmProtocolo: boolean;
  protocoloId?: number;
  numeroProtocolo?: string;
  processosObrigatorios: boolean;
  obrigarSanitizacao: boolean;
  obrigarDescaracterizacao: boolean;
  obrigarPerfuracaoDisco: boolean;
  obrigarEvidencias: boolean;
}

export interface EstatisticasProtocolo {
  protocoloId: number;
  numeroProtocolo: string;
  totalEquipamentos: number;
  equipamentosPendentes: number;
  equipamentosEmProcesso: number;
  equipamentosConcluidos: number;
  percentualConclusao: number;
  totalEvidencias: number;
  valorTotalEstimado?: number;
  podeFinalizar: boolean;
  statusProtocolo: string;
  dataCriacao: Date;
  dataConclusao?: Date;
}

export interface AtualizarProcessoRequest {
  sanitizacao: boolean;
  descaracterizacao: boolean;
  perfuracao: boolean;
  evidencias: boolean;
}

export enum TipoDescarteEnum {
  DOACAO = 'DOACAO',
  VENDA = 'VENDA',
  DEVOLUCAO = 'DEVOLUCAO',
  LOGISTICA_REVERSA = 'LOGISTICA_REVERSA',
  DESCARTE_FINAL = 'DESCARTE_FINAL'
}

export enum StatusProtocoloEnum {
  EM_ANDAMENTO = 'EM_ANDAMENTO',
  CONCLUIDO = 'CONCLUIDO',
  CANCELADO = 'CANCELADO'
}

export enum StatusItemEnum {
  PENDENTE = 'PENDENTE',
  EM_PROCESSO = 'EM_PROCESSO',
  CONCLUIDO = 'CONCLUIDO'
}

export enum MetodoSanitizacaoEnum {
  FORMATACAO_SIMPLES = 'FORMATACAO_SIMPLES',
  SOBREGRAVAR_MIDIA = 'SOBREGRAVAR_MIDIA',
  DESTRUICAO_FISICA = 'DESTRUICAO_FISICA',
  DESMAGNETIZACAO = 'DESMAGNETIZACAO',
  RESTAURACAO_FABRICA = 'RESTAURACAO_FABRICA'
}

export const METODOS_SANITIZACAO_LABELS = {
  [MetodoSanitizacaoEnum.FORMATACAO_SIMPLES]: 'Formatação Simples',
  [MetodoSanitizacaoEnum.SOBREGRAVAR_MIDIA]: 'Sobregravar Mídia (DoD 5220.22-M)',
  [MetodoSanitizacaoEnum.DESTRUICAO_FISICA]: 'Destruição Física',
  [MetodoSanitizacaoEnum.DESMAGNETIZACAO]: 'Desmagnetização',
  [MetodoSanitizacaoEnum.RESTAURACAO_FABRICA]: 'Restauração de Fábrica'
};

export enum MtrEmitidoPorEnum {
  GERADOR = 'GERADOR',
  TRANSPORTADOR = 'TRANSPORTADOR',
  DESTINADOR = 'DESTINADOR'
}

export const MTR_EMITIDO_POR_LABELS = {
  [MtrEmitidoPorEnum.GERADOR]: 'Gerador (Nossa Empresa)',
  [MtrEmitidoPorEnum.TRANSPORTADOR]: 'Empresa Transportadora',
  [MtrEmitidoPorEnum.DESTINADOR]: 'Empresa Destinadora'
};

// ==================== FORNECEDORES ====================
export interface Fornecedor {
  id: number;
  cliente: number;
  nome: string;
  cnpj: string;
  ativo: boolean;
  migrateid?: number;
  destinadorResiduos?: boolean; // NOVO: Campo para identificar destinadores de resíduos
}
