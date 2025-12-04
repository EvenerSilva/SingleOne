import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { UtilService } from 'src/app/util/util.service';
import { saveAs as importedSaveAs } from "file-saver";
import { MatTabGroup } from '@angular/material/tabs';
import { TabStateService } from 'src/app/util/tab-state.service';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';

@Component({
  selector: 'app-equipamentos',
  templateUrl: './equipamentos.component.html',
  styleUrls: ['./equipamentos.component.scss']
})
export class EquipamentosComponent implements OnInit, AfterViewInit {
  public session: any = {};
  public colunas = ['equipamento', 'status', 'localizacao', 'empresa', 'centrocusto', 'acao'];
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  @ViewChild('abas', { static: false }) tabGroup: MatTabGroup;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public filtroStatus = new FormControl();
  public cliente = 0;
  totalLength = 0;
  pageSize = 10;
  currentQuery = '';
  idContrato = null;
  
  // Dados filtrados para exibição
  public dadosFiltrados: any[] = [];
  public dadosOriginais: any[] = [];
  public dadosPagina: any[] = [];
  public currentPageIndex = 0;

  // Filtro exato por status vindo do dashboard
  public exactStatusFilter: string | null = null;

  // Resumo estatístico dos recursos
  public recursosResumo: any = {
    total: 0,
    ativos: 0,           // Entregue
    emTransito: 0,       // Requisitado, Devolvido
    inativos: 0,         // Danificado, Descartado, Roubado, Extraviado, Sinistrado
    emLancamento: 0,     // Novo
    emEstoque: 0         // Em Estoque
  };

  // Filtros do estoque mínimo
  public filtrosEstoqueMinimo: any = null;

  // Tipos de aquisição para cards
  public tiposAquisicao: any[] = [];
  public filtroTipoAquisicao = new FormControl();

  // Modal de exportação
  public showExportModal = false;

  // Modal de Boletim de Ocorrência
  public showBoModal = false;
  public selectedEquipamento: any = null;
  public boData: any = {
    descricao: '',
    anexos: []
  };
  public selectedFiles: File[] = [];
  public saving = false;

  // Modal de Visualização do Recurso
  public showVisualizarModal = false;
  public visualizarRecurso: any = null;

  // Modal de Liberação para Estoque
  public showLiberacaoModal = false;
  public liberacaoRow: any = null;
  public liberacaoData: { localizacao: string; empresa: string; centrocusto: string; localizacaoId?: number, empresaId?: number, centroCustoId?: number } = {
    localizacao: '',
    empresa: '',
    centrocusto: ''
  };
  public liberacaoRequerInfo = false;
  public liberacaoRequerTodos = false;
  public opcoesLocalidades: any[] = [];
  public opcoesEmpresas: any[] = [];
  public opcoesCentrosCusto: any[] = [];

  constructor(
    private util: UtilService,
    private api: EquipamentoApiService,
    private requisicaoApiService: RequisicaoApiService,
    private apiTelefonia: TelefoniaApiService,
    private configApi: ConfiguracoesApiService,
    private router: Router,
    private tabState: TabStateService,
    private ar: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Inicializar dataSource vazio
    this.dataSource = new MatTableDataSource([]);
    
    // Capturar parâmetros de rota
    this.ar.paramMap.subscribe(param => {
      var parametro = param.get('idContrato');
      if(parametro != null) {
        this.idContrato = parametro;
      }
    });
    
    // Capturar parâmetros de query para busca automática e filtros do estoque mínimo
    this.ar.queryParams.subscribe(params => {
      if (params['search']) {
        const searchTerm = params['search'];
        this.consulta.setValue(searchTerm);
        this.currentQuery = searchTerm.trim().toLowerCase();
        this.util.exibirMensagemToast(
          `Busca automática aplicada para: "${searchTerm}"`, 
          3000
        );
      }
      // Novo: filtro direto por status vindo do dashboard (exato)
      const statusParamRaw = params['status'];
      if (statusParamRaw) {
        const statusParam = statusParamRaw.toString().toLowerCase();
        if (statusParam === 'requisitado' || statusParam === 'devolvido') {
          this.exactStatusFilter = statusParam;
          // Opcional: ajustar filtroStatus visual (não obrigatório)
          this.filtroStatus.setValue(null);
        } else {
          this.exactStatusFilter = null;
        }
      } else {
        this.exactStatusFilter = null;
      }
      
      // 🎯 NOVO: Processar filtros do estoque mínimo
      if (params['origemEstoqueMinimo'] === 'true') {
        this.processarFiltrosEstoqueMinimo(params);
      }
    });
    
    // Filtro baseado na consulta
    this.consulta.valueChanges
    .pipe(debounceTime(500))
    .subscribe(value => {
      this.currentQuery = value ? value.trim().toLowerCase() : '';
      this.aplicarFiltros();
    });
    
    // Carregar todos os dados
    this.carregarTodosDados();
    
    // Carregar tipos de aquisição
    this.carregarTiposAquisicao();
  }

  ngAfterViewInit(): void {
    if (this.paginator) {
      this.paginator.pageSize = 10;
      this.dataSource.paginator = this.paginator;
    }
  }

  // Método principal: Carregar todos os dados de uma vez
  carregarTodosDados() {
    const pesquisa = this.consulta.value || '';
    const cliente = this.session.usuario.cliente;
    
    this.listar(pesquisa, cliente);
  }

  // Método principal: Listar equipamentos
  listar(pesquisa: string = '', cliente: number = 0) {
    this.util.aguardar(true);
    
    // Usar idContrato se disponível, senão usar 0 (todos os equipamentos)
    const contratoId = this.idContrato ? parseInt(this.idContrato, 10) : 0;
    // Se há filtros do estoque mínimo, aplicar modelo e localidade
    const modeloId = this.filtrosEstoqueMinimo?.modeloId || null;
    const localidadeId = this.filtrosEstoqueMinimo?.localidadeId || null;
    
    this.api.listarEquipamentos(pesquisa, contratoId, 1, 9999, modeloId, localidadeId).then(res => {
      this.util.aguardar(false);
      
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      } else {
        // Usar estrutura igual aos colaboradores
        this.dadosOriginais = [...res.data.results];
        this.dadosFiltrados = [...res.data.results];
        
        // Mapear tipoAquisicao se necessário (mesmo que no modal individual)
        this.dadosOriginais.forEach((equipamento, index) => {
          if (!equipamento.tipoaquisicao && equipamento.TipoAquisicao) {
            equipamento.tipoaquisicao = equipamento.TipoAquisicao;
          }
        });
        
        // Aplicar o mesmo mapeamento nos dados filtrados
        this.dadosFiltrados.forEach((equipamento, index) => {
          if (!equipamento.tipoaquisicao && equipamento.TipoAquisicao) {
            equipamento.tipoaquisicao = equipamento.TipoAquisicao;
          }
        });
        
        // Configurar MatTableDataSource para paginação local
        this.dataSource = new MatTableDataSource<any>(this.dadosFiltrados);
        
        // Configurar paginador para paginação local
        if (this.paginator) {
          this.paginator.pageIndex = 0;
          this.paginator.pageSize = 10;
          this.paginator.length = this.dadosFiltrados.length;
          this.dataSource.paginator = this.paginator;
        }
        if (this.dadosOriginais.length > 0) {
          const primeiroEquipamento = this.dadosOriginais[0];
        }
        
        // Calcular estatísticas diretamente
        this.calcularEstatisticasSimples();
        
        // Aplicar filtros iniciais
        this.aplicarFiltrosLocais();
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('[EQUIPAMENTOS] ❌ Erro ao carregar dados:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  // Método: Aplicar filtros locais
  aplicarFiltros() {
    this.aplicarFiltrosLocais();
  }

  // Método: Aplicar filtros locais
  aplicarFiltrosLocais(preservePage: boolean = false) {
    if (!this.dadosOriginais || this.dadosOriginais.length === 0) {
      console.warn('[FILTRO] Nenhum dado original disponível para filtrar');
      this.dadosFiltrados = [];
      this.dataSource.data = [];
      this.totalLength = 0;
      this.dadosPagina = [];
      return;
    }

    let dadosFiltrados = [...this.dadosOriginais];

    // Aplicar filtro de texto
    if (this.currentQuery && this.currentQuery.trim() !== '') {
      const query = this.currentQuery.toLowerCase();
      dadosFiltrados = dadosFiltrados.filter(item =>
        (item.tipoequipamento && item.tipoequipamento.toLowerCase().includes(query)) ||
        (item.fabricante && item.fabricante.toLowerCase().includes(query)) ||
        (item.modelo && item.modelo.toLowerCase().includes(query)) ||
        (item.numeroserie && item.numeroserie.toLowerCase().includes(query)) ||
        (item.patrimonio && item.patrimonio.toLowerCase().includes(query)) ||
        (item.equipamentostatus && item.equipamentostatus.toLowerCase().includes(query))
      );
    }

    // Aplicar filtro por tipo de aquisição
    if (this.filtroTipoAquisicao.value !== null && this.filtroTipoAquisicao.value !== undefined) {
      const tipoAquisicaoId = this.filtroTipoAquisicao.value;
      dadosFiltrados = dadosFiltrados.filter(item => {
        const valor = item?.tipoAquisicao || item?.tipoaquisicao || item?.TipoAquisicao;
        const valorNumerico = parseInt(valor, 10);
        return valorNumerico === tipoAquisicaoId;
      });
    }

    // Aplicar filtro de status
    if (this.filtroStatus.value) {
      const norm = (v: any) => (v ?? '').toString().toLowerCase().trim();
      switch (this.filtroStatus.value) {
        case 'ativos':
          dadosFiltrados = dadosFiltrados.filter(item => norm(item.equipamentostatus) === 'entregue');
          break;
        case 'em_transito':
          dadosFiltrados = dadosFiltrados.filter(item => {
            const status = norm(item.equipamentostatus);
            return status === 'requisitado' || status === 'devolvido';
          });
          break;
        case 'inativos':
          dadosFiltrados = dadosFiltrados.filter(item => {
            const status = norm(item.equipamentostatus);
            return status === 'danificado' || status === 'descartado' || 
                   status === 'roubado' || status === 'extraviado' || 
                   status === 'sinistrado';
          });
          break;
        case 'em_lancamento':
          dadosFiltrados = dadosFiltrados.filter(item => norm(item.equipamentostatus) === 'novo');
          break;
        case 'estoque':
          dadosFiltrados = dadosFiltrados.filter(item => norm(item.equipamentostatus) === 'em estoque');
          break;
      }
    }

    // Aplicar filtro exato por status (requisitado/devolvido) vindo do dashboard
    if (this.exactStatusFilter) {
      const norm = (v: any) => (v ?? '').toString().toLowerCase().trim();
      dadosFiltrados = dadosFiltrados.filter(item => norm(item.equipamentostatus) === this.exactStatusFilter);
    }

    this.dadosFiltrados = dadosFiltrados;
    this.dataSource.data = this.dadosFiltrados;
    
    // Atualiza paginação local independente dos cards
    this.totalLength = this.dadosFiltrados.length;
    if (!preservePage) {
      this.currentPageIndex = 0;
    }
    this.atualizarPagina();
  }

  // Método: Atualizar dados da página corrente (paginação local)
  private atualizarPagina() {
    const inicio = this.currentPageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    this.dadosPagina = this.dadosFiltrados.slice(inicio, fim);
  }

  // Método: Calcular estatísticas simples
  calcularEstatisticasSimples() {
    if (!this.dadosOriginais || this.dadosOriginais.length === 0) {
      this.recursosResumo = { 
        total: 0, 
        ativos: 0, 
        emTransito: 0, 
        inativos: 0, 
        emLancamento: 0, 
        emEstoque: 0 
      };
      return;
    }

    const norm = (v: any) => (v ?? '').toString().toLowerCase().trim();
    this.recursosResumo.total = this.dadosOriginais.length;
    this.recursosResumo.ativos = this.dadosOriginais.filter(r => norm(r.equipamentostatus) === 'entregue').length;
    
    this.recursosResumo.emTransito = this.dadosOriginais.filter(r => {
      const s = norm(r.equipamentostatus);
      return s === 'requisitado' || s === 'devolvido';
    }).length;
    
    this.recursosResumo.inativos = this.dadosOriginais.filter(r => {
      const s = norm(r.equipamentostatus);
      return s === 'danificado' || s === 'descartado' || s === 'roubado' || s === 'extraviado' || s === 'sinistrado';
    }).length;
    
    this.recursosResumo.emLancamento = this.dadosOriginais.filter(r => norm(r.equipamentostatus) === 'novo').length;
    
    this.recursosResumo.emEstoque = this.dadosOriginais.filter(r => norm(r.equipamentostatus) === 'em estoque').length;
  }

  // Método: Mudança de página (paginação local)
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPageIndex = event.pageIndex;
    // Atualiza o slice local sem nova requisição
    this.atualizarPagina();
  }

  // Método: Limpar busca
  limparBusca() {
    this.consulta.setValue('');
    this.currentQuery = '';
    this.aplicarFiltros();
  }

  // Método: Verificar se tem filtros ativos
  temFiltrosAtivos(): boolean {
    return !!(
      this.consulta.value || 
      this.filtroStatus.value || 
      this.filtroTipoAquisicao.value
    );
  }

  // Método: Limpar todos os filtros
  limparTodosFiltros() {
    // Limpar filtro de contrato (parâmetro de rota)
    this.idContrato = null;
    
    // Navegar para a rota base /recursos (sem parâmetros de rota ou query)
    this.router.navigate(['/recursos'], {
      queryParams: {}
    });
    
    // Limpar todos os filtros locais
    this.consulta.setValue('');
    this.currentQuery = '';
    this.filtroStatus.setValue(null);
    this.filtroTipoAquisicao.setValue(null);
    this.exactStatusFilter = null;
    this.filtrosEstoqueMinimo = null;
    this.currentPageIndex = 0;
    
    // Recarregar TODOS os dados do servidor (sem filtros)
    this.listar('', this.session.usuario.cliente);
    
    // Notificar usuário
    this.util.exibirMensagemToast('Filtros limpos! Mostrando todos os recursos.', 3000);
  }

  // Método: Exportar dados
  exportarDados() {
  }

  // Método: Obter quantidade de selecionados
  getSelecionados(): number {
    return this.dadosFiltrados ? this.dadosFiltrados.length : 0;
  }

  // Método: Filtrar por card
  filtrarPorCard(status: string | null) {
    this.filtroStatus.setValue(status);
    this.aplicarFiltros();
  }

  // Método: Abrir modal BO
  abrirModalBO(row: any) {
    this.selectedEquipamento = row;
    this.showBoModal = true;
  }

  // Abrir modal para liberar equipamento para estoque
  abrirModalLiberarEstoque(row: any) {
    this.liberacaoRow = row;
    this.liberacaoData = { localizacao: '', empresa: '', centrocusto: '', localizacaoId: undefined, empresaId: undefined, centroCustoId: undefined };
    const isNaoDef = (v: any) => !v || v === 'Nao definido' || v === 'Não definido';
    // Requisitar SEMPRE os campos quando status for "Devolvido" (id == 2)
    this.liberacaoRequerTodos = (row?.equipamentostatusid === 2);
    this.liberacaoRequerInfo = this.liberacaoRequerTodos || isNaoDef(row.localizacao) || isNaoDef(row.empresa) || isNaoDef(row.centrocusto);
    this.showLiberacaoModal = true;
    if (this.liberacaoRequerInfo) {
      this.carregarOpcoesLiberacao();
    }
  }

  closeLiberacaoModal(event: Event) {
    event.stopPropagation();
    this.showLiberacaoModal = false;
    this.liberacaoRow = null;
  }

  get liberacaoValida(): boolean {
    if (!this.liberacaoRequerInfo) return true;
    // Regra: exigir pelo menos Localização e Empresa (Centro de Custo é opcional)
    return !!(this.liberacaoData.localizacaoId && this.liberacaoData.empresaId);
  }

  confirmarLiberacaoEstoque() {
    if (!this.liberacaoRow) return;
    if (!this.liberacaoValida) {
      this.util.exibirMensagemToast('Informe Localização e Empresa.', 4000);
      return;
    }

    // Atualiza associações (se necessário) e prossegue com a liberação
    this.atualizarAssociacoesSeNecessario(this.liberacaoRow)
      .then(() => {
        this.liberarEqpEstoque(this.liberacaoRow);
        this.showLiberacaoModal = false;
      })
      .catch(() => {
        this.util.exibirMensagemToast('Falha ao atualizar dados antes da liberação', 4000);
      });
  }

  private async atualizarAssociacoesSeNecessario(row: any): Promise<void> {
    const temLocal = !!this.liberacaoData.localizacaoId;
    const temEmp = !!this.liberacaoData.empresaId;
    const temCC = !!this.liberacaoData.centroCustoId;
    if (!temLocal && !temEmp && !temCC) return Promise.resolve();

    this.util.aguardar(true);
    // Buscar equipamento completo para evitar erros de payload parcial
    const atual = await this.api.buscarEquipamentoPorId(row.id, this.session.token).then(r => (r && (r.status === 200 || r.status === 204)) ? r.data : null).catch(() => null);
    if (!atual) {
      this.util.aguardar(false);
      return Promise.reject(new Error('Não foi possível carregar o recurso para atualização'));
    }

    if (temLocal) {
      (atual as any).Localidade = this.liberacaoData.localizacaoId; // backend espera 'Localidade' (localidade_id)
      (atual as any).LocalidadeId = this.liberacaoData.localizacaoId;
      (atual as any).localidadeid = this.liberacaoData.localizacaoId;
      (atual as any).localidade_id = this.liberacaoData.localizacaoId;
    }
    if (temEmp) {
      (atual as any).Empresa = this.liberacaoData.empresaId; // backend espera 'Empresa'
      (atual as any).EmpresaId = this.liberacaoData.empresaId;
      (atual as any).empresaid = this.liberacaoData.empresaId;
    }
    if (temCC) {
      (atual as any).Centrocusto = this.liberacaoData.centroCustoId; // backend espera 'Centrocusto'
      (atual as any).CentrocustoId = this.liberacaoData.centroCustoId;
      (atual as any).centrocustoid = this.liberacaoData.centroCustoId;
    }

    return this.api.salvarEquipamento(atual, this.session.token).then(res => {
      this.util.aguardar(false);
      if (!(res && (res.status === 200 || res.status === 204))) {
        return Promise.reject(new Error('Erro ao salvar associações'));
      }
      // Refletir na UI nomes básicos quando possível
      if (temLocal) {
        const loc = this.opcoesLocalidades.find(x => x.id === this.liberacaoData.localizacaoId);
        row.localizacao = loc ? (loc.descricao || loc.nome) : row.localizacao;
      }
      if (temEmp) {
        const emp = this.opcoesEmpresas.find(x => x.id === this.liberacaoData.empresaId);
        row.empresa = emp ? (emp.nome || emp.razaosocial || emp.descricao) : row.empresa;
      }
      if (temCC) {
        const cc = this.opcoesCentrosCusto.find(x => x.id === this.liberacaoData.centroCustoId);
        row.centrocusto = cc ? (cc.nome || cc.descricao) : row.centrocusto;
      }
      // Atualiza arrays locais e re-slice
      const syncLocal = (lista: any[]) => {
        const i = lista.findIndex(x => x.id === row.id);
        if (i >= 0) lista[i] = { ...lista[i], ...row };
      };
      syncLocal(this.dadosOriginais);
      syncLocal(this.dadosFiltrados);
      this.aplicarFiltrosLocais();
      return Promise.resolve();
    }).catch(err => {
      this.util.aguardar(false);
      return Promise.reject(err);
    });
  }

  private carregarOpcoesLiberacao() {
    if (!this.session || !this.session.usuario || !this.session.token) return;
    const cliente = this.session.usuario.cliente;
    const token = this.session.token;

    // Localidades
    this.configApi.listarLocalidades(cliente, token).then((res: any) => {
      if (res && (res.status === 200 || res.status === 204)) {
        this.opcoesLocalidades = res.data || [];
      }
    }).catch(() => {});

    // Empresas
    this.configApi.listarEmpresas('null', cliente, token).then((res: any) => {
      if (res && (res.status === 200 || res.status === 204)) {
        this.opcoesEmpresas = res.data?.results || res.data || [];
      }
    }).catch(() => {});
  }

  onEmpresaLiberacaoChange() {
    this.liberacaoData.centroCustoId = undefined;
    this.opcoesCentrosCusto = [];
    if (!this.liberacaoData.empresaId) return;
    this.configApi.listarCentroCustoDaEmpresa(this.liberacaoData.empresaId, this.session.token)
      .then((res: any) => {
        if (res && (res.status === 200 || res.status === 204)) {
          this.opcoesCentrosCusto = res.data || [];
        }
      }).catch(() => {});
  }

  // Método: Liberar equipamento para estoque (executa a API)
  private liberarEqpEstoque(row: any) {
    if (!row || !row.id) {
      this.util.exibirMensagemToast('Recurso inválido para liberar', 3000);
      return;
    }

    if (!this.session || !this.session.usuario || !this.session.token) {
      this.util.exibirFalhaComunicacao();
      return;
    }

    const usuarioId = this.session.usuario.id;
    const equipamentoId = row.id;
    this.util.aguardar(true);
    this.api.liberarEquipamentoParaEstoque(usuarioId, equipamentoId, this.session.token)
      .then(res => {
        if (res && (res.status === 200 || res.status === 204)) {
          // Rebuscar o recurso atualizado e refletir na UI
          return this.api.buscarEquipamentoPorId(equipamentoId, this.session.token).then(refresh => {
            this.util.aguardar(false);
            if (refresh && (refresh.status === 200 || refresh.status === 204)) {
              const atualizado = refresh.data;
              // Força status local para "Em Estoque" caso backend ainda não replique imediatamente
              const atualizadoComStatus = { ...atualizado } as any;
              atualizadoComStatus.equipamentostatus = 'Em Estoque';
              // Padrão adotado em outras telas: Em Estoque = 3
              atualizadoComStatus.equipamentostatusid = 3;
              // Atualiza nos arrays locais
              const updateLocal = (lista: any[]) => {
                const idx = lista.findIndex(x => x.id === equipamentoId);
                if (idx >= 0) {
                  const anterior = lista[idx];
                  const merged: any = { ...anterior, ...atualizadoComStatus };
                  // Preserva nomes locais se a API ainda não retornou (evita "Não definido")
                  if (!atualizadoComStatus.localizacao && anterior.localizacao) merged.localizacao = anterior.localizacao;
                  if (!atualizadoComStatus.empresa && anterior.empresa) merged.empresa = anterior.empresa;
                  if (!atualizadoComStatus.centrocusto && anterior.centrocusto) merged.centrocusto = anterior.centrocusto;
                  // Se backend devolveu IDs numéricos e já temos nomes (string), preserva nomes
                  if (typeof atualizadoComStatus.localizacao === 'number' && typeof anterior.localizacao === 'string') {
                    merged.localizacao = anterior.localizacao;
                  }
                  if (typeof atualizadoComStatus.empresa === 'number' && typeof anterior.empresa === 'string') {
                    merged.empresa = anterior.empresa;
                  }
                  if (typeof atualizadoComStatus.centrocusto === 'number' && typeof anterior.centrocusto === 'string') {
                    merged.centrocusto = anterior.centrocusto;
                  }
                  // Evita trocar nomes por IDs (quando BuscarEquipamentoPorId retorna numéricos)
                  if (typeof atualizadoComStatus.tipoequipamento === 'number' && anterior.tipoequipamento) {
                    merged.tipoequipamento = anterior.tipoequipamento;
                  }
                  if (typeof atualizadoComStatus.fabricante === 'number' && anterior.fabricante) {
                    merged.fabricante = anterior.fabricante;
                  }
                  if (typeof atualizadoComStatus.modelo === 'number' && anterior.modelo) {
                    merged.modelo = anterior.modelo;
                  }
                  if (typeof atualizadoComStatus.equipamentostatus === 'number' && anterior.equipamentostatus) {
                    merged.equipamentostatus = anterior.equipamentostatus;
                  }
                  lista[idx] = merged;
                }
              };
              updateLocal(this.dadosOriginais);
              updateLocal(this.dadosFiltrados);
              // Reaplica filtros e paginação preservando a página atual
              this.aplicarFiltrosLocais(true);
              this.util.exibirMensagemToast('Recurso liberado para estoque com sucesso', 4000);
            } else {
              this.util.exibirMensagemToast('Liberado, mas falhou ao atualizar a visualização', 4000);
            }
          });
        } else {
          this.util.aguardar(false);
          this.util.exibirMensagemToast('Falha ao liberar para estoque', 4000);
        }
      })
      .catch(err => {
        this.util.aguardar(false);
        console.error('[EQUIPAMENTOS] Erro ao liberar para estoque:', err);
        this.util.exibirFalhaComunicacao();
      });
  }

  // Método: Redirecionar para timeline
  redirectToTimeline(id: number) {
    // ✅ CORREÇÃO: Buscar o recurso pelo ID para obter o S/N e então navegar
    const recurso = this.dadosFiltrados.find(r => r.id === id) || this.dadosOriginais.find(r => r.id === id);
    if (recurso && recurso.numeroserie) {
      this.router.navigate(['relatorios/timeline-recursos'], { queryParams: { sn: recurso.numeroserie } });
    } else {
      this.util.exibirMensagemToast('Não foi possível acessar o timeline deste recurso', 3000);
    }
  }

  // Método: Abrir modal visualizar
  abrirModalVisualizar(row: any) {
    this.visualizarRecurso = row;
    this.showVisualizarModal = true;
  }

  // Método: Reativar equipamento
  reativarEquipamento(row: any) {
  }

  // Método: Ir para entrega
  irParaEntrega(row: any) {
    this.router.navigate(['/movimentacoes/requisicoes'], { queryParams: { search: row.numeroserie, source: 'recursos' } });
  }

  // Método: Requisitar equipamento
  requisitarEquipamento(row: any) {
    // Navegar sem :id para abrir fluxo de NOVA requisição e pré-preencher via query params
    this.router.navigate(['nova-requisicao'], {
      queryParams: {
        equipamentoId: row.id,
        numeroSerie: row.numeroserie,
        tipoEquipamento: row.tipoequipamento,
        fabricante: row.fabricante,
        modelo: row.modelo,
        patrimonio: row.patrimonio
      }
    });
  }

  // Método: Fechar modal de exportação
  closeExportModal(event: Event) {
    event.stopPropagation();
    this.showExportModal = false;
  }

  // Método: Exportar para Excel
  exportarParaExcel() {
  }

  // Método: Exportar para CSV
  exportarParaCSV() {
  }

  // Método: Fechar modal BO
  closeBoModal(event: Event) {
    event.stopPropagation();
    this.showBoModal = false;
    this.selectedEquipamento = null;
    this.boData = { descricao: '', anexos: [] };
    this.selectedFiles = [];
  }

  // Método: Selecionar arquivo
  onFileSelected(event: any) {
    const files = event.target.files;
    for (let i = 0; i < files.length; i++) {
      this.selectedFiles.push(files[i]);
    }
  }

  // Método: Remover arquivo
  removeFile(index: number) {
    this.selectedFiles.splice(index, 1);
  }

  // Método: Salvar BO
  salvarBO() {
    this.saving = true;
    // Implementar lógica se necessário
    setTimeout(() => {
      this.saving = false;
      this.closeBoModal(new Event('click'));
    }, 2000);
  }

  // Método: Fechar modal visualizar
  closeVisualizarModal(event: Event) {
    event.stopPropagation();
    this.showVisualizarModal = false;
    this.visualizarRecurso = null;
  }

  // Getters para verificar status
  get deveMostrarTabela(): boolean {
    return this.dadosFiltrados && this.dadosFiltrados.length > 0;
  }

  get dadosVazios(): boolean {
    return !this.dadosFiltrados || this.dadosFiltrados.length === 0;
  }

  get temErroCarregamento(): boolean {
    return false;
  }

  get backendOffline(): boolean {
    return false;
  }

  get temDados(): boolean {
    return this.dadosFiltrados && this.dadosFiltrados.length > 0;
  }

  get temErro(): boolean {
    return false;
  }

  // Método: Obter texto do tipo de aquisição
  getTipoAquisicaoText(equipamento: any): string {
    // Tentar extrair o valor do tipo de aquisição
    const valor = equipamento?.tipoAquisicao || equipamento?.tipoaquisicao || equipamento?.TipoAquisicao;
    
    // Converter para número se for string
    const valorNumerico = parseInt(valor, 10);
    
    switch (valorNumerico) {
      case 1:
        return 'Alugado';
      case 2:
        return 'Próprio';
      case 3:
        return 'Corporativo';
      default:
        return 'N/A';
    }
  }

  // Método: Obter classe CSS do tipo de aquisição
  getTipoAquisicaoClass(equipamento: any): string {
    // Tentar extrair o valor do tipo de aquisição
    const valor = equipamento?.tipoAquisicao || equipamento?.tipoaquisicao || equipamento?.TipoAquisicao;
    const valorNumerico = parseInt(valor, 10);
    
    switch (valorNumerico) {
      case 1:
        return 'tipo-alugado';
      case 2:
        return 'tipo-proprio';
      case 3:
        return 'tipo-corporativo';
      default:
        return 'tipo-indefinido';
    }
  }

  // Método: Calcular status da garantia para modal de visualização
  getStatusGarantiaVisualizar(dataGarantia: any): { text: string, class: string } | null {
    if (!dataGarantia) {
      return null;
    }

    try {
      const dataGarantiaObj = new Date(dataGarantia);
      const hoje = new Date();
      
      // Zerar as horas para comparar apenas as datas
      hoje.setHours(0, 0, 0, 0);
      dataGarantiaObj.setHours(0, 0, 0, 0);
      
      if (isNaN(dataGarantiaObj.getTime())) {
        return null;
      }

      const diferencaMs = dataGarantiaObj.getTime() - hoje.getTime();
      const diferencaDias = Math.ceil(diferencaMs / (1000 * 60 * 60 * 24));

      if (diferencaDias < 0) {
        // Garantia vencida
        const diasVencida = Math.abs(diferencaDias);
        return {
          text: `⚠️ Garantia vencida há ${diasVencida} ${diasVencida === 1 ? 'dia' : 'dias'}`,
          class: 'garantia-vencida'
        };
      } else if (diferencaDias === 0) {
        // Vence hoje
        return {
          text: '⚠️ Garantia vence hoje',
          class: 'garantia-hoje'
        };
      } else if (diferencaDias <= 30) {
        // Vence em breve (30 dias ou menos)
        return {
          text: `⚠️ Garantia vence em ${diferencaDias} ${diferencaDias === 1 ? 'dia' : 'dias'}`,
          class: 'garantia-proximo-vencimento'
        };
      } else {
        // Garantia válida
        return {
          text: `✅ Garantia válida por mais ${diferencaDias} ${diferencaDias === 1 ? 'dia' : 'dias'}`,
          class: 'garantia-valida'
        };
      }
    } catch (error) {
      console.error('❌ [EQUIPAMENTOS] Erro ao calcular status da garantia:', error);
      return null;
    }
  }

  // Método: Verificar se data é menor que hoje (método existente)
  dataMenorQueHoje(data: any): boolean {
    if (!data) return false;
    try {
      const dataObj = new Date(data);
      const hoje = new Date();
      hoje.setHours(0, 0, 0, 0);
      dataObj.setHours(0, 0, 0, 0);
      return dataObj < hoje;
    } catch {
      return false;
    }
  }

  // Método: Obter classe de status (método existente)
  getStatusClass(status: string): string {
    if (!status) return 'status-indefinido';
    
    const statusLower = status.toLowerCase();
    
    if (statusLower.includes('entregue')) return 'status-entregue';
    if (statusLower.includes('estoque')) return 'status-estoque';
    if (statusLower.includes('requisitado')) return 'status-requisitado';
    if (statusLower.includes('devolvido')) return 'status-devolvido';
    if (statusLower.includes('danificado')) return 'status-danificado';
    if (statusLower.includes('descartado')) return 'status-descartado';
    if (statusLower.includes('roubado')) return 'status-roubado';
    if (statusLower.includes('extraviado')) return 'status-extraviado';
    if (statusLower.includes('sinistrado')) return 'status-sinistrado';
    if (statusLower.includes('novo')) return 'status-novo';
    
    return 'status-indefinido';
  }

  // Método: Obter nome do tipo de aquisição (fallback)
  getTipoAquisicaoNome(tipoAquisicaoId: number): string {
    if (!tipoAquisicaoId) return 'N/A';
    
    switch (tipoAquisicaoId) {
      case 1: return 'Alugado';
      case 2: return 'Particular';
      case 3: return 'Corporativo';
      default: return 'N/A';
    }
  }

  // Método: Carregar tipos de aquisição
  carregarTiposAquisicao() {
    this.configApi.listarTiposAquisicao(this.session.token).then(res => {
      if (res.status === 200) {
        this.tiposAquisicao = res.data || [];
      } else {
        console.error('❌ [EQUIPAMENTOS] Erro ao carregar tipos de aquisição:', res);
        this.tiposAquisicao = [];
      }
    }).catch(error => {
      console.error('❌ [EQUIPAMENTOS] Erro na API de tipos de aquisição:', error);
      this.tiposAquisicao = [];
    });
  }

  // Método: Filtrar por tipo de aquisição
  filtrarPorTipoAquisicao(tipoAquisicaoId: number | null) {
    this.filtroTipoAquisicao.setValue(tipoAquisicaoId);
    this.aplicarFiltros();
  }

  // Método: Obter contagem de recursos por tipo de aquisição
  getContagemPorTipoAquisicao(tipoAquisicaoId: number): number {
    if (!this.dadosOriginais || this.dadosOriginais.length === 0) return 0;
    
    return this.dadosOriginais.filter(equipamento => {
      const valor = equipamento?.tipoAquisicao || equipamento?.tipoaquisicao || equipamento?.TipoAquisicao;
      const valorNumerico = parseInt(valor, 10);
      return valorNumerico === tipoAquisicaoId;
    }).length;
  }

  // Método: Obter ícone do tipo de aquisição
  getTipoAquisicaoIcon(tipoAquisicaoId: number): string {
    switch (tipoAquisicaoId) {
      case 1: return 'cil-calendar-check'; // Alugado (contrato periódico)
      case 2: return 'cil-user'; // Particular
      case 3: return 'cil-building'; // Corporativo
      default: return 'cil-devices';
    }
  }

  // Método: Obter exibição do contrato
  getContratoDisplay(recurso: any): string {
    if (!recurso) return 'N/A';
    
    // Se tem contrato definido e não é "Nao definido"
    if (recurso.contrato && recurso.contrato !== 'Nao definido' && recurso.contrato.trim() !== '') {
      return recurso.contrato;
    }
    
    // Se tem contratoid (ID do contrato) mas não tem descrição
    if (recurso.contratoid && recurso.contratoid !== 'Nao definido') {
      return `Contrato ID: ${recurso.contratoid}`;
    }
    
    // Se não tem contrato associado, verificar se é tipo alugado
    if (recurso.tipoAquisicaoNome === 'Alugado' || recurso.tipoaquisicao === 1) {
      return 'Contrato não informado (Tipo: Alugado)';
    }
    
    // Para outros tipos, não é obrigatório ter contrato
    return 'Não aplicável';
  }

  // 🎯 NOVO: Processar filtros vindos do estoque mínimo
  private processarFiltrosEstoqueMinimo(params: any) {
    const filtros = {
      modeloId: params['modeloId'] ? parseInt(params['modeloId']) : null,
      localidadeId: params['localidadeId'] ? parseInt(params['localidadeId']) : null,
      clienteId: params['clienteId'] ? parseInt(params['clienteId']) : null,
      modeloDescricao: params['modeloDescricao'] || 'N/A',
      fabricanteDescricao: params['fabricanteDescricao'] || 'N/A',
      localidadeDescricao: params['localidadeDescricao'] || 'N/A'
    };
    this.filtrosEstoqueMinimo = filtros;
    
    // Exibir mensagem informativa
    this.util.exibirMensagemToast(
      `Filtros aplicados: ${filtros.fabricanteDescricao} - ${filtros.modeloDescricao} em ${filtros.localidadeDescricao}`,
      4000
    );
    // Recarregar dados com filtros aplicados na API
    setTimeout(() => {
      this.carregarTodosDados();
    }, 1000);
  }

  // 🎯 NOVO: Aplicar filtros específicos do estoque mínimo
  private aplicarFiltrosEstoqueMinimo(filtros: any) {
    if (!this.dataSource.data || this.dataSource.data.length === 0) {
      return;
    }
    const primeiroEquipamento = this.dataSource.data[0];
    
    // Filtrar por modelo e localidade
    const dadosFiltrados = this.dataSource.data.filter(equipamento => {
      // Corrigir comparação: usar IDs corretos
      let modeloMatch = true;
      let localidadeMatch = true;
      
      if (filtros.modeloId) {
        // Usar modeloid que é o campo correto
        modeloMatch = equipamento.modeloid === filtros.modeloId;
      }
      
      if (filtros.localidadeId) {
        // Tentar diferentes campos para a localidade
        const localidadeEncontrada = equipamento.localidade === filtros.localidadeId || 
                                   equipamento.localidadeid === filtros.localidadeId ||
                                   equipamento.localidade_id === filtros.localidadeId ||
                                   (equipamento.localidadeNavigation && equipamento.localidadeNavigation.id === filtros.localidadeId);
        
        // Se não encontrou localidade, verificar se há algum campo de localidade disponível
        if (!localidadeEncontrada) {
          // Se todos os campos de localidade estão undefined, considerar como match
          const todosCamposLocalidadeUndefined = equipamento.localidade === undefined && 
                                               equipamento.localidadeid === undefined && 
                                               equipamento.localidade_id === undefined && 
                                               equipamento.localidadeNavigation === undefined;
          
          if (todosCamposLocalidadeUndefined) {
            localidadeMatch = true; // Considerar como match se não há localidade definida
          } else {
            localidadeMatch = false;
          }
        } else {
          localidadeMatch = true;
        }
      }
      
      return modeloMatch && localidadeMatch;
    });
    const equipamentosModelo3 = this.dataSource.data.filter(e => e.modeloid === 3);
    
    // Verificar se há equipamentos sem localidade definida
    const equipamentosSemLocalidade = dadosFiltrados.filter(e => 
      e.localidade === undefined && 
      e.localidadeid === undefined && 
      e.localidade_id === undefined && 
      e.localidadeNavigation === undefined
    );
    if (equipamentosSemLocalidade.length > 0) {
    }
    
    // Atualizar dataSource com dados filtrados
    this.dataSource.data = dadosFiltrados;
    
    // Atualizar resumo com dados filtrados
    const norm = (v: any) => (v ?? '').toString().toLowerCase().trim();
    
    this.recursosResumo.total = dadosFiltrados.length;
    this.recursosResumo.ativos = dadosFiltrados.filter(r => norm(r.equipamentostatus) === 'entregue').length;
    this.recursosResumo.emTransito = dadosFiltrados.filter(r => {
      const status = norm(r.equipamentostatus);
      return status === 'em transito' || status === 'em trânsito';
    }).length;
    this.recursosResumo.inativos = dadosFiltrados.filter(r => {
      const status = norm(r.equipamentostatus);
      return status === 'inativo' || status === 'desativado';
    }).length;
    this.recursosResumo.emLancamento = dadosFiltrados.filter(r => norm(r.equipamentostatus) === 'novo').length;
    this.recursosResumo.devolvido = dadosFiltrados.filter(r => norm(r.equipamentostatus) === 'devolvido').length;
    this.recursosResumo.emEstoque = dadosFiltrados.filter(r => norm(r.equipamentostatus) === 'em estoque').length;
    let mensagemResultado = `${dadosFiltrados.length} equipamentos encontrados para o filtro aplicado`;
    
    if (equipamentosSemLocalidade.length > 0) {
      mensagemResultado += ` (${equipamentosSemLocalidade.length} sem localidade definida)`;
    }
    
    this.util.exibirMensagemToast(mensagemResultado, 4000);
  }
}
