import { Component, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { UtilService } from './util/util.service';
import { ConfiguracoesApiService } from './api/configuracoes/configuracoes-api.service';
import { environment } from '../environments/environment';

declare let gtag: Function;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  public mostrarMenu = true;
  public sidebarVisible = false;
  public sidebarMinimized = false;
  public paginas: any[] = [];
  public session: any;
  public dropdownUsuarioAberto = false; // Controla o dropdown do usuário
  public clienteLogo: string | null = null; // Logo do cliente para exibir no cabeçalho
  public systemVersion: string = 'v2.5.18'; // Versão do sistema (fallback)
  public menuMinimizadoPorPadrao = false; // Preferência do usuário

  constructor(
    public route: Router,
    private util: UtilService,
    private configuracoesApi: ConfiguracoesApiService
  ) {
    this.route.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        // Google Analytics está desabilitado
        if (typeof gtag !== 'undefined') {
          gtag('config', 'G-VFC9ZT8T0P', { 'page_path': event.urlAfterRedirects });
        }
        
        // Fechar sidebar em mobile após navegação
        if (window.innerWidth < 768) {
          this.sidebarVisible = false;
        }
      }
    });
  }

  ngOnInit() {
    const session = this.util.getSession('usuario');
    if (session) {
      this.configurarSessao(session);
    } else {
      this.limparEstado();
    }
    
    // Carregar logo do cliente
    this.carregarLogoCliente();
    
    // Carregar versão do sistema
    this.carregarVersaoSistema();
    
    // Listener para mudanças de sessão (importante para 2FA!)
    this.util.sessaoMudou.subscribe((novaSessao: any) => {
      if (novaSessao) {
        this.configurarSessao(novaSessao);
      } else {
        this.limparEstado();
      }
    });

    // Listener para fechar dropdown quando clicar fora
    document.addEventListener('click', (event) => {
      if (!event.target || !(event.target as Element).closest('.dropdown')) {
        this.fecharDropdownUsuario();
      }
    });

    // Listener para mudanças de rota
    this.route.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        if (event.url === '/login' || event.url === '/') {
          this.limparEstado();
        }
      }
    });
  }

  // Tratamento de erro ao carregar logo
  onLogoError(event: any): void {
    console.error('[APP] ❌ Erro ao carregar imagem da logo:', event);
    console.error('[APP] URL da logo:', this.clienteLogo);
    this.clienteLogo = null;
  }

  // Logo carregada com sucesso
  onLogoLoad(): void {
    console.log('[APP] ✅ Logo do cliente carregada com sucesso:', this.clienteLogo);
  }

  // Carregar logo do cliente
  private async carregarLogoCliente() {
    try {
      const response = await this.configuracoesApi.buscarLogoCliente();
      
      // A resposta do axios vem em response.data
      // O backend retorna: { Logo: "/api/logos/{fileName}", ClienteNome: "...", Mensagem: "..." }
      const logoData = response?.data;
      
      if (logoData && (logoData.Logo || logoData.logo)) {
        // A logo retornada é uma URL relativa como /api/logos/{fileName}
        let logoUrl = logoData.Logo || logoData.logo;
        
        // Se a URL já começa com /api/, manter como está (nginx faz proxy)
        // Em produção, sempre usar URL relativa
        // Em desenvolvimento, se environment.apiUrl tiver baseURL completo, usar ele
        if (logoUrl && logoUrl.startsWith('/api/')) {
          // Em produção, manter URL relativa (nginx faz proxy)
          // Em desenvolvimento, verificar se precisa construir URL completa
          if (!environment.production && environment.apiUrl && !environment.apiUrl.startsWith('/')) {
            // environment.apiUrl tem baseURL completo (ex: http://localhost:5000/api)
            const baseUrl = environment.apiUrl.replace('/api', '');
            logoUrl = baseUrl + logoUrl;
          }
          // Caso contrário, manter logoUrl como está (relativa)
        }
        
        console.log('[APP] Logo do cliente carregada:', logoUrl);
        this.clienteLogo = logoUrl;
      } else {
        console.log('[APP] Nenhuma logo retornada pelo backend');
        this.clienteLogo = null;
      }
    } catch (error) {
      console.error('[APP] ❌ Erro ao carregar logo do cliente:', error);
      this.clienteLogo = null;
    }
  }

  // Configurar sessão e montar menu
  private configurarSessao(session: any) {
    this.session = session;
    this.mostrarMenu = true;
    
    // Garantir que o token está no localStorage (importante para interceptor!)
    if (session.token) {
      const tokenAtual = localStorage.getItem('token');
      if (!tokenAtual || tokenAtual !== session.token) {
        localStorage.setItem('token', session.token);
      }
    } else {
      console.warn('[APP] ⚠️ Sessão sem token!');
    }
    
    // Verificar permissões do usuário
    this.verificarPermissoesUsuario(session.usuario);
    
    // Montar menu de acesso
    try {
      this.paginas = this.util.montarMenuDeAcesso(session.usuario);
      this.carregarLogoCliente();
      
      // Carregar preferência do usuário
      this.carregarPreferenciaSidebar();
      
      // Menu começa FECHADO/MINIMIZADO por padrão em todas as resoluções
      // Usuário precisa clicar no botão de menu para abrir
      if (window.innerWidth >= 992) {
        // Desktop: menu visível mas MINIMIZADO por padrão
        this.sidebarVisible = true;
        this.sidebarMinimized = true; // Sempre começa minimizado
        
        // Aplicar classe minimizado
        setTimeout(() => {
          const sidebar = document.getElementById('sidebar');
          if (sidebar) {
            sidebar.classList.add('c-sidebar-minimized');
          }
        }, 0);
      } else {
        // Mobile: menu OCULTO por padrão
        this.sidebarVisible = false;
        this.sidebarMinimized = false;
      }
    } catch (error) {
      console.error('[APP] Erro ao montar menu:', error);
      this.paginas = [];
    }
  }

  // Verificar permissões do usuário
  private verificarPermissoesUsuario(usuario: any) {
    try {
      if (!usuario) {
        console.warn('[APP] Usuário não definido para verificação de permissões');
        return;
      }
      
      const permissoes = {
        consulta: usuario.consulta,
        su: usuario.su,
        temPaginas: !!usuario.paginas,
        totalPaginas: usuario.paginas ? usuario.paginas.length : 0
      };
      if (usuario.consulta) {
      }
      
      // Se o usuário é super usuário
      if (usuario.su) {
      }
      
    } catch (error) {
      console.error('[APP] Erro ao verificar permissões:', error);
    }
  }

  /**
   * Abre o menu lateral
   */
  abrirMenu() {
    try {
      this.sidebarVisible = true;
    } catch (error) {
      console.error('[APP] Erro ao abrir menu:', error);
    }
  }

  /**
   * Fecha o menu lateral
   */
  fecharMenu() {
    try {
      this.sidebarVisible = false;
    } catch (error) {
      console.error('[APP] Erro ao fechar menu:', error);
    }
  }

  /**
   * Alterna o menu de forma inteligente (responsivo)
   * - Mobile: abre/fecha o menu lateral (overlay)
   * - Desktop: sempre visível, alterna entre expandido/minimizado
   */
  toggleMenu() {
    try {
      if (window.innerWidth < 992) {
        this.sidebarVisible = !this.sidebarVisible;
      } 
      // Em desktop (>= 992px): alterna minimizado
      else {
        // Garante que está sempre visível no desktop
        this.sidebarVisible = true;
        
        // Alterna a classe minimizado
        const sidebar = document.getElementById('sidebar');
        if (sidebar) {
          this.sidebarMinimized = !this.sidebarMinimized;
          
          if (this.sidebarMinimized) {
            sidebar.classList.add('c-sidebar-minimized');
          } else {
            sidebar.classList.remove('c-sidebar-minimized');
          }
          
          // Se a preferência está fixada, atualizar automaticamente
          if (this.menuMinimizadoPorPadrao) {
            this.salvarPreferenciaSidebar(this.sidebarMinimized);
          }
        }
      }
    } catch (error) {
      console.error('[APP] Erro ao alternar menu:', error);
    }
  }
  
  /**
   * Salva a preferência de sidebar minimizada do usuário
   */
  private salvarPreferenciaSidebar(minimizado: boolean) {
    try {
      localStorage.setItem('singleone_sidebar_minimizada', minimizado.toString());
      this.menuMinimizadoPorPadrao = minimizado;
    } catch (error) {
      console.error('[APP] Erro ao salvar preferência:', error);
    }
  }
  
  /**
   * Carrega a preferência de sidebar minimizada do usuário
   */
  private carregarPreferenciaSidebar() {
    try {
      const preferencia = localStorage.getItem('singleone_sidebar_minimizada');
      if (preferencia !== null) {
        this.menuMinimizadoPorPadrao = preferencia === 'true';
      }
    } catch (error) {
      console.error('[APP] Erro ao carregar preferência:', error);
    }
  }
  
  /**
   * Alterna a preferência de fixar o estado atual da sidebar
   */
  togglePreferenciaSidebar() {
    try {
      if (this.menuMinimizadoPorPadrao) {
        // Desfixar - remover preferência
        localStorage.removeItem('singleone_sidebar_minimizada');
        this.menuMinimizadoPorPadrao = false;
        this.util.exibirMensagemToast('Preferência removida. O menu voltará ao padrão expandido.', 3000);
      } else {
        // Fixar - salvar estado atual
        this.salvarPreferenciaSidebar(this.sidebarMinimized);
        const estado = this.sidebarMinimized ? 'minimizado' : 'expandido';
        this.util.exibirMensagemToast(`Menu fixado como ${estado} por padrão!`, 3000);
      }
    } catch (error) {
      console.error('[APP] Erro ao alternar preferência:', error);
    }
  }

  /**
   * Alterna entre menu expandido e minimizado (desktop)
   * @deprecated Use toggleMenu() em vez disso
   */
  toggleSidebar() {
    try {
      // Em mobile, apenas abre/fecha
      if (window.innerWidth >= 768) {
        const sidebar = document.getElementById('sidebar');
        if (sidebar) {
          sidebar.classList.toggle('c-sidebar-minimized');
        }
      } else {
        // Em mobile, apenas fecha o menu
        this.sidebarVisible = false;
      }
    } catch (error) {
      console.error('[APP] Erro ao alternar sidebar:', error);
    }
  }

  /**
   * Alterna o dropdown do usuário
   */
  toggleDropdownUsuario() {
    this.dropdownUsuarioAberto = !this.dropdownUsuarioAberto;
  }

  /**
   * Fecha o dropdown do usuário
   */
  fecharDropdownUsuario() {
    this.dropdownUsuarioAberto = false;
  }

  /**
   * Limpa o estado do componente
   */
  limparEstado() {
    this.mostrarMenu = false;
    this.sidebarVisible = false;
    this.sidebarMinimized = false;
    this.dropdownUsuarioAberto = false;
    this.session = null;
    this.paginas = [];
  }

  /**
   * Recarrega o menu
   */
  recarregarMenu() {
    try {
      if (this.session?.usuario) {
        this.paginas = this.util.montarMenuDeAcesso(this.session.usuario);
      }
    } catch (error) {
      console.error('[APP] Erro ao recarregar menu:', error);
    }
  }

  /**
   * Sair do sistema
   */
  sair() {
    this.util.exibirMensagemPopUp('Tem certeza que deseja sair do sistema? Esta ação irá encerrar sua sessão atual.', true).then(res => {
      if (res) {
        this.limparEstado();
        
        // Limpar estado do sistema
        this.util.registrarStatus(false);
        this.util.sair();
      }
    })
  }

  /**
   * Abrir ajuda
   */
  ajuda() {
    var url = 'https://singleone.com.br/ajuda/';
    window.open(url, "_blank");
  }

  /**
   * Abrir meus dados
   */
  MeusDados() {
    this.route.navigate(['/meu-usuario']);
  }

  // 📋 TRACKBY PARA PERFORMANCE
  public trackByPagina(index: number, pagina: any): any {
    return pagina.url || index;
  }

  /**
   * Converte ícones do Material Design para ícones CoreUI
   * Inclui fallbacks HTML caso CoreUI não carregue
   */
  getIconClass(materialIcon: string): string {
    const iconMap: { [key: string]: string } = {
      'dashboard': 'cil-speedometer',
      'dashboard_outline': 'cil-speedometer',
      'people': 'cil-people',
      'people_outline': 'cil-people',
      'person': 'cil-user',
      'person_outline': 'cil-user',
      'build': 'cil-settings',
      'build_outline': 'cil-settings',
      'business': 'cil-building',
      'business_outline': 'cil-building',
      'description': 'cil-file',
      'description_outline': 'cil-file',
      'assessment': 'cil-chart',
      'assessment_outline': 'cil-chart',
      'phone': 'cil-phone',
      'phone_outline': 'cil-phone',
      'logout': 'cil-account-logout',
      'exit_to_app': 'cil-account-logout',
      'refresh': 'cil-reload',
      'error': 'cil-warning',
      'warning': 'cil-warning',
      'info': 'cil-info',
      'help': 'cil-question',
      'account_circle': 'cil-user',
      'menu': 'cil-menu',
      'home': 'cil-home',
      'list': 'cil-list',
      'add': 'cil-plus',
      'edit': 'cil-pencil',
      'delete': 'cil-trash',
      'search': 'cil-magnifying-glass',
      'filter_list': 'cil-filter',
      'sort': 'cil-sort-ascending',
      'download': 'cil-cloud-download',
      'upload': 'cil-cloud-upload',
      'print': 'cil-print',
      'email': 'cil-envelope-closed',
      'notifications': 'cil-bell',
      'settings': 'cil-cog',
      'security': 'cil-shield-alt',
      'lock': 'cil-lock-locked',
      'visibility': 'cil-eye',
      'visibility_off': 'cil-eye-slash',
      'check_circle': 'cil-check-circle',
      'cancel': 'cil-x-circle',
      'done': 'cil-check',
      'close': 'cil-x',
      'arrow_back': 'cil-arrow-left',
      'arrow_forward': 'cil-arrow-right',
      'arrow_upward': 'cil-arrow-top',
      'arrow_downward': 'cil-arrow-bottom',
      'expand_more': 'cil-chevron-bottom',
      'expand_less': 'cil-chevron-top',
      'more_vert': 'cil-options',
      'more_horiz': 'cil-options-horizontal',
      'favorite': 'cil-heart',
      'star': 'cil-star',
      'thumb_up': 'cil-thumb-up',
      'thumb_down': 'cil-thumb-down',
      'share': 'cil-share',
      'link': 'cil-link',
      'open_in_new': 'cil-external-link',
      'get_app': 'cil-get-app',
      'file_download': 'cil-cloud-download',
      'file_upload': 'cil-cloud-upload',
      'folder': 'cil-folder',
      'folder_open': 'cil-folder-open',
      'image': 'cil-image',
      'video_library': 'cil-video',
      'music_note': 'cil-music-note',
      'attach_file': 'cil-paperclip',
      'insert_drive_file': 'cil-file',
      'picture_as_pdf': 'cil-file-pdf',
      'table_chart': 'cil-table',
      'pie_chart': 'cil-chart-pie',
      'bar_chart': 'cil-chart',
      'timeline': 'cil-timeline',
      'schedule': 'cil-calendar',
      'event': 'cil-calendar',
      'today': 'cil-calendar-check',
      'date_range': 'cil-calendar-range',
      'access_time': 'cil-clock',
      'location_on': 'cil-location-pin',
      'map': 'cil-map',
      'navigation': 'cil-compass',
      'directions': 'cil-directions',
      'directions_car': 'cil-car',
      'directions_bus': 'cil-bus-front',
      'directions_walk': 'cil-walk',
      'flight': 'cil-plane',
      'train': 'cil-train',
      'hotel': 'cil-home',
      'restaurant': 'cil-restaurant',
      'shopping_cart': 'cil-cart',
      'store': 'cil-shop',
      'local_shipping': 'cil-truck',
      'local_offer': 'cil-tag',
      'local_phone': 'cil-phone',
      'local_printshop': 'cil-print',
      'local_hospital': 'cil-hospital',
      'sync_alt': 'cil-sync',
      'devices': 'cil-devices',
      'view_timeline': 'cil-timeline',
      'emoji_flags_outlined': 'cil-flag-alt'
    };
    
    return iconMap[materialIcon] || 'cil-circle';
  }

  /**
   * Obtém o fallback HTML para um ícone CoreUI
   * Garante que sempre haja um símbolo visível
   */
  getIconFallback(iconClass: string): string {
    const fallbackMap: { [key: string]: string } = {
      'cil-speedometer': '📊',
      'cil-people': '👥',
      'cil-user': '👤',
      'cil-settings': '⚙️',
      'cil-building': '🏢',
      'cil-file': '📄',
      'cil-chart': '📈',
      'cil-phone': '📞',
      'cil-account-logout': '🚪',
      'cil-reload': '🔄',
      'cil-warning': '⚠️',
      'cil-info': 'ℹ️',
      'cil-question': '❓',
      'cil-menu': '☰',
      'cil-home': '🏠',
      'cil-list': '📋',
      'cil-plus': '➕',
      'cil-pencil': '✏️',
      'cil-trash': '🗑️',
      'cil-magnifying-glass': '🔍',
      'cil-filter': '🔧',
      'cil-sort-ascending': '⬆️',
      'cil-cloud-download': '⬇️',
      'cil-cloud-upload': '⬆️',
      'cil-print': '🖨️',
      'cil-envelope-closed': '✉️',
      'cil-bell': '🔔',
      'cil-cog': '⚙️',
      'cil-shield-alt': '🛡️',
      'cil-lock-locked': '🔒',
      'cil-eye': '👁️',
      'cil-eye-slash': '🙈',
      'cil-check-circle': '✅',
      'cil-x-circle': '❌',
      'cil-check': '✓',
      'cil-x': '✗',
      'cil-arrow-left': '←',
      'cil-arrow-right': '→',
      'cil-arrow-top': '↑',
      'cil-arrow-bottom': '↓',
      'cil-chevron-bottom': '⌄',
      'cil-chevron-top': '⌃',
      'cil-options': '⋮',
      'cil-options-horizontal': '⋯',
      'cil-heart': '❤️',
      'cil-star': '⭐',
      'cil-thumb-up': '👍',
      'cil-thumb-down': '👎',
      'cil-share': '📤',
      'cil-link': '🔗',
      'cil-external-link': '🔗',
      'cil-get-app': '📱',
      'cil-folder-open': '📁',
      'cil-image': '🖼️',
      'cil-video': '🎥',
      'cil-music-note': '🎵',
      'cil-paperclip': '📎',
      'cil-file-pdf': '📕',
      'cil-table': '📊',
      'cil-chart-pie': '🥧',
      'cil-timeline': '⏳',
      'cil-calendar': '📅',
      'cil-calendar-check': '✅',
      'cil-calendar-range': '📅',
      'cil-clock': '🕐',
      'cil-location-pin': '📍',
      'cil-map': '🗺️',
      'cil-compass': '🧭',
      'cil-directions': '🧭',
      'cil-car': '🚗',
      'cil-bus-front': '🚌',
      'cil-walk': '🚶',
      'cil-plane': '✈️',
      'cil-train': '🚂',
      'cil-restaurant': '🍽️',
      'cil-cart': '🛒',
      'cil-shop': '🏪',
      'cil-truck': '🚚',
      'cil-tag': '🏷️',
      'cil-hospital': '🏥',
      'cil-sync': '🔄',
      'cil-devices': '💻',
      'cil-flag-alt': '🏁',
      'cil-circle': '●'
    };
    
    return fallbackMap[iconClass] || '●';
  }

  /**
   * Carrega a versão do sistema do arquivo version.txt
   */
  private async carregarVersaoSistema() {
    try {
      const response = await fetch('/assets/version.txt');
      if (response.ok) {
        const version = await response.text();
        const trimmedVersion = version.trim();
        
        // Validar se a resposta não é HTML (caso o Nginx retorne index.html)
        if (trimmedVersion.startsWith('<!doctype') || trimmedVersion.startsWith('<html') || trimmedVersion.length > 50) {
          console.warn('[APP] Resposta parece ser HTML ao invés de version.txt, usando fallback:', this.systemVersion);
          return; // Manter o fallback
        }
        
        this.systemVersion = trimmedVersion;
      } else {
        console.warn('[APP] Não foi possível carregar a versão, usando fallback:', this.systemVersion);
      }
    } catch (error) {
      console.warn('[APP] Erro ao carregar versão, usando fallback:', error);
    }
  }
}
