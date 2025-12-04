import { Injectable } from '@angular/core';
import { ConfigApiService } from '../config-api.service';

@Injectable({
  providedIn: 'root'
})
export class CampanhaApiService extends ConfigApiService {
  
  // ==================== CRUD BÁSICO ====================
  
  /**
   * Criar nova campanha de assinaturas
   */
  criarCampanha(dados: any, token: string) {
    return this.instance.post('/campanhaassinatura/CriarCampanha', dados, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CAMPANHA] ❌ Erro:', err);
      console.error('[API-CAMPANHA] Erro Response:', err.response);
      return err.response || { status: 500, data: { mensagem: err.message } };
    });
  }

  /**
   * Obter campanha por ID
   */
  obterCampanhaPorId(id: number, token: string) {
    return this.instance.get(`/CampanhaAssinatura/${id}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Listar campanhas por cliente
   */
  listarCampanhasPorCliente(clienteId: number, status: string | null, token: string) {
    const statusParam = status ? `?status=${status}` : '';
    return this.instance.get(`/CampanhaAssinatura/Cliente/${clienteId}${statusParam}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Atualizar campanha
   */
  atualizarCampanha(campanha: any, token: string) {
    return this.instance.put('/CampanhaAssinatura/Atualizar', campanha, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Inativar campanha
   */
  inativarCampanha(id: number, token: string) {
    return this.instance.put(`/CampanhaAssinatura/Inativar/${id}`, {}, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Concluir campanha
   */
  concluirCampanha(id: number, token: string) {
    return this.instance.put(`/CampanhaAssinatura/Concluir/${id}`, {}, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  // ==================== GERENCIAMENTO DE COLABORADORES ====================

  /**
   * Adicionar colaboradores na campanha
   */
  adicionarColaboradores(campanhaId: number, colaboradoresIds: number[], token: string) {
    return this.instance.post(`/CampanhaAssinatura/${campanhaId}/AdicionarColaboradores`, colaboradoresIds, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Remover colaborador da campanha
   */
  removerColaborador(campanhaId: number, colaboradorId: number, token: string) {
    return this.instance.delete(`/CampanhaAssinatura/${campanhaId}/RemoverColaborador/${colaboradorId}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Obter colaboradores da campanha
   */
  obterColaboradoresDaCampanha(campanhaId: number, statusAssinatura: string | null, token: string) {
    const statusParam = statusAssinatura ? `?statusAssinatura=${statusAssinatura}` : '';
    return this.instance.get(`/CampanhaAssinatura/${campanhaId}/Colaboradores${statusParam}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  // ==================== ENVIO DE TERMOS ====================

  /**
   * Enviar termo para colaborador específico
   */
  enviarTermo(campanhaId: number, colaboradorId: number, dados: any, token: string) {
    return this.instance.post(`/CampanhaAssinatura/${campanhaId}/EnviarTermo/${colaboradorId}`, dados, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Enviar termos em massa
   */
  enviarTermosEmMassa(campanhaId: number, dados: any, token: string) {
    return this.instance.post(`/CampanhaAssinatura/${campanhaId}/EnviarTermosEmMassa`, dados, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Marcar como assinado
   */
  marcarComoAssinado(campanhaId: number, colaboradorId: number, token: string) {
    return this.instance.put(`/CampanhaAssinatura/${campanhaId}/MarcarAssinado/${colaboradorId}`, {}, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Atualizar estatísticas da campanha
   */
  atualizarEstatisticas(campanhaId: number, token: string) {
    return this.instance.post(`/CampanhaAssinatura/${campanhaId}/AtualizarEstatisticas`, {}, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  // ==================== RELATÓRIOS ====================

  /**
   * Obter resumo da campanha
   */
  obterResumoCampanha(id: number, token: string) {
    return this.instance.get(`/CampanhaAssinatura/${id}/Resumo`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Obter resumo de campanhas por cliente
   */
  obterResumosCampanhas(clienteId: number, token: string) {
    return this.instance.get(`/CampanhaAssinatura/Cliente/${clienteId}/Resumos`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Obter relatório de aderência
   */
  obterRelatorioAderencia(id: number, token: string) {
    return this.instance.get(`/CampanhaAssinatura/${id}/RelatorioAderencia`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Obter colaboradores pendentes
   */
  obterColaboradoresPendentes(id: number, token: string) {
    return this.instance.get(`/CampanhaAssinatura/${id}/Pendentes`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  // ==================== HANGFIRE / AGENDAMENTO ====================

  /**
   * Obter estatísticas do Hangfire (para dashboard)
   */
  obterEstatisticasHangfire(token: string) {
    return this.instance.get('/CampanhaAssinatura/Hangfire/Estatisticas', {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CAMPANHA] Erro ao obter estatísticas Hangfire:', err);
      return { status: 500, data: null };
    });
  }

  /**
   * Cancelar job agendado
   */
  cancelarJob(jobId: string, token: string) {
    return this.instance.delete(`/CampanhaAssinatura/Hangfire/Job/${jobId}`, {
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      }
    }).then(res => {
      return res;
    }).catch(err => {
      return err;
    });
  }

  /**
   * Processar campanhas vencidas manualmente (concluir automaticamente)
   */
  processarCampanhasVencidas(clienteId: number, token: string) {
    return this.instance.post('/CampanhaAssinatura/ProcessarVencidas', 
      { ClienteId: clienteId },
      {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + token
        }
      }
    ).then(res => {
      return res;
    }).catch(err => {
      console.error('[API-CAMPANHA] ❌ Erro ao processar campanhas:', err);
      return err.response || { status: 500, data: { mensagem: err.message } };
    });
  }
}

