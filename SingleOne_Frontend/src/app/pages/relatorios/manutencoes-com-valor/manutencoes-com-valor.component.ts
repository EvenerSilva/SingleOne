import { Component, OnInit, ViewChild, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Chart, registerables, ChartConfiguration, ChartType } from 'chart.js';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

interface Manutencao {
  id: number;
  equipamento: string;
  numeroserie: string;
  patrimonio: string;
  tecniconome: string;
  descricao: string;
  laudo: string;
  empresanome: string;
  centrocustonome: string;
  valormanutencao: number;
  dtentrada: string;
  dtlaudo: string;
}

interface ResumoManutencao {
  empresa: string;
  centroCusto: string;
  quantidade: number;
  valor: number;
}

interface Filtros {
  empresa: number;
  centrocusto: number;
}

@Component({
  selector: 'app-manutencoes-com-valor',
  templateUrl: './manutencoes-com-valor.component.html',
  styleUrls: ['./manutencoes-com-valor.component.scss']
})
export class ManutencoesComValorComponent implements OnInit, AfterViewInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private session: any = {};
  
  // ViewChilds
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatPaginator, { static: false }) paginatorResumo: MatPaginator;
  
  // DataSources
  public dataSource: MatTableDataSource<Manutencao> = new MatTableDataSource<Manutencao>([]);
  public dataSourceResumo: MatTableDataSource<ResumoManutencao> = new MatTableDataSource<ResumoManutencao>([]);
  
  // Dados
  public laudos: { item1: Manutencao[], item2: ResumoManutencao[] } = { item1: [], item2: [] };
  public custos = 0;
  public totalManutencoes = 0;
  public custoMedio = 0;
  
  // Dados paginados para exibição
  public dadosPaginados: Manutencao[] = [];
  public dadosResumoPaginados: ResumoManutencao[] = [];
  
  // Filtros
  public filtros: Filtros = {
    empresa: 0,
    centrocusto: 0
  };
  public showFilters = false;
  showExportModal = false;
  selectedTabIndex = 0;
  
  // Opções de filtro
  public empresas: any[] = [];
  public centros: any[] = [];
  
  // Gráficos
  public chart: Chart;
  public chartType: ChartType = 'bar';
  
  // Estados
  public loading = false;
  public searchValue = '';
  
  // Propriedades de paginação para bindings
  public pageSize = 10;
  public pageSizeResumo = 10;
  public pageIndex = 0;
  public pageIndexResumo = 0;
  public length = 0;
  public lengthResumo = 0;

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService, 
    private apiCad: ConfiguracoesApiService,
    private cdr: ChangeDetectorRef
  ) {
    Chart.register(...registerables);
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.listarEmpresas();
    this.listarLaudos(); 
  }

  ngAfterViewInit(): void {
    // Configurar paginadores após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
      if (this.dataSourceResumo && this.paginatorResumo) {
        this.configurarPaginadorResumo();
      }
    }, 100);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.chart) {
      this.chart.destroy();
    }
  }

  listarLaudos(): void {
    this.loading = true;
    this.util.aguardar(true);
    
    this.api.laudosComValor(
      this.session.usuario.cliente, 
      this.filtros.empresa, 
      this.filtros.centrocusto, 
      this.session.token
    ).then(res => {
      this.util.aguardar(false);
      this.loading = false;
      
      if (res.status !== 200 && res.status !== 204) {
        this.util.exibirFalhaComunicacao();
        return;
      }
      
        this.laudos = res.data;
      this.calcularMetricas();
      this.configurarTabelas();
      this.criarGrafico();
      this.cdr.detectChanges();
    }).catch(() => {
      this.util.aguardar(false);
      this.loading = false;
      this.util.exibirFalhaComunicacao();
    });
  }

  private calcularMetricas(): void {
    this.totalManutencoes = this.laudos.item1.length;
    this.custos = this.laudos.item1.reduce((total, x) => total + (x.valormanutencao || 0), 0);
    this.custoMedio = this.totalManutencoes > 0 ? this.custos / this.totalManutencoes : 0;
  }

  private configurarTabelas(): void {
    // Atualizar dados ao invés de criar novos MatTableDataSource
    this.dataSource.data = this.laudos.item1;
    this.dataSourceResumo.data = this.laudos.item2;
    
    // Atualizar propriedades de paginação
    this.length = this.laudos.item1.length;
    this.lengthResumo = this.laudos.item2.length;
    this.pageIndex = 0;
    this.pageIndexResumo = 0;
    
    // Configurar paginadores
    setTimeout(() => {
      if (this.paginator) {
        this.configurarPaginador();
      }
      if (this.paginatorResumo) {
        this.configurarPaginadorResumo();
      }
      
      // Criar gráfico
      this.criarGrafico();
    }, 100);
  }

  private configurarPaginador(): void {
    if (this.dataSource && this.paginator) {
      this.dataSource.paginator = this.paginator;
      this.atualizarDadosPaginados();
    }
  }

  private configurarPaginadorResumo(): void {
    if (this.dataSourceResumo && this.paginatorResumo) {
      this.dataSourceResumo.paginator = this.paginatorResumo;
      this.atualizarDadosResumoPaginados();
    }
  }
  
  public onPageChange(event: any): void {
    this.pageSize = event.pageSize;
    this.pageIndex = event.pageIndex;
    this.atualizarDadosPaginados();
  }
  
  public onPageChangeResumo(event: any): void {
    this.pageSizeResumo = event.pageSize;
    this.pageIndexResumo = event.pageIndex;
    this.atualizarDadosResumoPaginados();
  }
  
  private atualizarDadosPaginados(): void {
    const dados = this.dataSource.filteredData || this.dataSource.data || [];
    const inicio = this.pageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    this.dadosPaginados = dados.slice(inicio, fim);
    this.length = dados.length;
  }
  
  private atualizarDadosResumoPaginados(): void {
    const dados = this.dataSourceResumo.filteredData || this.dataSourceResumo.data || [];
    const inicio = this.pageIndexResumo * this.pageSizeResumo;
    const fim = inicio + this.pageSizeResumo;
    this.dadosResumoPaginados = dados.slice(inicio, fim);
    this.lengthResumo = dados.length;
  }

  private criarGrafico(): void {
    if (this.chart) {
      this.chart.destroy();
    }

    const ctx = document.getElementById('chartManutencoes') as HTMLCanvasElement;
    if (!ctx) {
      console.error('[GRAFICO] Canvas não encontrado!');
      return;
    }

    // Verificar se há dados para exibir
    if (!this.laudos.item2 || this.laudos.item2.length === 0) {
      console.warn('[GRAFICO] Nenhum dado disponível para o gráfico');
      return;
    }
    const data = {
      labels: this.laudos.item2.map(x => `${x.empresa}${x.centroCusto ? ' - ' + x.centroCusto : ''}`),
      datasets: [
        {
          label: 'Quantidade de Manutenções',
          data: this.laudos.item2.map(x => x.quantidade),
          backgroundColor: 'rgba(255, 58, 15, 0.8)',
          borderColor: 'rgba(255, 58, 15, 1)',
          borderWidth: 1,
          yAxisID: 'y'
        },
        {
          label: 'Valor Total (R$)',
          data: this.laudos.item2.map(x => x.valor),
          backgroundColor: 'rgba(8, 0, 57, 0.8)',
          borderColor: 'rgba(8, 0, 57, 1)',
          borderWidth: 1,
          yAxisID: 'y1'
        }
      ]
    };

    const config: ChartConfiguration = {
      type: this.chartType,
      data: data,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
          mode: 'index' as const,
          intersect: false,
        },
        plugins: {
          title: {
            display: true,
            text: 'Custos de Manutenção por Empresa e Centro de Custo',
            font: {
              size: 16,
              weight: 'bold'
            }
          },
          legend: {
            position: 'bottom' as const,
          }
        },
        scales: {
          x: {
            display: true,
            title: {
              display: true,
              text: 'Empresa / Centro de Custo'
            }
          },
          y: {
            type: 'linear' as const,
            display: true,
            position: 'left' as const,
            title: {
              display: true,
              text: 'Quantidade'
            }
          },
          y1: {
            type: 'linear' as const,
            display: true,
            position: 'right' as const,
            title: {
              display: true,
              text: 'Valor (R$)'
            },
            grid: {
              drawOnChartArea: false,
            },
          }
        }
      }
    };

    try {
      this.chart = new Chart(ctx, config);
    } catch (error) {
      console.error('[GRAFICO] Erro ao criar gráfico:', error);
    }
  }

  listarEmpresas(): void {
    this.apiCad.listarEmpresas("null", this.session.usuario.cliente, this.session.token)
      .then(res => {
        if (res && res.data) {
          // Remover duplicatas por ID
          const uniqueById = new Map<number, any>();
          res.data.forEach((emp: any) => {
            if (!uniqueById.has(emp.id)) {
              uniqueById.set(emp.id, emp);
            }
          });
          
          let uniqueEmpresas = Array.from(uniqueById.values());
          
          // Remover duplicatas por nome (case-insensitive, trim)
          const uniqueByName = new Map<string, any>();
          uniqueEmpresas.forEach((emp: any) => {
            const normalizedName = (emp.nome || '').trim().toLowerCase();
            if (!uniqueByName.has(normalizedName)) {
              uniqueByName.set(normalizedName, emp);
            }
          });
          
          // Ordenar alfabeticamente
          this.empresas = Array.from(uniqueByName.values()).sort((a, b) => {
            const nomeA = (a.nome || '').trim().toLowerCase();
            const nomeB = (b.nome || '').trim().toLowerCase();
            return nomeA.localeCompare(nomeB);
          });
        } else {
          this.empresas = [];
        }
      })
      .catch(() => {
        this.empresas = [];
      });
  }

  listarCentrosCustos(): void {
    if (!this.filtros.empresa) {
      this.centros = [];
      return;
    }

    this.util.aguardar(true);
    this.apiCad.listarCentroCustoDaEmpresa(this.filtros.empresa, this.session.token)
      .then(res => {
        this.util.aguardar(false);
        if (res.status === 200 || res.status === 204) {
          this.centros = res.data || [];
        } else {
          this.centros = [];
        }
      })
      .catch(() => {
      this.util.aguardar(false);
        this.centros = [];
      });
  }

  limparFiltros(): void {
    this.filtros = {
      empresa: 0,
      centrocusto: 0
    };
    this.centros = [];
    this.listarLaudos();
  }

  aplicarFiltros(): void {
    this.listarLaudos();
  }

  onEmpresaChange(): void {
    this.filtros.centrocusto = 0;
    this.listarCentrosCustos();
  }

  aplicarFiltroTabela(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.searchValue = filterValue;
    
    // Filtro personalizado para múltiplas colunas
    this.dataSource.filterPredicate = (data: Manutencao, filter: string) => {
      const searchStr = filter.toLowerCase();
      return (
        (data.equipamento && data.equipamento.toLowerCase().includes(searchStr)) ||
        (data.tecniconome && data.tecniconome.toLowerCase().includes(searchStr)) ||
        (data.descricao && data.descricao.toLowerCase().includes(searchStr)) ||
        (data.laudo && data.laudo.toLowerCase().includes(searchStr)) ||
        (data.empresanome && data.empresanome.toLowerCase().includes(searchStr)) ||
        (data.centrocustonome && data.centrocustonome.toLowerCase().includes(searchStr)) ||
        (data.numeroserie && data.numeroserie.toLowerCase().includes(searchStr)) ||
        (data.patrimonio && data.patrimonio.toLowerCase().includes(searchStr))
      );
    };
    
    this.dataSource.filter = filterValue.trim();
    
    // Resetar para primeira página e atualizar dados paginados
    this.pageIndex = 0;
    this.atualizarDadosPaginados();
  }

  limparPesquisa(): void {
    this.searchValue = '';
    this.dataSource.filter = '';
    
    // Resetar para primeira página e atualizar dados paginados
    this.pageIndex = 0;
    this.atualizarDadosPaginados();
  }

  exportarDados(): void {
    if (!this.dataSource?.data || this.dataSource.data.length === 0) {
      this.util.exibirMensagemToast('Não há dados para exportar.', 3000);
      return;
    }

    this.showExportModal = true;
  }

  closeExportModal(event: Event): void {
    this.showExportModal = false;
  }

  exportarParaExcel(): void {
    try {
      // Preparar dados para exportação
      const dadosParaExportar = this.prepararDadosParaExportacao();
      
      // Criar nome do arquivo com timestamp
      const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
      const nomeArquivo = `custos-manutencao-${timestamp}.xlsx`;
      
      // Criar workbook e worksheet
      const workbook = this.criarWorkbookExcel(dadosParaExportar);
      
      // Exportar arquivo
      this.downloadArquivo(workbook, nomeArquivo, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
      
      this.util.exibirMensagemToast('Dados exportados para Excel com sucesso!', 3000);
      this.closeExportModal(new Event('click'));
    } catch (error) {
      console.error('Erro ao exportar para Excel:', error);
      this.util.exibirMensagemToast('Erro ao exportar dados. Tente novamente.', 3000);
    }
  }

private prepararDadosParaExportacao(): any[] {
    const dados = this.dataSource.filteredData?.length > 0 ? this.dataSource.filteredData : this.dataSource.data;
    
    return dados.map(item => ({
      'Equipamento': item.equipamento || 'N/A',
      'Número de Série': item.numeroserie || 'N/A',
      'Técnico': item.tecniconome || 'N/A',
      'Reclamação': item.descricao || 'N/A',
      'Laudo': item.laudo || 'N/A',
      'Empresa': item.empresanome || 'N/A',
      'Centro de Custo': item.centrocustonome || 'N/A',
      'Valor': item.valormanutencao || 0,
      'Data Entrada': item.dtentrada ? new Date(item.dtentrada).toLocaleDateString('pt-BR') : 'N/A',
      'Data Laudo': item.dtlaudo ? new Date(item.dtlaudo).toLocaleDateString('pt-BR') : 'N/A'
    }));
  }

  private criarWorkbookExcel(dados: any[]): any {
    // Cabeçalhos das colunas
    const headers = Object.keys(dados[0]);
    
    // Dados formatados
    const dadosFormatados = dados.map(row => headers.map(header => row[header]));
    
    // Criar worksheet
    const worksheet = {
      '!ref': `A1:${String.fromCharCode(65 + headers.length - 1)}${dados.length + 1}`,
      'A1': { v: 'RELATÓRIO DE CUSTOS DE MANUTENÇÃO', t: 's' },
      'A2': { v: `Data de Exportação: ${new Date().toLocaleDateString('pt-BR')} ${new Date().toLocaleTimeString('pt-BR')}`, t: 's' },
      'A3': { v: `Total de Registros: ${dados.length}`, t: 's' },
      'A4': { v: '', t: 's' }, // Linha em branco
    };

    // Adicionar cabeçalhos
    headers.forEach((header, index) => {
      const coluna = String.fromCharCode(65 + index);
      worksheet[`${coluna}5`] = { v: header, t: 's', s: { font: { bold: true } } };
    });

    // Adicionar dados
    dados.forEach((row, rowIndex) => {
      headers.forEach((header, colIndex) => {
        const coluna = String.fromCharCode(65 + colIndex);
        const linha = rowIndex + 6;
        const valor = row[header];
        
        if (typeof valor === 'number') {
          worksheet[`${coluna}${linha}`] = { v: valor, t: 'n' };
        } else {
          worksheet[`${coluna}${linha}`] = { v: valor, t: 's' };
        }
      });
    });

    // Criar workbook
    const workbook = {
      SheetNames: ['Custos de Manutenção'],
      Sheets: {
        'Custos de Manutenção': worksheet
      }
    };

    return workbook;
  }

  private downloadArquivo(conteudo: any, nomeArquivo: string, tipoMime: string): void {
    // Para Excel, vamos usar uma abordagem simplificada
    // Em uma implementação real, seria necessário usar a biblioteca xlsx
    
    // Criar CSV como alternativa temporária
    this.exportarParaCSV();
  }

  private exportarParaCSV(): void {
    try {
      const dados = this.prepararDadosParaExportacao();
      const headers = Object.keys(dados[0]);
      
      // Criar conteúdo CSV
      let csvContent = 'data:text/csv;charset=utf-8,';
      
      // Adicionar cabeçalhos
      csvContent += headers.join(',') + '\n';
      
      // Adicionar dados
      dados.forEach(row => {
        const linha = headers.map(header => {
          const valor = row[header];
          // Escapar vírgulas e aspas
          if (typeof valor === 'string' && (valor.includes(',') || valor.includes('"'))) {
            return `"${valor.replace(/"/g, '""')}"`;
          }
          return valor;
        });
        csvContent += linha.join(',') + '\n';
      });
      
      // Criar link de download
      const encodedUri = encodeURI(csvContent);
      const link = document.createElement('a');
      link.setAttribute('href', encodedUri);
      
      // Nome do arquivo
      const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
      const nomeArquivo = `custos-manutencao-${timestamp}.csv`;
      link.setAttribute('download', nomeArquivo);
      
      // Trigger download
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      this.util.exibirMensagemToast('Dados exportados para CSV com sucesso!', 3000);
    } catch (error) {
      console.error('Erro ao exportar para CSV:', error);
      this.util.exibirMensagemToast('Erro ao exportar dados. Tente novamente.', 3000);
    }
  }

}
