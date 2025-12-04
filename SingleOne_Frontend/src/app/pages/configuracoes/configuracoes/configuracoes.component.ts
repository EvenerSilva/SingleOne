import { Component, OnInit } from '@angular/core';
import { UtilService } from 'src/app/util/util.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-configuracoes',
  templateUrl: './configuracoes.component.html',
  styleUrls: ['./configuracoes.component.scss']
})
export class ConfiguracoesComponent implements OnInit {

  public session: any = {};

  constructor(
    private util: UtilService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
  }

  // 🚀 NAVEGAÇÃO PARA CARDS
  navegarParaParametros() {
    this.router.navigate(['/parametros']);
  }

  navegarParaTemplates() {
    this.router.navigate(['/templates']);
  }

  navegarParaUsuarios() {
    this.router.navigate(['/usuarios']);
  }

  navegarParaPoliticasElegibilidade() {
    this.router.navigate(['/configuracoes/politicas-elegibilidade']);
  }

  navegarParaCargosConfianca() {
    this.router.navigate(['/configuracoes/cargosconfianca']);
  }

  navegarParaTinOne() {
    this.router.navigate(['/configuracoes/tinone']);
  }

}
