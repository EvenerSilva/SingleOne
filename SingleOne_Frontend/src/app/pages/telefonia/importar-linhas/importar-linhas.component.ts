import { Component, OnInit } from '@angular/core';
import { ImportacaoLinhasService, ResultadoValidacao, DetalheLinhaStagingDTO, ResumoValidacao, ResultadoImportacao, HistoricoImportacao } from 'src/app/services/importacao-linhas.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-importar-linhas',
  templateUrl: './importar-linhas.component.html',
  styleUrls: ['./importar-linhas.component.scss']
})
export class ImportarLinhasComponent implements OnInit {
  // Controle de passos
  passoAtual: number = 1;
  
  // Arquivo
  arquivoSelecionado: File | null = null;
  nomeArquivo: string = '';
  
  // Validação
  loteAtual: string | null = null;
  resultadoValidacao: ResultadoValidacao | null = null;
  resumoValidacao: ResumoValidacao | null = null;
  detalhesValidacao: DetalheLinhaStagingDTO[] = [];
  filtroStatus: string = '';
  
  // Importação
  resultadoImportacao: ResultadoImportacao | null = null;
  
  // Histórico
  historico: HistoricoImportacao[] = [];
  
  // Loading states
  uploadando: boolean = false;
  validando: boolean = false;
  importando: boolean = false;
  carregandoDetalhes: boolean = false;
  carregandoHistorico: boolean = false;
  
  // Modal de detalhes
  modalDetalhesAberto: boolean = false;

  constructor(
    private importacaoService: ImportacaoLinhasService,
    private util: UtilService
  ) { }

  ngOnInit(): void {
    // Verificar se há token no localStorage
    const token = localStorage.getItem('token');
    if (!token) {
      console.warn('[IMPORTAR-LINHAS] Sem token! Tentando obter da sessão...');
      const session = this.util.getSession('usuario');
      if (session && session.token) {
        localStorage.setItem('token', session.token);
      } else {
        console.error('[IMPORTAR-LINHAS] Sem token na sessão também!');
      }
    }
    
    this.carregarHistorico();
  }

  /**
   * Quando arquivo é selecionado
   */
  onArquivoSelecionado(event: any): void {
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

    this.arquivoSelecionado = arquivo;
    this.nomeArquivo = arquivo.name;
    
    // Fazer upload automaticamente
    this.fazerUpload();
  }

  /**
   * Upload e validação do arquivo
   */
  fazerUpload(): void {
    if (!this.arquivoSelecionado) {
      this.util.exibirMensagemToast('Selecione um arquivo primeiro', 3000);
      return;
    }

    this.uploadando = true;
    this.passoAtual = 2;

    this.importacaoService.uploadArquivo(this.arquivoSelecionado).subscribe({
      next: (resultado) => {
        this.resultadoValidacao = resultado;
        this.loteAtual = resultado.loteId;
        this.uploadando = false;
        
        if (resultado.podeImportar) {
          this.util.exibirMensagemToast('✅ ' + resultado.mensagem, 5000);
        } else {
          this.util.exibirMensagemToast('⚠️ ' + resultado.mensagem, 5000);
        }

        // Carregar resumo detalhado
        this.carregarResumo();
      },
      error: (erro) => {
        this.uploadando = false;
        this.passoAtual = 1;
        const mensagem = erro.error?.mensagem || 'Erro ao processar arquivo';
        this.util.exibirMensagemToast('❌ ' + mensagem, 5000);
        console.error('Erro no upload:', erro);
      }
    });
  }

  /**
   * Carrega resumo detalhado
   */
  carregarResumo(): void {
    if (!this.loteAtual) return;

    this.importacaoService.obterResumo(this.loteAtual).subscribe({
      next: (resumo) => {
        this.resumoValidacao = resumo;
      },
      error: (erro) => {
        console.error('Erro ao carregar resumo:', erro);
      }
    });
  }

  /**
   * Abre modal com detalhes da validação
   */
  abrirDetalhes(): void {
    if (!this.loteAtual) return;

    this.modalDetalhesAberto = true;
    this.carregandoDetalhes = true;

    this.importacaoService.obterValidacao(this.loteAtual, this.filtroStatus).subscribe({
      next: (detalhes) => {
        this.detalhesValidacao = detalhes;
        this.carregandoDetalhes = false;
      },
      error: (erro) => {
        this.carregandoDetalhes = false;
        this.util.exibirMensagemToast('Erro ao carregar detalhes', 3000);
        console.error('Erro ao carregar detalhes:', erro);
      }
    });
  }

  /**
   * Filtrar detalhes por status
   */
  filtrarDetalhes(status: string): void {
    this.filtroStatus = status;
    this.abrirDetalhes();
  }

  /**
   * Fecha modal de detalhes
   */
  fecharDetalhes(): void {
    this.modalDetalhesAberto = false;
    this.filtroStatus = '';
  }

  /**
   * Confirma a importação
   */
  confirmarImportacao(): void {
    if (!this.loteAtual) return;

    if (!confirm('Confirma a importação? Esta ação criará novos registros no banco de dados.')) {
      return;
    }

    this.importando = true;
    this.passoAtual = 3;

    this.importacaoService.confirmarImportacao(this.loteAtual).subscribe({
      next: (resultado) => {
        this.resultadoImportacao = resultado;
        this.importando = false;
        this.passoAtual = 4;
        
        this.util.exibirMensagemToast('✅ ' + resultado.mensagem, 5000);

        // Recarregar histórico
        this.carregarHistorico();
      },
      error: (erro) => {
        this.importando = false;
        this.passoAtual = 2;
        const mensagem = erro.error?.mensagem || 'Erro ao importar dados';
        this.util.exibirMensagemToast('❌ ' + mensagem, 5000);
        console.error('Erro na importação:', erro);
      }
    });
  }

  /**
   * Cancela a importação
   */
  cancelarImportacao(): void {
    if (!this.loteAtual) {
      this.resetarFormulario();
      return;
    }

    if (!confirm('Deseja cancelar esta importação? Os dados validados serão descartados.')) {
      return;
    }

    this.importacaoService.cancelarImportacao(this.loteAtual).subscribe({
      next: () => {
        this.util.exibirMensagemToast('ℹ️ Importação cancelada', 3000);
        this.resetarFormulario();
        this.carregarHistorico();
      },
      error: (erro) => {
        console.error('Erro ao cancelar:', erro);
        this.resetarFormulario();
      }
    });
  }

  /**
   * Reseta o formulário para nova importação
   */
  resetarFormulario(): void {
    this.passoAtual = 1;
    this.arquivoSelecionado = null;
    this.nomeArquivo = '';
    this.loteAtual = null;
    this.resultadoValidacao = null;
    this.resumoValidacao = null;
    this.detalhesValidacao = [];
    this.resultadoImportacao = null;
    this.filtroStatus = '';
  }

  /**
   * Carrega histórico de importações
   */
  carregarHistorico(): void {
    this.carregandoHistorico = true;

    this.importacaoService.obterHistorico(10).subscribe({
      next: (historico) => {
        this.historico = historico;
        this.carregandoHistorico = false;
      },
      error: (erro) => {
        this.carregandoHistorico = false;
        console.error('Erro ao carregar histórico:', erro);
      }
    });
  }

  /**
   * Download do template
   */
  baixarTemplate(): void {
    const url = this.importacaoService.getUrlTemplate();
    window.open(url, '_blank');
  }

  /**
   * Retorna classe CSS para o status
   */
  getStatusClass(status: string): string {
    const classes: any = {
      'V': 'badge-success',
      'E': 'badge-danger',
      'P': 'badge-warning',
      'I': 'badge-info'
    };
    return classes[status] || 'badge-secondary';
  }

  /**
   * Retorna classe CSS para o status do log
   */
  getStatusLogClass(status: string): string {
    const classes: any = {
      'CONCLUIDO': 'badge-success',
      'ERRO': 'badge-danger',
      'PROCESSANDO': 'badge-warning',
      'CANCELADO': 'badge-secondary'
    };
    return classes[status] || 'badge-secondary';
  }

  /**
   * Retorna ícone para o status
   */
  getStatusIcon(status: string): string {
    const icons: any = {
      'V': 'cil-check-circle',
      'E': 'cil-x-circle',
      'P': 'cil-clock',
      'I': 'cil-cloud-download'
    };
    return icons[status] || 'cil-info';
  }

  /**
   * Formata número de telefone
   */
  formatarTelefone(numero: number): string {
    const numStr = numero.toString();
    if (numStr.length === 11) {
      return `(${numStr.substring(0, 2)}) ${numStr.substring(2, 7)}-${numStr.substring(7)}`;
    } else if (numStr.length === 10) {
      return `(${numStr.substring(0, 2)}) ${numStr.substring(2, 6)}-${numStr.substring(6)}`;
    }
    return numStr;
  }

  /**
   * Formata data
   */
  formatarData(data: any): string {
    if (!data) return '-';
    const d = new Date(data);
    return d.toLocaleString('pt-BR');
  }
}

