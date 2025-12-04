import { Component, Input } from '@angular/core';
import { TinOneCampoInfo } from '../../models/tinone.models';

/**
 * Componente de tooltip contextual (futuro)
 * Para mostrar ajuda sobre campos
 */
@Component({
  selector: 'app-tinone-tooltip',
  templateUrl: './tinone-tooltip.component.html',
  styleUrls: ['./tinone-tooltip.component.scss']
})
export class TinOneTooltipComponent {
  @Input() campoInfo: TinOneCampoInfo | null = null;
  @Input() posicao: { top: number, left: number } = { top: 0, left: 0 };
}

