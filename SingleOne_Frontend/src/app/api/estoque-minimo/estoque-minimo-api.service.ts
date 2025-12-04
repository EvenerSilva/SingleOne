import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { ConfigApiService } from '../config-api.service';
import { UtilService } from '../../util/util.service';

export interface EstoqueMinimoEquipamento {
  id: number;
  cliente: number;
  modelo: number;
  localidade: number;
  quantidadeMinima: number;
  quantidadeMaxima: number;
  quantidadeTotalLancada: number; // Calculado dinamicamente
  estoqueAtual: number; // Calculado dinamicamente
  ativo: boolean;
  dtCriacao: string;
  usuarioCriacao: number;
  dtAtualizacao?: string;
  usuarioAtualizacao?: number;
  observacoes?: string;
  
  // Campos calculados
  percentualUtilizacao: number;
  statusEstoque: string;
  quantidadeFaltante: number;
  quantidadeExcesso: number;
  
  // Informações de navegação (para exibição)
  modeloDescricao?: string;
  fabricanteDescricao?: string;
  tipoEquipamentoDescricao?: string;
  localidadeDescricao?: string;
  
  // Navegação (mantido para compatibilidade)
  modeloNavigation?: {
    id: number;
    descricao: string;
    fabricanteNavigation?: {
      id: number;
      descricao: string;
    };
  };
  localidadeNavigation?: {
    id: number;
    descricao: string;
  };
}

export interface EstoqueMinimoLinha {
  id: number;
  cliente: number;
  operadora: number;
  plano: number;
  localidade: number;
  quantidadeMinima: number;
  quantidadeMaxima?: number;
  perfilUso?: string;
  observacoes?: string;
  ativo: boolean;
  dtCriacao: string;
  usuarioCriacao: number;
  dtAtualizacao?: string;
  usuarioAtualizacao?: number;
  
  // Navegação
  operadoraNavigation?: {
    id: number;
    nome: string;
  };
  planoNavigation?: {
    id: number;
    nome: string;
    contratoNavigation?: {
      id: number;
      nome: string;
    };
  };
  localidadeNavigation?: {
    id: number;
    descricao: string;
  };
}

export interface EstoqueAlerta {
  tipo: string;
  cliente: number;
  localidade: string;
  descricao: string;
  estoqueAtual: number;
  estoqueMinimo: number;
  estoqueMaximo: number;
  totalLancado: number;
  quantidadeFaltante: number;
  quantidadeExcesso: number;
  percentualUtilizacao: number;
  status: string;
  statusEstoque: string; // Campo correto retornado pelo backend
  prioridade?: string;
  observacoes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EstoqueMinimoApiService extends ConfigApiService {

  constructor(
    route: Router,
    util: UtilService
  ) {
    super(route, util);
  }

  // =====================================================
  // MÉTODOS PARA EQUIPAMENTOS
  // =====================================================

  listarEquipamentos(clienteId: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/equipamentos/${clienteId}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

buscarEquipamento(id: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/equipamentos/buscar/${id}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  salvarEquipamento(estoqueMinimo: EstoqueMinimoEquipamento): Promise<any> {
    return this.instance.post('/EstoqueMinimo/equipamentos', estoqueMinimo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  excluirEquipamento(id: number): Promise<any> {
    return this.instance.delete(`/EstoqueMinimo/equipamentos/${id}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  cadastrarEquipamento(estoqueMinimo: any): Promise<any> {
    const dados = {
      id: 0, // Novo registro
      cliente: estoqueMinimo.clienteId,
      modelo: estoqueMinimo.modeloId,
      localidade: estoqueMinimo.localidadeId,
      quantidadeMinima: estoqueMinimo.estoqueMinimo,
      quantidadeMaxima: estoqueMinimo.estoqueMaximo,
      quantidadeTotalLancada: 0,
      ativo: true,
      dtCriacao: new Date().toISOString(),
      usuarioCriacao: estoqueMinimo.usuarioId || 1,
      observacoes: estoqueMinimo.observacoes || null
    };
    return this.instance.post('/EstoqueMinimo/equipamentos', dados, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + estoqueMinimo.token
    }});
  }

  atualizarEquipamento(id: number, estoqueMinimo: any): Promise<any> {
    const dados = {
      id: id,
      cliente: estoqueMinimo.clienteId,
      modelo: estoqueMinimo.modeloId,
      localidade: estoqueMinimo.localidadeId,
      quantidadeMinima: estoqueMinimo.estoqueMinimo,
      quantidadeMaxima: estoqueMinimo.estoqueMaximo,
      quantidadeTotalLancada: estoqueMinimo.quantidadeTotalLancada || 0,
      ativo: true,
      dtAtualizacao: new Date().toISOString(),
      usuarioAtualizacao: estoqueMinimo.usuarioId || 1,
      observacoes: estoqueMinimo.observacoes || null
    };
    return this.instance.post('/EstoqueMinimo/equipamentos', dados, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + estoqueMinimo.token
    }});
  }

  // =====================================================
  // MÉTODOS PARA LINHAS TELEFÔNICAS
  // =====================================================

  listarLinhas(clienteId: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/linhas/${clienteId}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  buscarLinha(id: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/linhas/buscar/${id}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  salvarLinha(estoqueMinimo: EstoqueMinimoLinha): Promise<any> {
    return this.instance.post('/EstoqueMinimo/linhas', estoqueMinimo, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  excluirLinha(id: number): Promise<any> {
    return this.instance.delete(`/EstoqueMinimo/linhas/${id}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  // =====================================================
  // MÉTODOS PARA RELATÓRIOS E ALERTAS
  // =====================================================

  listarAlertas(clienteId: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/alertas/${clienteId}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  listarAlertasEquipamentos(clienteId: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/alertas/equipamentos/${clienteId}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  listarAlertasLinhas(clienteId: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/alertas/linhas/${clienteId}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }

  contarAlertas(clienteId: number): Promise<any> {
    return this.instance.get(`/EstoqueMinimo/alertas/contar/${clienteId}`, { headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer ' + this.util.getSession('usuario').token
    }});
  }
}
