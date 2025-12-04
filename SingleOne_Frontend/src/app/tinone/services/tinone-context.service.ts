import { Injectable } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

/**
 * Serviço de contexto do TinOne
 * Detecta em qual página/tela o usuário está
 */
@Injectable({
  providedIn: 'root'
})
export class TinOneContextService {
  private currentUrl: string = '';
  private currentPageName: string = '';

  constructor(private router: Router) {
    // Monitora mudanças de rota
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.currentUrl = event.url;
        this.currentPageName = this.extractPageName(event.url);
      });

    // Inicializa com a URL atual
    this.currentUrl = this.router.url;
    this.currentPageName = this.extractPageName(this.router.url);
  }

  /**
   * Obtém a URL atual
   */
  getCurrentUrl(): string {
    return this.currentUrl;
  }

  /**
   * Obtém o nome da página atual
   */
  getCurrentPageName(): string {
    return this.currentPageName;
  }

  /**
   * Verifica se está em uma página específica
   */
  isOnPage(pagePattern: string): boolean {
    return this.currentUrl.includes(pagePattern);
  }

  /**
   * Obtém contexto completo da página
   */
  getPageContext(): any {
    return {
      url: this.currentUrl,
      nome: this.currentPageName,
      timestamp: new Date()
    };
  }

  /**
   * Extrai nome amigável da página a partir da URL
   */
  private extractPageName(url: string): string {
    // Remove query params
    const cleanUrl = url.split('?')[0];

    // Mapeia URLs para nomes amigáveis
    const pageMap: { [key: string]: string } = {
      '/dashboard': 'Dashboard',
      '/requisicoes': 'Requisições',
      '/cadastros/equipamentos': 'Equipamentos',
      '/cadastros/colaboradores': 'Colaboradores',
      '/cadastros/empresas': 'Empresas',
      '/cadastros/contratos': 'Contratos',
      '/cadastros/notasfiscais': 'Notas Fiscais',
      '/cadastros/fornecedores': 'Fornecedores',
      '/configuracoes': 'Configurações',
      '/usuarios': 'Usuários',
      '/contestacoes': 'Contestações',
      '/patrimonio': 'Meu Patrimônio',
      '/relatorios': 'Relatórios',
      '/termo-eletronico': 'Termo Eletrônico e Campanhas'
    };

    // Procura correspondência exata
    if (pageMap[cleanUrl]) {
      return pageMap[cleanUrl];
    }

    // Procura correspondência parcial
    for (const [pattern, name] of Object.entries(pageMap)) {
      if (cleanUrl.startsWith(pattern)) {
        return name;
      }
    }

    // Fallback: extrai da URL
    const segments = cleanUrl.split('/').filter(s => s);
    if (segments.length > 0) {
      return this.capitalize(segments[segments.length - 1]);
    }

    return 'Página';
  }

  /**
   * Capitaliza primeira letra
   */
  private capitalize(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }

  /**
   * Gera ID de sessão único
   */
  getSessionId(): string {
    let sessionId = sessionStorage.getItem('tinone_session_id');
    if (!sessionId) {
      sessionId = this.generateSessionId();
      sessionStorage.setItem('tinone_session_id', sessionId);
    }
    return sessionId;
  }

  /**
   * Gera ID de sessão aleatório
   */
  private generateSessionId(): string {
    return `tinone_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }
}

