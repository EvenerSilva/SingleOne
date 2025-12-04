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
  selector: 'app-filiais',
  templateUrl: './filiais.component.html',
  styleUrls: ['./filiais.component.scss']
})
export class FiliaisComponent implements OnInit, AfterViewInit {

  private session: any = {};
  public colunas = ['razaosocial', 'cnpj', 'empresa', 'ativo', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;

  // 🆕 Propriedades para o modal
  public mostrarFormulario = false;
  public filialEditando: any = null;
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
      
      if (!this.session?.usuario?.cliente || !this.session?.token) {
        this.util.exibirFalhaComunicacao();
        return;
      }
      
      this.dataSource = new MatTableDataSource<any>([]);
      
      this.resultado = this.consulta.valueChanges.pipe(
        debounceTime(1000),
        tap(value => this.buscar(value))
      );
      this.resultado.subscribe();
      
      this.listarFiliais();
      
    } catch (error) {
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

  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      return;
    }
    
    try {
      this.dataSource.paginator = this.paginator;
      this.paginator.pageSize = 10;
      this.paginator.pageIndex = 0;
      
      this.paginator.page.subscribe(() => {
        this.cdr.detectChanges();
        this.cdr.markForCheck();
      });
    } catch (error) {
      // Erro silencioso
    }
  }

  listarFiliais() {
    if (!this.session?.usuario?.cliente || !this.session?.token) {
      this.util.exibirFalhaComunicacao();
      return;
    }
    
    this.util.aguardar(true);
    this.api.listarFiliais("", this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      
      // Verificar se res é um erro do Axios
      if (res?.response) {
        this.util.exibirFalhaComunicacao();
        return;
      }
      
      if (res?.status === 200 || res?.status === 204) {
        if (res.data && Array.isArray(res.data)) {
          this.dataSource = new MatTableDataSource<any>(res.data);
          this.cdr.detectChanges();
          
          setTimeout(() => {
            this.configurarPaginador();
            this.cdr.detectChanges();
          }, 100);
        } else {
          this.dataSource = new MatTableDataSource<any>([]);
        }
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(err => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  buscar(valor) {
    if (valor != '' && valor != null) {
      // Busca local
      if (!this.dataSource?.data || this.dataSource.data.length === 0) {
        this.listarFiliais();
        return;
      }
      
      const valorUpper = valor.toUpperCase();
      const dadosFiltrados = this.dataSource.data.filter(filial => 
        (filial.nome && filial.nome.toUpperCase().includes(valorUpper)) ||
        (filial.cnpj && filial.cnpj.includes(valor)) ||
        (filial.empresa?.nome && filial.empresa.nome.toUpperCase().includes(valorUpper))
      );
      this.dataSource = new MatTableDataSource(dadosFiltrados);
      setTimeout(() => {
        this.configurarPaginador();
      }, 100);
    } else {
      this.listarFiliais();
    }
  }

editar(obj) {
    this.filialEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  novaFilial() {
    this.filialEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  // 🆕 Métodos para controlar o modal
  onFilialSalva(filial: any) {
    this.mostrarFormulario = false;
    this.filialEditando = null;
    this.util.exibirMensagemToast('Filial salva com sucesso!', 5000);
    this.listarFiliais(); // Recarregar lista
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.filialEditando = null;
  }

  excluir(obj) {
    if (confirm('Deseja realmente excluir a filial ' + obj.razaosocial + '?')) {
      this.util.aguardar(true);
      this.api.excluirFilial(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200) {
          this.util.exibirFalhaComunicacao();
        } else {
          this.util.exibirMensagemToast('Filial excluida com sucesso!', 5000);
          this.listarFiliais();
        }
      })
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listarFiliais();
  }
}
