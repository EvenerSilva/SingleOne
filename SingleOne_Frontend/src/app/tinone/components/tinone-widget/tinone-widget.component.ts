import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription, interval } from 'rxjs';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { TinOneConfigService } from '../../services/tinone-config.service';
import { UtilService } from 'src/app/util/util.service';

/**
 * Componente principal do widget TinOne
 * Botão flutuante que abre o chat
 */
@Component({
  selector: 'app-tinone-widget',
  templateUrl: './tinone-widget.component.html',
  styleUrls: ['./tinone-widget.component.scss']
})
export class TinOneWidgetComponent implements OnInit, OnDestroy {
  isEnabled = false;
  isChatOpen = false;
  posicao = 'bottom-right';
  corPrimaria = '#4a90e2';
  isAuthenticated = false;
  isRotaPublica = false; // ✅ NOVO: Flag para identificar rotas públicas
  
  private wasAuthenticatedBefore = false;
  private subscriptions: Subscription[] = [];
  private lastConfigState: { habilitado?: boolean; chatHabilitado?: boolean; isEnabled?: boolean } | null = null;
  
  // ✅ Rotas públicas onde o Oni NÃO deve aparecer (exclusivo para usuários do sistema)
  private readonly rotasPublicas = [
    '/login',
    '/esqueci-senha',
    '/termos',
    '/verificar-termo',
    '/patrimonio',
    '/portaria'
  ];

  constructor(
    private configService: TinOneConfigService,
    private util: UtilService,
    private router: Router // ✅ NOVO: Router para verificar rota atual
  ) {}

  ngOnInit(): void {
    // ✅ NOVO: Verificar rota inicial
    this.checkRotaPublica();
    
    // ✅ NOVO: Listener para mudanças de rota
    const routerSub = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.checkRotaPublica();
      this.updateEnabledState();
    });
    this.subscriptions.push(routerSub);
    
    // Inscrever-se nas mudanças de configuração
    const configSub = this.configService.config$.subscribe(config => {
      if (this.isAuthenticated && !this.isRotaPublica) {
        this.updateEnabledState();
      }
    });
    this.subscriptions.push(configSub);

    // Listener para recarregamento de configuração
    const reloadListener = () => {
      console.log('[Oni Widget] Evento tinone-config-reload recebido, recarregando...');
      this.configService.reload();
      // Aguardar um pouco e atualizar estado
      setTimeout(() => {
        this.updateEnabledState();
      }, 1000);
    };
    window.addEventListener('tinone-config-reload', reloadListener);
    this.subscriptions.push({
      unsubscribe: () => window.removeEventListener('tinone-config-reload', reloadListener)
    } as Subscription);

    // Verifica autenticação continuamente (a cada 2 segundos)
    const authCheckSub = interval(2000).subscribe(() => {
      this.checkAuthentication();
      this.checkRotaPublica(); // ✅ NOVO: Verificar rota também
      this.updateEnabledState();
    });

    // Verificação inicial
    this.checkAuthentication();
    this.updateEnabledState();

    this.subscriptions.push(authCheckSub);
  }

  /**
   * Verifica se a rota atual é pública
   */
  private checkRotaPublica(): void {
    const currentUrl = this.router.url.split('?')[0]; // Remove query params
    this.isRotaPublica = this.rotasPublicas.some(rota => currentUrl.startsWith(rota));
    
    if (this.isRotaPublica) {
      console.log(`[Oni Widget] 🚫 Rota pública detectada: ${currentUrl} - Oni oculto`);
    }
  }

  /**
   * Atualiza o estado habilitado do widget
   */
  private updateEnabledState(): void {
    // ✅ NOVO: Se for rota pública, sempre desabilitar (mesmo com autenticação)
    if (this.isRotaPublica) {
      this.isEnabled = false;
      return;
    }
    
    if (!this.isAuthenticated) {
      this.isEnabled = false;
      this.wasAuthenticatedBefore = false;
      return;
    }

    // Se acabou de autenticar, recarrega a configuração
    if (this.isAuthenticated && !this.wasAuthenticatedBefore) {
      this.configService.reload();
      this.wasAuthenticatedBefore = true;
      
      // Aguarda um pouco para a config carregar
      setTimeout(() => {
        const config = this.configService.getConfig();
        if (config) {
          this.posicao = config.posicao || 'bottom-right';
          this.corPrimaria = config.corPrimaria || '#4a90e2';
          this.isEnabled = !!(config.habilitado && config.chatHabilitado);
          
          this.lastConfigState = {
            habilitado: config.habilitado,
            chatHabilitado: config.chatHabilitado,
            isEnabled: this.isEnabled
          };
          
          console.log('[Oni Widget] Config carregada após autenticação:', this.lastConfigState);
        }
      }, 500);
      return;
    }

    // Verifica config normalmente
    const config = this.configService.getConfig();
    
    if (config) {
      const newPosicao = config.posicao || 'bottom-right';
      const newCorPrimaria = config.corPrimaria || '#4a90e2';
      const newIsEnabled = !!(config.habilitado && config.chatHabilitado);
      
      // Só atualiza e loga se realmente mudou
      const configChanged = 
        this.posicao !== newPosicao ||
        this.corPrimaria !== newCorPrimaria ||
        this.isEnabled !== newIsEnabled ||
        this.lastConfigState?.habilitado !== config.habilitado ||
        this.lastConfigState?.chatHabilitado !== config.chatHabilitado;
      
      if (configChanged) {
        this.posicao = newPosicao;
        this.corPrimaria = newCorPrimaria;
        this.isEnabled = newIsEnabled;
        
        this.lastConfigState = {
          habilitado: config.habilitado,
          chatHabilitado: config.chatHabilitado,
          isEnabled: this.isEnabled
        };
        
        console.log('[Oni Widget] Config atualizada:', this.lastConfigState);
      }
    } else {
      if (this.lastConfigState !== null) {
        console.log('[Oni Widget] Config não disponível ainda');
        this.lastConfigState = null;
      }
      this.isEnabled = false;
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  /**
   * Alterna abertura do chat
   */
  toggleChat(): void {
    this.isChatOpen = !this.isChatOpen;
  }

  /**
   * Fecha o chat
   */
  closeChat(): void {
    this.isChatOpen = false;
  }

  /**
   * Obtém classes CSS baseadas na posição
   */
  getPositionClass(): string {
    return `tinone-widget-${this.posicao}`;
  }

  /**
   * Verifica se o usuário está autenticado
   */
  private checkAuthentication(): void {
    try {
      // O sistema SingleOne armazena a sessão em localStorage.getItem('usuario')
      // que contém um objeto JSON com { token, usuario: {...}, cliente, ... }
      const session = this.util.getSession('usuario');
      
      // Considera autenticado se tiver sessão E token dentro da sessão
      this.isAuthenticated = !!(session && session.token);
    } catch (error) {
      console.error('[Oni Widget] Erro ao verificar autenticação:', error);
      this.isAuthenticated = false;
    }
  }
}

