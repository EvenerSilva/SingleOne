import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-centro-custos',
  templateUrl: './centro-custos.component.html',
  styleUrls: ['./centro-custos.component.scss']
})
export class CentroCustosComponent implements OnInit, AfterViewInit {

  private session: any = {};
  public colunas = ['nome', 'codigo', 'empresa', 'ativo', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  
  // Propriedades para o modal
  public mostrarFormulario = false;
  public centroEditando: any = null;
  public modoFormulario: 'criar' | 'editar' = 'criar';

  // Getter para dados paginados
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.data) {
      return [];
    }
    
    if (!this.dataSource.paginator) {
      return this.dataSource.data;
    }
    
    const startIndex = this.dataSource.paginator.pageIndex * this.dataSource.paginator.pageSize;
    const endIndex = startIndex + this.dataSource.paginator.pageSize;
    return this.dataSource.data.slice(startIndex, endIndex);
  }

  constructor(
    private util: UtilService, 
    private api: ConfiguracoesApiService, 
    private route: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    try {
      this.session = this.util.getSession('usuario');
      if (!this.session || !this.session.usuario) {
        console.error('Sessão inválida ou usuário não encontrado');
        this.util.exibirFalhaComunicacao();
        return;
      }
      
      this.cliente = this.session.usuario.cliente;
      this.dataSource = new MatTableDataSource<any>([]);
      
      // Garantir que o dataSource tenha dados iniciais
      this.dataSource.data = [];
      
      this.resultado = this.consulta.valueChanges.pipe(
        debounceTime(1000),
        tap(value => this.buscar(value))
      );
      this.resultado.subscribe();
      this.listarCentros();
    } catch (error) {
      console.error('Erro no ngOnInit:', error);
      this.util.exibirFalhaComunicacao();
    }
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

  listarCentros() {
    this.util.aguardar(true);
    this.api.listarCentroCusto("null", this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status != 200 && res.status != 204) {
        console.error('❌ [CENTRO CUSTOS] Erro ao listar centros de custo:', res);
        this.util.exibirFalhaComunicacao();
      } else {
        // Garantir que res.data seja um array
        const dados = Array.isArray(res.data) ? res.data : [];
        this.dataSource = new MatTableDataSource<any>(dados);
        this.configurarPaginador();
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('❌ [CENTRO CUSTOS] Erro na requisição:', err);
      this.util.exibirFalhaComunicacao();
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarCentroCusto(valor, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          console.error('Erro ao buscar centros de custo:', res);
          this.util.exibirFalhaComunicacao();
        } else {
          const dados = Array.isArray(res.data) ? res.data : [];
          this.dataSource = new MatTableDataSource<any>(dados);
          this.configurarPaginador();
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('Erro na busca:', err);
        this.util.exibirFalhaComunicacao();
      })
    } else {
      this.listarCentros();
    }
  }

  editar(obj) {
    this.centroEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  novoCentroCusto() {
    this.centroEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  onCentroSalvo(centro: any) {
    this.mostrarFormulario = false;
    this.centroEditando = null;
    this.modoFormulario = 'criar';
    this.listarCentros();
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.centroEditando = null;
    this.modoFormulario = 'criar';
  }

  excluir(obj) {
    if (confirm('Deseja realmente excluir o centro de custo ' + obj.nome + '?')) {
      this.util.aguardar(true);
      this.api.excluirCentroCusto(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200) {
          this.util.exibirFalhaComunicacao();
        } else {
          this.util.exibirMensagemToast(res.data.Mensagem || 'Centro de custo excluído com sucesso!', 5000);
          this.listarCentros();
        }
      })
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listarCentros();
  }
}
