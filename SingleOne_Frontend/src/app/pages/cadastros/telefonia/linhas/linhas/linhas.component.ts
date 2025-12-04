import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';
import { TabStateService } from 'src/app/util/tab-state.service';

@Component({
  selector: 'app-linhas',
  templateUrl: './linhas.component.html',
  styleUrls: ['./linhas.component.scss']
})
export class LinhasComponent implements OnInit, AfterViewInit {

  private session:any = {};
  public colunas = ['operadora', 'contrato', 'plano', 'numero', 'iccid', 'emuso', 'acao'];
  
  // 🎯 PROPRIEDADES PARA MODAL
  public mostrarFormulario = false;
  public linhaEditando: any = null;
  public modoFormulario: 'novo' | 'editar' = 'novo';
  
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

  // 🆕 PROPRIEDADES PARA FILTRO DE LINHAS LIVRES
  public filtroLinhasLivresAtivo = false;
  public totalizadores: { livres: number; emUso: number } | null = null;

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
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    // ✅ DEBUG: Verificar propriedades do modal
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

  // listar() {
  //   this.util.aguardar(true);
  //   this.api.listarLinhas("null", this.cliente, this.pagina, this.session.token).then(res => {
  //     this.util.aguardar(false);
  //     if(res.status != 200 && res.status != 204) {
  //       this.util.exibirFalhaComunicacao();
  //     }
  //     else {
  //       this.dataSource = new MatTableDataSource<any>(res.data.results);
  //       this.dataSource.paginator = this.paginator;
  //     }
  //   })
  // }
  listar() {
    this.util.aguardar(true);
    this.api.listarLinhas("null", this.cliente, 1, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        // ✅ CORRIGIDO: Backend agora retorna lista direta (sem paginação)
        this.dataSource = new MatTableDataSource<any>(res.data);
        this.configurarPaginador();
        
        // 🆕 ATUALIZAR TOTALIZADORES APÓS CARREGAR LISTA COMPLETA
        this.atualizarTotalizadores();
      }
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarLinhas(valor, this.cliente, 1, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          // ✅ CORRIGIDO: Backend agora retorna lista direta (sem paginação)
          this.dataSource = new MatTableDataSource<any>(res.data);
          this.configurarPaginador();
        }
      })
    }
    else {
      this.listar();
    }
  }

  // ✅ NOVO: MÉTODO PARA APLICAR FILTRO AUTOMÁTICO
  private aplicarFiltroAutomatico() {
    return new Promise<boolean>((resolve) => {
      this.activatedRoute.queryParams.subscribe(params => {
        const searchParam = params['search'];
        const planoParam = params['plano'];
        const operadoraParam = params['operadora'];
        const contaParam = params['conta'];
        const tipoParam = params['tipo'];
        const contaIdParam = params['contaId'];
        const planoIdParam = params['planoId'];
        const clienteParam = params['cliente'];
        if (contaIdParam && tipoParam && clienteParam) {
          // Filtro por conta e tipo
          this.carregarLinhasPorContaETipo(parseInt(contaIdParam), tipoParam, parseInt(clienteParam));
          resolve(true); // Indica que um filtro foi aplicado
        } else if (planoIdParam && tipoParam && clienteParam) {
          // Filtro por plano e tipo
          this.carregarLinhasPorPlanoETipo(parseInt(planoIdParam), tipoParam, parseInt(clienteParam));
          resolve(true); // Indica que um filtro foi aplicado
        } else if (searchParam) {
          // Filtro genérico (fallback)
          this.consulta.setValue(searchParam);
          this.buscar(searchParam);
          resolve(true); // Indica que um filtro foi aplicado
        } else {
          resolve(false); // Indica que nenhum filtro foi aplicado
        }
      });
    });
  }

  // 🆕 NOVOS MÉTODOS PARA CARREGAR LINHAS COM FILTROS ESPECÍFICOS
  private carregarLinhasPorContaETipo(contaId: number, tipo: string, cliente: number) {
    this.util.aguardar(true);
    
    this.api.listarLinhasPorTipo(contaId, tipo, cliente, 1, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200 && res.data) {
        // ✅ CORRIGIDO: API específica ainda retorna PagedResult
        const dados = res.data.results || res.data;
        this.dataSource = new MatTableDataSource<any>(dados);
        this.configurarPaginador();
        
        // Mostrar mensagem informativa
        const tipoTexto = tipo === 'em-uso' ? 'em uso' : tipo === 'livres' ? 'livre' : '';
        this.util.exibirMensagemToast(`${dados.length} linha(s) ${tipoTexto} encontrada(s).`, 3000);
      } else {
        console.error('[LINHAS] Erro na API específica:', res);
        this.util.exibirFalhaComunicacao();
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[LINHAS] Erro na chamada da API específica:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  private carregarLinhasPorPlanoETipo(planoId: number, tipo: string, cliente: number) {
    this.util.aguardar(true);
    
    this.api.listarLinhasPorPlanoETipo(planoId, tipo, cliente, 1, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200 && res.data) {
        // ✅ CORRIGIDO: API específica ainda retorna PagedResult
        const dados = res.data.results || res.data;
        this.dataSource = new MatTableDataSource<any>(dados);
        this.configurarPaginador();
        
        // Mostrar mensagem informativa
        const tipoTexto = tipo === 'em-uso' ? 'em uso' : tipo === 'livres' ? 'livre' : '';
        this.util.exibirMensagemToast(`${dados.length} linha(s) ${tipoTexto} encontrada(s).`, 3000);
      } else {
        console.error('[LINHAS] Erro na API específica:', res);
        this.util.exibirFalhaComunicacao();
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[LINHAS] Erro na chamada da API específica:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  // 🧹 MÉTODO PARA LIMPAR BUSCA
  limparBusca() {
    this.consulta.setValue('');
    this.listar();
  }

  editar(obj) {
    this.linhaEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  excluir(obj) {
    if(confirm('Deseja realmente excluir a linha ' + obj.numero + '?')) {
      this.util.aguardar(true);
      this.api.excluirLinha(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Linha excluida com sucesso!', 5000);
          this.listar();
        }
      })
    }
  }

  voltarParaTelecom() {
    this.route.navigate(['/telecom']);
  }

  // 🕒 MÉTODO PARA REDIRECIONAR PARA TIMELINE DA LINHA
  redirectToTimelineLinha(id: number) {
    // Buscar a linha pelo ID para obter o número
    const linha = this.dataSource.data.find(l => l.id === id);
    if (linha && linha.numero) {
      this.route.navigate(['relatorios/timeline-recursos'], { queryParams: { sn: linha.numero } });
    } else {
      this.util.exibirMensagemToast('Não foi possível acessar o histórico desta linha', 3000);
    }
  }

  novo() {
    this.linhaEditando = null;
    this.modoFormulario = 'novo';
    this.mostrarFormulario = true;
  }

  // 🎯 MÉTODOS PARA CONTROLE DO MODAL
  onLinhaSalva(linha: any) {
    this.mostrarFormulario = false;
    this.linhaEditando = null;
    this.listar(); // Recarregar a lista
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.linhaEditando = null;
  }

  // 🆕 MÉTODOS PARA NAVEGAÇÃO DO BREADCRUMB
  navegarParaOperadoras() {
    this.route.navigate(['/operadoras']);
  }

  navegarParaContas() {
    this.route.navigate(['/contratos-telefonia']);
  }

  navegarParaPlanos() {
    this.route.navigate(['/planos']);
  }

  // 🆕 MÉTODO PARA FILTRAR LINHAS LIVRES
  filtrarLinhasLivres() {
    if (this.filtroLinhasLivresAtivo) {
      // Se já está ativo, desativar e voltar para lista completa
      this.filtroLinhasLivresAtivo = false;
      this.listar();
      this.util.exibirMensagemToast('Filtro de linhas livres desativado', 2000);
    } else {
      // Ativar filtro e buscar linhas livres
      this.filtroLinhasLivresAtivo = true;
      this.carregarLinhasLivres();
    }
  }

  // 🆕 MÉTODO PARA CARREGAR LINHAS LIVRES
  private async carregarLinhasLivres() {
    this.util.aguardar(true);
    
    try {
      // ✅ CORRIGIDO: Usar os mesmos parâmetros do método listar()
      const res = await this.api.listarLinhas("null", this.cliente, 1, this.session.token);
      
      if (res.status === 200 && res.data) {
        // ✅ CORRIGIDO: Backend agora retorna lista direta
        const todasLinhas = res.data;
        const linhasLivres = todasLinhas.filter(linha => !linha.emuso);
        const linhasEmUso = todasLinhas.filter(linha => linha.emuso);
        
        this.totalizadores = {
          livres: linhasLivres.length,
          emUso: linhasEmUso.length
        };
        this.dataSource = new MatTableDataSource<any>(linhasLivres);
        this.configurarPaginador();
        
        this.util.exibirMensagemToast(`${linhasLivres.length} linha(s) livre(s) encontrada(s)`, 3000);
      } else {
        console.error('[LINHAS] Erro na API:', res);
        this.util.exibirFalhaComunicacao();
      }
    } catch (error) {
      console.error('[LINHAS] Erro ao carregar linhas livres:', error);
      this.util.exibirFalhaComunicacao();
    } finally {
      this.util.aguardar(false);
    }
  }

  // 🆕 MÉTODO PARA ATUALIZAR TOTALIZADORES
  private atualizarTotalizadores() {
    if (!this.dataSource?.data) return;
    
    const todasLinhas = this.dataSource.data;
    const linhasLivres = todasLinhas.filter(linha => !linha.emuso);
    const linhasEmUso = todasLinhas.filter(linha => linha.emuso);
    
    this.totalizadores = {
      livres: linhasLivres.length,
      emUso: linhasEmUso.length
    };
  }

}
