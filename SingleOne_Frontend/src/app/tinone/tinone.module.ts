import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Componentes
import { TinOneWidgetComponent } from './components/tinone-widget/tinone-widget.component';
import { TinOneChatComponent } from './components/tinone-chat/tinone-chat.component';
import { TinOneTooltipComponent } from './components/tinone-tooltip/tinone-tooltip.component';
import { TinOneQuickSuggestionsComponent } from './components/tinone-quick-suggestions/tinone-quick-suggestions.component';

// Serviços
import { TinOneService } from './services/tinone.service';
import { TinOneConfigService } from './services/tinone-config.service';
import { TinOneContextService } from './services/tinone-context.service';

// Diretivas
import { TinOneHelpDirective } from './directives/tinone-help.directive';

/**
 * Módulo isolado do assistente TinOne
 * Não depende de outros módulos do sistema
 * Pode ser removido sem impacto nas funcionalidades existentes
 */
@NgModule({
  declarations: [
    TinOneWidgetComponent,
    TinOneChatComponent,
    TinOneTooltipComponent,
    TinOneQuickSuggestionsComponent,
    TinOneHelpDirective
  ],
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    RouterModule
  ],
  providers: [
    TinOneService,
    TinOneConfigService,
    TinOneContextService
  ],
  exports: [
    TinOneWidgetComponent,
    TinOneHelpDirective // Exporta diretiva para uso opcional
  ]
})
export class TinOneModule { }

