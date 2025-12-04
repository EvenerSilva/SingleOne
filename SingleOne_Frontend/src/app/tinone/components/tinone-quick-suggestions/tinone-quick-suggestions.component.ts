import { Component, EventEmitter, Input, OnInit, OnChanges, SimpleChanges, Output } from '@angular/core';
import { TinOneSuggestion, getSuggestionsByRoute, getRouteContext } from '../../config/tinone-suggestions.config';

/**
 * Componente de sugestões rápidas contextuais do Oni
 */
@Component({
  selector: 'app-tinone-quick-suggestions',
  templateUrl: './tinone-quick-suggestions.component.html',
  styleUrls: ['./tinone-quick-suggestions.component.scss']
})
export class TinOneQuickSuggestionsComponent implements OnInit, OnChanges {
  @Input() currentRoute: string = '';
  @Input() corPrimaria: string = '#4a90e2';
  @Output() suggestionSelected = new EventEmitter<string>();

  suggestions: TinOneSuggestion[] = [];
  routeContext: string | null = null;
  showSuggestions = true;
  updateTimestamp = Date.now(); // Timestamp para forçar re-renderização

  ngOnInit(): void {
    this.loadSuggestions();
  }

  /**
   * Detecta mudanças no Input currentRoute e atualiza sugestões
   */
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['currentRoute'] && !changes['currentRoute'].firstChange) {
      // Rota mudou, recarrega sugestões e reexibe
      this.showSuggestions = true;
      this.loadSuggestions();
    }
  }

  /**
   * Carrega sugestões baseadas na rota atual
   */
  private loadSuggestions(): void {
    this.suggestions = getSuggestionsByRoute(this.currentRoute);
    this.routeContext = getRouteContext(this.currentRoute);
    this.updateTimestamp = Date.now(); // Força atualização visual
  }

  /**
   * Emite a pergunta selecionada
   */
  onSuggestionClick(suggestion: TinOneSuggestion): void {
    this.suggestionSelected.emit(suggestion.query);
  }

  /**
   * Oculta as sugestões
   */
  hideSuggestions(): void {
    this.showSuggestions = false;
  }
}

