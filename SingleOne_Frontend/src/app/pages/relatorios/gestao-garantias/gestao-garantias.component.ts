import { Component, OnInit, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-gestao-garantias',
  templateUrl: './gestao-garantias.component.html',
  styleUrls: ['./gestao-garantias.component.scss']
})
export class GestaoGarantiasComponent implements OnInit, AfterViewInit, OnDestroy {

  private session: any = {};
  public filtros: any = {
    statusGarantia: null,
    tipoRecurso: null,
    fabricante: null,
    patrimonio: null
  };
  
  public equipamentos: any = [];
  public tiposRecursos: any[] = [];
  public statusGarantia = [
    { valor: 'expiradas', descricao: 'Expiradas', icone: 'cil-warning', cor: 'danger' },
    { valor: 'vence30', descricao: 'Vencendo em 30 dias', icone: 'cil-bell', cor: 'danger' },
    { valor: 'vence90', descricao: 'Vencendo em 90 dias', icone: 'cil-clock', cor: 'warning' },
    { valor: 'vence180', descricao: 'Vencendo em 180 dias', icone: 'cil-calendar', cor: 'info' },
    { valor: 'vigentes', descricao: 'Vigentes', icone: 'cil-check-circle', cor: 'success' },
    { valor: 'naoInformado', descricao: 'Não Informado', icone: 'cil-info', cor: 'secondary' }
  ];
  
  public colunas = ['patrimonio', 'tipoEquipamento', 'fabricante', 'modelo', 'numeroSerie', 'dataGarantia', 'diasRestantes', 'statusGarantia'];
  
  // Paginação local (como em auditoria de acessos)
  public dadosPagina: any[] = [];
  public totalLength = 0;
  public pageSize = 10;
  public currentPageIndex = 0;
  
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;

  public loading = false;
  public showExportModal = false;
  public showDetalhesModal = false;
  public equipamentoSelecionado: any = null;
  private destroy$ = new Subject<void>();

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService,
    private configApi: ConfiguracoesApiService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.carregarTiposRecursos();
  }

  carregarTiposRecursos() {
    this.configApi.listarTiposRecursos(null, this.session.usuario.cliente, this.session.token).then(res => {
      if (res.status === 200 && res.data) {
        this.tiposRecursos = res.data;
      }
    }).catch(error => {
      console.error('Erro ao carregar tipos de recursos:', error);
    });
  }

  ngAfterViewInit(): void {
    this.inicializarDataSource();
  }

  private inicializarDataSource(): void {
    if (!this.dataSource) {
      this.dataSource = new MatTableDataSource<any>([]);
    }
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  consultar() {
    this.loading = true;
    const payload = {
      statusGarantia: this.filtros.statusGarantia || null,
      tipoEquipamento: this.filtros.tipoRecurso || null,
      fabricante: this.filtros.fabricante || null,
      patrimonio: this.filtros.patrimonio || null,
      clienteId: this.session.usuario.cliente
    };

    this.api.consultarGarantias(payload, this.session.token).then(res => {
      this.loading = false;
      if (res.status === 200 && res.data) {
        this.equipamentos = res.data;
        
        // Configurar paginação local (como em auditoria de acessos)
        this.totalLength = this.equipamentos.length;
        this.currentPageIndex = 0;
        this.atualizarPagina();
      } else {
        this.util.exibirMensagemToast('Erro ao buscar dados de garantias', 3000);
      }
    }).catch(error => {
      this.loading = false;
      console.error('Erro ao consultar garantias:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  limparBusca(): void {
    this.filtros = {
      statusGarantia: null,
      tipoRecurso: null,
      fabricante: null,
      patrimonio: null
    };
    this.equipamentos = [];
    this.dadosPagina = [];
    this.totalLength = 0;
    this.currentPageIndex = 0;
  }

  // Método: Mudança de página (paginação local - como em auditoria)
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPageIndex = event.pageIndex;
    this.atualizarPagina();
  }

  // Método: Atualizar dados da página corrente (paginação local)
  private atualizarPagina() {
    const inicio = this.currentPageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    this.dadosPagina = this.equipamentos.slice(inicio, fim);
  }

  // Métodos para métricas
  getTotalEquipamentos(): number {
    return this.equipamentos?.length || 0;
  }

  getTotalExpiradas(): number {
    if (!this.equipamentos) return 0;
    return this.equipamentos.filter(item => item.statusGarantia === 'expiradas').length;
  }

  getTotalVencendo30(): number {
    if (!this.equipamentos) return 0;
    return this.equipamentos.filter(item => item.statusGarantia === 'vence30').length;
  }

  getTotalVencendo90(): number {
    if (!this.equipamentos) return 0;
    return this.equipamentos.filter(item => item.statusGarantia === 'vence90').length;
  }

  getTotalVencendo180(): number {
    if (!this.equipamentos) return 0;
    return this.equipamentos.filter(item => item.statusGarantia === 'vence180').length;
  }

  getTotalVigentes(): number {
    if (!this.equipamentos) return 0;
    return this.equipamentos.filter(item => item.statusGarantia === 'vigentes').length;
  }

  getTotalNaoInformado(): number {
    if (!this.equipamentos) return 0;
    return this.equipamentos.filter(item => item.statusGarantia === 'naoInformado').length;
  }

  // Métodos de exportação
  exportarDados(): void {
    this.showExportModal = true;
  }

  fecharModalExportacao(): void {
    this.showExportModal = false;
  }

  closeExportModal(event: Event): void {
    this.showExportModal = false;
  }

  exportar(formato: 'excel' | 'csv'): void {
    try {
      if (!this.equipamentos || this.equipamentos.length === 0) {
        this.util.exibirMensagemToast('Nenhum dado para exportar', 3000);
        return;
      }

      const dadosExportacao = this.prepararDadosParaExportacao();
      const dataAtual = new Date().toISOString().split('T')[0];

      if (formato === 'excel') {
        this.exportarExcel(dadosExportacao, dataAtual);
      } else {
        this.exportarCSV(dadosExportacao, dataAtual);
      }

      this.fecharModalExportacao();
    } catch (error) {
      console.error('Erro ao exportar:', error);
      this.util.exibirMensagemToast('Erro ao exportar relatório', 3000);
    }
  }

  private exportarExcel(dados: any[], dataAtual: string): void {
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dados);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Gestão de Garantias');

    // Ajustar largura das colunas
    const colWidths = [
      { wch: 15 }, // Patrimônio
      { wch: 25 }, // Tipo de Recurso
      { wch: 20 }, // Fabricante
      { wch: 20 }, // Modelo
      { wch: 20 }, // Número de Série
      { wch: 15 }, // Data de Garantia
      { wch: 15 }, // Dias Restantes
      { wch: 20 }  // Status da Garantia
    ];
    ws['!cols'] = colWidths;

    const nomeArquivo = `gestao-garantias-${dataAtual}.xlsx`;
    XLSX.writeFile(wb, nomeArquivo);
    this.util.exibirMensagemToast('Relatório Excel exportado com sucesso!', 3000);
  }

  private exportarCSV(dados: any[], dataAtual: string): void {
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dados);
    const csv = XLSX.utils.sheet_to_csv(ws, { FS: ';' }); // Separador ponto-e-vírgula

    const blob = new Blob(['\ufeff' + csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', `gestao-garantias-${dataAtual}.csv`);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    this.util.exibirMensagemToast('Relatório CSV exportado com sucesso!', 3000);
  }

  // Manter método antigo para compatibilidade
  exportarParaCSV(): void {
    this.exportar('csv');
  }

  private prepararDadosParaExportacao(): any[] {
    if (!this.equipamentos) return [];
    
    return this.equipamentos.map(item => ({
      'Patrimônio': item.patrimonio || 'N/A',
      'Tipo de Recurso': item.tipoEquipamento || 'N/A',
      'Fabricante': item.fabricante || 'N/A',
      'Modelo': item.modelo || 'N/A',
      'Número de Série': item.numeroSerie || 'N/A',
      'Data de Garantia': item.dataGarantia ? this.formatarData(item.dataGarantia) : 'Não Informado',
      'Dias Restantes': item.diasRestantes !== null ? item.diasRestantes : 'N/A',
      'Status da Garantia': this.getStatusGarantiaDescricao(item.statusGarantia)
    }));
  }

  formatarData(dataStr: string): string {
    if (!dataStr) return 'Não Informado';
    const data = new Date(dataStr);
    return data.toLocaleDateString('pt-BR');
  }

  getStatusGarantiaDescricao(status: string): string {
    const statusEncontrado = this.statusGarantia.find(s => s.valor === status);
    return statusEncontrado ? statusEncontrado.descricao : status;
  }

  getStatusGarantiaClass(status: string): string {
    const statusEncontrado = this.statusGarantia.find(s => s.valor === status);
    return statusEncontrado ? `status-${statusEncontrado.cor}` : 'status-secondary';
  }

  getStatusGarantiaIcone(status: string): string {
    const statusEncontrado = this.statusGarantia.find(s => s.valor === status);
    return statusEncontrado ? statusEncontrado.icone : 'cil-info';
  }

  getDiasRestantesClass(diasRestantes: number): string {
    if (diasRestantes === null) return 'dias-na';
    if (diasRestantes < 0) return 'dias-expirado';
    if (diasRestantes <= 30) return 'dias-critico';
    if (diasRestantes <= 90) return 'dias-atencao';
    if (diasRestantes <= 180) return 'dias-alerta';
    return 'dias-ok';
  }

  verDetalhes(equipamento: any): void {
    this.equipamentoSelecionado = equipamento;
    this.showDetalhesModal = true;
  }

  closeDetalhesModal(event: Event): void {
    this.showDetalhesModal = false;
    this.equipamentoSelecionado = null;
  }
}
