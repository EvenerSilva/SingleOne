import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { CategoriasApiService } from 'src/app/api/categorias/categorias-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil, debounceTime, tap } from 'rxjs/operators';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-equipamentos-status-detalhe',
  templateUrl: './equipamentos-status-detalhe.component.html',
  styleUrls: ['./equipamentos-status-detalhe.component.scss']
})
export class EquipamentosStatusDetalheComponent implements OnInit, OnDestroy {

  private session: any = {};
  public relatorio: any = {};
  public localidades: any = [];
  public tipoEquipamentos: any = [];
  public statusEqp: any = [];
  public detalhes: any = [];
  public empresas: any = [];
  public centros: any = [];
  public fabricantes: any = [];
  public modelos: any = [];
  public categorias: any = [];
  public tiposAquisicao: any = [];
  public colunas = ['equipamento', 'numeroserie', 'patrimonio', 'status', 'localizacao', 'empresa', 'centro'];
  
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;

  // Paginação local (como em auditoria-acessos)
  public dadosPagina: any[] = [];
  public totalLength = 0;
  public pageSize = 10;
  public currentPageIndex = 0;
  public todosEquipamentos: any[] = []; // Array completo de equipamentos

  // Novas propriedades para modernização
  public loading: boolean = false;
  public searchTerm: string = '';
  public showExportModal: boolean = false;
  private destroy$ = new Subject<void>();

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService, 
    private apiCad: ConfiguracoesApiService, 
    private apiEqp: EquipamentoApiService,
    private categoriasApi: CategoriasApiService,
    private router: Router
  ) {
    // Inicializar dataSource vazio
    this.dataSource = new MatTableDataSource<any>([]);
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.listarCampos();
    this.setupSearchFilter();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearchFilter(): void {
    // Implementar filtro de busca se necessário
  }

  listarCampos() {
    this.loading = true;
    this.apiCad.listarLocalidades(this.session.usuario.cliente, this.session.token).then(res => {
      this.localidades = this.uniqueById(res.data).sort(this.byDescricao);
      this.apiCad.listarTiposRecursos("null", this.session.usuario.cliente, this.session.token).then(res => {
        this.tipoEquipamentos = this.uniqueById(res.data).sort(this.byDescricao);
        this.apiEqp.listarStatusEquipamentos(this.session.token).then(res => {
          this.statusEqp = this.uniqueById(res.data).sort(this.byDescricao);
          this.apiCad.listarEmpresas("null", this.session.usuario.cliente, this.session.token).then(res => {
            // Deduplicar por ID e depois por nome
            const uniqueById = this.uniqueById(res.data);
            this.empresas = this.uniqueByNome(uniqueById).sort(this.byDescricao);
            // Inicialmente não carregar fabricantes/modelos. Aguardará seleção do tipo.
            this.fabricantes = [];
            this.modelos = [];
            // Carregar categorias e tipos de aquisição
            this.categoriasApi.listarCategorias().then(cr => {
              const catData = (cr.data?.dados || cr.data || []) as any[];
              this.categorias = this.uniqueById(catData).sort(this.byDescricaoCategoria);
              this.apiCad.listarTiposAquisicao(this.session.token).then(ta => {
                this.tiposAquisicao = this.uniqueById(ta.data || []).sort(this.byDescricao);
                this.loading = false;
              })
            })
          })
        })
      })
    })
  }

  onTipoChange(): void {
    const tipoId = this.relatorio.tipoequipamentoid || 0;
    this.relatorio.fabricanteid = '';
    this.relatorio.modeloid = '';
    this.fabricantes = [];
    this.modelos = [];
    if (!tipoId) {
      // Se limpar o tipo, recarregar fabricantes e modelos gerais
      this.apiCad.listarFabricantes('null', this.session.usuario.cliente, this.session.token).then(fr => {
        this.fabricantes = this.uniqueById(fr.data || []).sort(this.byDescricao);
      });
      this.apiCad.listarModelos('null', this.session.usuario.cliente, this.session.token).then(mr => {
        this.modelos = this.uniqueById(mr.data || []).sort(this.byDescricao);
      });
      return;
    }
    this.apiCad.listarFabricantesPorTipoRecurso(tipoId, this.session.usuario.cliente, this.session.token).then(fr => {
      this.fabricantes = this.uniqueById(fr.data || []).sort(this.byDescricao);
    });
  }

  onFabricanteChange(): void {
    const fabricanteId = this.relatorio.fabricanteid || 0;
    this.modelos = [];
    if (!fabricanteId) {
      // Se limpar fabricante, recarrega todos modelos genéricos
      this.apiCad.listarModelos('null', this.session.usuario.cliente, this.session.token).then(mr => {
        this.modelos = this.uniqueById(mr.data || []).sort(this.byDescricao);
      });
      return;
    }
    this.apiCad.listarModelosDoFabricante(fabricanteId, this.session.usuario.cliente, this.session.token).then(mr => {
      this.modelos = this.uniqueById(mr.data || []).sort(this.byDescricao);
    });
  }

  consultar() {
    this.relatorio.cliente = this.session.usuario.cliente;
    this.loading = true;
    // Garantir que IDs vazios não quebrem o backend
    const payload: any = { ...this.relatorio };
    ['localidadeid','tipoequipamentoid','equipamentostatusid','empresaid','centrocustoid','fabricanteid','modeloid','categoriaid','tipoaquisicao']
      .forEach(k => {
        if (payload[k] === '' || payload[k] === undefined) payload[k] = null;
        if (payload[k] !== null) {
          const num = Number(payload[k]);
          payload[k] = isNaN(num) ? payload[k] : num;
        }
      });
    this.api.consultarDetalhesEquipamentos(payload, this.session.token).then(res => {
      this.loading = false;
      if (res.status === 200 && res.data) {
        this.todosEquipamentos = res.data;
        
        // Configurar paginação local (como em auditoria-acessos)
        this.totalLength = this.todosEquipamentos.length;
        this.currentPageIndex = 0;
        this.atualizarPagina();
      } else {
        this.todosEquipamentos = [];
        this.dadosPagina = [];
        this.totalLength = 0;
      }
    }).catch(error => {
      this.loading = false;
      console.error('[EQUIPAMENTOS-DETALHE] Erro ao consultar:', error);
      this.util.exibirFalhaComunicacao();
    })
  }

  // Utilidades: deduplicar por id e ordenadores
  private uniqueById<T extends { id?: any }>(arr: T[]): T[] {
    if (!Array.isArray(arr)) return [] as T[];
    const seen = new Set<any>();
    const result: T[] = [];
    for (const item of arr) {
      const key = item?.id ?? JSON.stringify(item);
      if (!seen.has(key)) {
        seen.add(key);
        result.push(item);
      }
    }
    return result;
  }
  
  private uniqueByNome<T extends { nome?: string, descricao?: string, id?: any }>(arr: T[]): T[] {
    if (!Array.isArray(arr)) return [] as T[];
    const seen = new Map<string, T>();
    for (const item of arr) {
      const nome = (item?.nome || item?.descricao || '').trim().toLowerCase();
      if (nome && !seen.has(nome)) {
        seen.set(nome, item);
      }
    }
    return Array.from(seen.values());
  }

  private byDescricao = (a: any, b: any) => {
    const ad = (a?.descricao || a?.nome || '').toString().toLowerCase();
    const bd = (b?.descricao || b?.nome || '').toString().toLowerCase();
    return ad.localeCompare(bd);
  };

  private byDescricaoCategoria = (a: any, b: any) => {
    const ad = (a?.nome || a?.descricao || '').toString().toLowerCase();
    const bd = (b?.nome || b?.descricao || '').toString().toLowerCase();
    return ad.localeCompare(bd);
  };

  listarCentrosCustos() {
    this.loading = true;
    this.apiCad.listarCentroCustoDaEmpresa(this.relatorio.empresaid, this.session.token).then(res => {
      this.loading = false;
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      } else {
        this.centros = res.data;
      }
    })
  }

  limparBusca(): void {
    this.searchTerm = '';
    this.relatorio = {
      categoriaid: null,
      tipoequipamentoid: null,
      fabricanteid: null,
      modeloid: null,
      equipamentostatusid: null,
      tipoaquisicao: null,
      empresaid: null,
      localidadeid: null,
      centrocustoid: null
    } as any;
    this.centros = [];
    this.todosEquipamentos = [];
    this.dadosPagina = [];
    this.totalLength = 0;
    this.currentPageIndex = 0;
    if (this.dataSource) {
      this.dataSource.data = [];
    }
  }

  // Método: Mudança de página (paginação local - como em auditoria-acessos)
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPageIndex = event.pageIndex;
    this.atualizarPagina();
  }

  // Método: Atualizar dados da página corrente (paginação local)
  private atualizarPagina() {
    const inicio = this.currentPageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    this.dadosPagina = this.todosEquipamentos.slice(inicio, fim);
  }

  // Métodos para métricas
  getTotalEquipamentos(): number {
    return this.todosEquipamentos?.length || 0;
  }

  getTotalStatus(): number {
    if (!this.todosEquipamentos) return 0;
    const uniqueStatus = new Set(this.todosEquipamentos.map(item => item.equipamentostatus));
    return uniqueStatus.size;
  }

  getTotalLocalidades(): number {
    if (!this.todosEquipamentos) return 0;
    const uniqueLocalidades = new Set(this.todosEquipamentos.map(item => item.localidade));
    return uniqueLocalidades.size;
  }

  // Métodos de exportação
  exportarDados(): void {
    this.showExportModal = true;
  }

  closeExportModal(event: Event): void {
    this.showExportModal = false;
  }

  exportarParaExcel(): void {
    try {
      if (!this.todosEquipamentos || this.todosEquipamentos.length === 0) {
        this.util.exibirMensagemToast('Nenhum dado para exportar', 3000);
        return;
      }

      const dados = this.prepararDadosParaExportacao();
      const dataAtual = new Date().toISOString().split('T')[0];
      
      // Criar worksheet
      const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dados);
      
      // Ajustar largura das colunas
      const colWidths = [
        { wch: 25 }, // Tipo de Equipamento
        { wch: 20 }, // Fabricante
        { wch: 20 }, // Modelo
        { wch: 20 }, // Número de Série
        { wch: 15 }, // Patrimônio
        { wch: 15 }, // Status
        { wch: 20 }, // Localização
        { wch: 25 }, // Empresa
        { wch: 25 }  // Centro de Custo
      ];
      ws['!cols'] = colWidths;
      
      // Criar workbook
      const wb: XLSX.WorkBook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, 'Equipamentos');
      
      // Fazer download
      const nomeArquivo = `equipamentos-status-detalhe-${dataAtual}.xlsx`;
      XLSX.writeFile(wb, nomeArquivo);
      
      this.closeExportModal(new Event('click'));
      this.util.exibirMensagemToast('Relatório Excel exportado com sucesso!', 3000);
    } catch (error) {
      console.error('Erro ao exportar para Excel:', error);
      this.util.exibirMensagemToast('Erro ao exportar dados. Tente novamente.', 3000);
    }
  }

  exportarParaCSV(): void {
    try {
      if (!this.todosEquipamentos || this.todosEquipamentos.length === 0) {
        this.util.exibirMensagemToast('Nenhum dado para exportar', 3000);
        return;
      }

      const dados = this.prepararDadosParaExportacao();
      const dataAtual = new Date().toISOString().split('T')[0];
      
      // Converter para CSV usando XLSX
      const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dados);
      const csv = XLSX.utils.sheet_to_csv(ws, { FS: ';' }); // Separador ponto-e-vírgula
      
      // Adicionar BOM UTF-8
      const blob = new Blob(['\ufeff' + csv], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      
      link.setAttribute('href', url);
      link.setAttribute('download', `equipamentos-status-detalhe-${dataAtual}.csv`);
      link.style.visibility = 'hidden';
      
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      this.closeExportModal(new Event('click'));
      this.util.exibirMensagemToast('Relatório CSV exportado com sucesso!', 3000);
    } catch (error) {
      console.error('Erro ao exportar para CSV:', error);
      this.util.exibirMensagemToast('Erro ao exportar dados. Tente novamente.', 3000);
    }
  }

  private prepararDadosParaExportacao(): any[] {
    if (!this.todosEquipamentos) return [];
    
    return this.todosEquipamentos.map(item => ({
      'Tipo de Equipamento': item.tipoequipamento || 'N/A',
      'Fabricante': item.fabricante || 'N/A',
      'Modelo': item.modelo || 'N/A',
      'Número de Série': item.numeroserie || 'N/A',
      'Patrimônio': item.patrimonio || 'N/A',
      'Status': item.equipamentostatus || 'N/A',
      'Localização': item.localidade || 'N/A',
      'Empresa': item.empresa || 'N/A',
      'Centro de Custo': item.centrocusto || 'N/A'
    }));
  }

// Método para navegar para a tela de recursos
  navegarParaRecurso(numeroserie: string): void {
    try {
      // Navegação direta para recursos com parâmetro de busca
      this.router.navigate(['/recursos'], { 
        queryParams: { search: numeroserie },
        skipLocationChange: false
      });
    } catch (error) {
      console.error('Erro na navegação:', error);
    }
  }
}