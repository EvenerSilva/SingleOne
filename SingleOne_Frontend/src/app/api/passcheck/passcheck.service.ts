import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PassCheckResponse {
  sucesso: boolean;
  mensagem: string;
  colaborador?: {
    id: number;
    nome: string;
    cpf: string;
    matricula: string;
    cargo: string;
    setor: string;
    empresa: string;
    empresaNome: string;
    centroCusto: string;
    centroCustoNome: string;
    localidade: string;
    localidadeNome: string;
    situacao: string;
    dtAdmissao: string;
    dtDemissao?: string;
    superiorImediato: string;
    superiorImediatoNome: string;
  };
  equipamentos: Array<{
    id: number;
    patrimonio: string;
    numeroSerie: string;
    tipoEquipamento: string;
    tipoEquipamentoTransitoLivre: boolean; // Novo campo para trânsito livre
    fabricante: string;
    modelo: string;
    status: string;
    dtEntrega: string;
    observacao: string;
    tipoAquisicao: string;
  }>;
  statusLiberacao: string;
  motivosPendencia: string[];
}

@Injectable({
  providedIn: 'root'
})
export class PassCheckService {
  private apiUrl = `${environment.apiUrl}/PassCheck`;

  constructor(private http: HttpClient) {}

  /**
   * Consulta colaborador por CPF (acesso público)
   */
  consultarColaborador(cpf: string): Observable<PassCheckResponse> {
    // Limpar CPF (remover pontos e hífens)
    const cpfLimpo = cpf.replace(/[^\d]/g, '');
    
    return this.http.get<PassCheckResponse>(`${this.apiUrl}/consultar/${cpfLimpo}`);
  }

  /**
   * Consulta por QR Code (acesso público)
   */
  consultarPorQRCode(qrCode: string): Observable<PassCheckResponse> {
    return this.http.post<PassCheckResponse>(`${this.apiUrl}/consultar-qr`, qrCode);
  }
}
