import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { CategoriasApiService, Categoria } from '../../../../api/categorias/categorias-api.service';
import { UtilService } from '../../../../util/util.service';

@Component({
  selector: 'app-categorias',
  templateUrl: './categorias.component.html',
  styleUrls: ['./categorias.component.scss']
})
export class CategoriasComponent implements OnInit, AfterViewInit {

  private session: any = {};
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public mostrarFormulario = false;
  public categoriaEditando: Categoria | null = null;
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
    private categoriasApiService: CategoriasApiService,
    private route: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.listarCategorias();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  listarCategorias() {
    this.util.aguardar(true);
    this.categoriasApiService.listarCategorias().then(res => {
      this.util.aguardar(false);
      if (res.data.sucesso) {
        this.dataSource = new MatTableDataSource<any>(res.data.dados);
        this.configurarPaginador();
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(error => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
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

buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.categoriasApiService.listarCategorias(valor).then(res => {
        this.util.aguardar(false);
        if (res.data.sucesso) {
          this.dataSource = new MatTableDataSource(res.data.dados);
          this.configurarPaginador();
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch(error => {
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
    }
    else {
      this.listarCategorias();
    }
  }

/**
   * Navega para a tela de nova categoria
   */
  public novaCategoria(): void {
    this.modoFormulario = 'criar';
    this.categoriaEditando = null;
    this.mostrarFormulario = true;
  }

  editar(obj) {
    this.modoFormulario = 'editar';
    this.categoriaEditando = { ...obj };
    this.mostrarFormulario = true;
  }

  desativarCategoria(categoria: any) {
    const mensagem = `Tem certeza que deseja desativar a categoria <strong>"${categoria.nome}"</strong>?<br><br>⚠️ <strong>ATENÇÃO:</strong> A categoria será marcada como inativa no sistema, mas os dados serão preservados para auditoria.`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        this.util.aguardar(true);
        
        this.categoriasApiService.desativarCategoria(categoria.id).then(res => {
          this.util.aguardar(false);
          
          // Verificar se a operação foi bem-sucedida
          if (res.status === 200 && res.data?.sucesso) {
            this.util.exibirMensagemToast('Categoria desativada com sucesso!', 5000);
            this.listarCategorias();
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(error => {
          console.error('[CATEGORIAS] Erro ao desativar categoria:', error);
          this.util.aguardar(false);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  reativarCategoria(categoria: any) {
    const mensagem = `Tem certeza que deseja reativar a categoria <strong>"${categoria.nome}"</strong>?<br><br>🔄 A categoria voltará a estar ativa no sistema e poderá ser utilizada normalmente.`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        this.util.aguardar(true);
        
        this.categoriasApiService.reativarCategoria(categoria.id).then(res => {
          this.util.aguardar(false);
          
          // Verificar se a operação foi bem-sucedida
          if (res.status === 200 && res.data?.sucesso) {
            this.util.exibirMensagemToast('Categoria reativada com sucesso!', 5000);
            this.listarCategorias();
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(error => {
          console.error('[CATEGORIAS] Erro ao reativar categoria:', error);
          this.util.aguardar(false);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  onCategoriaSalva(categoria: Categoria): void {
    this.mostrarFormulario = false;
    this.listarCategorias();
    
    if (this.modoFormulario === 'criar') {
      this.util.exibirMensagemToast('Categoria criada com sucesso!', 5000);
    } else {
      this.util.exibirMensagemToast('Categoria atualizada com sucesso!', 5000);
    }
  }

  /**
   * Callback quando formulário é cancelado
   */
  public onCancelado(): void {
    this.mostrarFormulario = false;
    this.categoriaEditando = null;
  }

  // 🧹 MÉTODO PARA LIMPAR BUSCA
  limparBusca() {
    this.consulta.setValue('');
    this.listarCategorias();
  }
}
