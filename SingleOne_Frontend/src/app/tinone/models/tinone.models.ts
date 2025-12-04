/**
 * Modelos do TinOne (TypeScript)
 */

export interface TinOneConfig {
  habilitado: boolean;
  chatHabilitado: boolean;
  tooltipsHabilitado: boolean;
  guiasHabilitado: boolean;
  sugestoesProativas: boolean;
  iaHabilitada: boolean;
  analytics: boolean;
  debugMode: boolean;
  posicao: string;
  corPrimaria: string;
}

export interface TinOnePergunta {
  pergunta: string;
  paginaContexto?: string;
  sessaoId?: string;
  usuarioId?: number;
  clienteId?: number;
}

export interface TinOneResposta {
  resposta: string;
  tipo: 'texto' | 'guia' | 'navegacao' | 'erro';
  dados?: any;
  sucesso: boolean;
  erroMensagem?: string;
}

export interface TinOneCampoInfo {
  campoId: string;
  nome: string;
  descricao: string;
  exemplo?: string;
  tipo?: string;
  obrigatorio: boolean;
  formato?: string;
  regraNegocio?: string;
  dicas?: string[];
}

export interface TinOneProcesso {
  processoId: string;
  nome: string;
  descricao: string;
  passos: TinOnePasso[];
}

export interface TinOnePasso {
  id: number;
  titulo: string;
  descricao: string;
  rota?: string;
  acao?: string;
  elementoDestaque?: string;
  dica?: string;
}

export interface TinOneFeedback {
  analyticsId?: number;
  foiUtil: boolean;
  comentario?: string;
}

export interface TinOneMensagem {
  tipo: 'usuario' | 'assistente';
  texto: string;
  timestamp: Date;
  dados?: any;
}

