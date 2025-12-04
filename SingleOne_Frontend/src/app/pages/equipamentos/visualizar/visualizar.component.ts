import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-visualizar',
  templateUrl: './visualizar.component.html',
  styleUrls: ['./visualizar.component.scss']
})
export class VisualizarComponent implements OnInit {
  private session:any = {};
  public tipoequipamento:any = {};
  public fabricante:any = {};
  public modelo:any = {};
  public notafiscal:any = {};
  public localizacao:any = {};
  public empresa:any = {};
  public centrocusto:any = {};
  public numeroserie:any = {};
  public patrimonio:any = {};
  public dtlimitegarantia:any = {};
  public tipoaquisicao:any = {};
  public equipamentostatus:any = {};
  public contrato:any = {};

  constructor(private util: UtilService, private ar: ActivatedRoute, private api: EquipamentoApiService) { }

  ngOnInit(): void {
    
    this.session = this.util.getSession('usuario');
    this.visualizarRecurso();
  }

  visualizarRecurso(){
    this.util.aguardar(true);
    this.ar.paramMap.subscribe(param => {
      const id = Number(this.ar.snapshot.paramMap.get('id'));
      this.api.visualizarRecurso(id, this.session.token).then(res => {
        this.tipoequipamento = res.data.tipoequipamentoNavigation.descricao;
        this.fabricante = res.data.fabricanteNavigation.descricao;
        this.modelo = res.data.modeloNavigation.descricao;
        this.notafiscal = res.data.notafiscalNavigation?.numero;
        this.localizacao = res.data.localizacaoNavigation.descricao;
        this.empresa = res.data.empresaNavigation?.nome;
        this.centrocusto = res.data.centrocustoNavigation?.nome;
        this.numeroserie = res.data.numeroserie;
        this.patrimonio = res.data.patrimonio;
        this.dtlimitegarantia = res.data.dtlimitegarantia;
        this.tipoaquisicao = res.data.tipoaquisicaoNavigation.nome;
        this.equipamentostatus = res.data.equipamentostatusNavigation.descricao;
        this.contrato = res.data.contratoNavigation?.descricao;
      });      
    })
    this.util.aguardar(false);
  }

  dataMenorQueHoje(data: Date): boolean {
    const dataAtual = new Date();
    const dataFormat = new Date(data);
    return dataFormat < dataAtual;
  }
}
