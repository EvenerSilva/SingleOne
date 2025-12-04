import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { EstoqueMinimoApiService, EstoqueAlerta } from 'src/app/api/estoque-minimo/estoque-minimo-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-estoque-minimo-widget',
  templateUrl: './estoque-minimo-widget.component.html',
  styleUrls: ['./estoque-minimo-widget.component.scss']
})
export class EstoqueMinimoWidgetComponent implements OnInit {

  @Input() clienteId: number = 0;
  
  public alertas: EstoqueAlerta[] = [];
  public carregando: boolean = false;
  public totalAlertas: number = 0;

  // Estatísticas
  public estatisticas = {
    totalEquipamentos: 0,
    totalLinhas: 0,
    alertasCriticos: 0,
    alertasModerados: 0,
    alertasBaixos: 0
  };

  constructor(
    private util: UtilService,
    private api: EstoqueMinimoApiService,
    private router: Router
  ) { }

  ngOnInit(): void {
    if (this.clienteId > 0) {
      this.carregarDados();
    }
  }

  carregarDados(): void {
    this.carregando = true;
    
    this.api.listarAlertas(this.clienteId).then((response) => {
      this.alertas = response.data || response;
      this.totalAlertas = this.alertas.length;
      this.calcularEstatisticas();
      this.carregando = false;
    }).catch((error) => {
      console.error('Erro ao carregar alertas:', error);
      this.carregando = false;
    });
  }

  calcularEstatisticas(): void {
    this.estatisticas.totalEquipamentos = this.alertas.filter(a => a.tipo === 'EQUIPAMENTO').length;
    this.estatisticas.totalLinhas = this.alertas.filter(a => a.tipo === 'LINHA_TELEFONICA').length;
    this.estatisticas.alertasCriticos = this.alertas.filter(a => a.quantidadeFaltante > 5).length;
    this.estatisticas.alertasModerados = this.alertas.filter(a => a.quantidadeFaltante > 1 && a.quantidadeFaltante <= 5).length;
    this.estatisticas.alertasBaixos = this.alertas.filter(a => a.quantidadeFaltante === 1).length;
  }

  getAlertasCriticos(): EstoqueAlerta[] {
    return this.alertas.filter(a => a.quantidadeFaltante > 5).slice(0, 5);
  }

  getSeveridadeClass(quantidadeFaltante: number): string {
    if (quantidadeFaltante > 5) return 'severidade-critica';
    if (quantidadeFaltante > 1) return 'severidade-moderada';
    return 'severidade-baixa';
  }

  getSeveridadeText(quantidadeFaltante: number): string {
    if (quantidadeFaltante > 5) return 'Crítico';
    if (quantidadeFaltante > 1) return 'Moderado';
    return 'Baixo';
  }

  getTipoIcon(tipo: string): string {
    switch (tipo) {
      case 'EQUIPAMENTO':
        return 'fas fa-laptop';
      case 'LINHA_TELEFONICA':
        return 'fas fa-phone';
      default:
        return 'fas fa-question';
    }
  }

  navegarParaEstoqueMinimo(): void {
    this.router.navigate(['/estoque-minimo']);
  }
}
