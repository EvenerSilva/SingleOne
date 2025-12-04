import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ElegibilidadeApiService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  /**
   * Lista todas as políticas de elegibilidade de um cliente
   */
  listarPoliticas(clienteId: number, token: string, tipoColaborador?: string, tipoEquipamentoId?: number): Promise<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });

    let url = `${this.apiUrl}/Configuracoes/ListarPoliticasElegibilidade/${clienteId}`;
    const params: string[] = [];

    if (tipoColaborador) {
      params.push(`tipoColaborador=${encodeURIComponent(tipoColaborador)}`);
    }

    if (tipoEquipamentoId) {
      params.push(`tipoEquipamentoId=${tipoEquipamentoId}`);
    }

    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }

    return this.http.get(url, { headers }).toPromise()
      .then((res: any) => {
        return { status: 200, data: res };
      })
      .catch((error: any) => {
        console.error('[ELEGIBILIDADE API] Erro ao listar políticas:', error);
        return { status: error.status || 500, mensagem: error.error?.Mensagem || error.message };
      });
  }

  /**
   * Busca uma política específica por ID
   */
  buscarPolitica(id: number, token: string): Promise<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });

    return this.http.get(`${this.apiUrl}/Configuracoes/BuscarPoliticaElegibilidade/${id}`, { headers }).toPromise()
      .then((res: any) => {
        return { status: 200, data: res };
      })
      .catch((error: any) => {
        console.error('[ELEGIBILIDADE API] Erro ao buscar política:', error);
        return { status: error.status || 500, mensagem: error.error?.Mensagem || error.message };
      });
  }

  /**
   * Salva (cria ou atualiza) uma política de elegibilidade
   */
  salvarPolitica(politica: any, token: string): Promise<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
    return this.http.post(`${this.apiUrl}/Configuracoes/SalvarPoliticaElegibilidade`, politica, { headers }).toPromise()
      .then((res: any) => {
        return { status: 200, data: res };
      })
      .catch((error: any) => {
        console.error('[ELEGIBILIDADE API] Erro ao salvar política:', error);
        return { status: error.status || 500, mensagem: error.error?.Mensagem || error.message };
      });
  }

  /**
   * Exclui (inativa) uma política de elegibilidade
   */
  excluirPolitica(id: number, token: string): Promise<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });

    return this.http.delete(`${this.apiUrl}/Configuracoes/ExcluirPoliticaElegibilidade/${id}`, { headers }).toPromise()
      .then((res: any) => {
        return { status: 200, data: res };
      })
      .catch((error: any) => {
        console.error('[ELEGIBILIDADE API] Erro ao excluir política:', error);
        return { status: error.status || 500, mensagem: error.error?.Mensagem || error.message };
      });
  }

  /**
   * Verifica se um colaborador é elegível para um tipo de equipamento
   */
  verificarElegibilidade(colaboradorId: number, tipoEquipamentoId: number, token: string): Promise<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });

    return this.http.get(`${this.apiUrl}/Configuracoes/VerificarElegibilidade/${colaboradorId}/${tipoEquipamentoId}`, { headers }).toPromise()
      .then((res: any) => {
        return { status: 200, data: res };
      })
      .catch((error: any) => {
        console.error('[ELEGIBILIDADE API] Erro ao verificar elegibilidade:', error);
        return { status: error.status || 500, mensagem: error.error?.Mensagem || error.message };
      });
  }

  /**
   * Lista os tipos de colaborador disponíveis
   */
  listarTiposColaborador(token: string): Promise<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });

    return this.http.get(`${this.apiUrl}/Configuracoes/ListarTiposColaborador`, { headers }).toPromise()
      .then((res: any) => {
        return { status: 200, data: res };
      })
      .catch((error: any) => {
        console.error('[ELEGIBILIDADE API] Erro ao listar tipos de colaborador:', error);
        return { status: error.status || 500, mensagem: error.error?.Mensagem || error.message };
      });
  }

  /**
   * Consulta relatório de não conformidade de elegibilidade
   */
  consultarNaoConformidade(filtros: any, token: string): Promise<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
    return this.http.post(`${this.apiUrl}/Relatorio/ConsultarNaoConformidadeElegibilidade`, filtros, { headers }).toPromise()
      .then((res: any) => {
        return { status: 200, data: res.Data || res };
      })
      .catch((error: any) => {
        console.error('[ELEGIBILIDADE API] Erro ao consultar não conformidades:', error);
        return { status: error.status || 500, mensagem: error.error?.Mensagem || error.message };
      });
  }
}

