import { Component, OnInit, ViewChild, ChangeDetectorRef, ViewEncapsulation } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { DesligadosModalComponent } from './modals/desligados-modal/desligados-modal.component';
import { DevolucoesProgramadasModalComponent } from './modals/devolucoes-programadas-modal/devolucoes-programadas-modal.component';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { CampanhaApiService } from 'src/app/api/campanhas/campanha-api.service';
import { UtilService } from 'src/app/util/util.service';
import { DatePipe } from '@angular/common';
import { DashboardData, KPIPrincipal, MetricasSinalizacoes, MetricasAuditoria, NotificacaoDashboard } from 'src/app/models/dashboard.model';
import { Router } from '@angular/router';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  encapsulation: ViewEncapsulation.None, // Desabilita encapsulamento para garantir aplicação dos estilos
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease-in', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease-out', style({ opacity: 0 }))
      ])
    ])
  ]
})
export class DashboardComponent implements OnInit {

  private session:any = {};
  public vm: Partial<DashboardData> = {};
  public Math = Math; // Para usar Math.round no template
  public colunasEqpColDesligados = ['colaborador', 'dataDesligamento', 'acoes'];
  public colunasDevolucoes = ['equipamento', 'colaborador', 'dataDevolucao', 'status', 'acoes'];
  public colunasDevolucaoProgramada = ['colaborador', 'dtprogramacao'];
  public colunasEquipamentosStatus = ['tipo', 'requisitado', 'devolvido', 'novo', 'emestoque', 'entregue', 'danificado', 'extraviado', 'semconserto', 'roubado', 'descartado'];
  
  // 🆕 NOVAS PROPRIEDADES
  public ultimaAtualizacao: Date = new Date();
  public isRefreshing: boolean = false;
  public notificacoesNaoLidas: number = 0;
  public showNotificationsPanel: boolean = false;
  public alertaStatusTransitoriosExpanded: boolean = false; // Alerta recolhido por padrão
  
  // 🎛️ CONFIGURAÇÕES DE VISIBILIDADE
  public visibilitySettings = {
    acoesPendentes: true,
    colaboradoresDesligados: true,
    devolucoesProgramadas: true,
    estatisticasGerais: true,
    graficosMetricas: true
  };

  // 📋 CONTROLE DE EXPANSÃO DAS SEÇÕES
  public expandedSections: {
    acoesRapidas: boolean;           // 🎯 SEÇÃO PRINCIPAL 1: Ações Rápidas
    outrasInformacoes: boolean;      // 📊 SEÇÃO PRINCIPAL 2: Outras Informações
    kpis: boolean;
    acoesPendentes: boolean;
    colaboradoresDesligados: boolean;
    devolucoesProgramadas: boolean;
    estatisticas: boolean;
    graficos: boolean;
  } = {
    acoesRapidas: true,              // 🎯 Ações Rápidas: aberto por padrão
    outrasInformacoes: false,        // 📊 Outras Informações: recolhido por padrão
    kpis: true,
    acoesPendentes: true,
    colaboradoresDesligados: false,
    devolucoesProgramadas: false,
    estatisticas: true,
    graficos: false
  };
  
  @ViewChild('paginatorEqpColDesligado', { static: true }) paginatorEqpColDesligado: MatPaginator;
  @ViewChild('paginatorDevolucaoProgramada', { static: true }) paginatorDevolucaoProgramada: MatPaginator;
  @ViewChild('paginatorEquipamentosStatus', { static: true }) paginatorEquipamentosStatus: MatPaginator;
  @ViewChild('paginatorMovimentacoesUsuarios', { static: true }) paginatorMovimentacoesUsuario: MatPaginator;
  public dataSourceEqpColDesligado: MatTableDataSource<any>;
  public dataSourceDevolucaoProgramada: MatTableDataSource<any>;
  public dataSourceEquipamentosStatus: MatTableDataSource<any>;
  public dataSourceMovimentacoesUsuario: MatTableDataSource<any>;
  public chart;

  // 📧 Estatísticas do Hangfire
  public hangfireStats: any = null;

  /**
   * Retorna URL do dashboard Hangfire baseada na URL da API
   */
  get hangfireDashboardUrl(): string {
    // Remove '/api' da URL base e adiciona '/hangfire'
    return environment.apiUrl.replace('/api', '/hangfire');
  }

  /**
   * Abre o dashboard do Hangfire em nova aba
   */
  abrirHangfire(event?: MouseEvent): void {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    
    try {
      // Construir URL completa para evitar interceptação do Angular Router
      let url = this.hangfireDashboardUrl;
      
      // Se a URL começa com '/', construir URL absoluta
      if (url.startsWith('/')) {
        url = window.location.origin + url;
      }
      
      // Garantir que a URL seja absoluta (com protocolo)
      if (!url.startsWith('http://') && !url.startsWith('https://')) {
        url = window.location.origin + (url.startsWith('/') ? url : '/' + url);
      }
      
      console.log('[DASHBOARD] Abrindo Hangfire:', url);
      
      // Abrir em nova aba usando window.open com URL absoluta
      const newWindow = window.open(url, '_blank', 'noopener,noreferrer');
      
      if (!newWindow) {
        console.error('[DASHBOARD] Falha ao abrir janela (popup bloqueado?)');
        // Fallback: tentar navegar na mesma janela
        window.location.href = url;
      }
    } catch (error) {
      console.error('[DASHBOARD] Erro ao abrir Hangfire:', error);
    }
  }

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService,
    private usuarioApi: UsuarioApiService,
    private campanhaApi: CampanhaApiService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.loadVisibilitySettings(); // Carregar configurações salvas ao iniciar
    this.loadExpandedSections(); // Carregar estado de expansão das seções
    
    // Debug: verificar configurações carregadas
    this.listar();
  }

  listar() {
    this.util.aguardar(true);
    this.isRefreshing = true;
    
    // Buscar dados do dashboard
    this.api.dashboardWeb(this.session.usuario.cliente, this.session.token).then(res => {
      this.vm = (res && res.data) ? res.data : {} as any;
      this.ultimaAtualizacao = this.vm.ultimaAtualizacao ? new Date(this.vm.ultimaAtualizacao) : new Date();
      
      // Calcular notificações não lidas
      this.notificacoesNaoLidas = this.vm.notificacoes?.filter(n => !n.lida).length || 0;

      // Buscar usuários para contagem de administradores
      this.buscarUsuariosParaContagem();

      // 📧 Buscar estatísticas do Hangfire
      this.carregarEstatisticasHangfire();

      this.dataSourceEqpColDesligado = new MatTableDataSource<any>(this.vm.equipamentosComColaboradorDesligado || []);
      this.dataSourceEqpColDesligado.paginator = this.paginatorEqpColDesligado;

      this.dataSourceDevolucaoProgramada = new MatTableDataSource<any>(this.vm.devolucoesProgramadas || []);
      this.dataSourceDevolucaoProgramada.paginator = this.paginatorDevolucaoProgramada;

      this.dataSourceEquipamentosStatus = new MatTableDataSource<any>(this.vm.equipamentosPorStatus || []);
      this.dataSourceEquipamentosStatus.paginator = this.paginatorEquipamentosStatus;

      // Converter objeto para array para MatTableDataSource
      const movimentacoesArray = this.vm.ultimosUsuariosQueMovimentaram 
        ? Object.entries(this.vm.ultimosUsuariosQueMovimentaram).map(([usuario, quantidade]) => ({ 
            usuario: this.formatarNomeUsuario(usuario), 
            quantidade 
          }))
        : [];
      this.dataSourceMovimentacoesUsuario = new MatTableDataSource<any>(movimentacoesArray);
      this.dataSourceMovimentacoesUsuario.paginator = this.paginatorMovimentacoesUsuario;

      this.chart = (<any>window).google;
      this.chart.charts.load('current', {'packages':['corechart']});
      setTimeout(() => {
        this.chart.charts.setOnLoadCallback(this.desenharAdesaoTermoResponsabilidade.bind(this));
        this.chart.charts.setOnLoadCallback(this.desenharMovimentacoesUsuarios5Dias.bind(this));
      }, 1000)
      
      this.util.aguardar(false);
      this.isRefreshing = false;
    }).catch(error => {
      this.util.aguardar(false);
      this.isRefreshing = false;
      console.error('[DASHBOARD] Erro ao carregar dashboard:', error);
    });
  }

  // 🔄 Refresh manual do dashboard
  refreshDashboard(): void {
    this.listar();
  }

  // Buscar usuários para contagem de administradores
  private buscarUsuariosParaContagem() {
    try {
      this.usuarioApi.listarUsuarios(null, this.session.usuario.cliente, this.session.token).then(res => {
        if (res.status === 200 && res.data) {
          this.vm.usuarios = res.data;
          const totalAdmins = this.getTotalAdministradores();
          this.cdr.detectChanges();
        }
      }).catch(error => {
        console.error('[DASHBOARD] Erro ao buscar usuários:', error);
      });
      
    } catch (error) {
      console.error('[DASHBOARD] Erro ao buscar usuários para contagem:', error);
    }
  }

  // 📧 Carregar estatísticas do Hangfire
  private carregarEstatisticasHangfire() {
    try {
      this.campanhaApi.obterEstatisticasHangfire(this.session.token).then(res => {
        if (res.status === 200 && res.data) {
          this.hangfireStats = res.data;
          this.cdr.detectChanges();
        } else {
          console.warn('[DASHBOARD] Hangfire não disponível ou sem dados');
          this.hangfireStats = null;
        }
      }).catch(error => {
        console.error('[DASHBOARD] Erro ao buscar estatísticas Hangfire:', error);
        this.hangfireStats = null;
      });
      
    } catch (error) {
      console.error('[DASHBOARD] Erro ao carregar estatísticas Hangfire:', error);
      this.hangfireStats = null;
    }
  }

  desenharAdesaoTermoResponsabilidade() {
    try {
      // Verificar se o elemento existe
      const element = document.getElementById('totaladesao_div');
      if (!element) {
        console.warn('[GRÁFICO] Elemento totaladesao_div não encontrado');
        return;
      }

      // Verificar se os dados existem
      if (!this.vm?.adesaoTermoResponsabilidade) {
        console.warn('[GRÁFICO] Dados de adesão não encontrados');
        return;
      }

      const data = new this.chart.visualization.DataTable();
      data.addColumn('string', 'Status');
      data.addColumn('number', 'Quantidade');
      data.addColumn({type: 'string', role: 'tooltip'});

      const assinados = this.vm.adesaoTermoResponsabilidade.assinados || 0;
      const naoAssinados = this.vm.adesaoTermoResponsabilidade.naoAssinados || 0;
      const total = assinados + naoAssinados;

      // Adicionar dados com tooltips informativos
      data.addRow([
        'Assinados', 
        assinados, 
        `Assinados: ${assinados} (${total > 0 ? Math.round((assinados/total)*100) : 0}%)`
      ]);
      
      data.addRow([
        'Não Assinados', 
        naoAssinados, 
        `Não Assinados: ${naoAssinados} (${total > 0 ? Math.round((naoAssinados/total)*100) : 0}%)`
      ]);

      // Configurações melhoradas do gráfico
      const options = {
        title: {
          text: 'Adesão ao Termo de Responsabilidade',
          fontSize: 16,
          bold: true,
          color: '#080039'
        },
        legend: { 
          position: 'bottom', 
          alignment: 'center',
          textStyle: {
            fontSize: 12,
            color: '#333'
          }
        },
        pieHole: 0.4, // Donut chart mais elegante
        colors: ['#28a745', '#dc3545'], // Verde para assinados, vermelho para não assinados
        backgroundColor: 'transparent',
        chartArea: {
          width: '90%',
          height: '80%'
        },
        pieSliceText: 'percentage', // Mostrar porcentagem no slice
        pieSliceTextStyle: {
          color: 'white',
          fontSize: 14,
          bold: true
        },
        tooltip: {
          trigger: 'focus',
          textStyle: {
            fontSize: 12
          }
        },
        animation: {
          startup: true,
          duration: 1000,
          easing: 'out'
        },
        enableInteractivity: true
      };

      // Criar e desenhar o gráfico
      const grafico = new this.chart.visualization.PieChart(element);
      grafico.draw(data, options);
    } catch (error) {
      console.error('[GRÁFICO] Erro ao desenhar gráfico de adesão:', error);
    }
  }

  desenharMovimentacoesUsuarios5Dias() {
    var data = new this.chart.visualization.DataTable();
    try {
      var data = new this.chart.visualization.DataTable();
    }
    catch {
      var data = new this.chart.visualization.DataTable();
    }

    data.addColumn('string', 'Usuário');
    data.addColumn('number', 'Quantidade');
    data.addColumn({type: 'string', role: 'annotation'});

    // Verificar se ultimosUsuariosQueMovimentaram existe e não é null
    if (this.vm.ultimosUsuariosQueMovimentaram && typeof this.vm.ultimosUsuariosQueMovimentaram === 'object') {
      for(const [key, value] of Object.entries(this.vm.ultimosUsuariosQueMovimentaram)) {
        // ✅ CORREÇÃO: Formatar nome com primeira letra maiúscula
        const nomeFormatado = this.formatarNomeUsuario(key);
        data.addRow([{v: nomeFormatado}, value, value.toString()]);
      }
    }

var grafico:any = {};
    grafico = new this.chart.visualization.ColumnChart(document.getElementById('grpMovimentacoesUsuarios'));
    grafico.draw(data, {
      'colors': ['#FF3A0F', "#080039", '#2dd36f'],
      annotations: {
        alwaysOutside: true,
        textStyle: {
          fontSize: 14,
          color: '#000',
          auraColor: 'none'
        }
      },
    });
  }

  dataMenorQueHoje(data: Date): boolean {
    return new Date(data) < new Date();
  }

  // 🆘 MÉTODOS PARA AÇÕES PENDENTES
  getTotalRecursosDesligados(): number {
    try {
      const lista = (this.dataSourceEqpColDesligado?.data as any[]) || (this.vm as any)?.equipamentosComColaboradorDesligado || [];
      if (!lista || lista.length === 0) return 0;
      let total = 0;
      for (const item of lista) {
        // Backend pode enviar como 'qtde' (camelCase) ou 'Qtde' (PascalCase)
        const valor: any = (item && (item.qtde ?? item.Qtde)) ?? 0;
        const n = typeof valor === 'number' ? valor : parseInt(valor, 10) || 0;
        total += n;
      }
      return total;
    } catch (e) {
      console.warn('[DASHBOARD] Falha ao somar recursos de desligados:', e);
      return 0;
    }
  }

  getDevolucoesVencidas(): number {
    if (!this.dataSourceDevolucaoProgramada?.data) return 0;
    return this.dataSourceDevolucaoProgramada.data.filter(item => 
      this.isDataProxima(item.dtprogramadaretorno)
    ).length;
  }

  getDevolucoesProximas(): number {
    if (!this.dataSourceDevolucaoProgramada?.data) return 0;
    return this.dataSourceDevolucaoProgramada.data.filter(item => 
      !this.isDataProxima(item.dtprogramadaretorno)
    ).length;
  }

  isDataProxima(data: Date | string): boolean {
    if (!data) return false;
    
    const dataObj = new Date(data);
    const hoje = new Date();
    const diffTime = dataObj.getTime() - hoje.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    // Considera "próxima" se estiver entre hoje e 7 dias
    return diffDays >= 0 && diffDays <= 7;
  }

  // 📊 MÉTODOS PARA MÉTRICAS E COMPARAÇÕES (DADOS REAIS)
  getComparisonClass(metricType: string): string {
    const today = this.vm?.qtdeAtivosMovimentadoDia || 0;
    const yesterday = this.vm?.qtdeAtivosMovimentadoDiaAnterior || 0;
    
    if (today > yesterday) return 'comparison-up';
    if (today < yesterday) return 'comparison-down';
    return 'comparison-same';
  }

  getComparisonIcon(metricType: string): string {
    const today = this.vm?.qtdeAtivosMovimentadoDia || 0;
    const yesterday = this.vm?.qtdeAtivosMovimentadoDiaAnterior || 0;
    
    if (today > yesterday) return 'trending_up';
    if (today < yesterday) return 'trending_down';
    return 'trending_flat';
  }

  getComparisonText(metricType: string): string {
    const today = this.vm?.qtdeAtivosMovimentadoDia || 0;
    const yesterday = this.vm?.qtdeAtivosMovimentadoDiaAnterior || 0;
    
    if (yesterday === 0) {
      return today > 0 ? '+100%' : '0%';
    }
    
    if (today > yesterday) {
      const increase = Math.round(((today - yesterday) / yesterday) * 100);
      return `+${increase}%`;
    }
    if (today < yesterday) {
      const decrease = Math.round(((yesterday - today) / yesterday) * 100);
      return `-${decrease}%`;
    }
    return '0%';
  }

  // 📊 MÉTODOS PARA ESTATÍSTICAS DE MOVIMENTAÇÕES
  getTotalMovimentacoes(): number {
    try {
      if (!this.vm?.ultimosUsuariosQueMovimentaram) return 0;
      
      const valores = Object.values(this.vm.ultimosUsuariosQueMovimentaram);
      let total = 0;
      
      for (const valor of valores) {
        if (typeof valor === 'number') {
          total += valor;
        } else if (typeof valor === 'string') {
          total += parseInt(valor) || 0;
        } else if (valor !== null && valor !== undefined) {
          total += Number(valor) || 0;
        }
      }
      
      return total;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao calcular total de movimentações:', error);
      return 0;
    }
  }

  getUsuarioMaisAtivo(): string {
    try {
      if (!this.vm?.ultimosUsuariosQueMovimentaram) return 'N/A';
      
      const usuarios = Object.entries(this.vm.ultimosUsuariosQueMovimentaram);
      if (usuarios.length === 0) return 'N/A';
      
      let usuarioMaisAtivo = usuarios[0];
      let maxValue = 0;
      
      for (const [nome, valor] of usuarios) {
        let currentValue = 0;
        
        if (typeof valor === 'number') {
          currentValue = valor;
        } else if (typeof valor === 'string') {
          currentValue = parseInt(valor) || 0;
        } else if (valor !== null && valor !== undefined) {
          currentValue = Number(valor) || 0;
        }
        
        if (currentValue > maxValue) {
          maxValue = currentValue;
          usuarioMaisAtivo = [nome, valor];
        }
      }
      
      // ✅ CORREÇÃO: Formatar nome com primeira letra maiúscula
      const nomeFormatado = this.formatarNomeUsuario(usuarioMaisAtivo[0] as string);
      return nomeFormatado;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao calcular usuário mais ativo:', error);
      return 'Erro';
    }
  }

  // ✅ CORREÇÃO: Função para formatar nome com primeira letra maiúscula
  private formatarNomeUsuario(nome: string): string {
    if (!nome || nome.trim() === '') return nome;
    
    return nome
      .toLowerCase()
      .split(' ')
      .map(palavra => palavra.charAt(0).toUpperCase() + palavra.slice(1))
      .join(' ');
  }

  // 📊 MÉTODOS PARA RESUMO GERAL
  getTotalAdministradores(): number {
    try {
      // Se já temos os dados de usuários carregados, usar eles
      if (this.vm?.usuarios && Array.isArray(this.vm.usuarios)) {
        const administradores = this.vm.usuarios.filter(usuario => 
          usuario.Adm === true || usuario.adm === true || 
          usuario.Administrador === true || usuario.administrador === true ||
          usuario.tipo === 'A'
        );
        return administradores.length;
      }
      
      // Fallback: se não temos dados, retornar 0
      return 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao calcular total de administradores:', error);
      return 0;
    }
  }

  getUsuariosMovimentaramHoje(): number {
    try {
      return this.getUsuariosAtivosCount();
    } catch (error) {
      console.error('[DASHBOARD] Erro ao contar usuários que movimentaram hoje:', error);
      return 0;
    }
  }

  getUsuariosAtivosCount(): number {
    try {
      if (!this.vm?.ultimosUsuariosQueMovimentaram) {
        return 0;
      }
      return Object.keys(this.vm.ultimosUsuariosQueMovimentaram).length;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao contar usuários ativos:', error);
      return 0;
    }
  }

  getTaxaAdesao(): number {
    if (!this.vm?.adesaoTermoResponsabilidade) return 0;
    
    const assinados = this.vm.adesaoTermoResponsabilidade.assinados || 0;
    const naoAssinados = this.vm.adesaoTermoResponsabilidade.naoAssinados || 0;
    const total = assinados + naoAssinados;
    
    if (total === 0) return 0;
    return Math.round((assinados / total) * 100);
  }

  getStatusSistema(): string {
    try {
      const movimentacoes = this.vm?.qtdeAtivosMovimentadoDia || 0;
      const usuariosAtivos = this.getUsuariosAtivosCount();
      const admins = this.getTotalAdministradores();
      
      // Considerar também o número de administradores no status
      if (movimentacoes > 20 && usuariosAtivos > 5 && admins <= 5) return 'Excelente';
      if (movimentacoes > 10 && usuariosAtivos > 3 && admins <= 5) return 'Bom';
      if (movimentacoes > 5 && usuariosAtivos > 1 && admins <= 5) return 'Normal';
      if (admins > 5) return '⚠️ Crítico';
      if (movimentacoes > 0) return 'Baixo';
      return 'Inativo';
    } catch (error) {
      console.error('[DASHBOARD] Erro ao calcular status do sistema:', error);
      return 'Erro';
    }
  }

// 🎛️ MÉTODOS DE CONTROLE DE VISIBILIDADE
  
  // Atualizar visibilidade quando checkboxes mudam
  updateVisibility(): void {
    // Salvar configurações no localStorage
    localStorage.setItem('dashboardVisibilitySettings', JSON.stringify(this.visibilitySettings));
    
    // Forçar atualização da view
    setTimeout(() => {
      // Recalcular gráficos se necessário
      if (this.visibilitySettings.graficosMetricas && this.chart) {
        this.desenharAdesaoTermoResponsabilidade();
        this.desenharMovimentacoesUsuarios5Dias();
      }
    }, 100);
  }

  // 📋 MÉTODOS DE CONTROLE DE EXPANSÃO
  toggleSection(section: keyof DashboardComponent['expandedSections']): void {
    if (this.expandedSections.hasOwnProperty(section)) {
      this.expandedSections[section] = !this.expandedSections[section];
      
      // Salvar estado de expansão no localStorage
      this.saveExpandedSections();
    } else {
      console.warn(`[TOGGLE] Seção ${section} não encontrada nas configurações`);
    }
  }

  // Mostrar todas as seções
  showAll(): void {
    this.visibilitySettings = {
      acoesPendentes: true,
      colaboradoresDesligados: true,
      devolucoesProgramadas: true,
      estatisticasGerais: true,
      graficosMetricas: true
    };
    this.updateVisibility();
  }

  // Ocultar todas as seções
  hideAll(): void {
    this.visibilitySettings = {
      acoesPendentes: false,
      colaboradoresDesligados: false,
      devolucoesProgramadas: false,
      estatisticasGerais: false,
      graficosMetricas: false
    };
    this.updateVisibility();
  }

  // Mostrar apenas seções urgentes
  showOnlyUrgent(): void {
    this.visibilitySettings = {
      acoesPendentes: true,
      colaboradoresDesligados: true,
      devolucoesProgramadas: true,
      estatisticasGerais: false,
      graficosMetricas: false
    };
    this.updateVisibility();
  }

  // Mostrar apenas estatísticas
  showOnlyStats(): void {
    this.visibilitySettings = {
      acoesPendentes: false,
      colaboradoresDesligados: false,
      devolucoesProgramadas: false,
      estatisticasGerais: true,
      graficosMetricas: true
    };
    this.updateVisibility();
  }

  // Carregar configurações salvas
  loadVisibilitySettings(): void {
    const saved = localStorage.getItem('dashboardVisibilitySettings');
    if (saved) {
      try {
        const loadedSettings = JSON.parse(saved);
        // Garantir que pelo menos uma seção esteja visível
        this.visibilitySettings = { 
          ...this.visibilitySettings, 
          ...loadedSettings 
        };
        
        // Se todas estiverem false, mostrar pelo menos as ações pendentes
        if (!Object.values(this.visibilitySettings).some(v => v === true)) {
          this.visibilitySettings.acoesPendentes = true;
          this.visibilitySettings.colaboradoresDesligados = true;
          this.visibilitySettings.devolucoesProgramadas = true;
        }
      } catch (e) {
        console.warn('Erro ao carregar configurações de visibilidade:', e);
        // Em caso de erro, usar configurações padrão
        this.visibilitySettings = {
          acoesPendentes: true,
          colaboradoresDesligados: true,
          devolucoesProgramadas: true,
          estatisticasGerais: true,
          graficosMetricas: true
        };
      }
    }
    
    // VERIFICAÇÃO DE SEGURANÇA: Garantir que sempre haja conteúdo visível
    if (!Object.values(this.visibilitySettings).some(v => v === true)) {
      console.warn('[SEGURANÇA] Todas as seções estavam ocultas, forçando visibilidade das ações pendentes');
      this.visibilitySettings.acoesPendentes = true;
      this.visibilitySettings.colaboradoresDesligados = true;
      this.visibilitySettings.devolucoesProgramadas = true;
    }
  }

  // Carregar estado de expansão das seções
  loadExpandedSections(): void {
    const saved = localStorage.getItem('dashboardExpandedSections');
    if (saved) {
      try {
        const loadedSections = JSON.parse(saved);
        this.expandedSections = { 
          ...this.expandedSections, 
          ...loadedSections 
        };
      } catch (e) {
        console.warn('Erro ao carregar estado de expansão:', e);
      }
    }
  }

  // Salvar estado de expansão das seções
  saveExpandedSections(): void {
    try {
      localStorage.setItem('dashboardExpandedSections', JSON.stringify(this.expandedSections));
    } catch (e) {
      console.warn('Erro ao salvar estado de expansão:', e);
    }
  }

  // Resetar configurações para padrão
  resetVisibilitySettings(): void {
    localStorage.removeItem('dashboardVisibilitySettings');
    localStorage.removeItem('dashboardExpandedSections');
    
    this.visibilitySettings = {
      acoesPendentes: true,
      colaboradoresDesligados: true,
      devolucoesProgramadas: true,
      estatisticasGerais: true,
      graficosMetricas: true
    };
    
    this.expandedSections = {
      acoesRapidas: true,
      outrasInformacoes: false,
      kpis: true,
      acoesPendentes: true,
      colaboradoresDesligados: false,
      devolucoesProgramadas: false,
      estatisticas: true,
      graficos: false
    };
    
    this.updateVisibility();
  }

  // 📊 MÉTODOS PARA ESTATÍSTICAS GERAIS
  getTotalRecursos(): number {
    try {
      if (!this.dataSourceEquipamentosStatus?.data) return 0;
      
      let total = 0;
      for (const row of this.dataSourceEquipamentosStatus.data) {
        total += (row.requisitado || 0) + (row.devolvido || 0) + (row.danificado || 0) + 
                 (row.emestoque || 0) + (row.entregue || 0) + (row.extraviado || 0) + 
                 (row.novo || 0) + (row.roubado || 0) + (row.semconserto || 0) + (row.descartado || 0);
      }
      
      return total;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao calcular total de recursos:', error);
      return 0;
    }
  }

  getContestacoesPendentes(): number {
    try {
      return this.vm?.qtdeContestacoesPendentes || 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter contestações pendentes:', error);
      return 0;
    }
  }

  // Lista resumida de ações pendentes de contestação/auto inventário
  getAcoesPendentesContestacoes(): any[] {
    try {
      return this.vm?.contestacoesPendentesLista || [];
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter lista de contestações pendentes:', error);
      return [];
    }
  }

  // Abrir modal com agregação de desligados (nome, matricula, dtdemissao, qtde)
  abrirModalDesligados(): void {
    try {
      const origem = (this.vm as any)?.equipamentosComColaboradorDesligado || [];

      // Agregar por colaborador (usando colaboradorId como chave única)
      const mapa = new Map<number, { colaboradorId: number; nome: string; matricula: string; dtdemissao?: Date; qtde: number }>();
      for (const item of origem) {
        const colaboradorId = item?.ColaboradorId || item?.colaboradorId || 0;
        const nome = item?.Nome || item?.nome || 'Desconhecido';
        const matricula = item?.Matricula || item?.matricula || '';
        const dtdemissao = item?.Dtdemissao || item?.dtdemissao;
        
        const atual = mapa.get(colaboradorId) || { 
          colaboradorId, 
          nome, 
          matricula,
          dtdemissao, 
          qtde: 0 
        };
        
        // Backend já manda Qtde=1 por recurso; somar
        const inc = (typeof item?.Qtde === 'number' ? item.Qtde : (typeof item?.qtde === 'number' ? item.qtde : 1)) || 1;
        atual.qtde += inc;
        mapa.set(colaboradorId, atual);
      }

      const itens = Array.from(mapa.values()).sort((a, b) => {
        const da = a.dtdemissao ? new Date(a.dtdemissao as any).getTime() : 0;
        const db = b.dtdemissao ? new Date(b.dtdemissao as any).getTime() : 0;
        return da - db;
      });

      this.dialog.open(DesligadosModalComponent, {
        width: '800px',
        maxHeight: '80vh',
        data: { itens }
      });
    } catch (error) {
      console.error('[DASHBOARD] Erro ao abrir modal de desligados:', error);
    }
  }

  isAutoInventarioItem(item: any): boolean {
    return (item?.tipoContestacao || '').toLowerCase() === 'auto_inventario';
  }

  getTipoContestacaoLabel(item: any): string {
    const tipo = (item?.tipoContestacao || '').toLowerCase();
    if (tipo === 'auto_inventario') return 'Auto Inventário';
    return 'Contestação';
  }

  getRecursosAtivos(): number {
    try {
      if (!this.dataSourceEquipamentosStatus?.data) return 0;
      
      let ativos = 0;
      for (const row of this.dataSourceEquipamentosStatus.data) {
        ativos += (row.emestoque || 0) + (row.entregue || 0) + (row.novo || 0);
      }
      
      return ativos;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao calcular recursos ativos:', error);
      return 0;
    }
  }

  getRecursosProblema(): number {
    try {
      if (!this.dataSourceEquipamentosStatus?.data) return 0;
      
      let problemas = 0;
      for (const row of this.dataSourceEquipamentosStatus.data) {
        problemas += (row.requisitado || 0) + (row.devolvido || 0) + (row.danificado || 0) + 
                     (row.extraviado || 0) + (row.roubado || 0) + (row.semconserto || 0) + (row.descartado || 0);
      }
      
      return problemas;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao calcular recursos com problema:', error);
      return 0;
    }
  }

  // 🚨 MÉTODOS PARA CONSOLIDADO CRÍTICO (DADOS REAIS)
  getTotalRequisitados(): number {
    try {
      return this.vm?.metricasRequisicoes?.totalRequisitados || 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter total de requisitados:', error);
      return 0;
    }
  }

  getTotalDevolvidos(): number {
    try {
      return this.vm?.metricasDevolvidas?.totalDevolvidos || 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter total de devolvidos:', error);
      return 0;
    }
  }

  getRequisitadosUrgentes(): number {
    try {
      return this.vm?.metricasRequisicoes?.urgentes || 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter requisitados urgentes:', error);
      return 0;
    }
  }

  getRequisitadosPendentes(): number {
    try {
      return this.vm?.metricasRequisicoes?.pendentes || 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter requisitados pendentes:', error);
      return 0;
    }
  }

  getDevolvidosVencidos(): number {
    try {
      return this.vm?.metricasDevolvidas?.vencidos || 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter devolvidos vencidos:', error);
      return 0;
    }
  }

  getDevolvidosProximos(): number {
    try {
      return this.vm?.metricasDevolvidas?.proximos || 0;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao obter devolvidos próximos:', error);
      return 0;
    }
  }

  // ========== NOVOS MÉTODOS PARA DASHBOARD EXPANDIDO ==========

  // 🔔 Métodos para Notificações
  toggleNotificationsPanel(): void {
    this.showNotificationsPanel = !this.showNotificationsPanel;
  }

  marcarNotificacaoComoLida(notificacao: NotificacaoDashboard): void {
    if (this.vm.notificacoes) {
      const index = this.vm.notificacoes.findIndex(n => n.id === notificacao.id);
      if (index !== -1) {
        this.vm.notificacoes[index].lida = true;
        this.notificacoesNaoLidas = this.vm.notificacoes.filter(n => !n.lida).length;
      }
    }
  }

  navegarPara(link: string, notificacao?: NotificacaoDashboard): void {
    if (notificacao) {
      this.marcarNotificacaoComoLida(notificacao);
    }
    this.showNotificationsPanel = false;
    this.router.navigate([link]);
  }

  marcarTodasComoLidas(): void {
    if (this.vm.notificacoes) {
      this.vm.notificacoes.forEach(n => n.lida = true);
      this.notificacoesNaoLidas = 0;
    }
  }

  getNotificacaoIconClass(tipo: string): string {
    switch (tipo) {
      case 'critico': return 'notif-critico';
      case 'atencao': return 'notif-atencao';
      case 'info': return 'notif-info';
      default: return 'notif-info';
    }
  }

  // 📊 Métodos para KPIs
  getKPIIcon(tendencia: string): string {
    switch (tendencia) {
      case 'alta': return 'trending_up';
      case 'baixa': return 'trending_down';
      case 'estavel': return 'trending_flat';
      default: return 'trending_flat';
    }
  }

  getKPIClass(tendencia: string): string {
    switch (tendencia) {
      case 'alta': return 'kpi-up';
      case 'baixa': return 'kpi-down';
      case 'estavel': return 'kpi-stable';
      default: return 'kpi-stable';
    }
  }

  formatKPIVariacao(kpi: KPIPrincipal | undefined): string {
    if (!kpi) return '0';
    const sinal = kpi.variacao > 0 ? '+' : '';
    if (kpi.tipoVariacao === 'percentual') {
      return `${sinal}${kpi.variacao.toFixed(1)}%`;
    }
    return `${sinal}${kpi.variacao}`;
  }

  // ⏰ Formatar timestamp
  getTempoDecorrido(): string {
    if (!this.ultimaAtualizacao) return 'Há poucos instantes';
    
    const agora = new Date();
    const diffMs = agora.getTime() - this.ultimaAtualizacao.getTime();
    const diffMinutos = Math.floor(diffMs / 60000);
    
    if (diffMinutos < 1) return 'Agora';
    if (diffMinutos === 1) return 'Há 1 minuto';
    if (diffMinutos < 60) return `Há ${diffMinutos} minutos`;
    
    const diffHoras = Math.floor(diffMinutos / 60);
    if (diffHoras === 1) return 'Há 1 hora';
    if (diffHoras < 24) return `Há ${diffHoras} horas`;
    
    const diffDias = Math.floor(diffHoras / 24);
    if (diffDias === 1) return 'Há 1 dia';
    return `Há ${diffDias} dias`;
  }

  formatarDataHora(data: Date | undefined): string {
    if (!data) return 'N/A';
    const date = new Date(data);
    return date.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // 📋 Métodos para Sinalizações
  getSinalizacoesCriticasOuAltas(): number {
    if (!this.vm.sinalizacoes) return 0;
    return (this.vm.sinalizacoes.criticas || 0) + (this.vm.sinalizacoes.altas || 0);
  }

  getSinalizacoesPendentesTotal(): number {
    return this.vm.sinalizacoes?.pendentes || 0;
  }

  navegarParaSinalizacoes(): void {
    this.router.navigate(['/relatorios/sinalizacoes-suspeitas']);
  }

  // ✅ Verifica se há alguma ação pendente (para ocultar seção inteira quando vazia)
  temAcoesPendentes(): boolean {
    try {
      const temDesligados = (this.vm.totalDesligadosComRecursos || this.dataSourceEqpColDesligado?.data?.length || 0) > 0;
      const temDevolucoes = (this.dataSourceDevolucaoProgramada?.data?.length || 0) > 0;
      const temContestacoes = this.getContestacoesPendentes() > 0;
      const temSinalizacoes = this.getSinalizacoesPendentesTotal() > 0;
      
      return temDesligados || temDevolucoes || temContestacoes || temSinalizacoes;
    } catch (error) {
      console.error('[DASHBOARD] Erro ao verificar ações pendentes:', error);
      return false;
    }
  }

  // 🎯 Scroll suave para uma seção específica do dashboard
  scrollToSection(sectionId: string): void {
    try {
      // Mapear ID para a propriedade correta do expandedSections
      const sectionMap: { [key: string]: string } = {
        'colaboradores-desligados': 'colaboradoresDesligados',
        'devolucoes-programadas': 'devolucoesProgramadas',
        'card-adesao-campanhas': 'graficos',
        'card-movimentacoes-usuarios': 'graficos'
      };

      const sectionKey = sectionMap[sectionId] as keyof DashboardComponent['expandedSections'];
      
      // Expandir a seção se estiver recolhida
      if (sectionKey && this.expandedSections.hasOwnProperty(sectionKey) && !this.expandedSections[sectionKey]) {
        this.expandedSections[sectionKey] = true;
        this.saveExpandedSections();
      }

      // Aguardar a renderização e fazer scroll suave
      setTimeout(() => {
        const element = document.getElementById(sectionId);
        if (element) {
          element.scrollIntoView({ 
            behavior: 'smooth', 
            block: 'start',
            inline: 'nearest'
          });
          
          // Adicionar efeito visual temporário de destaque
          element.classList.add('highlight-section');
          setTimeout(() => {
            element.classList.remove('highlight-section');
          }, 2000);
        } else {
          console.warn(`[SCROLL] Seção '${sectionId}' não encontrada no DOM`);
        }
      }, 100);
    } catch (error) {
      console.error('[SCROLL] Erro ao navegar para seção:', error);
    }
  }

  // 📋 Abrir modal com devoluções programadas
  abrirModalDevolucoesProgramadas(): void {
    try {
      const itens = this.dataSourceDevolucaoProgramada?.data || this.vm?.devolucoesProgramadas || [];
      this.dialog.open(DevolucoesProgramadasModalComponent, {
        width: '900px',
        maxHeight: '85vh',
        data: { itens }
      });
    } catch (error) {
      console.error('[DASHBOARD] Erro ao abrir modal de devoluções programadas:', error);
    }
  }
}
