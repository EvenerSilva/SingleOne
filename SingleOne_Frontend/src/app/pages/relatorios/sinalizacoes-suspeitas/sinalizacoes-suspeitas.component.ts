import { Component, OnInit, ViewChild, OnDestroy, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import * as XLSX from 'xlsx';

export interface SinalizacaoSuspeita {
  id: number;
  colaboradorId: number;
  colaboradorNome: string;
  cpfConsultado: string;
  motivoSuspeita: string;
  descricaoDetalhada: string;
  observacoesVigilante: string;
  status: 'pendente' | 'em_investigacao' | 'resolvida' | 'arquivada';
  prioridade: 'baixa' | 'media' | 'alta' | 'critica';
  vigilanteId: number;
  vigilanteNome: string;
  investigadorId?: number;
  investigadorNome?: string;
  dataSinalizacao: string;
  dataInvestigacao?: string;
  dataResolucao?: string;
  resultadoInvestigacao?: string;
  acoesTomadas?: string;
  observacoesFinais?: string;
  dadosConsulta?: any;
  evidenciaUrls?: string[];
  numeroProtocolo: string;
}

@Component({
  selector: 'app-sinalizacoes-suspeitas',
  templateUrl: './sinalizacoes-suspeitas.component.html',
  styleUrls: ['./sinalizacoes-suspeitas.component.scss']
})
export class SinalizacoesSuspeitasComponent implements OnInit, AfterViewInit, OnDestroy {

  private session: any = {};
  public filtros: any = {
    dataInicio: null,
    dataFim: null,
    status: null,
    prioridade: null,
    motivoSuspeita: null,
    cpfConsultado: null,
    colaboradorNome: null,
    vigilanteNome: null
  };
  
  public sinalizacoes: SinalizacaoSuspeita[] = [];
  public statusOptions = [
    { valor: 'pendente', descricao: 'Pendente' },
    { valor: 'em_investigacao', descricao: 'Em Investigação' },
    { valor: 'resolvida', descricao: 'Resolvida' },
    { valor: 'arquivada', descricao: 'Arquivada' }
  ];
  
  public prioridadeOptions = [
    { valor: 'baixa', descricao: 'Baixa' },
    { valor: 'media', descricao: 'Média' },
    { valor: 'alta', descricao: 'Alta' },
    { valor: 'critica', descricao: 'Crítica' }
  ];

  public motivosSuspeita: any[] = [];
  public usuariosInvestigadores: any[] = [];
  
  public colunas = ['dataSinalizacao', 'colaboradorNome', 'motivoSuspeita', 'prioridade', 'status', 'vigilanteNome', 'acoes'];
  
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<SinalizacaoSuspeita>;
  
  // Paginação local
  public dadosPagina: SinalizacaoSuspeita[] = [];
  public totalLength = 0;
  public pageSize = 10;
  public currentPageIndex = 0;

  public loading = false;
  public showExportModal = false;
  public showDetalhesModal = false;
  public showInvestigatorModal = false;
  public showResolverModal = false;
  public showArquivarModal = false;
  public sinalizacaoSelecionada: SinalizacaoSuspeita | null = null;
  public investigadorSelecionado: any = null;
  
  // Formulário de resolução
  public formularioResolucao = {
    resultadoInvestigacao: '',
    acoesTomadas: '',
    observacoesFinais: ''
  };
  
  // Formulário de arquivamento
  public formularioArquivamento = {
    motivo: '',
    observacoes: ''
  };
  
  private destroy$ = new Subject<void>();

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService, 
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.configurarDatasPadrao();
    this.carregarMotivosSuspeita();
    this.carregarUsuariosInvestigadores();
  }

  ngAfterViewInit(): void {
    this.inicializarDataSource();
  }

  private inicializarDataSource(): void {
    if (!this.dataSource) {
      this.dataSource = new MatTableDataSource<SinalizacaoSuspeita>([]);
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

  async carregarMotivosSuspeita(): Promise<void> {
    try {
      const result = await this.api.obterMotivosSuspeita(this.session.token);
      
      if (result?.status === 200 && result?.data) {
        this.motivosSuspeita = result.data;
      } else {
        // Fallback com dados padrão se a API falhar
        this.motivosSuspeita = [
          { codigo: 'comportamento_estranho', descricao: 'Comportamento Estranho' },
          { codigo: 'documentos_inconsistentes', descricao: 'Documentos Inconsistentes' },
          { codigo: 'equipamentos_nao_reconhecidos', descricao: 'Equipamentos Não Reconhecidos' },
          { codigo: 'tentativa_evasao', descricao: 'Tentativa de Evasão' },
          { codigo: 'acompanhante_suspeito', descricao: 'Acompanhante Suspeito' },
          { codigo: 'horario_atipico', descricao: 'Horário Atípico' },
          { codigo: 'equipamentos_em_excesso', descricao: 'Equipamentos em Excesso' },
          { codigo: 'nervosismo_excessivo', descricao: 'Nervosismo Excessivo' },
          { codigo: 'outros', descricao: 'Outros Motivos' }
        ];
      }
    } catch (error) {
      console.error('Erro ao carregar motivos de suspeita:', error);
      
      // Fallback com dados padrão em caso de erro
      this.motivosSuspeita = [
        { codigo: 'comportamento_estranho', descricao: 'Comportamento Estranho' },
        { codigo: 'documentos_inconsistentes', descricao: 'Documentos Inconsistentes' },
        { codigo: 'equipamentos_nao_reconhecidos', descricao: 'Equipamentos Não Reconhecidos' },
        { codigo: 'tentativa_evasao', descricao: 'Tentativa de Evasão' },
        { codigo: 'acompanhante_suspeito', descricao: 'Acompanhante Suspeito' },
        { codigo: 'horario_atipico', descricao: 'Horário Atípico' },
        { codigo: 'equipamentos_em_excesso', descricao: 'Equipamentos em Excesso' },
        { codigo: 'nervosismo_excessivo', descricao: 'Nervosismo Excessivo' },
        { codigo: 'outros', descricao: 'Outros Motivos' }
      ];
    }
  }

  async carregarUsuariosInvestigadores(): Promise<void> {
    try {
      const result = await this.api.obterUsuariosInvestigadores(this.session.token);
      
      if (result?.status === 200 && result?.data) {
        this.usuariosInvestigadores = result.data;
      } else {
        this.usuariosInvestigadores = [];
      }
    } catch (error) {
      console.error('Erro ao carregar usuários investigadores:', error);
      this.usuariosInvestigadores = [];
    }
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
      status: this.filtros.status || null,
      prioridade: this.filtros.prioridade || null,
      motivoSuspeita: this.filtros.motivoSuspeita || null,
      cpfConsultado: this.filtros.cpfConsultado || null,
      colaboradorNome: this.filtros.colaboradorNome || null,
      pagina: 1,
      tamanhoPagina: 1000 // Buscar todos os registros
    };

    this.api.consultarSinalizacoesSuspeitas(payload, this.session.token).then(res => {
      this.loading = false;
      if (res.status === 200 && res.data) {
        // O backend retorna um objeto com Sinalizacoes, não um array direto
        let sinalizacoesArray = [];
        if (res.data.sinalizacoes && Array.isArray(res.data.sinalizacoes)) {
          sinalizacoesArray = res.data.sinalizacoes;
        } else if (Array.isArray(res.data)) {
          sinalizacoesArray = res.data;
        }
        
        this.sinalizacoes = sinalizacoesArray;
        
        // Atualizar o dataSource para habilitar o botão de exportação
        if (this.dataSource) {
          this.dataSource.data = this.sinalizacoes;
        }
        
        // Configurar paginação local
        this.totalLength = this.sinalizacoes.length;
        this.currentPageIndex = 0;
        this.atualizarPagina();
      } else {
        this.sinalizacoes = [];
        this.dadosPagina = [];
        this.totalLength = 0;
        
        // Limpar o dataSource
        if (this.dataSource) {
          this.dataSource.data = [];
        }
        
        this.util.exibirMensagemToast('Nenhuma sinalização encontrada para o período selecionado', 3000);
      }
    }).catch(error => {
      this.loading = false;
      this.sinalizacoes = [];
      this.dadosPagina = [];
      this.totalLength = 0;
      
      // Limpar o dataSource
      if (this.dataSource) {
        this.dataSource.data = [];
      }
      
      console.error('Erro ao consultar sinalizações:', error);
      this.util.exibirFalhaComunicacao();
    });
  }

limparBusca(): void {
    this.filtros = {
      dataInicio: null,
      dataFim: null,
      status: null,
      prioridade: null,
      motivoSuspeita: null,
      cpfConsultado: null,
      colaboradorNome: null,
      vigilanteNome: null
    };
    this.configurarDatasPadrao();
    this.sinalizacoes = [];
    this.dadosPagina = [];
    this.totalLength = 0;
    this.currentPageIndex = 0;
    
    // Limpar o dataSource
    if (this.dataSource) {
      this.dataSource.data = [];
    }
  }

  // Método: Mudança de página (paginação local)
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPageIndex = event.pageIndex;
    this.atualizarPagina();
  }

  // Método: Atualizar dados da página corrente (paginação local)
  private atualizarPagina() {
    const inicio = this.currentPageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    
    // Garantir que sinalizacoes é um array
    if (Array.isArray(this.sinalizacoes)) {
      this.dadosPagina = this.sinalizacoes.slice(inicio, fim);
    } else {
      this.dadosPagina = [];
    }
  }

  // Métodos para métricas
  getTotalSinalizacoes(): number {
    return Array.isArray(this.sinalizacoes) ? this.sinalizacoes.length : 0;
  }

  getTotalPendentes(): number {
    if (!Array.isArray(this.sinalizacoes)) return 0;
    return this.sinalizacoes.filter(item => item.status === 'pendente').length;
  }

  getTotalEmInvestigacao(): number {
    if (!Array.isArray(this.sinalizacoes)) return 0;
    return this.sinalizacoes.filter(item => item.status === 'em_investigacao').length;
  }

  getTotalResolvidas(): number {
    if (!Array.isArray(this.sinalizacoes)) return 0;
    return this.sinalizacoes.filter(item => item.status === 'resolvida').length;
  }

  getTotalCriticas(): number {
    if (!Array.isArray(this.sinalizacoes)) return 0;
    return this.sinalizacoes.filter(item => item.prioridade === 'critica').length;
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
      if (!this.sinalizacoes || this.sinalizacoes.length === 0) {
        this.util.exibirMensagemToast('Não há dados para exportar', 3000);
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
    XLSX.utils.book_append_sheet(wb, ws, 'Sinalizações de Suspeitas');

    // Ajustar largura das colunas
    const colWidths = [
      { wch: 12 }, // Data
      { wch: 10 }, // Hora
      { wch: 20 }, // Protocolo
      { wch: 30 }, // Colaborador
      { wch: 15 }, // CPF
      { wch: 25 }, // Motivo
      { wch: 12 }, // Prioridade
      { wch: 15 }, // Status
      { wch: 25 }, // Vigilante
      { wch: 25 }, // Investigador
      { wch: 50 }, // Descrição
      { wch: 50 }, // Observações
      { wch: 50 }, // Resultado
      { wch: 50 }  // Ações Tomadas
    ];
    ws['!cols'] = colWidths;

    const nomeArquivo = `sinalizacoes-suspeitas-${dataAtual}.xlsx`;
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
    link.setAttribute('download', `sinalizacoes-suspeitas-${dataAtual}.csv`);
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
    if (!Array.isArray(this.sinalizacoes)) return [];
    
    return this.sinalizacoes.map(item => ({
      'Data': this.formatarData(item.dataSinalizacao),
      'Hora': this.formatarHora(item.dataSinalizacao),
      'Protocolo': item.numeroProtocolo || 'N/A',
      'Colaborador': item.colaboradorNome,
      'CPF': item.cpfConsultado,
      'Motivo': this.getMotivoSuspeitaDescricao(item.motivoSuspeita),
      'Prioridade': this.getPrioridadeDescricao(item.prioridade),
      'Status': this.getStatusDescricao(item.status),
      'Vigilante': item.vigilanteNome || 'N/A',
      'Investigador': item.investigadorNome || 'N/A',
      'Descrição': item.descricaoDetalhada || 'N/A',
      'Observações': item.observacoesVigilante || 'N/A',
      'Resultado': item.resultadoInvestigacao || 'N/A',
      'Ações Tomadas': item.acoesTomadas || 'N/A'
    }));
  }

  private converterParaCSV(dados: any[]): string {
    if (dados.length === 0) return '';
    
    const headers = Object.keys(dados[0]);
    const csvRows = [headers.join(',')];
    
    for (const row of dados) {
      const values = headers.map(header => {
        const value = row[header];
        return `"${value}"`;
      });
      csvRows.push(values.join(','));
    }
    
    return csvRows.join('\n');
  }

  private downloadArquivo(conteudo: string, nomeArquivo: string, tipoMime: string): void {
    const blob = new Blob([conteudo], { type: tipoMime });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = nomeArquivo;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  // Métodos de formatação
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

  getMotivoSuspeitaDescricao(motivo: string): string {
    const motivoEncontrado = this.motivosSuspeita.find(m => m.codigo === motivo);
    return motivoEncontrado ? motivoEncontrado.descricao : motivo;
  }

  getPrioridadeDescricao(prioridade: string): string {
    const prioridadeEncontrada = this.prioridadeOptions.find(p => p.valor === prioridade);
    return prioridadeEncontrada ? prioridadeEncontrada.descricao : prioridade;
  }

  getStatusDescricao(status: string): string {
    const statusEncontrado = this.statusOptions.find(s => s.valor === status);
    return statusEncontrado ? statusEncontrado.descricao : status;
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'pendente': return 'status-pendente';
      case 'em_investigacao': return 'status-investigacao';
      case 'resolvida': return 'status-resolvida';
      case 'arquivada': return 'status-arquivada';
      default: return 'status-desconhecido';
    }
  }

  getPrioridadeClass(prioridade: string): string {
    switch (prioridade) {
      case 'baixa': return 'prioridade-baixa';
      case 'media': return 'prioridade-media';
      case 'alta': return 'prioridade-alta';
      case 'critica': return 'prioridade-critica';
      default: return 'prioridade-desconhecida';
    }
  }

  verDetalhes(sinalizacao: SinalizacaoSuspeita): void {
    this.sinalizacaoSelecionada = sinalizacao;
    this.showDetalhesModal = true;
  }

  fecharDetalhesModal(): void {
    this.showDetalhesModal = false;
    this.sinalizacaoSelecionada = null;
  }

  alterarStatus(sinalizacao: SinalizacaoSuspeita, novoStatus: string): void {
    const observacoes = prompt('Digite observações sobre a alteração de status (opcional):');
    
    this.api.alterarStatusSinalizacao(sinalizacao.id, novoStatus, observacoes || '', this.session.token).then(res => {
      if (res.status === 200) {
        this.util.exibirMensagemToast('Status alterado com sucesso!', 3000);
        // Atualizar a sinalização local
        sinalizacao.status = novoStatus as any;
        if (novoStatus === 'em_investigacao') {
          sinalizacao.dataInvestigacao = new Date().toISOString();
        } else if (novoStatus === 'resolvida') {
          sinalizacao.dataResolucao = new Date().toISOString();
        }
        this.cdr.detectChanges();
      } else {
        this.util.exibirMensagemToast('Erro ao alterar status: ' + (res.mensagem || 'Erro desconhecido'), 3000);
      }
    }).catch(error => {
      console.error('Erro ao alterar status:', error);
      this.util.exibirMensagemToast('Erro ao alterar status', 3000);
    });
  }

  iniciarInvestigacao(sinalizacao: SinalizacaoSuspeita): void {
    // Iniciar investigação requer atribuir um investigador
    this.atribuirInvestigador(sinalizacao);
  }

  iniciarInvestigacaoDoModal(sinalizacao: SinalizacaoSuspeita): void {
    // Fechar o modal de detalhes primeiro
    this.showDetalhesModal = false;
    
    // Aguardar um momento para o modal fechar antes de abrir o próximo
    setTimeout(() => {
      this.atribuirInvestigador(sinalizacao);
    }, 100);
  }

  async atribuirInvestigador(sinalizacao: SinalizacaoSuspeita): Promise<void> {
    // Se não há investigadores, carregar antes de abrir o modal
    if (!this.usuariosInvestigadores || this.usuariosInvestigadores.length === 0) {
      await this.carregarUsuariosInvestigadores();
    }
    
    // Verificar novamente se há investigadores após o carregamento
    if (!this.usuariosInvestigadores || this.usuariosInvestigadores.length === 0) {
      this.util.exibirMensagemToast('Nenhum usuário ativo disponível para ser investigador', 3000);
      return;
    }
    
    this.sinalizacaoSelecionada = sinalizacao;
    this.investigadorSelecionado = null;
    this.showInvestigatorModal = true;
    this.cdr.detectChanges();
  }

  selecionarInvestigador(investigador: any): void {
    this.investigadorSelecionado = investigador;
    this.cdr.detectChanges();
  }

  confirmarAtribuicaoInvestigador(): void {
    if (!this.investigadorSelecionado || !this.sinalizacaoSelecionada) {
      this.util.exibirMensagemToast('Selecione um investigador', 3000);
      return;
    }
    this.api.atribuirInvestigador(this.sinalizacaoSelecionada.id, this.investigadorSelecionado.id, this.session.token).then(res => {
      if (res.status === 200) {
        // Atualizar dados locais
        this.sinalizacaoSelecionada!.investigadorId = this.investigadorSelecionado.id;
        this.sinalizacaoSelecionada!.investigadorNome = this.investigadorSelecionado.nome;
        
        // Se ainda está pendente, mudar para em_investigacao
        if (this.sinalizacaoSelecionada!.status === 'pendente') {
          return this.api.alterarStatusSinalizacao(
            this.sinalizacaoSelecionada!.id, 
            'em_investigacao', 
            `Investigação iniciada por ${this.investigadorSelecionado.nome}`, 
            this.session.token
          ).then(statusRes => {
            if (statusRes.status === 200) {
              this.sinalizacaoSelecionada!.status = 'em_investigacao' as any;
              this.sinalizacaoSelecionada!.dataInvestigacao = new Date().toISOString();
              this.util.exibirMensagemToast('Investigação iniciada com sucesso!', 3000);
            } else {
              this.util.exibirMensagemToast('Erro ao alterar status: ' + (statusRes.mensagem || 'Erro desconhecido'), 3000);
            }
            this.cdr.detectChanges();
            this.fecharModalInvestigador();
          });
        } else {
          this.util.exibirMensagemToast('Investigador atribuído com sucesso!', 3000);
          this.cdr.detectChanges();
          this.fecharModalInvestigador();
        }
      } else {
        console.error('[SINALIZACOES] Erro na resposta:', res);
        this.util.exibirMensagemToast('Erro ao atribuir investigador: ' + (res.mensagem || res.message || 'Erro desconhecido'), 3000);
      }
    }).catch(error => {
      console.error('[SINALIZACOES] Erro ao atribuir investigador:', error);
      this.util.exibirMensagemToast('Erro ao atribuir investigador: ' + (error?.message || 'Erro na comunicação'), 3000);
    });
  }

  fecharModalInvestigador(): void {
    this.showInvestigatorModal = false;
    this.sinalizacaoSelecionada = null;
    this.investigadorSelecionado = null;
  }

  resolverInvestigacao(sinalizacao: SinalizacaoSuspeita): void {
    this.sinalizacaoSelecionada = sinalizacao;
    this.formularioResolucao = {
      resultadoInvestigacao: '',
      acoesTomadas: '',
      observacoesFinais: ''
    };
    this.showResolverModal = true;
  }

  confirmarResolucao(): void {
    if (!this.sinalizacaoSelecionada) return;

    // Validações
    if (!this.formularioResolucao.resultadoInvestigacao || this.formularioResolucao.resultadoInvestigacao.trim() === '') {
      this.util.exibirMensagemToast('O resultado da investigação é obrigatório', 3000);
      return;
    }

    if (!this.formularioResolucao.acoesTomadas || this.formularioResolucao.acoesTomadas.trim() === '') {
      this.util.exibirMensagemToast('As ações tomadas são obrigatórias', 3000);
      return;
    }

    // Montar observações completas
    const observacoes = `RESULTADO: ${this.formularioResolucao.resultadoInvestigacao}\n\nAÇÕES TOMADAS: ${this.formularioResolucao.acoesTomadas}\n\nOBSERVAÇÕES: ${this.formularioResolucao.observacoesFinais || 'Nenhuma'}`;

    // Alterar status
    this.api.alterarStatusSinalizacao(
      this.sinalizacaoSelecionada.id, 
      'resolvida', 
      observacoes, 
      this.session.token
    ).then(res => {
      if (res.status === 200) {
        this.util.exibirMensagemToast('Investigação resolvida com sucesso!', 3000);
        
        // Atualizar dados locais
        this.sinalizacaoSelecionada!.status = 'resolvida' as any;
        this.sinalizacaoSelecionada!.dataResolucao = new Date().toISOString();
        this.sinalizacaoSelecionada!.resultadoInvestigacao = this.formularioResolucao.resultadoInvestigacao;
        this.sinalizacaoSelecionada!.acoesTomadas = this.formularioResolucao.acoesTomadas;
        this.sinalizacaoSelecionada!.observacoesFinais = this.formularioResolucao.observacoesFinais;
        
        this.cdr.detectChanges();
        this.fecharModalResolver();
      } else {
        this.util.exibirMensagemToast('Erro ao resolver investigação: ' + (res.mensagem || 'Erro desconhecido'), 3000);
      }
    }).catch(error => {
      console.error('Erro ao resolver investigação:', error);
      this.util.exibirMensagemToast('Erro ao resolver investigação', 3000);
    });
  }

  fecharModalResolver(): void {
    this.showResolverModal = false;
    this.sinalizacaoSelecionada = null;
    this.formularioResolucao = {
      resultadoInvestigacao: '',
      acoesTomadas: '',
      observacoesFinais: ''
    };
  }

  arquivarInvestigacao(sinalizacao: SinalizacaoSuspeita): void {
    this.sinalizacaoSelecionada = sinalizacao;
    this.formularioArquivamento = {
      motivo: '',
      observacoes: ''
    };
    this.showArquivarModal = true;
  }

  confirmarArquivamento(): void {
    if (!this.sinalizacaoSelecionada) return;

    // Validação - motivo é obrigatório
    if (!this.formularioArquivamento.motivo || this.formularioArquivamento.motivo.trim() === '') {
      this.util.exibirMensagemToast('O motivo do arquivamento é obrigatório', 3000);
      return;
    }

    // Montar observações completas
    const observacoes = `MOTIVO: ${this.formularioArquivamento.motivo}\n\nOBSERVAÇÕES: ${this.formularioArquivamento.observacoes || 'Nenhuma'}`;

    // Alterar status
    this.api.alterarStatusSinalizacao(
      this.sinalizacaoSelecionada.id, 
      'arquivada', 
      observacoes, 
      this.session.token
    ).then(res => {
      if (res.status === 200) {
        this.util.exibirMensagemToast('Investigação arquivada com sucesso!', 3000);
        
        // Atualizar dados locais
        this.sinalizacaoSelecionada!.status = 'arquivada' as any;
        
        this.cdr.detectChanges();
        this.fecharModalArquivar();
      } else {
        this.util.exibirMensagemToast('Erro ao arquivar investigação: ' + (res.mensagem || 'Erro desconhecido'), 3000);
      }
    }).catch(error => {
      console.error('Erro ao arquivar investigação:', error);
      this.util.exibirMensagemToast('Erro ao arquivar investigação', 3000);
    });
  }

  fecharModalArquivar(): void {
    this.showArquivarModal = false;
    this.sinalizacaoSelecionada = null;
    this.formularioArquivamento = {
      motivo: '',
      observacoes: ''
    };
  }
}
