import { Component, OnInit, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-auditoria-acessos',
  templateUrl: './auditoria-acessos.component.html',
  styleUrls: ['./auditoria-acessos.component.scss']
})
export class AuditoriaAcessosComponent implements OnInit, AfterViewInit, OnDestroy {

  private session: any = {};
  public filtros: any = {
    dataInicio: null,
    dataFim: null,
    tipoAcesso: null,
    cpfConsultado: null
  };
  
  public logs: any = [];
  public tiposAcesso = [
    { valor: 'passcheck', descricao: 'Portaria (PassCheck)' },
    { valor: 'patrimonio', descricao: 'Meu Patrimônio' }
  ];
  
  public colunas = ['data', 'hora', 'tipoAcesso', 'cpfConsultado', 'colaborador', 'ipAddress', 'sucesso'];
  
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  
  // Paginação local (como em equipamentos)
  public dadosPagina: any[] = [];
  public totalLength = 0;
  public pageSize = 10;
  public currentPageIndex = 0;

  public loading = false;
  public showExportModal = false;
  private destroy$ = new Subject<void>();

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService, 
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.configurarDatasPadrao();
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

  private configurarDatasPadrao(): void {
    // Configurar data de início como 30 dias atrás
    const dataInicio = new Date();
    dataInicio.setDate(dataInicio.getDate() - 30);
    this.filtros.dataInicio = this.formatarDataParaInput(dataInicio);
    
    // Configurar data de fim como hoje
    this.filtros.dataFim = this.formatarDataParaInput(new Date());
  }

  private formatarDataParaInput(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    return `${ano}-${mes}-${dia}`;
  }

  consultar() {
    if (!this.filtros.dataInicio || !this.filtros.dataFim) {
      this.util.exibirMensagemToast('Por favor, informe o período de consulta', 3000);
      return;
    }

    this.loading = true;
    const payload = {
      dataInicio: this.filtros.dataInicio,
      dataFim: this.filtros.dataFim,
      tipoAcesso: this.filtros.tipoAcesso || null,
      cpfConsultado: this.filtros.cpfConsultado || null,
      clienteId: this.session.usuario.cliente
    };

    this.api.consultarLogsAcesso(payload, this.session.token).then(res => {
      this.loading = false;
      if (res.status === 200 && res.data) {
        this.logs = res.data;
        
        // Configurar paginação local (como em equipamentos)
        this.totalLength = this.logs.length;
        this.currentPageIndex = 0;
        this.atualizarPagina();
      } else {
        this.util.exibirMensagemToast('Erro ao buscar logs de acesso', 3000);
      }
    }).catch(error => {
      this.loading = false;
      console.error('Erro ao consultar logs:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  limparBusca(): void {
    this.filtros = {
      dataInicio: null,
      dataFim: null,
      tipoAcesso: null,
      cpfConsultado: null
    };
    this.configurarDatasPadrao();
    this.logs = [];
    this.dadosPagina = [];
    this.totalLength = 0;
    this.currentPageIndex = 0;
  }

  // Método: Mudança de página (paginação local - como em equipamentos)
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPageIndex = event.pageIndex;
    this.atualizarPagina();
  }

  // Método: Atualizar dados da página corrente (paginação local)
  private atualizarPagina() {
    const inicio = this.currentPageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    this.dadosPagina = this.logs.slice(inicio, fim);
  }

  // Métodos para métricas
  getTotalAcessos(): number {
    return this.logs?.length || 0;
  }

  getTotalPassCheck(): number {
    if (!this.logs) return 0;
    return this.logs.filter(item => item.tipoAcesso === 'passcheck').length;
  }

  getTotalMeuPatrimonio(): number {
    if (!this.logs) return 0;
    return this.logs.filter(item => item.tipoAcesso === 'patrimonio').length;
  }

  getTotalSucesso(): number {
    if (!this.logs) return 0;
    return this.logs.filter(item => item.sucesso).length;
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
      if (!this.logs || this.logs.length === 0) {
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
    XLSX.utils.book_append_sheet(wb, ws, 'Auditoria de Acessos');

    // Ajustar largura das colunas
    const colWidths = [
      { wch: 12 }, // Data
      { wch: 10 }, // Hora
      { wch: 25 }, // Tipo de Acesso
      { wch: 15 }, // CPF Consultado
      { wch: 30 }, // Colaborador
      { wch: 15 }, // IP Address
      { wch: 10 }, // Sucesso
      { wch: 40 }  // Mensagem Erro
    ];
    ws['!cols'] = colWidths;

    const nomeArquivo = `auditoria-acessos-${dataAtual}.xlsx`;
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
    link.setAttribute('download', `auditoria-acessos-${dataAtual}.csv`);
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
    if (!this.logs) return [];
    
    return this.logs.map(item => ({
      'Data': this.formatarData(item.createdAt),
      'Hora': this.formatarHora(item.createdAt),
      'Tipo de Acesso': this.getTipoAcessoDescricao(item.tipoAcesso),
      'CPF Consultado': item.cpfConsultado || 'N/A',
      'Colaborador': item.colaboradorNome || 'N/A',
      'IP Address': item.ipAddress || 'N/A',
      'Sucesso': item.sucesso ? 'Sim' : 'Não',
      'Mensagem Erro': item.mensagemErro || 'N/A'
    }));
  }

  formatarData(dataStr: string): string {
    if (!dataStr) return 'N/A';
    const data = new Date(dataStr);
    return data.toLocaleDateString('pt-BR');
  }

  formatarHora(dataStr: string): string {
    if (!dataStr) return 'N/A';
    const data = new Date(dataStr);
    return data.toLocaleTimeString('pt-BR');
  }

  getTipoAcessoDescricao(tipo: string): string {
    const tipoEncontrado = this.tiposAcesso.find(t => t.valor === tipo);
    return tipoEncontrado ? tipoEncontrado.descricao : tipo;
  }

  getStatusClass(sucesso: boolean): string {
    return sucesso ? 'status-sucesso' : 'status-erro';
  }

  verDetalhes(log: any): void {
    // Mostrar modal com detalhes do log
    const detalhes = JSON.parse(log.dadosConsultados || '{}');
  }
}
