import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { TinOneConfig } from '../models/tinone.models';
import { environment } from 'src/environments/environment';

/**
 * Serviço de configuração do TinOne
 * Busca parâmetros do backend e gerencia estado
 */
@Injectable({
  providedIn: 'root'
})
export class TinOneConfigService {
  private apiUrl = `${environment.apiUrl}/tinone`;
  private configSubject = new BehaviorSubject<TinOneConfig | null>(null);
  public config$ = this.configSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadConfig();
  }

  /**
   * Carrega configuração do backend
   */
  loadConfig(): void {
    // Tenta buscar cliente do localStorage (se houver)
    const clienteId = this.getClienteId();

    this.http.get<TinOneConfig>(`${this.apiUrl}/config`, {
      params: clienteId ? { clienteId: clienteId.toString() } : {}
    }).pipe(
      catchError(error => {
        console.error('[TinOne] Erro ao carregar configuração:', error);
        // Retorna config desabilitada em caso de erro
        return [{
          habilitado: false,
          chatHabilitado: false,
          tooltipsHabilitado: false,
          guiasHabilitado: false,
          sugestoesProativas: false,
          iaHabilitada: false,
          analytics: false,
          debugMode: false,
          posicao: 'bottom-right',
          corPrimaria: '#4a90e2'
        }];
      })
    ).subscribe(config => {
      this.configSubject.next(config);
      
      // Salva no localStorage para acesso rápido
      localStorage.setItem('tinone_config', JSON.stringify(config));

      if (config.debugMode) {
      }
    });
  }

  /**
   * Verifica se TinOne está habilitado
   */
  isEnabled(): Observable<boolean> {
    return this.config$.pipe(
      map(config => config?.habilitado ?? false)
    );
  }

  /**
   * Verifica se chat está habilitado
   */
  isChatEnabled(): Observable<boolean> {
    return this.config$.pipe(
      map(config => config?.habilitado && config?.chatHabilitado)
    );
  }

  /**
   * Verifica se tooltips estão habilitados
   */
  isTooltipsEnabled(): Observable<boolean> {
    return this.config$.pipe(
      map(config => config?.habilitado && config?.tooltipsHabilitado)
    );
  }

  /**
   * Verifica se guias estão habilitados
   */
  isGuiasEnabled(): Observable<boolean> {
    return this.config$.pipe(
      map(config => config?.habilitado && config?.guiasHabilitado)
    );
  }

  /**
   * Obtém configuração atual (síncrono)
   */
  getConfig(): TinOneConfig | null {
    return this.configSubject.value;
  }

  /**
   * Obtém cliente ID do localStorage
   */
  private getClienteId(): number | null {
    try {
      const userData = localStorage.getItem('usuario');
      if (userData) {
        const user = JSON.parse(userData);
        return user.cliente || null;
      }
    } catch (error) {
      console.error('[TinOne] Erro ao buscar cliente ID:', error);
    }
    return null;
  }

  /**
   * Recarrega configuração (útil após alterações nos parâmetros)
   */
  reload(): void {
    this.loadConfig();
  }
}

