import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PatrimonioResponse {
  sucesso: boolean;
  mensagem: string;
  token?: string;
  colaborador?: {
    id: number;
    nome: string;
    cpf: string;
    matricula: string;
    email: string;
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
    superiorImediato: string;
    superiorImediatoNome: string;
  };
  equipamentos?: Array<{
    id: number;
    patrimonio: string;
    numeroSerie: string;
    tipoEquipamento: string;
    tipoEquipamentoTransitoLivre?: boolean; // Novo campo para trânsito livre
    fabricante: string;
    modelo: string;
    tipoAquisicao?: string;
    status: string;
    dtEntrega: string;
    dtDevolucao?: string;
    observacao: string;
    podeContestar: boolean;
    temContestacao: boolean;
    assinado?: boolean;
    dataAssinatura?: Date;
    hashRequisicao?: string;
    isByod?: boolean;
    isHistorico?: boolean;
    isRecursoParticular?: boolean;
  }>;
  contestoes?: Array<{
    id: number;
    equipamentoId: number;
    equipamentoPatrimonio: string;
    motivo: string;
    descricao: string;
    status: string;
    evidenciaUrl: string;
    dataContestacao: string;
    dataResolucao?: string;
    observacaoResolucao: string;
  }>;
}

export interface AuthRequest {
  cpf: string;
  email?: string;
  matricula?: string;
  tipoAutenticacao: 'email' | 'matricula';
}

export interface ContestacaoRequest {
  colaboradorId: number;
  equipamentoId: number;
  motivo: string;
  descricao: string;
  evidenciaUrl?: string;
}

export interface CancelarContestacaoRequest {
  colaboradorId: number;
  equipamentoId: number;
  contestacaoId?: number;
  justificativa?: string;
}

export interface DevolucaoRequest {
  colaboradorId: number;
  equipamentoId: number;
  justificativa: string;
  dataDesejada: string;
}

@Injectable({
  providedIn: 'root'
})
export class PatrimonioService {
  private apiUrl = `${environment.apiUrl}/MeuPatrimonio`;

  constructor(private http: HttpClient) {}

  /**
   * Autentica colaborador para acessar Meu Patrimônio
   * Suporta autenticação por CPF + Email ou CPF + Matrícula
   */
  autenticar(authData: AuthRequest): Observable<PatrimonioResponse> {
    // Limpar CPF (remover pontos e hífens)
    const cpfLimpo = authData.cpf.replace(/[^\d]/g, '');
    const authDataLimpo = { 
      ...authData, 
      cpf: cpfLimpo,
      // Garantir que apenas o campo relevante seja enviado
      email: authData.tipoAutenticacao === 'email' ? authData.email : null,
      matricula: authData.tipoAutenticacao === 'matricula' ? authData.matricula : null
    };
    
    return this.http.post<PatrimonioResponse>(`${this.apiUrl}/autenticar`, authDataLimpo);
  }

  /**
   * Obtém patrimônio do colaborador autenticado
   */
  obterMeuPatrimonio(colaboradorId: number): Observable<PatrimonioResponse> {
    return this.http.get<PatrimonioResponse>(`${this.apiUrl}/meu-patrimonio?colaboradorId=${colaboradorId}`);
  }

  /**
   * Cria nova contestação de patrimônio
   */
  criarContestacao(contestacao: ContestacaoRequest): Observable<{sucesso: boolean; mensagem: string}> {
    return this.http.post<{sucesso: boolean; mensagem: string}>(`${this.apiUrl}/contestacao`, contestacao);
  }

  /**
   * Cancela uma contestação pendente
   */
  cancelarContestacao(payload: CancelarContestacaoRequest): Observable<{sucesso: boolean; mensagem: string}> {
    return this.http.post<{sucesso: boolean; mensagem: string}>(`${this.apiUrl}/contestacao/cancelar`, payload);
  }

  /**
   * Solicita devolução de equipamento
   */
  solicitarDevolucao(devolucao: DevolucaoRequest): Observable<{sucesso: boolean; mensagem: string}> {
    return this.http.post<{sucesso: boolean; mensagem: string}>(`${this.apiUrl}/solicitar-devolucao`, devolucao);
  }

  /**
   * Obtém histórico de movimentações do colaborador
   */
  obterHistorico(): Observable<{sucesso: boolean; historico: any[]}> {
    return this.http.get<{sucesso: boolean; historico: any[]}>(`${this.apiUrl}/historico`);
  }

  /**
   * Gera PDF do termo para um equipamento
   */
  gerarTermoPDF(equipamentoId: number, colaboradorId?: number): Observable<Blob> {
    const url = colaboradorId 
      ? `${this.apiUrl}/termo/pdf/${equipamentoId}?colaboradorId=${colaboradorId}`
      : `${this.apiUrl}/termo/pdf/${equipamentoId}`;
      
    return this.http.get(url, { 
      responseType: 'blob' 
    });
  }

  criarAutoInventario(dados: any): Observable<any> {
    const url = `${this.apiUrl}/auto-inventario`;
    return this.http.post<any>(url, dados, {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    });
  }

  /**
   * Obtém as contestações de um colaborador específico
   */
  obterContestoesColaborador(colaboradorId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/contestoes/${colaboradorId}`);
  }

  /**
   * Libera equipamento para assinatura do termo
   */
  liberarParaAssinatura(equipamentoId: number, colaboradorId: number): Observable<{sucesso: boolean; mensagem: string}> {
    return this.http.post<{sucesso: boolean; mensagem: string}>(`${this.apiUrl}/liberar-assinatura`, {
      equipamentoId: equipamentoId,
      colaboradorId: colaboradorId
    });
  }

  /**
   * Gera novo termo para equipamento
   */
  gerarTermo(equipamentoId: number, colaboradorId: number): Observable<{sucesso: boolean; mensagem: string}> {
    return this.http.post<{sucesso: boolean; mensagem: string}>(`${this.apiUrl}/gerar-termo`, {
      equipamentoId: equipamentoId,
      colaboradorId: colaboradorId
    });
  }

  /**
   * Envia termo por email para o colaborador
   */
  enviarTermoPorEmail(equipamentoId: number, colaboradorId: number): Observable<{sucesso: boolean; mensagem: string}> {
    return this.http.post<{sucesso: boolean; mensagem: string}>(`${this.apiUrl}/enviar-termo-email`, {
      equipamentoId: equipamentoId,
      colaboradorId: colaboradorId
    });
  }
}
