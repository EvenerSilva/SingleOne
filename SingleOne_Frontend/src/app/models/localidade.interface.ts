export interface Localidade {
  id: number;
  cliente: number;
  descricao: string;
  ativo: boolean;
  migrateid?: string;
  cidade?: string;      // Campo novo para cidade
  estado?: string;      // Campo novo para estado
  createdAt?: string;
  updatedAt?: string;
}

export interface LocalidadeForm {
  id?: number;
  cliente: number;
  descricao: string;
  ativo: boolean;
  migrateid?: string;
  cidade?: string;      // Campo novo para cidade
  estado?: string;      // Campo novo para estado
}
