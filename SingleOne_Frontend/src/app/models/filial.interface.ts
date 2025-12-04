export interface Filial {
  id: number;
  nome: string;
  empresaId: number;
  localidadeId: number;
  cnpj?: string;
  endereco?: string;
  telefone?: string;
  email?: string;
  ativo: boolean;
  createdAt?: Date;
  updatedAt?: Date;
  
  // Propriedades de navegação
  empresa?: any;
  localidade?: any;
}

export interface FilialForm {
  id: number;
  nome: string;
  empresaId: number;
  localidadeId: number;
  cnpj: string;
  endereco: string;
  telefone: string;
  email: string;
  ativo?: boolean;
}
