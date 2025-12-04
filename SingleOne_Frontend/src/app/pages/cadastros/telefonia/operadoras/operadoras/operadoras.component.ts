import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';
import { TabStateService } from 'src/app/util/tab-state.service';

@Component({
  selector: 'app-operadoras',
  templateUrl: './operadoras.component.html',
  styleUrls: ['./operadoras.component.scss']
})
export class OperadorasComponent implements OnInit, AfterViewInit {

  private session:any = {};

  // 🆕 Propriedades para o modal
  public mostrarFormulario = false;
  public operadoraEditando: any = null;
  public modoFormulario: 'criar' | 'editar' = 'criar';
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;

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
    
    // 🔍 IMPLEMENTAR FILTRO DE CONSULTA
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    this.listarOperadoras();
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

  listarOperadoras() {
    this.util.aguardar(true);
    this.api.listarOperadoras(this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        if (res.data && Array.isArray(res.data)) {
          res.data.map(x => {
            x.totalPlanos = 0;
            x.totalLinhas = 0;
            x.totalEmUso = 0;
            x.totalLivre = 0;
            
            if (x.telefoniacontratos) {
              x.telefoniacontratos.map(c => {
                x.totalPlanos += c.telefoniaplanos ? c.telefoniaplanos.length : 0;
                if (c.telefoniaplanos) {
                  c.telefoniaplanos.map(p => {
                    if (p.telefonialinhas) {
                      x.totalLinhas += p.telefonialinhas.length;
                      
                      // Contar linhas em uso e livres
                      var emUso = p.telefonialinhas.filter(l => l.emuso == true);
                      var livres = p.telefonialinhas.filter(l => l.emuso == false);
                      
                      x.totalEmUso += emUso.length;
                      x.totalLivre += livres.length;
                    }
                  })
                }
              })
            }
          })
          res.data.forEach((op, index) => {
            // Verificar se há dados que não estão sendo exibidos
            if (op.telefoniacontratos && op.telefoniacontratos.length > 0) {
              // Verificar campos importantes do contrato
              if (op.telefoniacontratos[0].id) {
              }
              if (op.telefoniacontratos[0].cliente) {
              }
              if (op.telefoniacontratos[0].operadora) {
              }
              if (op.telefoniacontratos[0].nome) {
              }
              if (op.telefoniacontratos[0].descricao) {
              }
            }
          });
          this.dataSource = new MatTableDataSource<any>(res.data);
          this.configurarPaginador();
        } else {
          this.dataSource = new MatTableDataSource<any>([]);
        }
      }
    }).catch(error => {
      this.util.aguardar(false);
      if (error.status === 500 || error.response?.status === 500) {
        this.util.exibirFalhaComunicacao();
      } else {
        this.util.exibirFalhaComunicacao();
      }
    })
  }

excluir(obj) {
    if(confirm('Deseja realmente excluir a operadora ' + obj.nome + '?')) {
      this.util.aguardar(true);
      this.api.excluirOperadora(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Operadora excluida com sucesso!', 5000);
          this.listarOperadoras();
        }
      })
    }
  }

  voltarParaTelecom() {
    this.route.navigate(['/telecom']);
  }

  // 🆕 Métodos para controlar o modal
  novo() {
    this.operadoraEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  editar(obj) {
    this.operadoraEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  onOperadoraSalva(operadora: any) {
    this.mostrarFormulario = false;
    this.operadoraEditando = null;
    setTimeout(() => {
      this.listarOperadoras(); // Recarregar lista
    }, 500);
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.operadoraEditando = null;
  }

  // 🔍 MÉTODO DE BUSCA/FILTRO
  buscar(valor: string) {
    if (valor && valor.trim() !== '') {
      this.util.aguardar(true);
      // Filtrar localmente os dados já carregados
      const operadorasFiltradas = this.dataSource?.data?.filter(op => 
        op.nome.toLowerCase().includes(valor.toLowerCase())
      ) || [];
      this.dataSource = new MatTableDataSource<any>(operadorasFiltradas);
      this.configurarPaginador();
      
      this.util.aguardar(false);
    } else {
      // Se o campo estiver vazio, recarregar todas as operadoras
      this.listarOperadoras();
    }
  }

  // 🧹 MÉTODO PARA LIMPAR BUSCA
  limparBusca() {
    this.consulta.setValue('');
    this.listarOperadoras();
  }

  // 🎯 NOVOS MÉTODOS PARA NAVEGAÇÃO FILTRADA
  navegarParaPlanos(operadoraId: number, operadoraNome: string) {
    this.route.navigate(['/planos'], {
      queryParams: { 
        operadora: operadoraId,
        search: operadoraNome 
      }
    });
  }

  // 🆕 MÉTODOS PARA NAVEGAÇÃO DO BREADCRUMB
  navegarParaContas() {
    this.route.navigate(['/contratos-telefonia']);
  }

  navegarParaPlanosGeral() {
    this.route.navigate(['/planos']);
  }

  navegarParaLinhasGeral() {
    this.route.navigate(['/linhas']);
  }

  navegarParaLinhas(operadoraId: number, operadoraNome: string) {
    this.route.navigate(['/linhas'], {
      queryParams: { 
        operadora: operadoraId,
        search: operadoraNome 
      }
    });
  }

}
