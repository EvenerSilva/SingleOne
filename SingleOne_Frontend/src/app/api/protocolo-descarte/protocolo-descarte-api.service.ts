import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { 
  ProtocoloDescarte, 
  ProtocoloDescarteItem, 
  EquipamentoDisponivel, 
  EstatisticasProtocolo,
  AtualizarProcessoRequest,
  Fornecedor
} from '../../models/protocolo-descarte.model';

@Injectable({
  providedIn: 'root'
})
export class ProtocoloDescarteApiService {
  private baseUrl = `${environment.apiUrl}/ProtocoloDescarte`;

  constructor(private http: HttpClient) { }

  private getHeaders(token: string): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  /**
   * Listar protocolos de descarte de um cliente
   */
  listarProtocolos(clienteId: number, incluirInativos: boolean = false): Observable<ProtocoloDescarte[]> {
    return this.http.get<ProtocoloDescarte[]>(`${this.baseUrl}/listar/${clienteId}?incluirInativos=${incluirInativos}`);
  }

  /**
   * Obter protocolo específico por ID
   */
  obterProtocolo(protocoloId: number): Observable<ProtocoloDescarte> {
    return this.http.get<ProtocoloDescarte>(`${this.baseUrl}/${protocoloId}`);
  }

  /**
   * Criar novo protocolo de descarte
   */
  criarProtocolo(protocolo: ProtocoloDescarte, token: string): Observable<ProtocoloDescarte> {
    return this.http.post<ProtocoloDescarte>(`${this.baseUrl}`, protocolo, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Atualizar protocolo existente
   */
  atualizarProtocolo(protocoloId: number, protocolo: ProtocoloDescarte, token: string): Observable<ProtocoloDescarte> {
    return this.http.put<ProtocoloDescarte>(`${this.baseUrl}/${protocoloId}`, protocolo, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Adicionar equipamento ao protocolo
   */
  adicionarEquipamento(protocoloId: number, equipamentoId: number, token: string): Observable<ProtocoloDescarteItem> {
    return this.http.post<ProtocoloDescarteItem>(`${this.baseUrl}/${protocoloId}/equipamentos/${equipamentoId}`, {}, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Remover equipamento do protocolo
   */
  removerEquipamento(protocoloId: number, equipamentoId: number, token: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${protocoloId}/equipamentos/${equipamentoId}`, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Atualizar processo de um equipamento no protocolo
   */
  atualizarProcessoEquipamento(itemId: number, request: AtualizarProcessoRequest, token: string): Observable<ProtocoloDescarteItem> {
    return this.http.put<ProtocoloDescarteItem>(`${this.baseUrl}/itens/${itemId}/processo`, request, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Atualizar um processo específico de um item
   */
  atualizarProcessoItem(itemId: number, processo: string, valor: boolean, token: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/itens/${itemId}/processo/${processo}`, 
      { valor }, 
      { headers: this.getHeaders(token) }
    );
  }

  /**
   * Atualizar campo específico de um item (método sanitização, ferramenta, etc)
   */
  atualizarCampoItem(itemId: number, campo: string, valor: any, token: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/itens/${itemId}/campo/${campo}`, 
      { valor }, 
      { headers: this.getHeaders(token) }
    );
  }

  /**
   * Finalizar protocolo
   */
  finalizarProtocolo(protocoloId: number, token: string): Observable<ProtocoloDescarte> {
    return this.http.post<ProtocoloDescarte>(`${this.baseUrl}/${protocoloId}/finalizar`, {}, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Cancelar protocolo
   */
  cancelarProtocolo(protocoloId: number, token: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/${protocoloId}/cancelar`, {}, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Listar equipamentos disponíveis para adicionar ao protocolo
   */
  listarEquipamentosDisponiveis(clienteId: number, filtro: string = '', token: string): Observable<EquipamentoDisponivel[]> {
    return this.http.get<EquipamentoDisponivel[]>(`${this.baseUrl}/equipamentos-disponiveis/${clienteId}?filtro=${filtro}`, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Obter estatísticas do protocolo
   */
  obterEstatisticas(protocoloId: number, token: string): Observable<EstatisticasProtocolo> {
    return this.http.get<EstatisticasProtocolo>(`${this.baseUrl}/${protocoloId}/estatisticas`, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Validar se protocolo pode ser finalizado
   */
  validarFinalizacao(protocoloId: number, token: string): Observable<{podeFinalizar: boolean}> {
    return this.http.get<{podeFinalizar: boolean}>(`${this.baseUrl}/${protocoloId}/validar-finalizacao`, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Gerar número de protocolo único
   */
  gerarNumeroProtocolo(token: string): Observable<{protocolo: string}> {
    return this.http.get<{protocolo: string}>(`${this.baseUrl}/gerar-numero-protocolo`, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Gerar documento PDF de descarte
   */
  gerarDocumentoDescarte(protocoloId: number, token: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${protocoloId}/documento`, {
      headers: this.getHeaders(token),
      responseType: 'blob'
    });
  }

  /**
   * Buscar fornecedores destinadores de resíduos
   */
  buscarFornecedoresDestinadores(clienteId: number, token: string): Observable<Fornecedor[]> {
    return this.http.get<Fornecedor[]>(`${environment.apiUrl}/Configuracoes/ListarFornecedoresDestinadores/${clienteId}`, {
      headers: this.getHeaders(token)
    });
  }

  /**
   * Métodos auxiliares para integração com sistema existente
   */
  
  /**
   * Converter equipamento do sistema atual para formato de protocolo
   */
  converterEquipamentoParaProtocolo(equipamento: any): ProtocoloDescarteItem {
    return {
      protocoloId: 0, // Será definido ao adicionar ao protocolo
      equipamento: equipamento.equipamento?.id || equipamento.id,
      equipamentoNavigation: equipamento.equipamento || equipamento,
      processoSanitizacao: equipamento.sanitizacaoExecutada || false,
      processoDescaracterizacao: equipamento.descaracterizacaoExecutada || false,
      processoPerfuracaoDisco: equipamento.perfuracaoDiscoExecutada || false,
      evidenciasObrigatorias: equipamento.obrigarEvidencias || false,
      evidenciasExecutadas: equipamento.evidenciasExecutadas || false,
      quantidadeEvidencias: equipamento.evidencias?.length || 0,
      evidencias: equipamento.evidencias || [],
      valorEstimado: equipamento.valorEstimado,
      observacoesItem: equipamento.observacoesItem,
      statusItem: 'PENDENTE',
      ativo: true
    };
  }

  /**
   * Validar se todos os processos obrigatórios foram executados
   */
  validarProcessosObrigatorios(item: ProtocoloDescarteItem): {valido: boolean, processosFaltando: string[]} {
    const processosFaltando: string[] = [];
    
    // Se não exige evidências: validar apenas checkboxes
    if (!item.evidenciasObrigatorias) {
      if (!item.processoSanitizacao) {
        processosFaltando.push('Sanitização');
      }
      if (!item.processoDescaracterizacao) {
        processosFaltando.push('Descaracterização');
      }
      if (!item.processoPerfuracaoDisco) {
        processosFaltando.push('Perfuração de Disco');
      }
    } else {
      // Se exige evidências: validar se tem evidências de cada processo
      const evidenciasSanitizacao = item.evidencias?.filter(e => e.tipoProcesso === 'SANITIZACAO').length || 0;
      const evidenciasDescaracterizacao = item.evidencias?.filter(e => e.tipoProcesso === 'DESCARACTERIZACAO').length || 0;
      const evidenciasPerfuracao = item.evidencias?.filter(e => e.tipoProcesso === 'PERFURACAO_DISCO').length || 0;
      
      if (evidenciasSanitizacao === 0) {
        processosFaltando.push('Evidência de Sanitização');
      }
      if (evidenciasDescaracterizacao === 0) {
        processosFaltando.push('Evidência de Descaracterização');
      }
      if (evidenciasPerfuracao === 0) {
        processosFaltando.push('Evidência de Perfuração de Disco');
      }
    }
    
    return {
      valido: processosFaltando.length === 0,
      processosFaltando
    };
  }
}
