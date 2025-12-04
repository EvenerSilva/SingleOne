import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import {
  DetalheColaboradorStagingDTO,
  HistoricoImportacaoColaboradores,
  ImportacaoColaboradoresService,
  ResumoValidacaoColaboradores,
  ResultadoImportacaoColaboradores
} from 'src/app/services/importacao-colaboradores.service';
import { UtilService } from 'src/app/util/util.service';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { ModalImportarColaboradoresComponent } from './modal-importar-colaboradores/modal-importar-colaboradores.component';
import { ModalExportarColaboradoresComponent } from './modal-exportar-colaboradores/modal-exportar-colaboradores.component';

@Component({
  selector: 'app-importacao-colaboradores-monitor',
  templateUrl: './importacao-colaboradores.component.html',
  styleUrls: ['./importacao-colaboradores.component.scss']
})
export class ImportacaoColaboradoresComponent implements OnInit, OnDestroy {
  pendentes: HistoricoImportacaoColaboradores[] = [];
  historico: HistoricoImportacaoColaboradores[] = [];

  loteSelecionado: HistoricoImportacaoColaboradores | null = null;
  resumoSelecionado: ResumoValidacaoColaboradores | null = null;
  detalhesSelecionados: DetalheColaboradorStagingDTO[] = [];
  filtroDetalhes: string = '';

  carregandoHistorico = false;
  carregandoResumo = false;
  carregandoDetalhes = false;
  baixandoErros = false;

  session: any = null;
  cliente = 0;

  abaAtiva = 0;
  detalheAbaAtiva: 'resumo' | 'registros' = 'resumo';
  mostrarPainelDetalhes = false;
  private pollingHistorico: Record<string, { tentativaAtual: number; timeoutId: any }> = {};

  constructor(
    private importacaoService: ImportacaoColaboradoresService,
    private util: UtilService,
    private router: Router,
    private colaboradorApi: ColaboradorApiService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session?.usuario?.cliente || 0;
    this.carregarHistorico();
  }

  ngOnDestroy(): void {
    Object.values(this.pollingHistorico).forEach(entry => {
      if (entry?.timeoutId) {
        clearTimeout(entry.timeoutId);
      }
    });
    this.pollingHistorico = {};
  }

  carregarHistorico(onComplete?: () => void): void {
    this.carregandoHistorico = true;

    this.importacaoService.obterHistorico(50).subscribe({
      next: (lista) => {
        const itensNormalizados = (lista || []).map(item => this.normalizarStatusHistorico(item));

        this.historico = itensNormalizados;
        this.pendentes = itensNormalizados.filter(item => item.status === 'VALIDADO');
        this.carregandoHistorico = false;

        if (this.loteSelecionado) {
          const atualizado = this.historico.find(h => h.loteId === this.loteSelecionado?.loteId);
          if (atualizado) {
            this.loteSelecionado = atualizado;
          } else {
            this.fecharPainelDetalhes();
          }
        }

        onComplete?.();
      },
      error: (erro) => {
        console.error('[IMPORTACAO-COLABORADORES] Erro ao carregar histórico', erro);
        this.carregandoHistorico = false;
        this.util.exibirFalhaComunicacao();
        onComplete?.();
      }
    });
  }

  get lotesProcessando(): HistoricoImportacaoColaboradores[] {
    return (this.historico || []).filter(item => item.status === 'PROCESSANDO');
  }

  get totalPendentes(): number {
    return this.pendentes?.length || 0;
  }

  get totalProcessando(): number {
    return this.lotesProcessando.length;
  }

  get totalConcluidosHoje(): number {
    const agora = new Date().getTime();
    return (this.historico || []).filter(item => {
      if (item.status !== 'CONCLUIDO') {
        return false;
      }
      const referencia = item.dataFim || item.dataInicio;
      if (!referencia) {
        return false;
      }
      const data = new Date(referencia).getTime();
      if (isNaN(data)) {
        return false;
      }
      return (agora - data) <= 24 * 60 * 60 * 1000;
    }).length;
  }

  get totalCanceladosRecentes(): number {
    const agora = new Date().getTime();
    const seteDias = 7 * 24 * 60 * 60 * 1000;
    return (this.historico || []).filter(item => {
      if (!['CANCELADO', 'ERRO'].includes(item.status)) {
        return false;
      }
      const referencia = item.dataFim || item.dataInicio;
      if (!referencia) {
        return false;
      }
      const data = new Date(referencia).getTime();
      if (isNaN(data)) {
        return false;
      }
      return (agora - data) <= seteDias;
    }).length;
  }

  mudarParaAba(destino: 'pendentes' | 'processando' | 'historico'): void {
    const mapa = {
      pendentes: 0,
      processando: 1,
      historico: 2
    };
    this.abaAtiva = mapa[destino] ?? 0;
  }

  selecionarLote(lote: HistoricoImportacaoColaboradores): void {
    this.loteSelecionado = lote;
    this.resumoSelecionado = null;
    this.detalhesSelecionados = [];
    this.filtroDetalhes = '';
    this.carregarResumo(lote.loteId);
  }

  abrirDetalhes(lote: HistoricoImportacaoColaboradores): void {
    this.mostrarPainelDetalhes = true;
    this.detalheAbaAtiva = 'resumo';
    this.selecionarLote(lote);
    this.carregarDetalhes();
  }

  fecharPainelDetalhes(): void {
    this.limparSelecao();
    this.mostrarPainelDetalhes = false;
  }

  selecionarAbaDetalhes(aba: 'resumo' | 'registros'): void {
    this.detalheAbaAtiva = aba;
    if (aba === 'registros' && this.loteSelecionado && this.detalhesSelecionados.length === 0 && !this.carregandoDetalhes) {
      this.carregarDetalhes(this.filtroDetalhes || undefined);
    }
  }

  carregarResumo(loteId: string): void {
    this.carregandoResumo = true;

    this.importacaoService.obterResumo(loteId).subscribe({
      next: (resumo) => {
        this.resumoSelecionado = resumo;
        this.carregandoResumo = false;
      },
      error: (erro) => {
        console.error('[IMPORTACAO-COLABORADORES] Erro ao carregar resumo', erro);
        this.carregandoResumo = false;
        this.util.exibirMensagemToast('Erro ao carregar resumo da validação.', 4000);
      }
    });
  }

  carregarDetalhes(status?: string): void {
    if (!this.loteSelecionado) {
      return;
    }

    this.filtroDetalhes = status || '';
    this.carregandoDetalhes = true;

    this.importacaoService.obterValidacao(this.loteSelecionado.loteId, this.filtroDetalhes).subscribe({
      next: (detalhes) => {
        this.detalhesSelecionados = detalhes || [];
        this.carregandoDetalhes = false;
      },
      error: (erro) => {
        console.error('[IMPORTACAO-COLABORADORES] Erro ao carregar detalhes', erro);
        this.carregandoDetalhes = false;
        this.util.exibirMensagemToast('Erro ao carregar detalhes do lote.', 4000);
      }
    });
  }

  selecionarStatusDetalhe(status: string): void {
    this.filtroDetalhes = status;
    this.detalheAbaAtiva = 'registros';
    this.carregarDetalhes(status);
  }

  limparFiltroDetalhes(): void {
    this.filtroDetalhes = '';
    if (this.loteSelecionado) {
      this.detalheAbaAtiva = 'registros';
      this.carregarDetalhes();
    }
  }

  async confirmarLote(lote: HistoricoImportacaoColaboradores): Promise<void> {
    // Validação adicional: não permitir confirmar lotes cancelados ou concluídos
    if (!this.podeConfirmar(lote.status)) {
      const statusDesc = this.getStatusDescricao(lote.status, lote.statusDescricao);
      this.util.exibirMensagemToast(`⚠️ Não é possível confirmar um lote com status "${statusDesc}".`, 5000);
      return;
    }

    const mensagem =
      `Tem certeza que deseja confirmar a importação?<br><br>` +
      `📦 <strong>Lote:</strong> ${lote.loteId}<br>` +
      `📁 <strong>Arquivo:</strong> ${lote.nomeArquivo || 'N/A'}<br>` +
      `👤 <strong>Usuário:</strong> ${lote.usuarioNome || 'N/A'}<br>` +
      `📅 <strong>Data:</strong> ${this.formatarData(lote.dataInicio)}<br><br>` +
      `<strong>Atenção:</strong> esta ação criará/atualizará colaboradores e não poderá ser desfeita.`;

    const confirmou = await this.util.exibirMensagemPopUp(mensagem, true);
    if (!confirmou) {
      return;
    }

    this.util.aguardar(true);
    this.importacaoService.confirmarImportacao(lote.loteId).subscribe({
      next: (resultado) => {
        this.util.aguardar(false);
        this.util.exibirMensagemToast(`✅ ${resultado.mensagem}`, 5000);

        // Atualiza status do lote localmente para refletir a conclusão imediata
        this.pendentes = this.pendentes.filter(item => item.loteId !== lote.loteId);
        this.historico = this.historico.map(item =>
          item.loteId === lote.loteId
            ? { ...item, status: 'CONCLUIDO', statusDescricao: 'Concluído', totalImportados: resultado.totalProcessado }
            : item
        );
        if (this.loteSelecionado?.loteId === lote.loteId) {
          this.loteSelecionado = {
            ...this.loteSelecionado,
            status: 'CONCLUIDO',
            statusDescricao: 'Concluído',
            totalValidados: resultado.totalProcessado,
            totalImportados: resultado.totalProcessado
          };

          this.carregarDetalhes(this.filtroDetalhes || undefined);
        }

        this.carregarHistorico();
        if (this.loteSelecionado?.loteId === lote.loteId) {
          this.carregarResumo(lote.loteId);
        }
        if (this.loteSelecionado?.loteId === lote.loteId) {
          this.fecharPainelDetalhes();
        }
      },
      error: (erro) => {
        console.error('[IMPORTACAO-COLABORADORES] Erro ao confirmar lote', erro);
        this.util.aguardar(false);
        
        let mensagemErro = 'Erro ao confirmar importação.';
        
        if (erro?.error?.mensagem) {
          mensagemErro = erro.error.mensagem;
          
          // Mensagem mais amigável para o caso específico de não haver registros válidos
          if (mensagemErro.includes('Não há registros válidos') || mensagemErro.includes('registros válidos')) {
            mensagemErro = '⚠️ Este lote não possui registros válidos para importar. Pode ter sido cancelado anteriormente ou não possui dados válidos.';
          }
        }
        
        this.util.exibirMensagemToast(`❌ ${mensagemErro}`, 5000);
      }
    });
  }

  async cancelarLote(lote: HistoricoImportacaoColaboradores): Promise<void> {
    const mensagem =
      `Deseja cancelar o lote <strong>${lote.loteId}</strong>?<br>` +
      `Os dados validados serão descartados e será necessário reenviar o arquivo.`;

    const confirmou = await this.util.exibirMensagemPopUp(mensagem, true);
    if (!confirmou) {
      return;
    }

    const loteIdParaCancelar = lote.loteId;
    
    this.util.aguardar(true);
    this.importacaoService.cancelarImportacao(loteIdParaCancelar).subscribe({
      next: () => {
        this.pendentes = this.pendentes.filter(item => item.loteId !== loteIdParaCancelar);
        this.historico = this.historico.map(item => {
          if (item.loteId === loteIdParaCancelar) {
            return {
              ...item,
              status: 'CANCELADO',
              statusDescricao: 'Cancelado',
              dataFim: new Date(),
              observacoes: 'Importação cancelada pelo usuário'
            };
          }
          return item;
        });

        if (this.loteSelecionado?.loteId === loteIdParaCancelar) {
          this.fecharPainelDetalhes();
        }

        this.util.aguardar(false);
        this.util.exibirMensagemToast('ℹ️ Importação cancelada com sucesso.', 4000);

        setTimeout(() => {
          this.carregarHistorico();
        }, 1000);
      },
      error: (erro) => {
        console.error('[IMPORTACAO-COLABORADORES] Erro ao cancelar lote', erro);
        this.util.aguardar(false);
        const mensagemErro = erro?.error?.mensagem || 'Erro ao cancelar importação.';
        this.util.exibirMensagemToast(`❌ ${mensagemErro}`, 5000);
      }
    });
  }

  baixarErros(lote: HistoricoImportacaoColaboradores): void {
    if (!this.session || !this.session.token) {
      this.util.exibirMensagemToast('Sessão expirada. Faça login novamente.', 4000);
      return;
    }

    this.baixandoErros = true;
    const url = this.importacaoService.getUrlErros(lote.loteId);

    this.importacaoService.baixarErros(url, this.session.token).subscribe({
      next: (blob) => {
        const link = document.createElement('a');
        const objectUrl = window.URL.createObjectURL(blob);
        link.href = objectUrl;
        link.download = `erros_importacao_${lote.loteId}.csv`;
        link.click();
        window.URL.revokeObjectURL(objectUrl);
        this.baixandoErros = false;
      },
      error: (erro) => {
        console.error('[IMPORTACAO-COLABORADORES] Erro ao baixar erros', erro);
        this.baixandoErros = false;
        this.util.exibirMensagemToast('Não foi possível baixar o arquivo de erros.', 4000);
      }
    });
  }

  limparSelecao(): void {
    this.loteSelecionado = null;
    this.resumoSelecionado = null;
    this.detalhesSelecionados = [];
    this.filtroDetalhes = '';
  }

  formatarData(data: any): string {
    if (!data) {
      return '-';
    }
    const parsed = new Date(data);
    if (isNaN(parsed.getTime())) {
      return '-';
    }
    return parsed.toLocaleString('pt-BR');
  }

  getStatusClasse(status: string): string {
    const mapa: Record<string, string> = {
      'CONCLUIDO': 'status-concluido',
      'PROCESSANDO': 'status-processando',
      'VALIDADO': 'status-validado',
      'PENDENTE_CORRECAO': 'status-processando',
      'ERRO': 'status-erro',
      'CANCELADO': 'status-cancelado'
    };
    return mapa[status] || 'status-cancelado';
  }

  getStatusDescricao(status: string, descricao?: string): string {
    const mapa: Record<string, string> = {
      'CONCLUIDO': 'Concluído',
      'PROCESSANDO': 'Processando',
      'VALIDADO': 'Validado',
      'PENDENTE_CORRECAO': 'Pendente de correção',
      'ERRO': 'Erro',
      'CANCELADO': 'Cancelado'
    };

    if (descricao && descricao !== 'Desconhecido') {
      return descricao;
    }

    if (status && mapa[status]) {
      return mapa[status];
    }

    return status || 'N/A';
  }

  podeConfirmar(status: string | undefined | null): boolean {
    if (!status) {
      return false;
    }

    const statusUpper = (status || '').toUpperCase().trim();
    
    // Não pode confirmar se estiver cancelado, concluído ou com erro
    if (['CANCELADO', 'CONCLUIDO', 'ERRO'].includes(statusUpper)) {
      return false;
    }

    return ['VALIDADO', 'PROCESSANDO'].includes(statusUpper);
  }

  podeCancelar(status: string | undefined | null): boolean {
    if (!status) {
      return false;
    }

    // Não pode cancelar se já estiver cancelado, concluído ou com erro
    return !['CANCELADO', 'CONCLUIDO', 'ERRO'].includes(status);
  }

  private normalizarStatusHistorico(item: HistoricoImportacaoColaboradores): HistoricoImportacaoColaboradores {
    // CRÍTICO: Preservar status CANCELADO e ERRO SEMPRE, antes de qualquer outra verificação
    const statusUpper = (item.status || '').toUpperCase().trim();

    if (statusUpper === 'CANCELADO') {
      return {
        ...item,
        status: 'CANCELADO',
        statusDescricao: 'Cancelado'
      };
    }

    if (statusUpper === 'ERRO') {
      return {
        ...item,
        status: 'ERRO',
        statusDescricao: 'Erro'
      };
    }

    const possuiDataFim = !!item.dataFim;
    const descricaoUpper = (item.statusDescricao || '').toUpperCase();
    const concluiuBackend = statusUpper === 'CONCLUIDO' || descricaoUpper.includes('CONCLU');
    const totalImportadosInformado = item.totalImportados !== null && item.totalImportados !== undefined;

    if (concluiuBackend || possuiDataFim || totalImportadosInformado) {
      return {
        ...item,
        status: 'CONCLUIDO',
        statusDescricao: 'Concluído'
      };
    }

    if (item.observacoes?.toLowerCase()?.includes('cancelada')) {
      return {
        ...item,
        status: 'CANCELADO',
        statusDescricao: 'Cancelado'
      };
    }

    return item;
  }

  irParaImportar(): void {
    this.abrirModalImportacao();
  }

  irParaExportar(): void {
    this.abrirModalExportar();
  }

  abrirModalImportacao(): void {
    const dialogRef = this.dialog.open(ModalImportarColaboradoresComponent, {
      width: '90%',
      maxWidth: '900px',
      disableClose: true,
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.sucesso) {
        window.location.reload();
      }
    });
  }

  abrirModalExportar(): void {
    this.dialog.open(ModalExportarColaboradoresComponent, {
      width: '600px',
      disableClose: false,
      data: {
        cliente: this.cliente,
        session: this.session
      }
    });
  }

  private iniciarPollingHistorico(loteId: string, tentativasMaximas = 10, intervaloMs = 3000): void {
    this.finalizarPolling(loteId);

    const executar = () => {
      this.carregarHistorico(() => {
        const lote = this.historico.find(item => item.loteId === loteId);
        const concluido = lote?.status === 'CONCLUIDO';
        const registro = this.pollingHistorico[loteId];

        if (!registro) {
          return;
        }

        registro.tentativaAtual++;

        if (concluido || registro.tentativaAtual >= tentativasMaximas) {
          this.finalizarPolling(loteId);
          return;
        }

        registro.timeoutId = setTimeout(executar, intervaloMs);
      });
    };

    this.pollingHistorico[loteId] = {
      tentativaAtual: 0,
      timeoutId: setTimeout(executar, intervaloMs)
    };
  }

  private atualizarEstadoPosConclusao(dados: { loteId: string; resultado?: ResultadoImportacaoColaboradores }): void {
    if (!dados?.loteId) {
      return;
    }

    const { loteId, resultado } = dados;
    const totalImportados = resultado
      ? (resultado.colaboradoresCriados + resultado.colaboradoresAtualizados)
      : undefined;
    const dataFim = resultado?.dataFim ? new Date(resultado.dataFim) : new Date();

    let loteAtualizado = false;

    this.historico = (this.historico || []).map(item => {
      if (item.loteId !== loteId) {
        return item;
      }

      loteAtualizado = true;

      return {
        ...item,
        status: 'CONCLUIDO',
        statusDescricao: 'Concluído',
        dataFim,
        totalImportados: totalImportados ?? item.totalImportados ?? 0,
        totalValidados: totalImportados ?? item.totalValidados,
        observacoes: resultado?.mensagem ?? item.observacoes
      };
    });

    if (!loteAtualizado && resultado) {
      const novoHistorico: HistoricoImportacaoColaboradores = {
        id: 0,
        loteId,
        tipoImportacao: 'COLABORADORES',
        nomeArquivo: resultado.loteId || 'Importação de colaboradores',
        dataInicio: resultado.dataInicio ? new Date(resultado.dataInicio) : new Date(),
        dataFim,
        status: 'CONCLUIDO',
        statusDescricao: 'Concluído',
        totalRegistros: resultado.totalProcessado,
        totalValidados: resultado.totalProcessado,
        totalErros: 0,
        totalImportados: totalImportados ?? resultado.totalProcessado,
        usuarioNome: this.session?.usuario?.nome || 'Você',
        usuarioEmail: this.session?.usuario?.email || '',
        observacoes: resultado.mensagem
      };

      this.historico = [novoHistorico, ...this.historico];
    }

    this.pendentes = (this.historico || []).filter(item => item.status === 'VALIDADO');

    if (this.loteSelecionado?.loteId === loteId) {
      this.loteSelecionado = {
        ...this.loteSelecionado,
        status: 'CONCLUIDO',
        statusDescricao: 'Concluído',
        dataFim,
        totalValidados: totalImportados ?? this.loteSelecionado.totalValidados,
        totalImportados: totalImportados ?? this.loteSelecionado.totalImportados
      };
    }
  }

  private finalizarPolling(loteId: string): void {
    const registro = this.pollingHistorico[loteId];
    if (registro?.timeoutId) {
      clearTimeout(registro.timeoutId);
    }
    delete this.pollingHistorico[loteId];
  }
}