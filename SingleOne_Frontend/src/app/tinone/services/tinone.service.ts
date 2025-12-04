import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import {
  TinOnePergunta,
  TinOneResposta,
  TinOneCampoInfo,
  TinOneProcesso,
  TinOneFeedback,
  TinOneMensagem
} from '../models/tinone.models';
import { TinOneContextService } from './tinone-context.service';
import { environment } from 'src/environments/environment';

/**
 * Serviço principal do TinOne
 * Gerencia comunicação com backend e estado do chat
 */
@Injectable({
  providedIn: 'root'
})
export class TinOneService {
  private apiUrl = `${environment.apiUrl}/tinone`;
  
  // Estado do chat
  private mensagensSubject = new BehaviorSubject<TinOneMensagem[]>([]);
  public mensagens$ = this.mensagensSubject.asObservable();

  private isLoadingSubject = new BehaviorSubject<boolean>(false);
  public isLoading$ = this.isLoadingSubject.asObservable();

  constructor(
    private http: HttpClient,
    private contextService: TinOneContextService
  ) {
    this.inicializarChat();
  }

  /**
   * Inicializa chat com mensagem de boas-vindas
   */
  private inicializarChat(): void {
    const mensagemBoasVindas: TinOneMensagem = {
      tipo: 'assistente',
      texto: '🦉 Olá! Sou o Oni o Sábio, seu assistente virtual do SingleOne!\n\nComo posso ajudá-lo hoje?',
      timestamp: new Date()
    };

    this.adicionarMensagem(mensagemBoasVindas);
  }

  /**
   * Envia uma pergunta para o assistente
   */
  perguntar(pergunta: string): Observable<TinOneResposta> {
    this.isLoadingSubject.next(true);

    // Adiciona mensagem do usuário
    this.adicionarMensagem({
      tipo: 'usuario',
      texto: pergunta,
      timestamp: new Date()
    });

    // Prepara dados da pergunta
    const perguntaDto: TinOnePergunta = {
      pergunta: pergunta,
      paginaContexto: this.contextService.getCurrentUrl(),
      sessaoId: this.contextService.getSessionId(),
      usuarioId: this.getUsuarioId(),
      clienteId: this.getClienteId()
    };

    return this.http.post<TinOneResposta>(`${this.apiUrl}/ask`, perguntaDto, {
      headers: this.getHeaders()
    }).pipe(
      tap(resposta => {
        // Adiciona resposta do assistente
        this.adicionarMensagem({
          tipo: 'assistente',
          texto: resposta.resposta,
          timestamp: new Date(),
          dados: resposta.dados
        });

        this.isLoadingSubject.next(false);
      }),
      catchError(error => {
        console.error('[TinOne] Erro ao enviar pergunta:', error);
        
        // Adiciona mensagem de erro
        this.adicionarMensagem({
          tipo: 'assistente',
          texto: 'Desculpe, tive um problema ao processar sua pergunta. Tente novamente.',
          timestamp: new Date()
        });

        this.isLoadingSubject.next(false);
        throw error;
      })
    );
  }

  /**
   * Obtém informações sobre um campo
   */
  getCampoInfo(campoId: string): Observable<TinOneCampoInfo> {
    return this.http.get<TinOneCampoInfo>(`${this.apiUrl}/field/${campoId}`, {
      headers: this.getHeaders()
    });
  }

  /**
   * Obtém um processo guiado
   */
  getProcesso(processoId: string): Observable<TinOneProcesso> {
    return this.http.get<TinOneProcesso>(`${this.apiUrl}/process/${processoId}`, {
      headers: this.getHeaders()
    });
  }

  /**
   * Envia feedback sobre uma resposta
   */
  enviarFeedback(feedback: TinOneFeedback): Observable<any> {
    return this.http.post(`${this.apiUrl}/feedback`, feedback, {
      headers: this.getHeaders()
    });
  }

  /**
   * Adiciona mensagem ao histórico
   */
  private adicionarMensagem(mensagem: TinOneMensagem): void {
    const mensagensAtuais = this.mensagensSubject.value;
    this.mensagensSubject.next([...mensagensAtuais, mensagem]);
  }

  /**
   * Limpa histórico de mensagens
   */
  limparHistorico(): void {
    this.mensagensSubject.next([]);
    this.inicializarChat();
  }

  /**
   * Obtém mensagens atuais
   */
  getMensagens(): TinOneMensagem[] {
    return this.mensagensSubject.value;
  }

  /**
   * Obtém headers HTTP com autenticação
   */
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  /**
   * Obtém ID do usuário atual
   */
  private getUsuarioId(): number | undefined {
    try {
      const userData = localStorage.getItem('usuario');
      if (userData) {
        const user = JSON.parse(userData);
        return user.usuario?.id || user.id;
      }
    } catch (error) {
      console.error('[TinOne] Erro ao buscar usuário ID:', error);
    }
    return undefined;
  }

  /**
   * Obtém ID do cliente atual
   */
  private getClienteId(): number | undefined {
    try {
      const userData = localStorage.getItem('usuario');
      if (userData) {
        const user = JSON.parse(userData);
        return user.cliente;
      }
    } catch (error) {
      console.error('[TinOne] Erro ao buscar cliente ID:', error);
    }
    return undefined;
  }
}

