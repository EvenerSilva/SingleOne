import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatTabGroup } from '@angular/material/tabs';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ContestacaoApiService } from 'src/app/api/contestacoes/contestacao-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Contestacao, ContestacaoStatus, ContestacaoEstatisticas } from 'src/app/models/contestacao.interface';

@Component({
  selector: 'app-contestacoes',
  templateUrl: './contestacoes.component.html',
  styleUrls: ['./contestacoes.component.scss']
})
export class ContestacoesComponent implements OnInit, AfterViewInit {

  private session: any = {};
  public colunas = ['id', 'status', 'colaborador', 'equipamento', 'dataContestacao', 'tecnicoResponsavel', 'acoes'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  @ViewChild(MatTabGroup, { static: false }) tabGroup: MatTabGroup;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public statusFiltro: string = 'pendente';
  public tipoFiltro: string = 'contestacao'; // fixo para exibir apenas contestações
  
  // Propriedades para estatísticas totais (sem filtro)
  public estatisticasTotais: ContestacaoEstatisticas = {
    total: 0,
    abertas: 0,
    emAnalise: 0,
    resolvidas: 0,
    canceladas: 0,
    negadas: 0,
    pendentesColaborador: 0,
    resolvidasHoje: 0,
    pendentesUrgentes: 0
  };
  
  // Estatísticas separadas por tipo
  public estatisticasContestacoes: ContestacaoEstatisticas = {
    total: 0,
    abertas: 0,
    emAnalise: 0,
    resolvidas: 0,
    canceladas: 0,
    negadas: 0,
    pendentesColaborador: 0,
    resolvidasHoje: 0,
    pendentesUrgentes: 0
  };
  
  public estatisticasAutoInventario: ContestacaoEstatisticas = {
    total: 0,
    abertas: 0,
    emAnalise: 0,
    resolvidas: 0,
    canceladas: 0,
    negadas: 0,
    pendentesColaborador: 0,
    resolvidasHoje: 0,
    pendentesUrgentes: 0
  };
  
  public estatisticasInventarioForcado: ContestacaoEstatisticas = {
    total: 0,
    abertas: 0,
    emAnalise: 0,
    resolvidas: 0,
    canceladas: 0,
    negadas: 0,
    pendentesColaborador: 0,
    resolvidasHoje: 0,
    pendentesUrgentes: 0
  };
  
  // Dados completos para estatísticas
  private dadosCompletos: any[] = [];
  private dadosContestacoes: any[] = [];
  private dadosAutoInventario: any[] = [];
  private dadosInventarioForcado: any[] = [];

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
    private api: ContestacaoApiService,
    private route: Router,
    private activatedRoute: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.session = this.util.getSession('usuario');
    if (this.session && this.session.usuario) {
      this.cliente = this.session.usuario.cliente;
    } else {
      console.error('[CONTESTACOES] Sessão ou usuário inválido');
      this.cliente = 1; // Valor padrão para evitar erro
    }
    
    // Validar se o cliente é válido antes de fazer a requisição
    if (!this.cliente || this.cliente <= 0) {
      console.error('[CONTESTACOES] Cliente inválido:', this.cliente);
      this.util.exibirMensagemToast('Cliente inválido. Por favor, faça login novamente.', 5000);
      this.route.navigate(['/']);
      return;
    }
    
    // Processar query parameters para busca automática e navegação de aba
    this.activatedRoute.queryParams.subscribe(params => {
      const temTab = params['tab'];
      const temSearch = params['search'];
      
      // 🎯 Verificar se veio com aba específica (ex: vindo de colaboradores sem recursos)
      if (temTab) {
        const tab = params['tab'];
        setTimeout(() => {
          if (this.tabGroup) {
            if (tab === 'inventario-forcado') {
              this.tabGroup.selectedIndex = 2;
              this.tipoFiltro = 'inventario_forcado';
            } else if (tab === 'auto-inventario') {
              this.tabGroup.selectedIndex = 1;
              this.tipoFiltro = 'auto_inventario';
            } else if (tab === 'contestacoes') {
              this.tabGroup.selectedIndex = 0;
              this.tipoFiltro = 'contestacao';
            }
            
            // 🔍 Se tem search, aguardar a busca ser executada antes de aplicar filtro de aba
            if (!temSearch) {
              this.aplicarFiltroPorTipo();
            }
          }
        }, 500);
      }
      
      // 🔍 PRIORIDADE: Busca tem prioridade sobre listagem completa
      if (temSearch) {
        const searchTerm = params['search'];
        setTimeout(() => {
          this.consulta.setValue(searchTerm, { emitEvent: false }); // Não emitir evento para evitar dupla busca
          this.buscar(searchTerm);
          
          this.util.exibirMensagemToast(
            `🔍 Buscando: "${searchTerm}"`, 
            3000
          );
        }, temTab ? 600 : 100); // Aguardar mais tempo se tiver que trocar aba
      } else if (!temTab) {
        // Se não há parâmetro de busca nem de aba, carregar apenas contestações pendentes por padrão
        this.listar(null);
      } else {
        // Se tem parâmetro de aba mas não tem busca, carregar dados
        setTimeout(() => {
          this.listar(null);
        }, 600);
      }
    });
    
    // Configurar subscription para o campo de busca
    this.consulta.valueChanges.pipe(
      debounceTime(500), // Aguardar 500ms após o usuário parar de digitar
      tap(valor => {
        if (valor && valor.trim()) {
          this.buscar(valor.trim());
        } else if (valor === '' || valor === null) {
          // Se o campo estiver vazio, recarregar todas as contestações
          this.listar(null);
        }
      })
    ).subscribe();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  // Método auxiliar para configurar paginador
  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      console.warn('[CONTESTACOES] Paginador ou dataSource não disponível para configuração');
      return;
    }
    
    this.dataSource.paginator = this.paginator;
    this.paginator.pageSize = 10;
    this.paginator.pageIndex = 0;
    
    this.paginator.page.subscribe(() => {
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    });
  }

  listar(event: PageEvent): Promise<void> {
    // Validar sessão antes de fazer a requisição
    if (!this.session || !this.session.token) {
      console.error('[CONTESTACOES] Sessão inválida ao listar');
      this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
      this.route.navigate(['/']);
      return;
    }
    
    this.util.aguardar(true);
    return this.carregarTodasContestacoes('null')
      .then(todosResultados => {
        this.util.aguardar(false);
        this.dadosCompletos = [...todosResultados];
        
        // Separar dados por tipo
        this.separarDadosPorTipo(todosResultados);
        
        // Calcular estatísticas com TODOS os dados
        this.calcularEstatisticas(todosResultados);
        
        // Aplicar filtro por tipo e status (pendente por padrão)
        this.aplicarFiltroPorTipo();
        
        // Configurar paginador corretamente com base nos dados filtrados
        if (this.paginator) {
          this.paginator.pageIndex = 0;
          this.paginator.pageSize = 10;
          this.paginator.length = this.dataSource?.data?.length || 0;
          this.configurarPaginador();
        }
        
        const totalFiltrado = this.dataSource?.data?.length || 0;
        this.util.exibirMensagemToast(
          `Exibindo ${totalFiltrado} contestações pendentes de ${todosResultados.length} no total`,
          3000
        );
      })
      .catch(err => {
        this.util.aguardar(false);
        console.error('[CONTESTACOES] Erro ao carregar todas as páginas:', err);
        const errorStatus = err?.response?.status || err?.status || err?.statusCode;
        if (errorStatus === 401) {
          this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
          this.route.navigate(['/']);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      });
  }

  buscar(valor) {
    // Validar sessão antes de fazer a requisição
    if (!this.session || !this.session.token) {
      console.error('[CONTESTACOES] Sessão inválida ao buscar');
      this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
      this.route.navigate(['/']);
      return;
    }
    
    if (valor != '' && valor != null && valor != 'null') {
      this.util.aguardar(true);
      
      this.carregarTodasContestacoes(valor)
        .then(results => {
          this.util.aguardar(false);
          this.dadosCompletos = [...results];
          this.separarDadosPorTipo(results);
          this.calcularEstatisticas(results);
          this.aplicarFiltroPorTipo();
          
          if (this.paginator) {
            this.paginator.pageIndex = 0;
            let tamanho = this.dadosContestacoes.length;
            if (this.tipoFiltro === 'auto_inventario') {
              tamanho = this.dadosAutoInventario.length;
            } else if (this.tipoFiltro === 'inventario_forcado') {
              tamanho = this.dadosInventarioForcado.length;
            }
            this.paginator.length = tamanho;
            this.configurarPaginador();
          }
          
          if (results.length === 0) {
            this.util.exibirMensagemToast(
              'Nenhuma contestação encontrada com os critérios informados', 
              3000
            );
          } else {
            this.util.exibirMensagemToast(
              `Busca realizada: ${results.length} registros encontrados`, 
              3000
            );
          }
        })
        .catch(err => {
          this.util.aguardar(false);
          console.error('[CONTESTACOES] Erro na busca agregada:', err);
          const errorStatus = err?.response?.status || err?.status || err?.statusCode;
          if (errorStatus === 401) {
            this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
            this.route.navigate(['/']);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        });
    } else {
      // Quando não há valor de busca, recarregar todas as contestações
      this.listar(null);
    }
  }

  // Carrega todas as páginas da API e agrega em uma única lista
  private async carregarTodasContestacoes(filtro: string | null): Promise<any[]> {
    const resultadosAgregados: any[] = [];
    let paginaAtual = 1;
    const maxPaginas = 100; // segurança para evitar loop infinito
    
    while (paginaAtual <= maxPaginas) {
      const res = await this.api.listarContestacoes(
        (filtro === '' || filtro === null) ? 'null' : filtro,
        this.cliente,
        paginaAtual,
        this.session.token
      );
      if (!res || res.error || (res.status !== 200 && res.status !== 204)) {
        console.warn('[CONTESTACOES] Interrompendo agregação por resposta inválida na página', paginaAtual, res);
        break;
      }
      
      const paginaResults = res.data?.results || res.data || [];
      const rowCount = res.data?.rowCount;
      
      if (!Array.isArray(paginaResults) || paginaResults.length === 0) {
        break;
      }
      
      resultadosAgregados.push(...paginaResults);
      if (rowCount && resultadosAgregados.length >= rowCount) {
        break;
      }
      
      paginaAtual++;
    }
    
    return resultadosAgregados;
  }

  // Métodos para estatísticas - baseados no tipo ativo
  getTotalContestacoes(): number {
    if (this.tipoFiltro === 'auto_inventario') {
      return this.estatisticasAutoInventario.total;
    } else if (this.tipoFiltro === 'inventario_forcado') {
      return this.estatisticasInventarioForcado.total;
    }
    return this.estatisticasContestacoes.total;
  }

  getContestacoesPendentes(): number {
    if (this.tipoFiltro === 'auto_inventario') {
      return this.estatisticasAutoInventario.abertas;
    } else if (this.tipoFiltro === 'inventario_forcado') {
      return this.estatisticasInventarioForcado.abertas;
    }
    return this.estatisticasContestacoes.abertas;
  }

  getContestacoesResolvidas(): number {
    if (this.tipoFiltro === 'auto_inventario') {
      return this.estatisticasAutoInventario.resolvidas;
    } else if (this.tipoFiltro === 'inventario_forcado') {
      return this.estatisticasInventarioForcado.resolvidas;
    }
    return this.estatisticasContestacoes.resolvidas;
  }

  getContestacoesCanceladas(): number {
    if (this.tipoFiltro === 'auto_inventario') {
      return this.estatisticasAutoInventario.canceladas;
    } else if (this.tipoFiltro === 'inventario_forcado') {
      return this.estatisticasInventarioForcado.canceladas;
    }
    return this.estatisticasContestacoes.canceladas;
  }

  getContestacoesNegadas(): number {
    if (this.tipoFiltro === 'auto_inventario') {
      return this.estatisticasAutoInventario.negadas;
    } else if (this.tipoFiltro === 'inventario_forcado') {
      return this.estatisticasInventarioForcado.negadas;
    }
    return this.estatisticasContestacoes.negadas;
  }

  // Métodos para retornar pendentes por tipo (para badges das abas)
  getPendentesContestacoes(): number {
    return this.estatisticasContestacoes.abertas;
  }

  getPendentesAutoInventario(): number {
    return this.estatisticasAutoInventario.abertas;
  }

  getPendentesInventarioForcado(): number {
    return this.estatisticasInventarioForcado.abertas;
  }

  // Métodos para classes de status
  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'pendente':
        return 'status-pending';
      case 'cancelada':
        return 'status-cancelled';
      case 'resolvida':
        return 'status-resolved';
      case 'em análise':
      case 'em_analise':
        return 'status-analyzing';
      default:
        return 'status-default';
    }
  }

  // Métodos getter para acessar dados da contestação de forma segura
  getContestacaoId(row: any): number {
    return row?.id || row?.Id || 0;
  }

  getContestacaoStatus(row: any): string {
    return row?.status || row?.Status || 'Desconhecido';
  }

  getContestacaoStatusId(row: any): number {
    return row?.statusId || row?.StatusId || 0;
  }

  getContestacaoColaborador(row: any): string {
    return row?.colaborador?.nome || row?.Colaborador?.Nome || row?.colaboradorNome || 'N/A';
  }

  getContestacaoColaboradorId(row: any): number {
    return row?.colaborador?.id || row?.Colaborador?.Id || row?.colaboradorId || 0;
  }

  getContestacaoTecnico(row: any): string {
    return row?.tecnicoResponsavel || row?.TecnicoResponsavel || 'N/A';
  }

  getContestacaoNome(row: any): string {
    const id = this.getContestacaoId(row);
    return `Contestação ${id}`;
  }

  getContestacaoDataCriacao(row: any): string {
    const data = row?.dataContestacao || row?.DataContestacao;
    if (data) {
      try {
        return new Date(data).toLocaleDateString('pt-BR');
      } catch (error) {
        return 'Data inválida';
      }
    }
    return 'N/A';
  }

  getContestacaoEquipamento(row: any): string {
    return row?.equipamento?.nome || row?.Equipamento?.Nome || row?.equipamentoNome || 'N/A';
  }

  getContestacaoEquipamentoId(row: any): number {
    return row?.equipamento?.id || row?.Equipamento?.Id || row?.equipamentoId || 0;
  }

  getContestacaoNumeroSerie(row: any): string {
    return row?.equipamento?.numeroSerie || row?.Equipamento?.NumeroSerie || row?.equipamentoNumeroSerie || 'N/A';
  }

  // Verifica se o equipamento é uma linha telefônica (número possui 10 ou 11 dígitos)
  isLinhaTelefonica(row: any): boolean {
    const nome = this.getContestacaoEquipamento(row);
    return /^\d{10,11}$/.test(nome);
  }

  // Formata número de telefone para exibição
  formatarNumeroTelefone(numero: string): string {
    if (!numero || numero === 'N/A') return numero;
    
    // Remove caracteres não numéricos
    const apenasNumeros = numero.replace(/\D/g, '');
    
    // Formata para (XX) XXXXX-XXXX ou (XX) XXXX-XXXX
    if (apenasNumeros.length === 11) {
      return `(${apenasNumeros.substring(0, 2)}) ${apenasNumeros.substring(2, 7)}-${apenasNumeros.substring(7)}`;
    } else if (apenasNumeros.length === 10) {
      return `(${apenasNumeros.substring(0, 2)}) ${apenasNumeros.substring(2, 6)}-${apenasNumeros.substring(6)}`;
    }
    
    return numero;
  }

  // Retorna equipamento formatado para exibição
  getContestacaoEquipamentoFormatado(row: any): string {
    const nome = this.getContestacaoEquipamento(row);
    
    if (this.isLinhaTelefonica(row)) {
      return `Linha ${this.formatarNumeroTelefone(nome)}`;
    }
    
    return nome;
  }

  // Retorna label do número de série
  getLabelNumeroSerie(row: any): string {
    return this.isLinhaTelefonica(row) ? 'ICCID' : 'S/N';
  }

  navegarParaTimelineColaborador(row: any): void {
    const colaboradorId = this.getContestacaoColaboradorId(row);
    
    if (colaboradorId > 0) {
      this.route.navigate(['relatorios/timeline-colaboradores'], { 
        queryParams: { id: colaboradorId.toString() } 
      });
    }
  }

  navegarParaTimelineRecurso(row: any): void {
    // Para timeline, SEMPRE usar número de série (para equipamentos é S/N, para linhas é ICCID)
    let numeroSerie = this.getContestacaoNumeroSerie(row);
    
    // FALLBACK: Se número de série for N/A, tenta usar o nome do equipamento (pode ser o número da linha)
    if ((!numeroSerie || numeroSerie === 'N/A') && this.isLinhaTelefonica(row)) {
      const nomeEquipamento = this.getContestacaoEquipamento(row);
      if (nomeEquipamento && nomeEquipamento !== 'N/A') {
        numeroSerie = nomeEquipamento;
      }
    }
    
    if (numeroSerie && numeroSerie !== 'N/A') {
      this.route.navigate(['relatorios/timeline-recursos'], { 
        queryParams: { sn: numeroSerie }
      });
    } else {
      this.util.exibirMensagemToast('Não foi possível identificar o recurso para exibir a timeline. Verifique se o ICCID está cadastrado.', 5000);
    }
  }

  // Verifica se a contestação está urgente (mais de 3 dias sem resolução)
  isContestacaoUrgente(row: any): boolean {
    const statusId = this.getContestacaoStatusId(row);
    if (statusId === 3 || statusId === 4) { // Resolvida ou Cancelada
      return false;
    }

    const data = row?.dataContestacao || row?.DataContestacao;
    if (data) {
      try {
        const dataContestacao = new Date(data);
        const hoje = new Date();
        const diffTime = Math.abs(hoje.getTime() - dataContestacao.getTime());
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return diffDays > 3;
      } catch (error) {
        return false;
      }
    }
    return false;
  }

  // Calcula quantos dias tem a contestação
  getDiasContestacao(row: any): number {
    const data = row?.dataContestacao || row?.DataContestacao;
    if (data) {
      try {
        const dataContestacao = new Date(data);
        const hoje = new Date();
        const diffTime = Math.abs(hoje.getTime() - dataContestacao.getTime());
        return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      } catch (error) {
        return 0;
      }
    }
    return 0;
  }

  // Retorna a mensagem de alerta para o tooltip
  getAlertaContestacao(row: any): string {
    const statusId = this.getContestacaoStatusId(row);
    if (statusId === 3 || statusId === 4) { // Resolvida ou Cancelada
      return '';
    }

    const dias = this.getDiasContestacao(row);
    if (dias > 3) {
      return `⚠️ ALERTA: Esta contestação está há ${dias} dias sem resolução!\n\nPrazo padrão: 3 dias\nStatus: URGENTE\n\nRecomendação: Priorizar atendimento imediatamente.`;
    }
    return '';
  }

  // Métodos para filtrar por status ao clicar nos cards
  filtrarPorStatusCard(status: string): void {
    try {
      let dadosBase: any[] = [];
      
      // Selecionar dados base baseado no tipo ativo
      if (this.tipoFiltro === 'auto_inventario') {
        dadosBase = this.dadosAutoInventario;
      } else if (this.tipoFiltro === 'inventario_forcado') {
        dadosBase = this.dadosInventarioForcado;
      } else {
        dadosBase = this.dadosContestacoes;
      }
      
      if (!dadosBase || dadosBase.length === 0) {
        console.warn('[CONTESTACOES] Nenhum dado disponível para filtrar no tipo:', this.tipoFiltro);
        return;
      }
      
      let dadosFiltrados: any[] = [];
      
      if (status === 'Total') {
        dadosFiltrados = [...dadosBase];
        this.statusFiltro = '';
        const tipoTexto = this.tipoFiltro === 'auto_inventario' ? 'solicitações de auto inventário' : 
                         this.tipoFiltro === 'inventario_forcado' ? 'inventários forçados' : 'contestações';
      } else {
        dadosFiltrados = dadosBase.filter(item => {
          const itemStatus = this.obterStatusContestacao(item);
          const isMatch = itemStatus === status || 
                         itemStatus === status.toLowerCase() || 
                         itemStatus === status.toUpperCase();
          
          if (isMatch) {
          }
          
          return isMatch;
        });
        this.statusFiltro = status;
      }
      
      // Atualizar dataSource com dados filtrados
      this.dataSource = new MatTableDataSource(dadosFiltrados);
      
      // Configurar paginador
      if (this.paginator) {
        this.paginator.pageIndex = 0;
        const pageSizeAtual = this.paginator.pageSize || 10;
        this.paginator.pageSize = pageSizeAtual;
        this.paginator.length = dadosFiltrados.length;
        
        this.configurarPaginador();
      }
      
      // Mostrar mensagem informativa sobre o filtro aplicado
      if (status === 'Total') {
        this.util.exibirMensagemToast(
          `Mostrando histórico completo: ${dadosFiltrados.length} contestações`, 
          3000
        );
      } else if (status === 'Aberta' && this.statusFiltro === '') {
        this.util.exibirMensagemToast(
          `Filtro padrão aplicado: ${dadosFiltrados.length} contestações abertas`, 
          3000
        );
      } else {
        this.util.exibirMensagemToast(
          `Filtro aplicado: ${dadosFiltrados.length} contestações com status "${status}"`, 
          3000
        );
      }
    } catch (error) {
      console.error('[CONTESTACOES] Erro ao filtrar por status:', error);
      this.util.exibirMensagemToast('Erro ao aplicar filtro', 3000);
    }
  }

  // Método para obter o status da contestação de forma segura
  private obterStatusContestacao(item: any): string {
    try {
      if (item?.status) {
        return item.status;
      }
      
      if (item?.Status) {
        return item.Status;
      }
      
      if (item?.statusId) {
        const statusId = item.statusId;
        let statusString = '';
        
        switch (statusId) {
          case 1: statusString = 'Aberta'; break;
          case 2: statusString = 'Em Análise'; break;
          case 3: statusString = 'Resolvida'; break;
          case 4: statusString = 'Cancelada'; break;
          case 5: statusString = 'Pendente Colaborador'; break;
          default: statusString = 'Desconhecido'; break;
        }
        return statusString;
      }
      
      console.warn('[CONTESTACOES] Estrutura de dados não reconhecida para status:', item);
      return 'Desconhecido';
      
    } catch (error) {
      console.error('[CONTESTACOES] Erro ao obter status da contestação:', error);
      return 'Erro';
    }
  }

  // Método para separar dados por tipo
  private separarDadosPorTipo(dados: any[]): void {
    try {
      const inferirTipo = (item: any): 'contestacao' | 'auto_inventario' | 'inventario_forcado' => {
        const bruto = item?.tipo_contestacao || item?.tipoContestacao || item?.TipoContestacao || '';
        const tipoNormalizado = (typeof bruto === 'string' ? bruto : String(bruto || ''))
          .trim()
          .toLowerCase()
          .replace(/[-\s]+/g, '_');

        if (tipoNormalizado === 'inventario_forcado') return 'inventario_forcado';
        if (tipoNormalizado === 'auto_inventario') return 'auto_inventario';
        if (tipoNormalizado === 'contestacao') return 'contestacao';
        if (tipoNormalizado) {
          if (tipoNormalizado === 'inventario_forcado') return 'inventario_forcado';
          if (tipoNormalizado === 'auto_inventario') return 'auto_inventario';
          return 'contestacao';
        }

        // Heurísticas quando o backend não envia tipo_contestacao
        const motivo: string = (item?.motivo || item?.Motivo || '').toString().toLowerCase();
        const equipamentoId: number = item?.equipamento?.id ?? item?.Equipamento?.Id ?? item?.equipamentoId ?? 0;
        
        if (motivo.includes('inventário forçado') || motivo.includes('inventario forcado')) {
          return 'inventario_forcado';
        }
        if (motivo.includes('auto invent') || equipamentoId === 0) {
          return 'auto_inventario';
        }
        return 'contestacao';
      };

      const dadosContest = [] as any[];
      const dadosAutoInv = [] as any[];
      const dadosInvForc = [] as any[];
      
      for (const item of dados) {
        const tipo = inferirTipo(item);
        if (tipo === 'inventario_forcado') {
          dadosInvForc.push(item);
        } else if (tipo === 'auto_inventario') {
          dadosAutoInv.push(item);
        } else {
          dadosContest.push(item);
        }
      }

      this.dadosContestacoes = dadosContest;
      this.dadosAutoInventario = dadosAutoInv;
      this.dadosInventarioForcado = dadosInvForc;
      if (this.dadosContestacoes.length > 0) {
      }
      if (this.dadosAutoInventario.length > 0) {
      }
      if (this.dadosInventarioForcado.length > 0) {
      }
      
      // Calcular estatísticas separadas por tipo
      this.calcularEstatisticasPorTipo();
      
    } catch (error) {
      console.error('[CONTESTACOES] Erro ao separar dados por tipo:', error);
      this.dadosContestacoes = [];
      this.dadosAutoInventario = [];
      this.dadosInventarioForcado = [];
    }
  }
  
  // Método para calcular estatísticas por tipo
  private calcularEstatisticasPorTipo(): void {
    // Calcular estatísticas para contestações
    this.estatisticasContestacoes = this.calcularEstatisticasParaTipo(this.dadosContestacoes);
    
    // Calcular estatísticas para auto inventário
    this.estatisticasAutoInventario = this.calcularEstatisticasParaTipo(this.dadosAutoInventario);
    
    // Calcular estatísticas para inventário forçado
    this.estatisticasInventarioForcado = this.calcularEstatisticasParaTipo(this.dadosInventarioForcado);
  }
  
  // Método auxiliar para calcular estatísticas de um tipo específico
  private calcularEstatisticasParaTipo(dados: any[]): ContestacaoEstatisticas {
    if (!dados || dados.length === 0) {
      return {
        total: 0,
        abertas: 0,
        emAnalise: 0,
        resolvidas: 0,
        canceladas: 0,
        negadas: 0,
        pendentesColaborador: 0,
        resolvidasHoje: 0,
        pendentesUrgentes: 0
      };
    }
    
    const estatisticas: ContestacaoEstatisticas = {
      total: dados.length,
      abertas: 0,
      emAnalise: 0,
      resolvidas: 0,
      canceladas: 0,
      negadas: 0,
      pendentesColaborador: 0,
      resolvidasHoje: 0,
      pendentesUrgentes: 0
    };
    
    dados.forEach(item => {
      const status = this.obterStatusContestacao(item);
      
      switch (status.toLowerCase()) {
        case 'pendente':
          estatisticas.abertas++;
          break;
        case 'em análise':
        case 'em_analise':
          estatisticas.emAnalise++;
          break;
        case 'resolvida':
          estatisticas.resolvidas++;
          break;
        case 'cancelada':
          estatisticas.canceladas++;
          break;
        case 'negada':
          estatisticas.negadas++;
          break;
        case 'pendente colaborador':
        case 'pendente_colaborador':
          estatisticas.pendentesColaborador++;
          break;
      }
    });
    
    return estatisticas;
  }
  
  // Método para aplicar filtro por tipo baseado na aba ativa
  private aplicarFiltroPorTipo(): void {
    try {
      let dadosFiltrados: any[] = [];
      
      if (this.tipoFiltro === 'auto_inventario') {
        dadosFiltrados = [...this.dadosAutoInventario];
      } else if (this.tipoFiltro === 'inventario_forcado') {
        dadosFiltrados = [...this.dadosInventarioForcado];
      } else {
        dadosFiltrados = [...this.dadosContestacoes];
      }
      
      // Se houver filtro de status, aplicar também
      if (this.statusFiltro && this.statusFiltro !== '') {
        const tamanhoAntes = dadosFiltrados.length;
        dadosFiltrados = dadosFiltrados.filter(item => {
          const itemStatus = this.obterStatusContestacao(item);
          return itemStatus === this.statusFiltro || 
                 itemStatus === this.statusFiltro.toLowerCase() || 
                 itemStatus === this.statusFiltro.toUpperCase();
        });
      }
      
      this.dataSource = new MatTableDataSource(dadosFiltrados);
      
      // Configurar paginador
      if (this.paginator) {
        this.paginator.pageIndex = 0;
        this.paginator.length = dadosFiltrados.length;
        this.configurarPaginador();
      }
      let tipoTexto = 'contestações';
      if (this.tipoFiltro === 'auto_inventario') {
        tipoTexto = 'solicitações de auto inventário';
      } else if (this.tipoFiltro === 'inventario_forcado') {
        tipoTexto = 'inventários forçados';
      }
      
      const mensagem = this.statusFiltro && this.statusFiltro !== '' 
        ? `Mostrando ${dadosFiltrados.length} ${tipoTexto} com status "${this.statusFiltro}"`
        : `Mostrando ${dadosFiltrados.length} ${tipoTexto}`;
      
      this.util.exibirMensagemToast(mensagem, 3000);
      
    } catch (error) {
      console.error('[CONTESTACOES] Erro ao aplicar filtro por tipo:', error);
    }
  }
  
  // Método para trocar de aba
  public trocarAba(event: any): void {
    try {
      const indice = event.index;
      
      if (indice === 0) {
        this.tipoFiltro = 'contestacao';
      } else if (indice === 1) {
        this.tipoFiltro = 'auto_inventario';
      } else if (indice === 2) {
        this.tipoFiltro = 'inventario_forcado';
      }
      this.statusFiltro = 'pendente';
      
      // Aplicar novo filtro por tipo
      this.aplicarFiltroPorTipo();
      
    } catch (error) {
      console.error('[CONTESTACOES] Erro ao trocar aba:', error);
    }
  }

  // Método para calcular estatísticas
  private calcularEstatisticas(dados: any[]): void {
    try {
      this.dadosCompletos = [...dados];
      if (!dados || dados.length === 0) {
        console.warn('[CONTESTACOES] Nenhum dado para calcular estatísticas');
        this.estatisticasTotais = {
          total: 0,
          abertas: 0,
          emAnalise: 0,
          resolvidas: 0,
          canceladas: 0,
          negadas: 0,
          pendentesColaborador: 0,
          resolvidasHoje: 0,
          pendentesUrgentes: 0
        };
        return;
      }

      // Calcular estatísticas totais
      this.estatisticasTotais.total = dados.length;
      this.estatisticasTotais.abertas = dados.filter(item => {
        const status = this.obterStatusContestacao(item);
        return status === 'Aberta' || status === 'aberta' || status === 'ABERTA';
      }).length;
      
      this.estatisticasTotais.emAnalise = dados.filter(item => {
        const status = this.obterStatusContestacao(item);
        return status === 'Em Análise' || status === 'em análise' || status === 'EM_ANALISE';
      }).length;
      
      this.estatisticasTotais.resolvidas = dados.filter(item => {
        const status = this.obterStatusContestacao(item);
        return status === 'Resolvida' || status === 'resolvida' || status === 'RESOLVIDA';
      }).length;
      
      this.estatisticasTotais.canceladas = dados.filter(item => {
        const status = this.obterStatusContestacao(item);
        return status === 'Cancelada' || status === 'cancelada' || status === 'CANCELADA';
      }).length;
      
      this.estatisticasTotais.negadas = dados.filter(item => {
        const status = this.obterStatusContestacao(item);
        return status === 'Negada' || status === 'negada' || status === 'NEGADA';
      }).length;
      
      this.estatisticasTotais.pendentesColaborador = dados.filter(item => {
        const status = this.obterStatusContestacao(item);
        return status === 'Pendente Colaborador' || status === 'pendente colaborador' || status === 'PENDENTE_COLABORADOR';
      }).length;
    } catch (error) {
      console.error('[CONTESTACOES] Erro ao calcular estatísticas:', error);
      this.estatisticasTotais = {
        total: 0,
        abertas: 0,
        emAnalise: 0,
        resolvidas: 0,
        canceladas: 0,
        negadas: 0,
        pendentesColaborador: 0,
        resolvidasHoje: 0,
        pendentesUrgentes: 0
      };
    }
  }

  // Método para limpar filtro e mostrar todos os registros do tipo ativo
  limparFiltroCard(): void {
    this.filtrarPorStatusCard('Total');
  }

  // Método para limpar busca
  limparBusca(): void {
    this.consulta.setValue('');
    this.listar(null); // Recarregar com filtro padrão
  }

  // Método para mostrar todos os registros do tipo ativo
  mostrarTodasContestacoes(): void {
    this.filtrarPorStatusCard('Total');
  }

  // Método para verificar se há filtro ativo
  getFiltroAtivo(): string {
    if (!this.statusFiltro || this.statusFiltro === '') {
      const tipoTexto = this.tipoFiltro === 'auto_inventario' ? 'Auto Inventários' : 
                       this.tipoFiltro === 'inventario_forcado' ? 'Inventários Forçados' : 'Contestações';
      return `Todos os ${tipoTexto}`;
    }
    return this.statusFiltro;
  }

  // Método para verificar se há filtro aplicado
  temFiltroAtivo(): boolean {
    return this.statusFiltro && this.statusFiltro !== '';
  }

  // Ações da contestação
  editarContestacao(contestacao: any): void {
    const id = this.getContestacaoId(contestacao);
    this.route.navigate(['/movimentacoes/contestacoes/contestacao', id], {
      queryParams: { tipo: this.tipoFiltro }
    });
  }

  resolverContestacao(contestacao: any): void {
    const id = this.getContestacaoId(contestacao);
    const tipo = this.tipoFiltro === 'auto_inventario' ? 'solicitação de auto inventário' : 
                this.tipoFiltro === 'inventario_forcado' ? 'inventário forçado' : 'contestação';
    
    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja marcar esta ${tipo} como resolvida?<br><br>` +
      `📋 <strong>ID:</strong> #${id}<br>` +
      `📊 <strong>Status atual:</strong> ${this.getContestacaoStatus(contestacao)}<br>` +
      `👤 <strong>Colaborador:</strong> ${this.getContestacaoColaborador(contestacao)}<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação irá finalizar o atendimento.`,
      true
    ).then(aceita => {
      if (aceita) {
        this.util.aguardar(true);
        const payload = {
          status: ContestacaoStatus.RESOLVIDA,
          observacaoResolucao: 'Resolvido pela equipe técnica',
          usuarioResolucao: this.session?.usuario?.id || null
        };
        this.api.atualizarContestacao(id, payload, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res && res.status === 200) {
            this.util.exibirMensagemToast(`${tipo.charAt(0).toUpperCase() + tipo.slice(1)} resolvida com sucesso!`, 5000);
            this.listar(null); // Recarregar dados
          } else {
            console.error('[CONTESTACOES] Erro na resposta:', res);
            this.util.exibirMensagemToast('Erro ao resolver: ' + (res?.data?.error || 'Erro desconhecido'), 5000);
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('[CONTESTACOES] Erro ao resolver:', err);
          console.error('[CONTESTACOES] Detalhes do erro:', err.response?.data);
          this.util.exibirMensagemToast('Erro ao resolver contestação: ' + (err.response?.data?.error || err.message), 5000);
        });
      }
    });
  }

  negarContestacao(contestacao: any): void {
    const id = this.getContestacaoId(contestacao);
    const tipo = this.tipoFiltro === 'auto_inventario' ? 'solicitação de auto inventário' : 
                this.tipoFiltro === 'inventario_forcado' ? 'inventário forçado' : 'contestação';
    
    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja negar esta ${tipo}?<br><br>` +
      `📋 <strong>ID:</strong> #${id}<br>` +
      `📊 <strong>Status atual:</strong> ${this.getContestacaoStatus(contestacao)}<br>` +
      `👤 <strong>Colaborador:</strong> ${this.getContestacaoColaborador(contestacao)}<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação marcará a contestação como negada pela equipe técnica e não pode ser desfeita.`,
      true
    ).then(aceita => {
      if (aceita) {
        this.util.aguardar(true);
        const payload = {
          status: ContestacaoStatus.NEGADA,
          observacaoResolucao: 'Negado pela equipe técnica',
          usuarioResolucao: this.session?.usuario?.id || null
        };
        this.api.atualizarContestacao(id, payload, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res && res.status === 200) {
            this.util.exibirMensagemToast(`${tipo.charAt(0).toUpperCase() + tipo.slice(1)} negada com sucesso!`, 5000);
            this.listar(null); // Recarregar dados
          } else {
            console.error('[CONTESTACOES] Erro na resposta:', res);
            this.util.exibirMensagemToast('Erro ao negar: ' + (res?.data?.error || 'Erro desconhecido'), 5000);
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('[CONTESTACOES] Erro ao negar:', err);
          console.error('[CONTESTACOES] Detalhes do erro:', err.response?.data);
          this.util.exibirMensagemToast('Erro ao negar contestação: ' + (err.response?.data?.error || err.message), 5000);
        });
      }
    });
  }
}
