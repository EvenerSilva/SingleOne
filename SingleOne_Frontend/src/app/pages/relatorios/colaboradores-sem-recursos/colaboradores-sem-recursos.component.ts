import { Component, OnInit, ViewChild, OnDestroy, AfterViewInit } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { ContestacaoApiService } from 'src/app/api/contestacoes/contestacao-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-colaboradores-sem-recursos',
  templateUrl: './colaboradores-sem-recursos.component.html',
  styleUrls: ['./colaboradores-sem-recursos.component.scss']
})
export class ColaboradoresSemRecursosComponent implements OnInit, AfterViewInit, OnDestroy {

  private session: any = {};
  public filtros: any = {
    cargo: null, // string - campo de texto livre
    tipoColaborador: null, // string - 'F', 'T', 'E', etc
    empresa: null,
    localidade: null,
    centroCusto: null,
    nome: null
  };
  
  public colaboradores: any = [];
  public tiposColaborador: any[] = [];
  public empresas: any[] = [];
  public localidades: any[] = [];
  public centrosCusto: any[] = [];
  
  public colunas = ['nome', 'matricula', 'cargo', 'empresa', 'localidade', 'centroCusto', 'tipoColaborador', 'dataAdmissao', 'acoes'];
  
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
  public showForcarInventarioModal = false;
  public colaboradorSelecionado: any = null;
  public colaboradoresSelecionados: any[] = [];
  public ocultarDesligados = true; // 🚫 Por padrão, ocultar desligados
  public ocultarComInventarioPendente = true; // 📋 Por padrão, ocultar quem já tem inventário forçado pendente
  public colaboradoresComInventarioPendente: number[] = []; // IDs dos colaboradores com inventário pendente
  
  // 📧 Propriedades para envio de email
  public enviarEmailNotificacao = true; // ✅ Por padrão, marcado para enviar email
  public mensagemAdicional = ''; // 📝 Mensagem adicional opcional
  
  private destroy$ = new Subject<void>();

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService,
    private configApi: ConfiguracoesApiService,
    private contestacaoApi: ContestacaoApiService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.carregarFiltros();
  }

  carregarFiltros() {
    this.loading = true;
    
    // Nota: Cargo é campo de texto livre (string), não FK - não tem lista para carregar
    // Tipos de colaborador são caracteres fixos no código
    this.tiposColaborador = [
      { id: 'F', descricao: 'Funcionário' },
      { id: 'T', descricao: 'Terceiro' },
      { id: 'C', descricao: 'Consultor' }
    ];
    this.api.obterEmpresasComColaboradores(this.session.usuario.cliente, this.session.token).then(res => {
      if (res && res.data !== undefined) {
        this.empresas = Array.isArray(res.data) ? res.data : [];
      } else {
        console.error('[EMPRESAS] ❌ Resposta inválida:', res);
        this.empresas = [];
      }
    }).catch(error => {
      console.error('[EMPRESAS] ❌ Erro ao carregar:', error);
      this.empresas = [];
    });

    // Carregar localidades que possuem colaboradores
    this.api.obterLocalidadesComColaboradores(this.session.usuario.cliente, this.session.token).then(res => {
      if (res && res.data !== undefined) {
        this.localidades = Array.isArray(res.data) ? res.data : [];
      } else {
        console.error('[LOCALIDADES] ❌ Resposta inválida:', res);
        this.localidades = [];
      }
    }).catch(error => {
      console.error('[LOCALIDADES] ❌ Erro ao carregar:', error);
      this.localidades = [];
    });

    // Carregar centros de custo que possuem colaboradores
    this.api.obterCentrosCustoComColaboradores(this.session.usuario.cliente, this.session.token).then(res => {
      if (res && res.data !== undefined) {
        this.centrosCusto = Array.isArray(res.data) ? res.data : [];
      } else {
        console.error('[CENTRO CUSTO] ❌ Resposta inválida:', res);
        this.centrosCusto = [];
      }
    }).catch(error => {
      console.error('[CENTRO CUSTO] ❌ Erro ao carregar:', error);
      this.centrosCusto = [];
    }).finally(() => {
      this.loading = false;
    });

    // Log de estado dos arrays após 2 segundos (para debug)
    setTimeout(() => {
    }, 2000);
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

  // Método: Buscar colaboradores que já possuem inventário forçado pendente
  private buscarColaboradoresComInventarioPendente(): Promise<void> {
    return this.contestacaoApi.listarContestacoes('null', this.session.usuario.cliente, 1, this.session.token)
      .then(async (res) => {
        if (res.status === 200 && res.data) {
          // Buscar todas as páginas
          const todosRegistros: any[] = [];
          let paginaAtual = 1;
          const maxPaginas = 100;
          
          while (paginaAtual <= maxPaginas) {
            const resPagina = await this.contestacaoApi.listarContestacoes('null', this.session.usuario.cliente, paginaAtual, this.session.token);
            
            if (!resPagina || resPagina.status !== 200) break;
            
            const resultados = resPagina.data?.results || resPagina.data || [];
            if (!Array.isArray(resultados) || resultados.length === 0) break;
            
            todosRegistros.push(...resultados);
            
            const rowCount = resPagina.data?.rowCount;
            if (rowCount && todosRegistros.length >= rowCount) break;
            
            paginaAtual++;
          }
          
          // Filtrar apenas inventários forçados pendentes
          const inventariosForcadosPendentes = todosRegistros.filter(item => {
            const tipo = (item?.tipo_contestacao || item?.tipoContestacao || item?.TipoContestacao || '').toLowerCase();
            const status = (item?.status || item?.Status || '').toLowerCase();
            return tipo === 'inventario_forcado' && status === 'pendente';
          });
          
          // Extrair IDs dos colaboradores
          this.colaboradoresComInventarioPendente = inventariosForcadosPendentes
            .map(item => item?.colaborador?.id || item?.Colaborador?.Id || item?.colaboradorId)
            .filter(id => id != null);
        }
      })
      .catch(error => {
        console.error('[COLABORADORES-SEM-RECURSOS] ❌ Erro ao buscar inventários pendentes:', error);
        this.colaboradoresComInventarioPendente = [];
      });
  }

  consultar() {
    this.loading = true;
    
    // Buscar colaboradores com inventário pendente antes de consultar
    this.buscarColaboradoresComInventarioPendente().then(() => {
      const payload = {
        cargo: this.filtros.cargo || null,
        tipoColaborador: this.filtros.tipoColaborador || null,
        empresa: this.filtros.empresa || null,
        localidade: this.filtros.localidade || null,
        centroCusto: this.filtros.centroCusto || null,
        nome: this.filtros.nome || null,
        clienteId: this.session.usuario.cliente
      };

      this.api.consultarColaboradoresSemRecursos(payload, this.session.token).then(res => {
      this.loading = false;
      if (res.status === 200 && res.data) {
        // Backend retorna: { status, mensagem, data }
        let dados = null;
        
        // Tentar todas as possíveis localizações dos dados
        if (res.data.data && Array.isArray(res.data.data)) {
          dados = res.data.data;
        } else if (res.data.Data && Array.isArray(res.data.Data)) {
          dados = res.data.Data;
        } else if (Array.isArray(res.data)) {
          dados = res.data;
        }
        
        if (dados && Array.isArray(dados)) {
          this.colaboradores = dados;
        } else {
          console.error('[COLABORADORES-SEM-RECURSOS] ❌ Dados não são um array:', dados);
          console.error('[COLABORADORES-SEM-RECURSOS] ❌ res.data completo:', res.data);
          this.colaboradores = [];
        }
        
        // Configurar paginação local (como em auditoria de acessos)
        this.totalLength = this.colaboradores.length;
        this.currentPageIndex = 0;
        this.atualizarPagina();
      } else {
        this.util.exibirMensagemToast('Erro ao buscar colaboradores sem recursos', 3000);
        this.colaboradores = [];
      }
      }).catch(error => {
        this.loading = false;
        console.error('❌ Erro ao consultar colaboradores sem recursos:', error);
        this.util.exibirFalhaComunicacao();
        this.colaboradores = [];
      });
    }); // Fim do buscarColaboradoresComInventarioPendente().then()
  }

  limparBusca(): void {
    this.filtros = {
      cargo: null,
      tipoColaborador: null,
      empresa: null,
      localidade: null,
      centroCusto: null,
      nome: null
    };
    this.colaboradores = [];
    this.dadosPagina = [];
    this.totalLength = 0;
    this.currentPageIndex = 0;
    this.colaboradoresSelecionados = [];
  }

  // Método: Mudança de página (paginação local - como em auditoria)
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPageIndex = event.pageIndex;
    this.atualizarPagina();
  }

  // Método: Verificar se colaborador está desligado (data demissão preenchida e <= hoje)
  private estaDesligado(colaborador: any): boolean {
    if (!colaborador.dataDemissao) {
      return false;
    }
    
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0); // Zerar horas para comparar apenas data
    
    const dataDemissao = new Date(colaborador.dataDemissao);
    dataDemissao.setHours(0, 0, 0, 0);
    
    return dataDemissao <= hoje;
  }

  // Método: Verificar se colaborador já tem inventário forçado pendente
  public temInventarioPendente(colaborador: any): boolean {
    return this.colaboradoresComInventarioPendente.includes(colaborador.id);
  }

  // Método: Atualizar dados da página corrente (paginação local)
  private atualizarPagina() {
    let colaboradoresFiltrados = this.colaboradores;
    
    if (Array.isArray(this.colaboradores)) {
      const totalAntes = this.colaboradores.length;
      
      // Filtro: desligados
      if (this.ocultarDesligados) {
        colaboradoresFiltrados = colaboradoresFiltrados.filter(c => !this.estaDesligado(c));
      }
      
      // Filtro: com inventário pendente
      if (this.ocultarComInventarioPendente) {
        colaboradoresFiltrados = colaboradoresFiltrados.filter(c => !this.temInventarioPendente(c));
      }
      
      const totalRemovido = totalAntes - colaboradoresFiltrados.length;
      if (totalRemovido > 0) {
        if (this.ocultarDesligados) {
        }
        if (this.ocultarComInventarioPendente) {
        }
      }
    }
    
    // Atualizar total com base nos dados filtrados
    this.totalLength = Array.isArray(colaboradoresFiltrados) ? colaboradoresFiltrados.length : 0;
    
    const inicio = this.currentPageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    
    if (Array.isArray(colaboradoresFiltrados)) {
      this.dadosPagina = colaboradoresFiltrados.slice(inicio, fim);
    } else {
      console.error('[COLABORADORES-SEM-RECURSOS] ❌ colaboradores NÃO é um array!', this.colaboradores);
      this.dadosPagina = [];
    }
  }
  
  // Método: Toggle para ocultar/exibir desligados
  toggleOcultarDesligados(): void {
    // NOTA: Não invertemos o valor aqui porque o [(ngModel)] já faz isso automaticamente
    this.currentPageIndex = 0; // Voltar para primeira página
    this.atualizarPagina();
    
    const acao = this.ocultarDesligados ? 'ocultados' : 'exibidos';
    this.util.exibirMensagemToast(`Colaboradores desligados ${acao}`, 2000);
  }

  // Método: Toggle para ocultar/exibir colaboradores com inventário pendente
  toggleOcultarComInventarioPendente(): void {
    // NOTA: Não invertemos o valor aqui porque o [(ngModel)] já faz isso automaticamente
    this.currentPageIndex = 0; // Voltar para primeira página
    this.atualizarPagina();
    
    const acao = this.ocultarComInventarioPendente ? 'ocultados' : 'exibidos';
    this.util.exibirMensagemToast(`Colaboradores com inventário pendente ${acao}`, 2000);
  }

  // Métodos para métricas
  getTotalColaboradores(): number {
    if (!this.colaboradores || !Array.isArray(this.colaboradores)) {
      return 0;
    }
    
    let filtrados = this.colaboradores;
    
    // Aplicar filtros ativos
    if (this.ocultarDesligados) {
      filtrados = filtrados.filter(c => !this.estaDesligado(c));
    }
    
    if (this.ocultarComInventarioPendente) {
      filtrados = filtrados.filter(c => !this.temInventarioPendente(c));
    }
    
    return filtrados.length;
  }

  getTotalPorTipo(tipoChar: string): number {
    if (!this.colaboradores) return 0;
    return this.colaboradores.filter(c => c.tipoColaboradorDescricao === tipoChar).length;
  }

  getTotalPorCargo(cargoNome: string): number {
    if (!this.colaboradores) return 0;
    return this.colaboradores.filter(c => c.cargoDescricao === cargoNome).length;
  }

  // Conta quantos tipos DISTINTOS existem nos colaboradores retornados
  getTiposDistintos(): number {
    if (!this.colaboradores || this.colaboradores.length === 0) return 0;
    const tiposUnicos = new Set(this.colaboradores.map(c => c.tipoColaboradorId));
    return tiposUnicos.size;
  }

  // Conta quantas empresas DISTINTAS existem nos colaboradores retornados
  getEmpresasDistintas(): number {
    if (!this.colaboradores || this.colaboradores.length === 0) return 0;
    const empresasUnicas = new Set(
      this.colaboradores
        .filter(c => c.empresaId != null)
        .map(c => c.empresaId)
    );
    return empresasUnicas.size;
  }

  // Seleção de colaboradores
  toggleColaboradorSelecionado(colaborador: any): void {
    const index = this.colaboradoresSelecionados.findIndex(c => c.id === colaborador.id);
    if (index >= 0) {
      this.colaboradoresSelecionados.splice(index, 1);
    } else {
      this.colaboradoresSelecionados.push(colaborador);
    }
  }

  isColaboradorSelecionado(colaborador: any): boolean {
    return this.colaboradoresSelecionados.some(c => c.id === colaborador.id);
  }

  selecionarTodos(): void {
    if (this.colaboradoresSelecionados.length === this.dadosPagina.length) {
      // Desselecionar todos da página
      this.dadosPagina.forEach(col => {
        const index = this.colaboradoresSelecionados.findIndex(c => c.id === col.id);
        if (index >= 0) {
          this.colaboradoresSelecionados.splice(index, 1);
        }
      });
    } else {
      // Selecionar todos da página
      this.dadosPagina.forEach(col => {
        if (!this.isColaboradorSelecionado(col)) {
          this.colaboradoresSelecionados.push(col);
        }
      });
    }
  }

  isTodosSelecionados(): boolean {
    if (this.dadosPagina.length === 0) return false;
    return this.dadosPagina.every(col => this.isColaboradorSelecionado(col));
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
      if (!this.colaboradores || this.colaboradores.length === 0) {
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
    XLSX.utils.book_append_sheet(wb, ws, 'Colaboradores Sem Recursos');

    // Ajustar largura das colunas
    const colWidths = [
      { wch: 30 }, // Nome
      { wch: 15 }, // Matrícula
      { wch: 25 }, // Cargo
      { wch: 25 }, // Empresa
      { wch: 20 }, // Localidade
      { wch: 25 }, // Centro de Custo
      { wch: 20 }, // Tipo de Colaborador
      { wch: 15 }  // Data de Admissão
    ];
    ws['!cols'] = colWidths;

    const nomeArquivo = `colaboradores-sem-recursos-${dataAtual}.xlsx`;
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
    link.setAttribute('download', `colaboradores-sem-recursos-${dataAtual}.csv`);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    this.util.exibirMensagemToast('Relatório CSV exportado com sucesso!', 3000);
  }

  private prepararDadosParaExportacao(): any[] {
    if (!this.colaboradores) return [];
    
    return this.colaboradores.map(item => ({
      'Nome': item.nome || 'N/A',
      'Matrícula': item.matricula || 'N/A',
      'Cargo': item.cargoDescricao || 'N/A',
      'Empresa': item.empresaDescricao || 'N/A',
      'Localidade': item.localidadeDescricao || 'N/A',
      'Centro de Custo': item.centroCustoDescricao || 'N/A',
      'Tipo de Colaborador': item.tipoColaboradorDescricao || 'N/A',
      'Data de Admissão': item.dataAdmissao ? this.formatarData(item.dataAdmissao) : 'N/A'
    }));
  }

  formatarData(dataStr: string): string {
    if (!dataStr) return 'N/A';
    const data = new Date(dataStr);
    return data.toLocaleDateString('pt-BR');
  }

  verDetalhes(colaborador: any): void {
    this.colaboradorSelecionado = colaborador;
    this.showDetalhesModal = true;
  }

  closeDetalhesModal(event: Event): void {
    this.showDetalhesModal = false;
    this.colaboradorSelecionado = null;
  }

  // Forçar inventário
  abrirModalForcarInventario(colaborador?: any): void {
    if (colaborador) {
      // Modo individual: forçar para um colaborador específico
      this.colaboradorSelecionado = colaborador;
      this.colaboradoresSelecionados = [colaborador];
      this.showForcarInventarioModal = true;
    } else {
      // Modo massa: filtrar colaboradores que JÁ TEM inventário pendente
      const colaboradoresValidos = this.colaboradoresSelecionados.filter(c => 
        !this.temInventarioPendente(c)
      );
      const colaboradoresComInventario = this.colaboradoresSelecionados.filter(c => 
        this.temInventarioPendente(c)
      );
      if (colaboradoresComInventario.length > 0) {
        const nomes = colaboradoresComInventario.map(c => c.nome).join(', ');
        const mensagem = colaboradoresComInventario.length === 1
          ? `⚠️ O colaborador "${nomes}" já possui inventário forçado em aberto e foi removido da seleção.`
          : `⚠️ ${colaboradoresComInventario.length} colaboradores já possuem inventário forçado em aberto e foram removidos da seleção:\n${nomes}`;
        
        this.util.exibirMensagemToast(mensagem, 6000);
      }
      
      if (colaboradoresValidos.length === 0) {
        this.util.exibirMensagemToast(
          '❌ Nenhum colaborador selecionado é elegível para forçar inventário. Todos já possuem inventário pendente.',
          5000
        );
        return;
      }
      
      // Atualizar a seleção apenas com colaboradores válidos
      this.colaboradoresSelecionados = colaboradoresValidos;
      this.showForcarInventarioModal = true;
    }
  }

  closeForcarInventarioModal(event: Event): void {
    this.showForcarInventarioModal = false;
    this.colaboradorSelecionado = null;
    // Resetar valores do modal
    this.enviarEmailNotificacao = true;
    this.mensagemAdicional = '';
  }

  confirmarForcarInventario(): void {
    if (this.colaboradoresSelecionados.length === 0) {
      this.util.exibirMensagemToast('Selecione pelo menos um colaborador', 3000);
      return;
    }

    // 🚫 VALIDAÇÃO: Verificar se algum colaborador já tem inventário pendente
    const colaboradoresComInventario = this.colaboradoresSelecionados.filter(c => 
      this.temInventarioPendente(c)
    );
    if (colaboradoresComInventario.length > 0) {
      const nomes = colaboradoresComInventario.map(c => c.nome).join(', ');
      const mensagem = colaboradoresComInventario.length === 1
        ? `❌ O colaborador "${nomes}" já possui um inventário forçado em aberto. Não é possível criar outro até que o atual seja finalizado.`
        : `❌ Os seguintes colaboradores já possuem inventário forçado em aberto: ${nomes}. Remova-os da seleção para continuar.`;
      
      this.util.exibirMensagemToast(mensagem, 5000);
      return;
    }

    const ids = this.colaboradoresSelecionados.map(c => c.id);
    const payload = {
      colaboradorId: ids[0], // Para compatibilidade com endpoint
      colaboradorIds: ids, // Lista completa para criar múltiplos
      motivo: 'Colaborador sem recursos cadastrados',
      descricao: `Inventário forçado pela equipe de TI. Colaborador(es) identificado(s) sem recursos no sistema. ${ids.length > 1 ? `Total: ${ids.length} colaboradores` : ''}`,
      usuarioId: this.session.usuario.id,
      clienteId: this.session.usuario.cliente,
      enviarEmail: this.enviarEmailNotificacao, // 📧 Novo campo
      mensagemAdicional: this.mensagemAdicional || '' // 💬 Novo campo
    };
    
    this.loading = true;
    this.contestacaoApi.criarInventarioForcado(payload, this.session.token).then(res => {
      this.loading = false;
      
      if (res.status === 200 || res.status === 201) {
        const mensagemEmail = this.enviarEmailNotificacao 
          ? ' E-mails de notificação foram enviados.' 
          : '';
        this.util.exibirMensagemToast(
          `✅ Inventário forçado criado com sucesso para ${ids.length} colaborador(es).${mensagemEmail}`, 
          4000
        );
        // Fechar modal e resetar
        this.showForcarInventarioModal = false;
        this.colaboradoresSelecionados = [];
        this.colaboradorSelecionado = null;
        this.enviarEmailNotificacao = true;
        this.mensagemAdicional = '';
        
        // Navegar para a tela de contestações na aba de inventário forçado
        setTimeout(() => {
          this.router.navigate(['/movimentacoes/contestacoes'], { 
            queryParams: { tab: 'inventario-forcado' } 
          });
        }, 1500);
      } else {
        this.util.exibirMensagemToast('❌ Erro ao criar inventário forçado', 3000);
      }
    }).catch(error => {
      this.loading = false;
      console.error('[COLABORADORES-SEM-RECURSOS] ❌ Erro ao forçar inventário:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

  navegarParaColaborador(id: number): void {
    // Buscar o colaborador pelo ID para pegar a matrícula
    const colaborador = this.colaboradores.find(c => c.id === id) || this.colaboradorSelecionado;
    
    if (colaborador && colaborador.matricula) {
      // Navegar para a tela de colaboradores COM FILTRO pela matrícula
      this.router.navigate(['/colaboradores'], { 
        queryParams: { 
          filtro: colaborador.matricula,
          origem: 'colaboradores-sem-recursos'
        } 
      });
    } else {
      // Fallback: navegar sem filtro
      this.router.navigate(['/colaboradores']);
    }
  }

  // Navegar para o inventário forçado pendente do colaborador
  navegarParaInventarioPendente(colaborador: any): void {
    if (!colaborador) return;
    
    // Navegar para a tela de contestações, aba de inventário forçado, 
    // usando o NOME do colaborador como termo de busca
    this.router.navigate(['/movimentacoes/contestacoes'], { 
      queryParams: { 
        tab: 'inventario-forcado',
        search: colaborador.nome, // 🔍 Busca pelo nome do colaborador
        origem: 'colaboradores-sem-recursos'
      } 
    });
  }
}

