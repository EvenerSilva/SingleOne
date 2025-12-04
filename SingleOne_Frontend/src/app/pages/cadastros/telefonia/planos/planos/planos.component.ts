import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';
import { TabStateService } from 'src/app/util/tab-state.service';

@Component({
  selector: 'app-planos',
  templateUrl: './planos.component.html',
  styleUrls: ['./planos.component.scss']
})
export class PlanosComponent implements OnInit, AfterViewInit {

  private session:any = {};
  public colunas = ['operadora', 'contrato', 'nome', 'valor', 'totalizadores', 'status', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  
  // 🎯 PROPRIEDADES PARA MODAL
  public mostrarFormulario = false;
  public planoEditando: any = null;
  public modoFormulario: 'novo' | 'editar' = 'novo';

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
    private activatedRoute: ActivatedRoute,
    private tabState: TabStateService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    
    // Inicializar dataSource vazio
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    // ✅ NOVO: Aplicar filtro automático se houver parâmetro na URL
    // Se não houver filtro automático, então chamar listar()
    this.aplicarFiltroAutomatico().then(temFiltro => {
      if (!temFiltro) {
        this.listar();
      } else {
      }
    });
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
    this.api.listarPlanos("null", 0, this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        res.data.map(x => {
          // ✅ Usar os contadores da view vwplanostelefonia (Pascal Case)
          x.totalLinhas = x.contLinhas || 0;
          x.totalLinhasEmUso = x.contLinhasEmUso || 0;
          x.totalLinhasLivres = x.contLinhasLivres || 0;
          
          // ✅ Mapear campos da view para o formato esperado pela grid
          x.nome = x.plano; // Campo 'plano' da view vira 'nome' na grid
        });
        this.dataSource = new MatTableDataSource<any>(res.data);
        this.configurarPaginador();
      }
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarPlanos(valor, 0, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          res.data.map(x => {
            // ✅ Usar os contadores da view vwplanostelefonia (Pascal Case)
            x.totalLinhas = x.contLinhas || 0;
            x.totalLinhasEmUso = x.contLinhasEmUso || 0;
            x.totalLinhasLivres = x.contLinhasLivres || 0;
            
            // ✅ Mapear campos da view para o formato esperado pela grid
            x.nome = x.plano; // Campo 'plano' da view vira 'nome' na grid
          });
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
    this.planoEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  excluir(obj) {
    if(confirm('Deseja realmente excluir o plano ' + obj.nome + '?')) {
      this.util.aguardar(true);
      this.api.excluirPlano(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Plano excluido com sucesso!', 5000);
          this.listar();
        }
      })
    }
  }

  voltarParaTelecom() {
    this.route.navigate(['/telecom']);
  }

  novo() {
    this.planoEditando = null;
    this.modoFormulario = 'novo';
    this.mostrarFormulario = true;
  }

  // 🎯 MÉTODOS PARA CONTROLE DO MODAL
  onPlanoSalvo(plano: any) {
    this.mostrarFormulario = false;
    this.planoEditando = null;
    this.listar(); // Recarregar a lista
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.planoEditando = null;
  }

  // ✅ NOVO: MÉTODO PARA APLICAR FILTRO AUTOMÁTICO
  private async aplicarFiltroAutomatico() {
    const params = this.activatedRoute.snapshot.queryParams;
    const searchParam = params['search'];
    const planoParam = params['plano'];
    const operadoraParam = params['operadora'];
    const contaParam = params['conta'];
    const tipoParam = params['tipo'];

    if (contaParam && operadoraParam) {
      // 🆕 FILTRO ESPECÍFICO POR CONTA
      await this.carregarPlanosPorConta(contaParam, operadoraParam);
      return true; // Indica que um filtro foi aplicado
    } else if (searchParam) {
      // Filtro genérico (fallback)
      this.consulta.setValue(searchParam);
      this.buscar(searchParam);
      return true; // Indica que um filtro foi aplicado
    }
    return false; // Indica que nenhum filtro foi aplicado
  }

  // 🆕 NOVO: MÉTODO PARA CARREGAR PLANOS POR CONTA
  private async carregarPlanosPorConta(contaNome: string, operadoraNome: string) {
    this.util.aguardar(true);
    
    try {
      // Buscar todos os planos e filtrar por conta
      const res = await this.api.listarPlanos("null", 0, this.cliente, this.session.token);
      this.util.aguardar(false);
      
      if (res.status === 200 && res.data) {
        // Filtrar planos que pertencem à conta específica
        const planosFiltrados = res.data.filter(plano => 
          plano.contrato === contaNome && 
          plano.operadora === operadoraNome
        );
        planosFiltrados.map(x => {
          x.totalLinhas = x.contLinhas || 0;
          x.totalLinhasEmUso = x.contLinhasEmUso || 0;
          x.totalLinhasLivres = x.contLinhasLivres || 0;
          
          // ✅ Mapear campos da view para o formato esperado pela grid
          x.nome = x.plano; // Campo 'plano' da view vira 'nome' na grid
        });
        
        this.dataSource = new MatTableDataSource<any>(planosFiltrados);
        this.configurarPaginador();
        
        // Mostrar mensagem informativa
        this.util.exibirMensagemToast(`${planosFiltrados.length} plano(s) da conta "${contaNome}" encontrado(s).`, 3000);
      } else {
        console.error('[PLANOS] Erro na API:', res);
        this.util.exibirFalhaComunicacao();
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('[PLANOS] Erro ao carregar planos por conta:', error);
      this.util.exibirFalhaComunicacao();
    }
  }

  // 🎯 NOVO: MÉTODO PARA NAVEGAÇÃO FILTRADA PARA LINHAS
  navegarParaLinhas(operadora: string, planoNome: string, tipoFiltro: 'todas' | 'em-uso' | 'livres') {
    const plano = this.dataSource.data.find(p => p.nome === planoNome);
    if (!plano) {
      console.error('[PLANOS] Plano não encontrado:', planoNome);
      this.util.exibirMensagemToast('Erro: Plano não encontrado', 3000);
      return;
    }
    this.route.navigate(['/linhas'], {
      queryParams: { 
        planoId: plano.id,
        tipo: tipoFiltro,
        cliente: this.cliente,
        operadora: operadora,
        plano: planoNome
      }
    });
  }

  // 🆕 MÉTODOS PARA NAVEGAÇÃO DO BREADCRUMB
  navegarParaOperadoras() {
    this.route.navigate(['/operadoras']);
  }

  navegarParaContas() {
    this.route.navigate(['/contratos-telefonia']);
  }

  navegarParaLinhasGeral() {
    this.route.navigate(['/linhas']);
  }

}
