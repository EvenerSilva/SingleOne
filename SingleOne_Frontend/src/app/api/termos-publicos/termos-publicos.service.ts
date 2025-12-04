import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TermoValidacaoAuditoriaData {
  ip: string;
  pais: string;
  cidade: string;
  regiao: string;
  latitude?: number;
  longitude?: number;
  precisao?: number;
  dataCaptura: string;
}

export interface TermoValidacaoResponse {
  sucesso: boolean;
  assinado: boolean;
  dataAssinatura?: string;
  entreguePor?: string;
  colaborador?: string;
  hash: string;
  recursos: Array<{ tipo: string; patrimonio: string; numeroSerie: string; tipoEquipamento: string }>;
  mensagem?: string;
  auditoria?: TermoValidacaoAuditoriaData;
}

@Injectable({ providedIn: 'root' })
export class TermosPublicosService {
  private apiUrl = `${environment.apiUrl}/TermosPublicos`;

  constructor(private http: HttpClient) {}

  validar(hash: string): Observable<TermoValidacaoResponse> {
    return this.http.get<TermoValidacaoResponse>(`${this.apiUrl}/validacao/${hash}`);
  }
}

