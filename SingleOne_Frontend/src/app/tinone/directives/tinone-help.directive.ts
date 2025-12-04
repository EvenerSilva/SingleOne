import { Directive, ElementRef, HostListener, Input, OnInit } from '@angular/core';
import { TinOneConfigService } from '../services/tinone-config.service';
import { TinOneService } from '../services/tinone.service';

/**
 * Diretiva para adicionar ajuda contextual aos campos
 * Uso: <input tinOneHelp="equipamento.patrimonio">
 * 
 * Esta diretiva é OPCIONAL e não afeta o funcionamento normal dos campos
 */
@Directive({
  selector: '[tinOneHelp]'
})
export class TinOneHelpDirective implements OnInit {
  @Input() tinOneHelp: string = '';  // ID do campo na base de conhecimento
  @Input() tinOneDescription: string = '';  // Descrição curta (opcional)
  @Input() tinOneExample: string = '';  // Exemplo (opcional)

  private enabled = false;
  private tooltipsEnabled = false;

  constructor(
    private el: ElementRef,
    private configService: TinOneConfigService,
    private tinOneService: TinOneService
  ) {}

  ngOnInit(): void {
    // Verifica se TinOne e tooltips estão habilitados
    this.configService.config$.subscribe(config => {
      if (config) {
        this.enabled = config.habilitado;
        this.tooltipsEnabled = config.tooltipsHabilitado;
      }
    });
  }

  @HostListener('focus')
  onFocus(): void {
    if (!this.enabled || !this.tooltipsEnabled) {
      return;
    }

    // Adiciona visual indicator (borda azul suave)
    this.el.nativeElement.style.borderColor = '#4a90e2';
    this.el.nativeElement.style.boxShadow = '0 0 0 2px rgba(74, 144, 226, 0.1)';

    // TODO: Futuro - mostrar tooltip com informações do campo
    // this.showTooltip();
  }

  @HostListener('blur')
  onBlur(): void {
    if (!this.enabled || !this.tooltipsEnabled) {
      return;
    }

    // Remove visual indicator
    this.el.nativeElement.style.borderColor = '';
    this.el.nativeElement.style.boxShadow = '';

    // TODO: Futuro - esconder tooltip
    // this.hideTooltip();
  }

  @HostListener('click', ['$event'])
  onClick(event: MouseEvent): void {
    if (!this.enabled || !this.tooltipsEnabled) {
      return;
    }

    // Se Shift+Click, busca ajuda detalhada
    if (event.shiftKey && this.tinOneHelp) {
      event.preventDefault();
      this.buscarAjudaDetalhada();
    }
  }

  private buscarAjudaDetalhada(): void {
    this.tinOneService.getCampoInfo(this.tinOneHelp).subscribe({
      next: (info) => {
      },
      error: (err) => {
        console.error('[TinOne Help] Erro ao buscar informações do campo:', err);
      }
    });
  }
}

