import { Component, OnInit, Output, EventEmitter, Input, OnDestroy, ViewChild, ElementRef, AfterViewChecked, ChangeDetectorRef } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { TinOneService } from '../../services/tinone.service';
import { TinOneConfigService } from '../../services/tinone-config.service';
import { TinOneMensagem } from '../../models/tinone.models';
import { UtilService } from '../../../util/util.service';
import { getRouteContext } from '../../config/tinone-suggestions.config';

/**
 * Componente de chat do TinOne
 */
@Component({
  selector: 'app-tinone-chat',
  templateUrl: './tinone-chat.component.html',
  styleUrls: ['./tinone-chat.component.scss']
})
export class TinOneChatComponent implements OnInit, OnDestroy, AfterViewChecked {
  @Output() close = new EventEmitter<void>();
  @Input() corPrimaria = '#4a90e2';
  @Input() posicao = 'bottom-right'; // Posição do widget (bottom-right ou bottom-left)

  @ViewChild('messagesContainer') messagesContainer!: ElementRef;
  @ViewChild('quickSuggestions') quickSuggestions: any;

  mensagens: TinOneMensagem[] = [];
  isLoading = false;
  perguntaAtual = '';
  guiaExpandido: { [key: number]: boolean } = {};
  
  // Sugestões Proativas
  currentRoute: string = '';
  showQuickSuggestions = false;
  mostrarSugestoesAposAtualizacao = false; // Flag para mostrar sugestões após atualização de tela
  sugestoesKey = 0; // Key para forçar re-renderização do componente
  
  // Detecção de mudança de tela
  private rotaAnterior: string = '';
  public mostrarAlertaMudancaTela: boolean = false;
  public novaTelaContexto: string = '';

  private subscriptions: Subscription[] = [];
  private shouldScrollToBottom = false;

  constructor(
    private tinOneService: TinOneService,
    private tinOneConfigService: TinOneConfigService,
    private router: Router,
    private util: UtilService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Observa mensagens
    const sub1 = this.tinOneService.mensagens$.subscribe(mensagens => {
      this.mensagens = mensagens;
      this.shouldScrollToBottom = true;
    });

    // Observa estado de loading
    const sub2 = this.tinOneService.isLoading$.subscribe(loading => {
      this.isLoading = loading;
      if (loading) {
        this.shouldScrollToBottom = true;
      }
    });

    // Inicializa sugestões proativas
    this.initQuickSuggestions();
    
    // Detecta mudança de rota
    this.rotaAnterior = this.router.url;
    const sub3 = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.detectarMudancaDeTela(event.url);
    });

    this.subscriptions.push(sub1, sub2, sub3);
  }

  /**
   * Inicializa sugestões proativas
   */
  private initQuickSuggestions(): void {
    // Verifica se sugestões proativas estão habilitadas
    const config = this.tinOneConfigService.getConfig();
    
    if (config && config.sugestoesProativas) {
      this.currentRoute = this.router.url;
      this.showQuickSuggestions = true;
    }
  }
  
  /**
   * Detecta mudança de tela e notifica o usuário
   */
  private detectarMudancaDeTela(novaRota: string): void {
    // Ignora mudanças muito rápidas (menos de 1 segundo)
    if (novaRota === this.rotaAnterior) {
      return;
    }
    
    // Verifica se a nova rota tem contexto (sugestões disponíveis)
    const contexto = getRouteContext(novaRota);
    
    if (contexto && this.rotaAnterior !== '') {
      // Só notifica se o chat estiver aberto
      const chatAberto = this.mensagens.length > 0 || true; // Sempre notifica
      
      if (chatAberto) {
        // NÃO atualiza currentRoute aqui - será atualizado apenas quando usuário aceitar
        
        // Exibe notificação de mudança de tela
        this.novaTelaContexto = contexto;
        this.mostrarAlertaMudancaTela = true;
        
        // Auto-oculta após 10 segundos
        setTimeout(() => {
          this.mostrarAlertaMudancaTela = false;
        }, 10000);
      }
    }
    
    this.rotaAnterior = novaRota;
  }
  
  /**
   * Atualiza sugestões ao aceitar a mudança de tela
   */
  public atualizarSugestoes(): void {
    this.mostrarAlertaMudancaTela = false;
    
    // Atualiza a rota IMEDIATAMENTE
    const novaRota = this.router.url;
    this.currentRoute = novaRota;
    
    // Envia mensagem automática do Oni informando a mudança
    const contexto = getRouteContext(this.currentRoute);
    const mensagemOni: TinOneMensagem = {
      tipo: 'assistente',
      texto: `🦉 Percebi que você está agora em: ${contexto}\n\nAtualizei as perguntas sugeridas para esta tela! Clique em uma das sugestões abaixo ou pergunte o que desejar.`,
      timestamp: new Date()
    };
    
    // Adiciona mensagem ao histórico
    this.mensagens.push(mensagemOni);
    
    // Garante que as sugestões estão visíveis
    this.mostrarSugestoesAposAtualizacao = true;
    this.showQuickSuggestions = true;
    
    // Força detecção de mudanças do Angular
    this.cdr.detectChanges();
    
    this.shouldScrollToBottom = true;
  }
  
  /**
   * Ignora a mudança de tela
   */
  public ignorarMudancaTela(): void {
    this.mostrarAlertaMudancaTela = false;
  }

  /**
   * Callback quando uma sugestão é selecionada
   */
  onSuggestionSelected(query: string): void {
    // Oculta sugestões após seleção
    this.mostrarSugestoesAposAtualizacao = false;
    this.perguntaAtual = query;
    this.enviarPergunta();
  }

  ngAfterViewChecked(): void {
    if (this.shouldScrollToBottom) {
      this.scrollToBottom();
      this.shouldScrollToBottom = false;
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  /**
   * Envia pergunta
   */
  enviarPergunta(): void {
    if (!this.perguntaAtual.trim() || this.isLoading) {
      return;
    }

    const pergunta = this.perguntaAtual.trim();
    this.perguntaAtual = '';

    this.tinOneService.perguntar(pergunta).subscribe({
      error: (err) => {
        console.error('[TinOne Chat] Erro ao enviar pergunta:', err);
      }
    });
  }

  /**
   * Fecha o chat
   */
  fecharChat(): void {
    this.close.emit();
  }

  /**
   * Limpa histórico
   */
  limparHistorico(): void {
    this.util.exibirMensagemPopUp(
      'Tem certeza que deseja limpar o histórico de conversas? Esta ação não poderá ser desfeita.',
      true
    ).then(res => {
      if (res) {
        this.tinOneService.limparHistorico();
      }
    });
  }

  /**
   * Trata tecla Enter
   */
  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.enviarPergunta();
    }
  }

  /**
   * Scroll para o final das mensagens
   */
  private scrollToBottom(): void {
    try {
      if (this.messagesContainer) {
        const element = this.messagesContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    } catch (err) {
      console.error('[TinOne Chat] Erro ao fazer scroll:', err);
    }
  }

  /**
   * Formata timestamp
   */
  formatarHora(data: Date): string {
    const d = new Date(data);
    return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
  }

  /**
   * Verifica se a mensagem é um guia com passos
   */
  isGuia(mensagem: TinOneMensagem): boolean {
    return mensagem.dados && 
           mensagem.dados.ProcessoId && 
           mensagem.dados.Passos && 
           mensagem.dados.Passos.length > 0;
  }

  /**
   * Alterna expansão do guia
   */
  toggleGuia(index: number): void {
    this.guiaExpandido[index] = !this.guiaExpandido[index];
    this.shouldScrollToBottom = true;
  }

  /**
   * Verifica se o guia está expandido
   */
  isGuiaExpandido(index: number): boolean {
    return !!this.guiaExpandido[index];
  }

  /**
   * Retorna a classe CSS baseada na posição do widget
   * Widget à esquerda → Chat à esquerda da tela
   * Widget à direita → Chat à direita da tela
   */
  getPositionClass(): string {
    // Agora com position: fixed, usamos a mesma posição do widget
    return this.posicao === 'bottom-left' ? 'position-left' : 'position-right';
  }
}

