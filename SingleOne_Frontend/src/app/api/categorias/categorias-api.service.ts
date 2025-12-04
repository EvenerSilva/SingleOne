import { Injectable } from '@angular/core';
import { AxiosInstance } from 'axios';
import { ConfigApiService } from '../config-api.service';

export interface Categoria {
  id: number;
  nome: string;
  descricao: string;
  ativo: boolean;
  dataCriacao: string;
  dataAtualizacao: string;
  totalTiposEquipamento: number;
}

export interface CategoriaCreate {
  nome: string;
  descricao: string;
}

export interface CategoriaUpdate {
  id: number;
  nome: string;
  descricao: string;
  ativo: boolean;
}

export interface CategoriaResponse {
  sucesso: boolean;
  mensagem: string;
  dados: Categoria;
  status: number;
}

export interface CategoriaListResponse {
  sucesso: boolean;
  mensagem: string;
  dados: Categoria[];
  status: number;
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class CategoriasApiService {

  private instance: AxiosInstance;

  constructor(private configuracoesApiService: ConfigApiService) {
    this.instance = this.configuracoesApiService.getInstance();
  }

  /**
   * Lista todas as categorias com filtro opcional
   */
  listarCategorias(filtro?: string) {
    const params = filtro ? { filtro } : {};
    
    return this.instance.get('/categorias', { params })
      .then(res => {
        return res;
      })
      .catch(err => {
        console.error('[API-CATEGORIAS] Erro ao listar categorias:', err);
        throw err;
      });
  }

  /**
   * Busca uma categoria específica por ID
   */
  buscarCategoriaPorId(id: number) {
    return this.instance.get(`/categorias/${id}`)
      .then(res => {
        return res;
      })
      .catch(err => {
        console.error('[API-CATEGORIAS] Erro ao buscar categoria:', err);
        throw err;
      });
  }

  /**
   * Cria uma nova categoria
   */
  criarCategoria(categoria: CategoriaCreate) {
    return this.instance.post('/categorias', categoria)
      .then(res => {
        return res;
      })
      .catch(err => {
        console.error('[API-CATEGORIAS] Erro ao criar categoria:', err);
        throw err;
      });
  }

  /**
   * Atualiza uma categoria existente
   */
  atualizarCategoria(categoria: CategoriaUpdate) {
    return this.instance.put(`/categorias/${categoria.id}`, categoria)
      .then(res => {
        return res;
      })
      .catch(err => {
        console.error('[API-CATEGORIAS] Erro ao atualizar categoria:', err);
        throw err;
      });
  }

  /**
   * Desativa uma categoria
   */
  desativarCategoria(id: number) {
    return this.instance.delete(`/categorias/${id}`)
      .then(res => {
        return res;
      })
      .catch(err => {
        console.error('[API-CATEGORIAS] Erro ao desativar categoria:', err);
        throw err;
      });
  }

  /**
   * Reativa uma categoria desativada
   */
  reativarCategoria(id: number) {
    return this.instance.patch(`/categorias/${id}/reativar`)
      .then(res => {
        return res;
      })
      .catch(err => {
        console.error('[API-CATEGORIAS] Erro ao reativar categoria:', err);
        throw err;
      });
  }

  /**
   * Verifica se um nome de categoria já existe
   */
  verificarNomeExistente(nome: string, idExcluir?: number) {
    const params: any = { nome };
    if (idExcluir) {
      params.idExcluir = idExcluir;
    }
    
    return this.instance.get('/categorias/verificar-nome', { params })
      .then(res => {
        return res;
      })
      .catch(err => {
        console.error('[API-CATEGORIAS] Erro ao verificar nome:', err);
        throw err;
      });
  }
}
