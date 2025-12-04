import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { UtilService } from '../../../util/util.service';

@Component({
  selector: 'app-estoque-minimo',
  templateUrl: './estoque-minimo.component.html',
  styleUrls: ['./estoque-minimo.component.scss']
})
export class EstoqueMinimoComponent implements OnInit {

  public session: any = {};
  public activeTab: string = 'equipamentos';

  constructor(
    private util: UtilService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.setupActiveTab();
  }

  // 🎯 CONFIGURAR TAB ATIVA
  private setupActiveTab() {
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        const url = event.url;
        if (url.includes('/equipamentos')) {
          this.activeTab = 'equipamentos';
        } else if (url.includes('/linhas')) {
          this.activeTab = 'linhas';
        }
      }
    });
  }

  // 🧭 NAVEGAÇÃO ENTRE TABS
  navigateToTab(tab: string) {
    this.activeTab = tab;
    this.router.navigate([`/estoque-minimo/${tab}`]);
  }

  // 🧭 NAVEGAÇÃO ENTRE TABS (alias)
  navegarParaTab(tab: string) {
    this.navigateToTab(tab);
  }

  // ➕ NOVO EQUIPAMENTO
  novoEquipamento() {
    // Emitir evento para o componente filho (equipamentos)
    const event = new CustomEvent('novoEquipamento');
    window.dispatchEvent(event);
  }

  // ➕ NOVA LINHA
  novaLinha() {
    // Emitir evento para o componente filho (linhas)
    const event = new CustomEvent('novaLinha');
    window.dispatchEvent(event);
  }

  // 🏠 VOLTAR PARA CADASTROS
  voltarParaCadastros() {
    this.router.navigate(['/cadastros']);
  }

}