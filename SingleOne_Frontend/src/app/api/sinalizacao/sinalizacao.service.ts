import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface MotivoSuspeita {
  id: number;
  codigo: string;
  descricao: string;
  descricaoDetalhada?: string;
  prioridadePadrao: string;
  ativo: boolean;
}

export interface CriarSinalizacaoRequest {
  colaboradorId: number;
  cpfConsultado: string;
  motivoSuspeita: string;
  prioridade: string;
  descricaoDetalhada: string;
  observacoesVigilante?: string;
  dadosConsulta?: string;
  evidenciaUrls?: string[];
}

export interface SinalizacaoResponse {
  sucesso: boolean;
  mensagem: string;
  sinalizacaoId?: number;
  numeroProtocolo?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SinalizacaoService {
  private apiUrl = `${environment.apiUrl}/PassCheck`;

  constructor(private http: HttpClient) { }

  /**
   * Obter motivos de suspeita disponíveis
   */
  obterMotivosSuspeita(): Observable<MotivoSuspeita[]> {
    return this.http.get<MotivoSuspeita[]>(`${this.apiUrl}/motivos-suspeita`);
  }

  /**
   * Criar nova sinalização de suspeita
   */
  criarSinalizacao(dados: CriarSinalizacaoRequest): Observable<SinalizacaoResponse> {
    return this.http.post<SinalizacaoResponse>(`${this.apiUrl}/sinalizar-suspeita`, dados);
  }
}
