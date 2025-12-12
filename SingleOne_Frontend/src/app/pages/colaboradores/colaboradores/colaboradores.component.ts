import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { UtilService } from 'src/app/util/util.service';
import { DesligamentoProgramadoComponent } from '../desligamento-programado/desligamento-programado.component';
import { 
  ImportacaoColaboradoresService, 
  ResultadoValidacaoColaboradores, 
  ResultadoImportacaoColaboradores 
} from 'src/app/services/importacao-colaboradores.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-colaboradores',
  templateUrl: './colaboradores.component.html',
  styleUrls: ['./colaboradores.component.scss']
})
export class ColaboradoresComponent implements OnInit, AfterViewInit {

  private session:any = {};
  public colunas = ['matricula', 'nome', 'empresa', 'centrocusto', 'tipoColaborador', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;

  // 🎯 VARIÁVEIS DO MODAL
  public mostrarFormulario = false;
  public colaboradorEditando: any = null;
  public modoFormulario: 'criar' | 'editar' = 'criar';
  
  // 🚀 CACHE PARA OTIMIZAÇÃO DE PERFORMANCE
  private cacheStats: { [key: string]: { value: number, timestamp: number } } = {};
  private readonly CACHE_DURATION = 5000; // 5 segundos
  
  // 🎯 FILTROS DOS CARDS E PAGINAÇÃO CLIENT-SIDE
  public filtroAtivo: string = 'total';
  public dadosOriginais: any[] = [];
  public dadosFiltrados: any[] = [];
  public dadosPagina: any[] = [];
  public indicadorFiltro: string = '';
  public totalLength = 0;
  public pageSize = 10; // Mantido em 10 registros por página (otimizado para performance)
  public currentPageIndex = 0;
  public mostrarAtalhoCentral = false;
  public totalRegistrosBackend = 0; // Total de registros informado pelo backend (RowCount)
  public totalRegistrosFiltrados = 0; // Total de registros quando há filtro ativo
  private termoPesquisaAtual: string = 'null'; // Termo de pesquisa usado na última chamada
  private tipoFiltroAtual: string = null; // Tipo de filtro ativo (funcionarios, terceiros, etc)
  public estatisticas: any = null; // Estatísticas do backend (total, funcionarios, terceiros, etc.)
  
  // 📤 VARIÁVEIS DO MODAL DE IMPORTAÇÃO
  public mostrarModalImportacao: boolean = false;
  public passoImportacao: number = 1;
  public arquivoSelecionadoImport: File | null = null;
  public uploadandoImportacao: boolean = false;
  public importandoColaboradores: boolean = false;
  public resultadoValidacaoImport: ResultadoValidacaoColaboradores | null = null;
  public resultadoImportacaoFinal: ResultadoImportacaoColaboradores | null = null;
  public loteAtualImport: string | null = null;
  public baixandoErros: boolean = false;
  
  // 📊 VARIÁVEIS DO MODAL DE EXPORTAÇÃO
  public mostrarModalExportacao: boolean = false;

  constructor(
    private util: UtilService, 
    private api: ColaboradorApiService, 
    private route: Router,
    private activatedRoute: ActivatedRoute,
    private dialog: MatDialog,
    private importacaoService: ImportacaoColaboradoresService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    // 🎯 Verificar se veio com filtro via queryParam (ex: vindo da tela de colaboradores sem recursos)
    this.activatedRoute.queryParams.subscribe(params => {
      const filtro = params['filtro'];
      const origem = params['origem'];
      const acao = params['acao'];
      
      if (filtro) {
        // Aplicar o filtro no campo de busca
        this.consulta.setValue(filtro, { emitEvent: false });
        // Executar a busca imediatamente
        this.buscar(filtro);
        
        // Exibir feedback visual
        this.indicadorFiltro = `Filtrado por: ${filtro}`;
        setTimeout(() => {
          this.indicadorFiltro = '';
        }, 5000);
      } else {
        // Carregar lista normal (primeira página, sem filtro)
        this.carregarEstatisticas();
        this.listar(1);
      }

      if (acao === 'importar') {
        setTimeout(() => this.abrirModalImportacao(), 300);
        setTimeout(() => this.limparAcaoQueryParam(), 1000);
      } else if (acao === 'exportar') {
        setTimeout(() => this.abrirModalExportacao(), 300);
        setTimeout(() => this.limparAcaoQueryParam(), 1000);
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
    
    // CONFIGURAÇÃO INICIAL DO PAGINADOR
    this.paginator.pageSize = this.pageSize;
    this.paginator.pageIndex = this.currentPageIndex;
    this.paginator.length = this.totalLength;
  }

  /**
   * Lista colaboradores usando paginação REAL no backend.
   * Cada chamada traz apenas uma página do endpoint paginado.
   */
  async listar(pagina: number = 1) {
    await this.carregarPagina(pagina, this.termoPesquisaAtual);
  }

  // 🎯 MÉTODO PARA ATUALIZAR DADOS DA PÁGINA CORRENTE (paginação local)
  private atualizarPagina() {
    // Com paginação no backend, a página atual já vem pronta do servidor
    this.dadosPagina = this.dadosFiltrados;
  }

  // 🎯 MÉTODO PARA TRATAR MUDANÇAS DE PÁGINA (paginação local)
  onPageChange(event: PageEvent) {
    this.currentPageIndex = event.pageIndex;
    this.pageSize = event.pageSize;

    const paginaBackend = event.pageIndex + 1;
    this.listar(paginaBackend);
  }

  async buscar(valor) {
    if (valor != '') {
      this.termoPesquisaAtual = valor;
      await this.carregarPagina(1, valor);
    } else {
      this.termoPesquisaAtual = 'null';
      await this.carregarPagina(1, 'null');
    }
  }

  /**
   * Carrega uma página específica de colaboradores a partir do backend.
   * Usa o endpoint paginado `ListarColaboradores`.
   */
  private async carregarPagina(pagina: number = 1, pesquisa?: string, tipoFiltro?: string): Promise<void> {
    const termo = (pesquisa !== undefined && pesquisa !== null && pesquisa !== '')
      ? pesquisa
      : this.termoPesquisaAtual || 'null';

    let filtro: string | null = null;
    if (tipoFiltro !== undefined && tipoFiltro !== null && tipoFiltro !== 'total') {
      filtro = tipoFiltro;
    } else if (this.tipoFiltroAtual !== null && this.tipoFiltroAtual !== 'total') {
      filtro = this.tipoFiltroAtual;
    }

    this.termoPesquisaAtual = termo;
    this.tipoFiltroAtual = filtro;
    this.util.aguardar(true);

    try {
      const res = await this.api.listarColaboradores(termo, this.cliente, pagina, this.session.token, filtro);

      this.util.aguardar(false);

      if (res.status !== 200 && res.status !== 204) {
        this.util.exibirFalhaComunicacao();
        return;
      }

      const body = res.data || {};
      const resultados = body.results || body.Results || [];
      const rowCount = body.rowCount || body.RowCount || resultados.length;
      const pageSizeFromApi = body.pageSize || body.PageSize || this.pageSize;
      const currentPageFromApi = body.currentPage || body.CurrentPage || pagina;

      // Armazenar dados da página atual
      this.dadosOriginais = [...resultados];
      this.dadosFiltrados = [...resultados];
      this.atualizarPagina();

      // Atualizar totais e paginação
      this.totalLength = rowCount;
      if (filtro && filtro !== 'total') {
        this.totalRegistrosFiltrados = rowCount;
      } else {
        this.totalRegistrosBackend = rowCount;
        this.totalRegistrosFiltrados = 0;
      }
      this.pageSize = pageSizeFromApi;
      this.currentPageIndex = Math.max(0, currentPageFromApi - 1);

      // Atualizar paginator, se existir
      if (this.paginator) {
        this.paginator.pageIndex = this.currentPageIndex;
        this.paginator.pageSize = this.pageSize;
        this.paginator.length = this.totalLength;
      }

      // Limpar cache de estatísticas e forçar atualização
      this.clearStatsCache();
      this.cdr.detectChanges();
    } catch (error) {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
      console.error('[COLABORADORES] Erro ao carregar página:', error);
    }
  }

  editar(obj) {
    this.route.navigate(['/colaborador', btoa(JSON.stringify(obj))]);
  }

  excluir(obj) {
    if(confirm('Deseja realmente excluir o colaborador ' + obj.nome + '?')) {
      this.util.aguardar(true);
      this.api.excluirColaborador(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Colaborador excluido com sucesso!', 5000);
          this.listar();
        }
      })
    }
  }

  agendamento(col) {
    const modalAgendamento = this.dialog.open(DesligamentoProgramadoComponent, {
      width: '500px',
      data: {
        colaborador: col
      }
    });
  }

  redirectToTimeline(colaboradorId: string) {
    this.route.navigate(['relatorios/timeline-colaboradores'], { queryParams: { id: colaboradorId } });
  }

  getTipoLabel(tipo: string): string {
    const codigo = this.getTipoCodigo(tipo);

    switch (codigo) {
      case 'F':
        return 'Funcionário';
      case 'T':
        return 'Terceirizado';
      case 'C':
        return 'Consultor';
      default:
        return tipo || 'N/A';
    }
  }

  getTipoClass(tipo: string): string {
    const codigo = this.getTipoCodigo(tipo);

    switch (codigo) {
      case 'F':
        return 'tipo-funcionario';
      case 'T':
        return 'tipo-terceirizado';
      case 'C':
        return 'tipo-consultor';
      default:
        return 'tipo-default';
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listar();
  }

  // 🎯 MÉTODOS DO MODAL
  novoColaborador() {
    this.colaboradorEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  editarColaborador(colaborador: any) {
    this.colaboradorEditando = colaborador;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  async onColaboradorSalvo(colaborador: any) {
    this.mostrarFormulario = false;
    this.colaboradorEditando = null;
    await this.carregarEstatisticas(); // Recarregar estatísticas
    this.listar(); // Recarregar lista
    this.util.exibirMensagemToast('Colaborador salvo com sucesso!', 5000);
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.colaboradorEditando = null;
  }

  // 🎯 MÉTODOS PARA ESTATÍSTICAS DOS CARDS (OTIMIZADOS COM CACHE)
  async carregarEstatisticas(): Promise<void> {
    try {
      const res = await this.api.obterEstatisticas(this.cliente, this.session.token);
      if (res.status === 200 && res.data) {
        this.estatisticas = res.data;
      }
    } catch (error) {
      console.error('[COLABORADORES] Erro ao carregar estatísticas:', error);
    }
  }

  getTotalColaboradores(): number {
    // Usa estatísticas do backend se disponível, senão usa totalRegistrosBackend
    if (this.estatisticas?.total !== undefined) {
      return this.estatisticas.total;
    }
    return this.totalRegistrosBackend || 0;
  }

  getSelecionados(): number {
    // ✅ CORREÇÃO: Retorna o total de registros quando há filtro ativo, senão retorna 0
    if (this.filtroAtivo && this.filtroAtivo !== 'total') {
      return this.totalRegistrosFiltrados || 0;
    }
    return 0;
  }

  getFuncionarios(): number {
    // Usa estatísticas do backend se disponível
    if (this.estatisticas?.funcionarios !== undefined) {
      return this.estatisticas.funcionarios;
    }
    return 0;
  }

  getTerceiros(): number {
    // Usa estatísticas do backend se disponível
    if (this.estatisticas?.terceiros !== undefined) {
      return this.estatisticas.terceiros;
    }
    return 0;
  }

  getConsultores(): number {
    // Usa estatísticas do backend se disponível
    if (this.estatisticas?.consultores !== undefined) {
      return this.estatisticas.consultores;
    }
    return 0;
  }

  getAtivos(): number {
    // Usa estatísticas do backend se disponível
    if (this.estatisticas?.ativos !== undefined) {
      return this.estatisticas.ativos;
    }
    return 0;
  }

  getDesligados(): number {
    // Usa estatísticas do backend se disponível
    if (this.estatisticas?.desligados !== undefined) {
      return this.estatisticas.desligados;
    }
    return 0;
  }

  // 🚀 MÉTODO DE CACHE PARA OTIMIZAÇÃO
  private getCachedStat(key: string, calculator: () => number): number {
    const now = Date.now();
    const cached = this.cacheStats[key];
    
    // Se existe cache válido, retorna o valor em cache
    if (cached && (now - cached.timestamp) < this.CACHE_DURATION) {
      return cached.value;
    }
    
    // Calcula o valor e armazena no cache
    const value = calculator();
    this.cacheStats[key] = { value, timestamp: now };
    
    return value;
  }

  // 🧹 MÉTODO PARA LIMPAR CACHE (chamado quando os dados mudam)
  private clearStatsCache(): void {
    this.cacheStats = {};
  }

  toggleAtalhoCentral(): void {
    this.mostrarAtalhoCentral = !this.mostrarAtalhoCentral;
  }

  private limparAcaoQueryParam(): void {
    this.route.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: { acao: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  private isColaboradorDesligado(colaborador: any, dataReferencia?: Date): boolean {
    // Verifica se o colaborador tem data de desligamento preenchida
    const dataDemissaoOriginal = this.obterValorCampo(colaborador, ['dtdemissao', 'Dtdemissao', 'DtDemissao']);
    if (!dataDemissaoOriginal) {
      return false;
    }
    
    try {
      // Converte a data de demissão para Date (pode vir como string ou Date)
      const dataDemissao = new Date(dataDemissaoOriginal);
      
      // Se não foi passada data de referência, usa a data atual
      const dataRef = dataReferencia || new Date();
      dataRef.setHours(0, 0, 0, 0);
      
      // Considera desligado se a data de demissão for menor ou igual à data de referência
      return dataDemissao <= dataRef;
    } catch (error) {
      // Se houver erro na conversão da data, considera como não desligado
      console.warn('Erro ao processar data de demissão:', colaborador.dtdemissao, error);
      return false;
    }
  }

  // 🎯 MÉTODO PARA FILTRAR POR TIPO (CARDS CLICÁVEIS)
  async filtrarPorTipo(tipo: string): Promise<void> {
    this.filtroAtivo = tipo;
    
    if (tipo === 'total') {
      this.tipoFiltroAtual = null;
      await this.carregarPagina(1, this.termoPesquisaAtual, null);
    } else {
      this.tipoFiltroAtual = tipo;
      await this.carregarPagina(1, this.termoPesquisaAtual, tipo);
    }
  }

  // ========== MÉTODOS DO MODAL DE IMPORTAÇÃO ==========

  /**
   * Abre modal de importação
   */
  abrirModalImportacao(): void {
    this.mostrarModalImportacao = true;
    this.passoImportacao = 1;
    this.resetarImportacao();
  }

  /**
   * Fecha modal de importação
   */
  fecharModalImportacao(): void {
    this.mostrarModalImportacao = false;
    this.resetarImportacao();
  }

  /**
   * Reseta estado da importação
   */
  resetarImportacao(): void {
    this.passoImportacao = 1;
    this.arquivoSelecionadoImport = null;
    this.uploadandoImportacao = false;
    this.importandoColaboradores = false;
    this.resultadoValidacaoImport = null;
    this.resultadoImportacaoFinal = null;
    this.loteAtualImport = null;
  }

  /**
   * Baixar template Excel
   */
  baixarTemplate(): void {
    const url = this.importacaoService.getUrlTemplate();
    window.open(url, '_blank');
  }

  /**
   * Quando arquivo é selecionado no modal
   */
  onArquivoSelecionadoImportacao(event: any): void {
    const arquivo: File = event.target.files[0];
    
    if (!arquivo) {
      return;
    }

    // Validar extensão
    const extensoesValidas = ['.xlsx', '.xls'];
    const extensao = arquivo.name.substring(arquivo.name.lastIndexOf('.')).toLowerCase();
    
    if (!extensoesValidas.includes(extensao)) {
      this.util.exibirMensagemToast('Formato de arquivo inválido. Use apenas arquivos Excel (.xlsx, .xls)', 5000);
      event.target.value = '';
      return;
    }

    // Validar tamanho (10MB)
    const tamanhoMaximo = 10 * 1024 * 1024;
    if (arquivo.size > tamanhoMaximo) {
      this.util.exibirMensagemToast('Arquivo muito grande. Tamanho máximo: 10MB', 5000);
      event.target.value = '';
      return;
    }

    this.arquivoSelecionadoImport = arquivo;
    
    // Fazer upload automaticamente
    this.fazerUploadImportacao();
  }

  /**
   * Upload e validação do arquivo
   */
  fazerUploadImportacao(): void {
    if (!this.arquivoSelecionadoImport) {
      this.util.exibirMensagemToast('Selecione um arquivo primeiro', 3000);
      return;
    }

    this.uploadandoImportacao = true;
    this.passoImportacao = 2;

    this.importacaoService.uploadArquivo(this.arquivoSelecionadoImport).subscribe({
      next: (resultado) => {
        this.resultadoValidacaoImport = resultado;
        this.loteAtualImport = resultado.loteId;
        this.uploadandoImportacao = false;
        
        if (resultado.podeImportar) {
          this.util.exibirMensagemToast('✅ ' + resultado.mensagem, 5000);
        } else {
          this.util.exibirMensagemToast('⚠️ ' + resultado.mensagem, 5000);
        }
      },
      error: (erro) => {
        this.uploadandoImportacao = false;
        this.passoImportacao = 1;
        const mensagem = erro.error?.mensagem || 'Erro ao processar arquivo';
        this.util.exibirMensagemToast('❌ ' + mensagem, 5000);
        console.error('Erro no upload:', erro);
      }
    });
  }

  /**
   * Confirma a importação
   */
  confirmarImportacao(): void {
    if (!this.loteAtualImport || !this.resultadoValidacaoImport) return;

    // Mensagem formatada seguindo o padrão do sistema
    const message = 
      `Tem certeza que deseja confirmar esta importação?<br><br>` +
      `📊 <strong>Resumo:</strong><br>` +
      `• <strong>Total de colaboradores:</strong> ${this.resultadoValidacaoImport.totalValidos}<br>` +
      (this.resultadoValidacaoImport.totalAtualizacoes && this.resultadoValidacaoImport.totalAtualizacoes > 0 ? 
        `• <strong>Atualizações detectadas:</strong> ${this.resultadoValidacaoImport.totalAtualizacoes}<br>` : '') +
      (this.resultadoValidacaoImport.totalNovos && this.resultadoValidacaoImport.totalNovos > 0 ? 
        `• <strong>Novos colaboradores:</strong> ${this.resultadoValidacaoImport.totalNovos}<br>` : '') +
      (this.resultadoValidacaoImport.totalSemAlteracao && this.resultadoValidacaoImport.totalSemAlteracao > 0 ? 
        `• <strong>Sem movimentação:</strong> ${this.resultadoValidacaoImport.totalSemAlteracao}<br>` : '') +
      (this.resultadoValidacaoImport.novasEmpresas > 0 ? 
        `• <strong>Empresas a criar:</strong> ${this.resultadoValidacaoImport.novasEmpresas}<br>` : '') +
      (this.resultadoValidacaoImport.novasLocalidades > 0 ? 
        `• <strong>Localidades a criar:</strong> ${this.resultadoValidacaoImport.novasLocalidades}<br>` : '') +
      (this.resultadoValidacaoImport.novoscentrosCusto > 0 ? 
        `• <strong>Centros de Custo a criar:</strong> ${this.resultadoValidacaoImport.novoscentrosCusto}<br>` : '') +
      (this.resultadoValidacaoImport.novasFiliais > 0 ? 
        `• <strong>Filiais a criar:</strong> ${this.resultadoValidacaoImport.novasFiliais}<br>` : '') +
      `<br>⚠️ <strong>Atenção:</strong> Esta ação criará novos registros no banco de dados e não poderá ser desfeita.`;

    this.util.exibirMensagemPopUp(message, true).then(res => {
      if (res) {
        this.importandoColaboradores = true;
        this.passoImportacao = 3;

        this.importacaoService.confirmarImportacao(this.loteAtualImport!).subscribe({
          next: (resultado) => {
            this.resultadoImportacaoFinal = resultado;
            this.importandoColaboradores = false;
            this.passoImportacao = 4;
            
            this.util.exibirMensagemToast('✅ ' + resultado.mensagem, 5000);

            // Recarregar lista de colaboradores
            this.listar();
          },
          error: (erro) => {
            this.importandoColaboradores = false;
            this.passoImportacao = 2;
            const mensagem = erro.error?.mensagem || 'Erro ao importar dados';
            this.util.exibirMensagemToast('❌ ' + mensagem, 5000);
            console.error('Erro na importação:', erro);
          }
        });
      }
    });
  }

  baixarErrosValidacao(): void {
    if (!this.loteAtualImport || this.baixandoErros) {
      return;
    }

    this.baixandoErros = true;

    const url = `${this.importacaoService.getUrlErros(this.loteAtualImport)}`;
    const token = this.session.token;

    this.importacaoService.baixarErros(url, token).subscribe({
      next: (blob) => {
        const link = document.createElement('a');
        const objectUrl = window.URL.createObjectURL(blob);
        link.href = objectUrl;
        link.download = `erros_importacao_${this.loteAtualImport}.csv`;
        link.click();
        window.URL.revokeObjectURL(objectUrl);
        this.baixandoErros = false;
      },
      error: (erro) => {
        console.error('Erro ao baixar erros:', erro);
        this.util.exibirMensagemToast('❌ Não foi possível baixar o arquivo de erros.', 5000);
        this.baixandoErros = false;
      }
    });
  }

  /**
   * Cancela a importação
   */
  cancelarImportacaoModal(): void {
    if (!this.loteAtualImport) {
      this.fecharModalImportacao();
      return;
    }

    // Mensagem formatada seguindo o padrão do sistema
    const message = 
      `Tem certeza que deseja cancelar esta importação?<br><br>` +
      `⚠️ <strong>Atenção:</strong> Os dados validados serão descartados e você precisará fazer o upload novamente.<br><br>` +
      `📋 <strong>Lote:</strong> ${this.loteAtualImport}`;

    this.util.exibirMensagemPopUp(message, true).then(res => {
      if (res) {
        this.importacaoService.cancelarImportacao(this.loteAtualImport!).subscribe({
          next: () => {
            this.util.exibirMensagemToast('ℹ️ Importação cancelada', 3000);
            this.fecharModalImportacao();
          },
          error: (erro) => {
            console.error('Erro ao cancelar:', erro);
            this.fecharModalImportacao();
          }
        });
      }
    });
  }

  // ========== MÉTODOS DO MODAL DE EXPORTAÇÃO ==========

  /**
   * Abre modal de exportação
   */
  abrirModalExportacao(): void {
    this.mostrarModalExportacao = true;
  }

  /**
   * Fecha modal de exportação
   */
  fecharModalExportacao(): void {
    this.mostrarModalExportacao = false;
  }

  /**
   * Exportar para Excel
   */
  async exportarExcel(): Promise<void> {
    try {
      this.util.aguardar(true);
      const dadosExportacao = this.prepararDadosColaboradores();
      
      if (dadosExportacao && dadosExportacao.length > 0) {
        // Criar workbook
        const wb = XLSX.utils.book_new();
        const ws = XLSX.utils.json_to_sheet(dadosExportacao);
        XLSX.utils.book_append_sheet(wb, ws, 'Colaboradores');
        
        // Gerar e baixar arquivo
        XLSX.writeFile(wb, `colaboradores_${new Date().toISOString().slice(0,10)}.xlsx`);
        
        this.util.exibirMensagemToast('✅ Exportação Excel concluída com sucesso!', 3000);
        this.fecharModalExportacao();
      } else {
        this.util.exibirMensagemToast('⚠️ Nenhum dado disponível para exportar', 3000);
      }
    } catch (error) {
      console.error('[COLABORADORES] Erro na exportação Excel:', error);
      this.util.exibirMensagemToast('❌ Erro na exportação Excel', 3000);
    } finally {
      this.util.aguardar(false);
    }
  }

  /**
   * Exportar para CSV
   */
  async exportarCSV(): Promise<void> {
    try {
      this.util.aguardar(true);
      const dadosExportacao = this.prepararDadosColaboradores();
      
      if (dadosExportacao && dadosExportacao.length > 0) {
        // Gerar CSV
        const csvContent = this.converterParaCSV(dadosExportacao);
        
        // Download
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        const url = URL.createObjectURL(blob);
        link.setAttribute('href', url);
        link.setAttribute('download', `colaboradores_${new Date().toISOString().slice(0,10)}.csv`);
        link.style.visibility = 'hidden';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        this.util.exibirMensagemToast('✅ Exportação CSV concluída com sucesso!', 3000);
        this.fecharModalExportacao();
      } else {
        this.util.exibirMensagemToast('⚠️ Nenhum dado disponível para exportar', 3000);
      }
    } catch (error) {
      console.error('[COLABORADORES] Erro na exportação CSV:', error);
      this.util.exibirMensagemToast('❌ Erro na exportação CSV', 3000);
    } finally {
      this.util.aguardar(false);
    }
  }

  /**
   * Prepara dados dos colaboradores para exportação
   */
  private prepararDadosColaboradores(): any[] {
    // Usar dadosFiltrados para exportar apenas o que está visível com os filtros atuais
    const dadosBase = this.dadosFiltrados && this.dadosFiltrados.length > 0
      ? this.dadosFiltrados
      : this.dadosOriginais;
    const dados = Array.isArray(dadosBase) ? [...dadosBase] : [];
    
    if (!dados || dados.length === 0) {
      return [];
    }

    return dados.map(col => ({
      'Matrícula': this.obterValorCampo(col, ['matricula', 'Matricula']) || 'N/A',
      'Nome': this.obterValorCampo(col, ['nome', 'Nome']) || 'N/A',
      'CPF': this.obterValorCampo(col, ['cpf', 'Cpf']) || 'N/A',
      'Email': this.obterValorCampo(col, ['email', 'Email']) || 'N/A',
      'Cargo': this.obterValorCampo(col, ['cargo', 'Cargo']) || 'N/A',
      'Setor': this.obterValorCampo(col, ['setor', 'Setor']) || 'N/A',
      'Tipo': this.getTipoLabel(this.obterValorCampo(col, ['tipoColaborador', 'Tipocolaborador', 'TipoColaborador'])),
      'Empresa': this.obterValorCampo(col, ['empresa', 'Empresa']) || 'N/A',
      'Centro de Custo': this.obterValorCampo(col, ['nomeCentroCusto', 'NomeCentroCusto']) || 'N/A',
      'Código Centro Custo': this.obterValorCampo(col, ['codigoCentroCusto', 'CodigoCentroCusto']) || 'N/A',
      'Localidade': this.montarDescricaoLocalidade(col),
      'Data Admissão': this.formatarData(this.obterValorCampo(col, ['dtadmissao', 'Dtadmissao', 'DtAdmissao'])),
      'Data Demissão': this.formatarData(this.obterValorCampo(col, ['dtdemissao', 'Dtdemissao', 'DtDemissao'])),
      'Situação': this.obterDescricaoSituacao(col),
      'Matrícula Superior': this.obterValorCampo(col, ['matriculasuperior', 'matriculaSuperior', 'MatriculaSuperior']) || 'N/A',
      'Data Cadastro': this.formatarData(this.obterValorCampo(col, ['dtcadastro', 'DtCadastro', 'dtCadastro']))
    }));
  }

  /**
   * Monta descrição amigável para localidade combinando descrição, cidade e estado
   */
  private montarDescricaoLocalidade(col: any): string {
    const descricao = this.obterValorCampo(col, ['localidadeDescricao', 'LocalidadeDescricao', 'localidade', 'Localidade']) || '';
    const cidade = this.obterValorCampo(col, ['localidadeCidade', 'LocalidadeCidade']) || '';
    const estado = this.obterValorCampo(col, ['localidadeEstado', 'LocalidadeEstado']) || '';

    if (!descricao && !cidade && !estado) {
      return 'N/A';
    }

    const partesLocalidade = [];
    if (descricao) {
      partesLocalidade.push(descricao);
    }

    const partesCidadeEstado = [cidade, estado].filter(part => !!part);
    if (partesCidadeEstado.length > 0) {
      partesLocalidade.push(partesCidadeEstado.join(' - '));
    }

    return partesLocalidade.join(' | ');
  }

  /**
   * Converte datas para formato pt-BR, aceitando string ou Date
   */
  private formatarData(valor: any): string {
    if (!valor) {
      return 'N/A';
    }

    try {
      const data = new Date(valor);
      if (isNaN(data.getTime())) {
        return 'N/A';
      }
      return data.toLocaleDateString('pt-BR');
    } catch (error) {
      console.warn('[COLABORADORES] Não foi possível converter data:', valor, error);
      return 'N/A';
    }
  }

  /**
   * Determina a descrição mais amigável para a situação do colaborador
   */
  private obterDescricaoSituacao(col: any): string {
    const situacao = (this.obterValorCampo(col, ['situacao', 'Situacao']) || '').toUpperCase();
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);

    if (situacao === 'D' || this.isColaboradorDesligado(col, hoje)) {
      return 'Desligado';
    }

    if (situacao === 'I') {
      return 'Inativo';
    }

    if (situacao === 'F') {
      return 'Férias';
    }

    if (situacao === 'A') {
      const dataDemissaoOriginal = this.obterValorCampo(col, ['dtdemissao', 'Dtdemissao', 'DtDemissao']);
      if (dataDemissaoOriginal) {
        const dataDemissao = new Date(dataDemissaoOriginal);
        if (!isNaN(dataDemissao.getTime()) && dataDemissao > hoje) {
          return 'Ativo (Programado)';
        }
      }
      return 'Ativo';
    }

    const dataDemissaoOriginal = this.obterValorCampo(col, ['dtdemissao', 'Dtdemissao', 'DtDemissao']);
    if (dataDemissaoOriginal) {
      const dataDemissao = new Date(dataDemissaoOriginal);
      if (!isNaN(dataDemissao.getTime())) {
        if (dataDemissao > hoje) {
          return 'Ativo (Programado)';
        }
        if (dataDemissao <= hoje) {
          return 'Desligado';
        }
      }
    }

    return 'Ativo';
  }

  /**
   * Recupera dinamicamente o valor de uma chave (camelCase/PascalCase) do objeto
   */
  private obterValorCampo(obj: any, chaves: string[]): any {
    if (!obj) {
      return undefined;
    }

    for (const chave of chaves) {
      if (obj.hasOwnProperty(chave) && obj[chave] !== undefined && obj[chave] !== null) {
        return obj[chave];
      }
    }
    return undefined;
  }

  /**
   * Normaliza o tipo de colaborador para código padrão (F, T, C)
   */
  private getTipoCodigo(tipo: any): string {
    if (tipo === null || tipo === undefined) {
      return '';
    }

    const valor = tipo.toString().trim().toUpperCase();

    if (!valor) {
      return '';
    }

    if (valor === 'F' || valor === 'FUNCIONARIO' || valor === 'FUNCIONÁRIO') {
      return 'F';
    }

    if (valor === 'T' || valor === 'TERCEIRIZADO') {
      return 'T';
    }

    if (valor === 'C' || valor === 'CONSULTOR') {
      return 'C';
    }

    return valor.length === 1 ? valor : '';
  }

  /**
   * Converte dados para CSV
   */
  private converterParaCSV(dados: any[]): string {
    if (!dados || dados.length === 0) {
      return '';
    }

    // Cabeçalhos
    const headers = Object.keys(dados[0]);
    let csvContent = headers.join(',') + '\n';
    
    // Dados
    for (const row of dados) {
      const values = headers.map(header => {
        const value = row[header];
        // Escapar aspas e adicionar aspas ao redor
        const escaped = String(value || '').replace(/"/g, '""');
        return `"${escaped}"`;
      });
      csvContent += values.join(',') + '\n';
    }
    
    return csvContent;
  }
}
