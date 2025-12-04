import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface ResultadoValidacaoColaboradores {
  loteId: string;
  totalRegistros: number;
  totalValidos: number;
  totalAvisos: number;
  totalErros: number;
  novasEmpresas: number;
  novasLocalidades: number;
  novoscentrosCusto: number;
  novasFiliais: number;
  podeImportar: boolean;
  mensagem: string;
  totalAtualizacoes?: number;
  totalSemAlteracao?: number;
  totalNovos?: number;
  errosCriticos?: ErroValidacaoResumo[];
}

export interface DetalheColaboradorStagingDTO {
  id: number;
  linhaArquivo: number;
  nomeColaborador: string;
  cpf: string;
  matricula: string;
  email: string;
  cargo: string;
  setor: string;
  dataAdmissao: Date;
  tipoColaborador: string;
  dataDemissao?: Date;
  empresaNome: string;
  empresaCnpj: string;
  localidadeDescricao: string;
  localidadeCidade: string;
  localidadeEstado: string;
  centroCustoCodigo: string;
  centroCustoNome: string;
  filialNome?: string;
  status: string;
  statusDescricao: string;
  erros: string[];
  avisos: string[];
  criarEmpresa: boolean;
  criarLocalidade: boolean;
  criarCentroCusto: boolean;
  criarFilial: boolean;
}

export interface ResumoValidacaoColaboradores {
  loteId: string;
  total: number;
  validos: number;
  avisos: number;
  erros: number;
  pendentes: number;
  importados: number;
  novasEmpresas: number;
  novasLocalidades: number;
  novosCentrosCusto: number;
  novasFiliais: number;
  nomesEmpresasNovas: string[];
  nomesLocalidadesNovas: string[];
  nomesCentrosCustoNovos: string[];
  nomesFiliaisNovas: string[];
  totalAtualizacoes: number;
  totalSemAlteracao: number;
  totalNovos: number;
}

export interface ResultadoImportacaoColaboradores {
  loteId: string;
  empresasCriadas: number;
  localidadesCriadas: number;
  centrosCustoCriados: number;
  filiaisCriadas: number;
  colaboradoresCriados: number;
  colaboradoresAtualizados: number;
  colaboradoresSemAlteracao: number;
  totalProcessado: number;
  dataInicio: Date;
  dataFim: Date;
  mensagem: string;
  sucesso: boolean;
}

export interface ErroValidacaoResumo {
  linha: number;
  nome: string;
  cpf: string;
  matricula: string;
  mensagens: string[];
}

export interface HistoricoImportacaoColaboradores {
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
  totalImportados: number | null;
  usuarioNome: string;
  usuarioEmail: string;
  observacoes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ImportacaoColaboradoresService {
  private baseUrl = `${environment.apiUrl}/ImportacaoColaboradores`;

  constructor(private http: HttpClient) { }

  /**
   * Faz upload do arquivo e processa validação
   */
  uploadArquivo(arquivo: File): Observable<ResultadoValidacaoColaboradores> {
    const formData = new FormData();
    formData.append('arquivo', arquivo, arquivo.name);

    return this.http.post<ResultadoValidacaoColaboradores>(`${this.baseUrl}/Upload`, formData);
  }

  /**
   * Obtém detalhes da validação de um lote
   */
  obterValidacao(loteId: string, status?: string): Observable<DetalheColaboradorStagingDTO[]> {
    let url = `${this.baseUrl}/Validacao/${loteId}`;
    
    if (status) {
      url += `?status=${status}`;
    }

    return this.http.get<DetalheColaboradorStagingDTO[]>(url);
  }

  /**
   * Obtém resumo da validação
   */
  obterResumo(loteId: string): Observable<ResumoValidacaoColaboradores> {
    return this.http.get<ResumoValidacaoColaboradores>(`${this.baseUrl}/Resumo/${loteId}`);
  }

  /**
   * Confirma e efetiva a importação
   */
  confirmarImportacao(loteId: string): Observable<ResultadoImportacaoColaboradores> {
    return this.http.post<ResultadoImportacaoColaboradores>(`${this.baseUrl}/Confirmar/${loteId}`, {});
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
  obterHistorico(limite?: number): Observable<HistoricoImportacaoColaboradores[]> {
    const url = limite 
      ? `${this.baseUrl}/Historico?limite=${limite}`
      : `${this.baseUrl}/Historico`;

    return this.http.get<HistoricoImportacaoColaboradores[]>(url);
  }

  /**
   * Gera URL para download do template
   */
  getUrlTemplate(): string {
    return `${this.baseUrl}/Template`;
  }

  getUrlErros(loteId: string): string {
    return `${this.baseUrl}/Erros/${loteId}/csv`;
  }

  baixarErros(url: string, token: string): Observable<Blob> {
    const headers = { Authorization: `Bearer ${token}` };
    return this.http.get(url, { headers, responseType: 'blob' });
  }
}

