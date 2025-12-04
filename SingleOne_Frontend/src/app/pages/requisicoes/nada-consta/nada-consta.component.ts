import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs/internal/Observable';
import { debounceTime, tap } from 'rxjs/operators';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import * as moment from 'moment';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';

@Component({
  selector: 'app-nada-consta',
  templateUrl: './nada-consta.component.html',
  styleUrls: ['./nada-consta.component.scss']
})
export class NadaConstaComponent implements OnInit {

  private session:any = {};
  public colaborador:any = {};
  public colaboradores:any = [];
  public nadaConsta:any = {};
  public recursos = new FormControl();
  public resultadoEqps: Observable<any>;

  constructor(private util: UtilService, private apiCol: ColaboradorApiService, private api: RelatorioApiService, private ar: ActivatedRoute) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.colaborador.cliente = this.session.usuario.cliente;

    /*BUsca de equipamentos*/
    this.resultadoEqps = this.recursos.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscarColaborador(value))
    );
    this.resultadoEqps.subscribe();

    // ✅ NOVO: Pré-busca via query param 'search'
    this.ar.queryParams.subscribe(params => {
      const search = params['search'];
      if (search && typeof search === 'string' && search.trim()) {
        const termo = search.trim();
        // 1) Disparar busca automática pelo nome vindo da devolução
        this.buscarColaborador(termo);
        // 2) Após pequena pausa, se achar apenas um colaborador, selecionar e consultar
        setTimeout(() => {
          const match = (this.colaboradores || []).filter((c: any) => {
            const nome = (c?.nome || '').toString().toLowerCase();
            return nome.includes(termo.toLowerCase());
          });
          if (match.length === 1) {
            this.colaborador.id = match[0].id;
            this.consultarHistorico();
          }
        }, 500);
      }
    });
  }

  buscarColaborador(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.apiCol.listarColaboradores(valor, this.colaborador.cliente, 1, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.colaboradores = res.data.results;
        }
      })
    }
  }

  consultarHistorico() {
    if(this.colaborador.id != undefined) {
      this.util.aguardar(true);
      this.apiCol.nadaConsta(this.colaborador.id, this.colaborador.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        this.nadaConsta = res.data;
      })
    }
    else {
      this.util.exibirMensagemToast('Selecione um equipamento para consultar o seu histórico', 5000);
    }
  }

  imprimir() {
    this.util.aguardar(true);
    this.apiCol.termoNadaConsta(this.colaborador.id).then(res => {
      this.util.aguardar(false);
      if(res.status != 200) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.util.gerarDocumentoNovaGuia(res.data);
      }
    })
  }

}
