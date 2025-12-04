import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';
import { Contestacao } from 'src/app/models/contestacao.interface';

@Injectable({
  providedIn: 'root'
})
export class ContestacaoApiService extends ConfigApiService {
  
  constructor() {
    super(null as any, null as any);
  }

  /**
   * Lista todas as contestações com filtros opcionais
   */
  listarContestacoes(filtro: string, cliente: number, pagina: number, token: string): Promise<any> {
    const filtroTratado = filtro === '' || filtro === null || filtro === undefined ? 'null' : filtro;
    let url;
    if (filtroTratado === 'null') {
      url = `/contestacoes/ListarContestacoes/null/${cliente}/${pagina}`;
    } else {
      url = `/contestacoes/ListarContestacoes/${filtroTratado}/${cliente}/${pagina}`;
    }
    return this.instance.get(url, { 
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[CONTESTACAO-API] Erro na requisição:', err);
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    });
  }

  /**
   * Obtém uma contestação específica por ID
   */
  obterContestacao(id: number, token: string): Promise<any> {
    return this.instance.get(`/contestacoes/${id}`, { 
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[CONTESTACAO-API] Erro ao obter contestação:', err);
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    });
  }

  /**
   * Cria uma nova contestação
   */
  criarContestacao(contestacao: Partial<Contestacao>, token: string): Promise<any> {
    return this.instance.post('/contestacoes', contestacao, { 
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[CONTESTACAO-API] Erro ao criar contestação:', err);
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    });
  }

  /**
   * Atualiza uma contestação existente
   */
  atualizarContestacao(id: number, contestacao: Partial<Contestacao>, token: string): Promise<any> {
    return this.instance.put(`/contestacoes/${id}`, contestacao, { 
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[CONTESTACAO-API] Erro ao atualizar contestação:', err);
      console.error('[CONTESTACAO-API] Resposta do servidor:', err.response?.data);
      console.error('[CONTESTACAO-API] Status code:', err.response?.status);
      console.error('[CONTESTACAO-API] Erro completo:', err);
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    });
  }

  /**
   * Exclui uma contestação
   */
  excluirContestacao(id: number, token: string): Promise<any> {
    return this.instance.delete(`/contestacoes/${id}`, { 
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[CONTESTACAO-API] Erro ao excluir contestação:', err);
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    });
  }

  /**
   * 📝 Cria registro de inventário forçado
   * Usado quando TI identifica colaboradores sem recursos e precisa fazer levantamento
   */
  criarInventarioForcado(payload: {
    colaboradorId: number,
    colaboradorIds?: number[], // Para criar múltiplos de uma vez
    motivo: string,
    descricao: string,
    usuarioId: number,
    clienteId: number
  }, token: string): Promise<any> {
    return this.instance.post('/contestacoes/CriarInventarioForcado', payload, { 
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    }).then(res => {
      return {
        status: res.status,
        data: res.data,
        headers: res.headers
      };
    }).catch(err => {
      console.error('[CONTESTACAO-API] ❌ Erro ao criar inventário forçado:', err);
      return {
        status: err.response?.status || 500,
        data: err.response?.data || { error: 'Erro desconhecido' },
        error: true
      };
    });
  }
}