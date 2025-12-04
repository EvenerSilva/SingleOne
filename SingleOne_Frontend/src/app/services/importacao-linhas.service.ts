import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface ResultadoValidacao {
  loteId: string;
  totalRegistros: number;
  totalValidos: number;
  totalAvisos: number;
  totalErros: number;
  novasOperadoras: number;
  novosContratos: number;
  novosPlanos: number;
  podeImportar: boolean;
  mensagem: string;
}

export interface DetalheLinhaStagingDTO {
  id: number;
  linhaArquivo: number;
  operadoraNome: string;
  contratoNome: string;
  planoNome: string;
  planoValor: number;
  numeroLinha: number;
  iccid: string;
  status: string;
  statusDescricao: string;
  erros: string[];
  avisos: string[];
  criarOperadora: boolean;
  criarContrato: boolean;
  criarPlano: boolean;
}

export interface ResumoValidacao {
  loteId: string;
  total: number;
  validos: number;
  avisos: number;
  erros: number;
  pendentes: number;
  importados: number;
  novasOperadoras: number;
  novosContratos: number;
  novosPlanos: number;
  nomesOperadorasNovas: string[];
  nomesContratosNovos: string[];
  nomesPlanosNovos: string[];
}

export interface ResultadoImportacao {
  loteId: string;
  operadorasCriadas: number;
  contratosCriados: number;
  planosCriados: number;
  linhasCriadas: number;
  totalProcessado: number;
  dataInicio: Date;
  dataFim: Date;
  mensagem: string;
  sucesso: boolean;
}

export interface HistoricoImportacao {
  id: number;
  loteId: string;
  tipoImportacao: string;
  nomeArquivo: string;
  dataInicio: Date;
  dataFim?: Date;
  status: string;
  statusDescricao: string;
  totalRegistros: number;
  totalValidados: number;
  totalErros: number;
  totalImportados: number;
  usuarioNome: string;
  usuarioEmail: string;
  observacoes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ImportacaoLinhasService {
  private baseUrl = `${environment.apiUrl}/ImportacaoLinhas`;

  constructor(private http: HttpClient) { }

  /**
   * Faz upload do arquivo e processa validação
   */
  uploadArquivo(arquivo: File): Observable<ResultadoValidacao> {
    const formData = new FormData();
    formData.append('arquivo', arquivo, arquivo.name);

    return this.http.post<ResultadoValidacao>(`${this.baseUrl}/Upload`, formData);
  }

  /**
   * Obtém detalhes da validação de um lote
   */
  obterValidacao(loteId: string, status?: string): Observable<DetalheLinhaStagingDTO[]> {
    let url = `${this.baseUrl}/Validacao/${loteId}`;
    
    if (status) {
      url += `?status=${status}`;
    }

    return this.http.get<DetalheLinhaStagingDTO[]>(url);
  }

  /**
   * Obtém resumo da validação
   */
  obterResumo(loteId: string): Observable<ResumoValidacao> {
    return this.http.get<ResumoValidacao>(`${this.baseUrl}/Resumo/${loteId}`);
  }

  /**
   * Confirma e efetiva a importação
   */
  confirmarImportacao(loteId: string): Observable<ResultadoImportacao> {
    return this.http.post<ResultadoImportacao>(`${this.baseUrl}/Confirmar/${loteId}`, {});
  }

  /**
   * Cancela importação e limpa staging
   */
  cancelarImportacao(loteId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/Cancelar/${loteId}`);
  }

  /**
   * Obtém histórico de importações
   */
  obterHistorico(limite?: number): Observable<HistoricoImportacao[]> {
    const url = limite 
      ? `${this.baseUrl}/Historico?limite=${limite}`
      : `${this.baseUrl}/Historico`;

    return this.http.get<HistoricoImportacao[]>(url);
  }

  /**
   * Gera URL para download do template
   */
  getUrlTemplate(): string {
    return `${this.baseUrl}/Template`;
  }
}

