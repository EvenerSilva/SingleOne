export interface RequisicaoItem {
  id: number;
  equipamento: number | null;
  linhatelefonica?: number;
  usuarioentrega?: number;
  usuariodevolucao?: number;
  dtentrega?: Date;
  dtdevolucao?: Date;
  observacaoentrega?: string;
  dtprogramadaretorno?: Date;
  equipamentoNavigation?: any; // ✅ ADICIONADO: Para armazenar dados do equipamento
  linhaTelefonicaNavigation?: any; // ✅ ADICIONADO: Para armazenar dados da linha telefônica
}

export interface Requisicao {
  id: number;
  cliente: number;
  usuariorequisicao: number;
  tecnicoresponsavel: number;
  requisicaostatus: number;
  colaboradorfinal?: number;
  dtsolicitacao?: Date;
  dtprocessamento?: Date;
  assinaturaeletronica: boolean;
  dtassinaturaeletronica?: Date;
  dtenviotermo?: Date;
  hashrequisicao?: string;
  migrateid?: number;
  requisicoesitens: RequisicaoItem[];
  tiporecurso: number; // 0 = Equipamento, 1 = Linha Telefônica
}
