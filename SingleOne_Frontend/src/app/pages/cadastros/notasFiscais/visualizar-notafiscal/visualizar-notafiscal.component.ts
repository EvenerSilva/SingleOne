import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-visualizar-notafiscal',
  templateUrl: './visualizar-notafiscal.component.html',
  styleUrls: ['./visualizar-notafiscal.component.scss']
})
export class VisualizarNotafiscalComponent implements OnInit {

  public nf:any = {};

  constructor(private util: UtilService, 
              private ar: ActivatedRoute,
              private api: ConfiguracoesApiService) { }

  ngOnInit(): void {
    this.carregarNota();
  }

  carregarNota() {
    this.util.aguardar(true);
    this.ar.paramMap.subscribe(param => {
      const id = Number(this.ar.snapshot.paramMap.get('id'));
      this.api.visualizarNotaFiscal(id).then(res => {
        this.nf = res.data;
        this.util.aguardar(false);
      });
    });
  }
}
