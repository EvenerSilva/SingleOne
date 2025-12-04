import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { TabStateService } from 'src/app/util/tab-state.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-contratos-telefonia',
  templateUrl: './contratos-telefonia.component.html',
  styleUrls: ['./contratos-telefonia.component.scss']
})
export class ContratosTelefoniaComponent implements OnInit, AfterViewInit {

  private session:any = {};
  public colunas = ['operadora', 'nome', 'descricao', 'totalizadores', 'ativo', 'acao'];
  
  // 🆕 Propriedades para o modal
  public mostrarFormulario = false;
  public contratoEditando: any = null;
  public modoFormulario: 'criar' | 'editar' = 'criar';
  
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;

  constructor(
    private util: UtilService, 
    private api: TelefoniaApiService, 
    private route: Router,
    private tabState: TabStateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    this.listar();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  // 🔧 MÉTODO AUXILIAR PARA CONFIGURAR PAGINADOR
  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      return;
    }
    
    // CONFIGURAÇÃO SIMPLES E DIRETA
    this.dataSource.paginator = this.paginator;
    
    // CONFIGURAR TAMANHO INICIAL
    this.paginator.pageSize = 10;
    this.paginator.pageIndex = 0;
    
    // ADICIONAR LISTENER PARA MUDANÇAS
    this.paginator.page.subscribe(() => {
      // FORÇAR ATUALIZAÇÃO DA VIEW
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    });
  }

  setTabAndNavigate() {
    this.tabState.setTabIndex(1);
    this.route.navigate(['/recursos']);
  }

  listar() {
    this.util.aguardar(true);
    this.api.listarContratos("null", 0, this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        res.data.map(x => {
          x.totalPlanos = 0;
          x.totalLinhas = 0;
          x.totalEmUso = 0;
          x.totalLivre = 0;
          x.totalPlanos = x.telefoniaplanos.length;
          x.telefoniaplanos.map(p => {
            x.totalLinhas += p.telefonialinhas.length;
            var emUso = p.telefonialinhas.filter(l => {
              return l.emuso == true;
            });
            var livres = p.telefonialinhas.filter(l => {
              return l.emuso == false;
            });
            x.totalEmUso += emUso.length;
            x.totalLivre += livres.length;
          })
        })
        this.dataSource = new MatTableDataSource<any>(res.data);
        this.configurarPaginador();
      }
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarContratos(valor, 0, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.dataSource = new MatTableDataSource(res.data);
          this.configurarPaginador();
        }
      })
    }
    else {
      this.listar();
    }
  }

  // 🧹 MÉTODO PARA LIMPAR BUSCA
  limparBusca() {
    this.consulta.setValue('');
    this.listar();
  }

  editar(obj) {
    this.contratoEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  excluir(obj) {
    if(confirm('Deseja realmente excluir a conta ' + obj.nome + '?')) {
      this.util.aguardar(true);
      this.api.excluirContrato(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Conta excluida com sucesso!', 5000);
          this.listar();
        }
      })
    }
  }

  voltarParaTelecom() {
    this.route.navigate(['/telecom']);
  }

  novo() {
    this.contratoEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  // 🆕 Métodos para controlar o modal
  onContratoSalvo(contrato: any) {
    this.mostrarFormulario = false;
    this.contratoEditando = null;
    setTimeout(() => {
      this.listar(); // Recarregar lista
    }, 500);
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.contratoEditando = null;
  }

  // 🎯 NOVOS MÉTODOS PARA NAVEGAÇÃO FILTRADA
  navegarParaPlanos(operadora: string, contaNome: string) {
    this.route.navigate(['/planos'], {
      queryParams: { 
        search: `${operadora} ${contaNome}`,
        conta: contaNome,
        operadora: operadora
      }
    });
  }

  navegarParaLinhas(operadora: string, contaNome: string, tipoFiltro: 'todas' | 'em-uso' | 'livres') {
    const conta = this.dataSource.data.find(c => c.nome === contaNome);
    if (!conta) {
      console.error('[CONTAS] Conta não encontrada:', contaNome);
      this.util.exibirMensagemToast('Erro: Conta não encontrada', 3000);
      return;
    }
    this.route.navigate(['/linhas'], {
      queryParams: { 
        contaId: conta.id,
        tipo: tipoFiltro,
        cliente: this.cliente,
        operadora: operadora,
        conta: contaNome
      }
    });
  }

  // 🆕 MÉTODOS PARA NAVEGAÇÃO DO BREADCRUMB
  navegarParaOperadoras() {
    this.route.navigate(['/operadoras']);
  }

  navegarParaPlanosGeral() {
    this.route.navigate(['/planos']);
  }

  navegarParaLinhasGeral() {
    this.route.navigate(['/linhas']);
  }

}
