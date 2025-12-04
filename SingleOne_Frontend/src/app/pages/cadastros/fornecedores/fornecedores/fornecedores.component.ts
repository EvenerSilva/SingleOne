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
  selector: 'app-fornecedores',
  templateUrl: './fornecedores.component.html',
  styleUrls: ['./fornecedores.component.scss']
})
export class FornecedoresComponent implements OnInit, AfterViewInit {

  private session: any = {};
  public colunas = ['nome', 'cnpj', 'destinadorResiduos', 'ativo', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  
  // Propriedades para o modal
  public mostrarFormulario = false;
  public fornecedorEditando: any = {};
  public modoFormulario: 'criar' | 'editar' = 'criar';

  // Getter para dados paginados
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.paginator) {
      return this.dataSource?.data || [];
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
      
      this.resultado = this.consulta.valueChanges.pipe(
        debounceTime(1000),
        tap(value => this.buscar(value))
      );
      this.resultado.subscribe();
      this.listarFornecedores();
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

  listarFornecedores() {
    this.util.aguardar(true);
    // ✅ CORREÇÃO: Usar string vazia em vez de "null"
    this.api.listarFornecedores("", this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status != 200 && res.status != 204) {
        console.error('Erro ao listar fornecedores:', res);
        this.util.exibirFalhaComunicacao();
      } else {
        this.dataSource = new MatTableDataSource<any>(res.data || []);
        this.configurarPaginador();
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro na requisição:', err);
      this.util.exibirFalhaComunicacao();
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarFornecedores(valor, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          console.error('Erro ao buscar fornecedores:', res);
          this.util.exibirFalhaComunicacao();
        } else {
          this.dataSource = new MatTableDataSource(res.data || []);
          this.configurarPaginador();
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('Erro na busca:', err);
        this.util.exibirFalhaComunicacao();
      })
    } else {
      this.listarFornecedores();
    }
  }

  novoFornecedor() {
    this.fornecedorEditando = {};
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  editar(obj) {
    this.fornecedorEditando = { ...obj };
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  onFornecedorSalvo(fornecedor: any) {
    this.mostrarFormulario = false;
    this.listarFornecedores();
  }

  onCancelado() {
    this.mostrarFormulario = false;
  }

  excluir(obj) {
    if (confirm('Deseja realmente excluir o fornecedor ' + obj.nome + '?')) {
      this.util.aguardar(true);
      this.api.excluirFornecedor(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200) {
          this.util.exibirFalhaComunicacao();
        } else {
          this.util.exibirMensagemToast(res.data.Mensagem || 'Fornecedor excluído com sucesso!', 5000);
          this.listarFornecedores();
        }
      })
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listarFornecedores();
  }
}
