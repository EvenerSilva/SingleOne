import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PatrimonioService } from '../../api/patrimonio/patrimonio.service';
import { PassCheckService } from '../../api/passcheck/passcheck.service';
import { SinalizacaoService } from '../../api/sinalizacao/sinalizacao.service';
import { UtilService } from '../../util/util.service';
import { environment } from '../../../environments/environment';

// Interface para equipamento do patrimônio
interface EquipamentoPatrimonio {
  id: number;
  patrimonio: string;
  numeroSerie: string;
  tipoEquipamento: string;
  fabricante: string;
  modelo: string;
  status: string;
  dtEntrega: string;
  dtDevolucao?: string;
  observacao: string;
  tipoAquisicao: string;
  podeContestar: boolean;
  temContestacao: boolean;
  assinado?: boolean;
  dataAssinatura?: Date;
  hashRequisicao?: string;
  isByod?: boolean;
}

@Component({
  selector: 'app-portaria',
  templateUrl: './portaria.component.html',
  styleUrls: ['./portaria.component.scss']
})
export class PortariaComponent implements OnInit {
  // Formulários
  passCheckForm: FormGroup;
  sinalizacaoForm: FormGroup;
  
  // Estados da aplicação
  loading = false;
  showSinalizacaoModal = false;
  
  // Dados do PassCheck
  passCheckResult: any = null;
  passCheckError: string = '';
  
  // Dados para sinalizações
  motivosSuspeita: any[] = [];
  
  // Controle de expansão das seções
  expandedSections: { [key: string]: boolean } = {
    emPosse: true,        // Por padrão, expandido no PassCheck
    byod: true,           // BYOD expandido por padrão
    transitoLivre: true,  // Trânsito livre expandido por padrão
    requerAutorizacao: true, // Recursos que requerem autorização expandidos por padrão
    historico: false      // Histórico colapsado por padrão
  };
  
  // Controle de expansão dos detalhes do colaborador
  colaboradorDetailsExpanded = false;

constructor(
    private fb: FormBuilder,
    private patrimonioService: PatrimonioService,
    private passCheckService: PassCheckService,
    private sinalizacaoService: SinalizacaoService,
    private util: UtilService,
    private cdr: ChangeDetectorRef
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.carregarMotivosSuspeita();
  }

  private initializeForms(): void {
    // Formulário do PassCheck
    this.passCheckForm = this.fb.group({
      cpf: ['', [Validators.required, Validators.minLength(11)]]
    });

    // Formulário de sinalização
    this.sinalizacaoForm = this.fb.group({
      motivoSuspeita: ['', Validators.required],
      prioridade: ['media', Validators.required],
      descricaoDetalhada: ['', Validators.required],
      nomeVigilante: ['', Validators.required],
      observacoesVigilante: ['']
    });
  }

  // =====================================================
  // MÉTODOS DO PASSCHECK
  // =====================================================
  
  /**
   * Limpa o campo CPF
   */
  limparCpf(): void {
    this.passCheckForm.patchValue({ cpf: '' });
    this.passCheckForm.get('cpf')?.markAsUntouched();
  }

  /**
   * Formata data para exibição
   */
  formatarData(date: string | Date): string {
    if (!date) return 'Não informado';
    
    try {
      // Forçar para string quando necessário
      const input = typeof date === 'string' ? date : (date as Date).toString();

      // Caso específico: "08T00:00:00/08/2025" ou similares
      if (input.includes('T') && input.includes('/')) {
        // Ex.: "08T00:00:00/08/2025" => dayPart = "08T00:00:00"
        const partsSlash = input.split('/');
        if (partsSlash.length === 3) {
          const dayPart = partsSlash[0];
          const month = partsSlash[1];
          const year = partsSlash[2];
          const day = dayPart.split('T')[0];
          const dd = day.padStart(2, '0');
          const mm = month.padStart(2, '0');
          return `${dd}/${mm}/${year}`;
        }

        // Fallback com regex caso venha com variações
        const m = input.match(/(\d{1,2})T.*?\/(\d{1,2})\/(\d{4})/);
        if (m) {
          const dd = m[1].padStart(2, '0');
          const mm = m[2].padStart(2, '0');
          const yy = m[3];
          return `${dd}/${mm}/${yy}`;
        }
      }

      // Formato ISO simples yyyy-MM-dd
      if (typeof date === 'string' && input.includes('-')) {
        const iso = input.split('T')[0]; // corta tempo se houver
        const isoParts = iso.split('-');
        if (isoParts.length === 3) {
          const [year, month, day] = isoParts;
          const dd = day.padStart(2, '0');
          const mm = month.padStart(2, '0');
          return `${dd}/${mm}/${year}`;
        }
      }

      // Tentar converter via Date
      const dateObj = new Date(input);
      if (!isNaN(dateObj.getTime())) {
        return dateObj.toLocaleDateString('pt-BR');
      }

      return 'Data inválida';
    } catch {
      return 'Data inválida';
    }
  }

  async consultarPassCheck(): Promise<void> {
    if (this.passCheckForm.invalid) {
      this.markFormGroupTouched(this.passCheckForm);
      return;
    }

    this.loading = true;
    this.passCheckError = '';
    this.passCheckResult = null;

    try {
      const cpf = this.passCheckForm.get('cpf')?.value;
      const result = await this.passCheckService.consultarColaborador(cpf).toPromise();
      
      if (result?.sucesso) {
        this.passCheckResult = result;

        // Buscar dados consolidados do Meu Patrimônio para alinhar status de assinatura
        let equipamentosPatrimonio: any[] = [];
        try {
          const colaboradorId = result?.colaborador?.id;
          if (colaboradorId) {
            const patr = await this.patrimonioService.obterMeuPatrimonio(colaboradorId).toPromise();
            if (patr?.sucesso && Array.isArray(patr.equipamentos)) {
              equipamentosPatrimonio = patr.equipamentos;
            }
          }
        } catch (e) {
          console.warn('[PORTARIA] Falha ao obter Meu Patrimônio para alinhar status de assinatura', e);
        }

        // Index para match rápido por patrimonio/numeroSerie
        const idxPorPatrimonio = new Map<string, any>();
        const idxPorSerie = new Map<string, any>();
        equipamentosPatrimonio.forEach((eq: any) => {
          if (eq?.patrimonio) idxPorPatrimonio.set(String(eq.patrimonio), eq);
          if (eq?.numeroSerie) idxPorSerie.set(String(eq.numeroSerie), eq);
        });

        // Mapear equipamentos para recursos para compatibilidade com o template,
        // preenchendo "assinado" e "isByod" de acordo com o Meu Patrimônio
        this.passCheckResult.recursos = (result.equipamentos || []).map((e: any) => {
          const keyPat = e?.patrimonio ? String(e.patrimonio) : '';
          const keySn = e?.numeroSerie ? String(e.numeroSerie) : '';
          const match = (keyPat && idxPorPatrimonio.get(keyPat)) || (keySn && idxPorSerie.get(keySn)) || null;

          const isByodInferido = (e.tipoAquisicao || '').toLowerCase().includes('próprio') || (e.isByod === true);
          const isByod = match?.isByod !== undefined ? match.isByod : isByodInferido;

          const assinado = match?.assinado === true
            || e.assinado === true
            || match?.dataAssinatura != null
            || match?.dtAssinatura != null
            || e.dataAssinatura != null
            || e.dtAssinatura != null;

          const hashRequisicao = match?.hashRequisicao || e.hashRequisicao;

          return {
            ...e,
            isByod,
            assinado,
            hashRequisicao
          };
        });
      } else {
        this.passCheckError = result?.mensagem || 'Erro na consulta';
      }
    } catch (error: any) {
      console.error('Erro na consulta PassCheck:', error);
      this.passCheckError = error.error?.mensagem || 'Erro ao consultar colaborador';
    } finally {
      this.loading = false;
    }
  }

  // =====================================================
  // MÉTODOS DE UTILIDADE
  // =====================================================
  /**
   * Recursos corporativos (não BYOD) com pendência de assinatura
   */
  getRecursosComPendenciaAssinatura(): any[] {
    const recursos = this.passCheckResult?.recursos || [];
    return recursos.filter((recurso: any) => {
      const naoEhByod = !recurso.isByod;
      const ativoEmPosse = recurso.status === 'Entregue' || recurso.status === 'Ativo';
      const naoAssinado = recurso.assinado !== true;
      return naoEhByod && ativoEmPosse && naoAssinado;
    });
  }

  /**
   * Recursos BYOD com pendência de assinatura
   */
  getRecursosByodComPendenciaAssinatura(): any[] {
    const recursos = this.passCheckResult?.recursos || [];
    return recursos.filter((recurso: any) => {
      const ehByod = recurso.isByod === true;
      const ativoEmPosse = recurso.status === 'Entregue' || recurso.status === 'Ativo';
      const naoAssinado = recurso.assinado !== true;
      return ehByod && ativoEmPosse && naoAssinado;
    });
  }

  /**
   * Existe alguma pendência de assinatura?
   */
  temPendenciasAssinatura(): boolean {
    return this.getRecursosComPendenciaAssinatura().length > 0 || this.getRecursosByodComPendenciaAssinatura().length > 0;
  }

  /**
   * Total de recursos com pendência
   */
  getTotalPendenciasAssinatura(): number {
    return this.getRecursosComPendenciaAssinatura().length + this.getRecursosByodComPendenciaAssinatura().length;
  }
  
  private clearResults(): void {
    this.passCheckResult = null;
    this.passCheckError = '';
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  private getFormErrors(formGroup: FormGroup): any {
    const errors: any = {};
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control && control.errors) {
        errors[key] = control.errors;
      }
    });
    return errors;
  }

  formatCpf(cpf: string): string {
    if (!cpf) return '';
    
    try {
      // ✅ CORREÇÃO: Descriptografar o CPF do Base64 primeiro
      const cpfDescriptografado = atob(cpf);
      
      // Remover caracteres não numéricos e formatar
      const cpfLimpo = cpfDescriptografado.replace(/\D/g, '');
      
      // Aplicar máscara de CPF
      return cpfLimpo.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } catch (error) {
      console.error('Erro ao descriptografar CPF:', error);
      // Fallback: tentar formatar diretamente se não for Base64
      const cpfLimpo = cpf.replace(/\D/g, '');
      if (cpfLimpo.length === 11) {
        return cpfLimpo.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
      }
      return cpf; // Retornar original se não conseguir formatar
    }
  }

getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'liberado': return 'success';
      case 'pendências': return 'warning';
      case 'pendente': return 'warning';
      case 'aprovada': return 'success';
      case 'rejeitada': return 'danger';
      default: return 'secondary';
    }
  }

  limparResultado(): void {
    this.passCheckResult = null;
    this.passCheckError = '';
    this.passCheckForm.reset();
  }

  // ✅ NOVOS MÉTODOS: Estilo da tela de entregas e devoluções
  getStatusClass(resultado: any): string {
    if (!resultado) return 'unknown';
    
    // Para PassCheck (tem statusLiberacao)
    if (resultado.statusLiberacao) {
      if (resultado.statusLiberacao === 'Liberado') {
        return 'liberado';
      } else if (resultado.statusLiberacao === 'Pendências') {
        return 'pendente';
      }
    }
    
    return 'unknown';
  }

  getStatusIcon(resultado: any): string {
    if (!resultado) return 'help';
    
    // Para PassCheck (tem statusLiberacao)
    if (resultado.statusLiberacao) {
      if (resultado.statusLiberacao === 'Liberado') {
        return 'check_circle';
      } else if (resultado.statusLiberacao === 'Pendências') {
        return 'warning';
      }
    }
    
    return 'help';
  }

  getStatusText(resultado: any): string {
    if (!resultado) return 'Desconhecido';
    
    // Para PassCheck (tem statusLiberacao)
    if (resultado.statusLiberacao) {
      return resultado.statusLiberacao;
    }
    
    return 'Desconhecido';
  }

  // =====================================================
  // MÉTODOS DE SINALIZAÇÃO DE SUSPEITAS
  // =====================================================

  /**
   * Carrega os motivos de suspeita disponíveis
   */
  async carregarMotivosSuspeita(): Promise<void> {
    try {
      const motivos = await this.sinalizacaoService.obterMotivosSuspeita().toPromise();
      
      if (motivos) {
        this.motivosSuspeita = motivos;
      } else {
        // Fallback com dados mockados se a API falhar
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
    } catch (error: any) {
      console.error('[SINALIZACAO] Erro ao carregar motivos:', error);
      
      // Fallback com dados mockados em caso de erro
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

  /**
   * Abre o modal de sinalização de suspeita
   */
  abrirModalSinalizacao(): void {
    if (!this.passCheckResult?.colaborador) {
      this.util.exibirMensagemPopUp('Nenhum colaborador consultado para sinalizar', false);
      return;
    }

    // Carregar motivos de suspeita
    this.carregarMotivosSuspeita();

    // Resetar formulário
    this.sinalizacaoForm.reset({
      prioridade: 'media'
    });
    
    this.showSinalizacaoModal = true;
    this.cdr.detectChanges();
  }

  /**
   * Fecha o modal de sinalização
   */
  fecharModalSinalizacao(): void {
    this.showSinalizacaoModal = false;
    this.sinalizacaoForm.reset();
  }

/**
   * Cria uma nova sinalização de suspeita
   */
  async criarSinalizacao(): Promise<void> {
    if (this.sinalizacaoForm.invalid) {
      this.markFormGroupTouched(this.sinalizacaoForm);
      this.util.exibirMensagemPopUp('Por favor, preencha todos os campos obrigatórios', false);
      return;
    }

    if (!this.passCheckResult?.colaborador) {
      this.util.exibirMensagemPopUp('Nenhum colaborador consultado', false);
      return;
    }

    this.loading = true;

    try {
      const formValue = this.sinalizacaoForm.value;
      
      // Preparar dados da sinalização
      const sinalizacaoData = {
        colaboradorId: this.passCheckResult.colaborador.id,
        cpfConsultado: this.passCheckResult.colaborador.cpf,
        motivoSuspeita: formValue.motivoSuspeita,
        prioridade: formValue.prioridade,
        descricaoDetalhada: formValue.descricaoDetalhada,
        nomeVigilante: formValue.nomeVigilante,
        observacoesVigilante: formValue.observacoesVigilante || '',
        dadosConsulta: JSON.stringify({
          colaborador: this.passCheckResult.colaborador,
          equipamentos: this.passCheckResult.equipamentos,
          statusLiberacao: this.passCheckResult.statusLiberacao,
          motivosPendencia: this.passCheckResult.motivosPendencia
        })
      };
      const resultado = await this.sinalizacaoService.criarSinalizacao(sinalizacaoData).toPromise();
      
      if (resultado?.sucesso) {
        this.util.exibirMensagemPopUp(`Suspeita sinalizada com sucesso! Protocolo: ${resultado.numeroProtocolo}`, false);
        this.fecharModalSinalizacao();
      } else {
        console.error('[SINALIZACAO] Erro na resposta da API:', resultado);
        this.util.exibirMensagemPopUp('Erro ao sinalizar suspeita: ' + (resultado?.mensagem || 'Erro desconhecido'), false);
      }
      
    } catch (error: any) {
      console.error('[SINALIZACAO] Erro ao criar sinalização:', error);
      this.util.exibirMensagemPopUp('Erro ao sinalizar suspeita: ' + (error?.error?.mensagem || error?.message || 'Erro desconhecido'), false);
    } finally {
      this.loading = false;
    }
  }

  /**
   * Ver detalhes completos do colaborador
   */
  verDetalhesCompletos(): void {
    if (!this.passCheckResult?.colaborador) {
      this.util.exibirMensagemPopUp('Nenhum colaborador consultado', false);
      return;
    }

    // Aqui você pode implementar uma funcionalidade para mostrar mais detalhes
    // Por exemplo, abrir um modal com informações mais completas
    const detalhes = `
      Nome: ${this.passCheckResult.colaborador.nome}
      CPF: ${this.formatCpf(this.passCheckResult.colaborador.cpf)}
      Matrícula: ${this.passCheckResult.colaborador.matricula}
      Cargo: ${this.passCheckResult.colaborador.cargo}
      Setor: ${this.passCheckResult.colaborador.setor}
      Status: ${this.getStatusText(this.passCheckResult)}
      Equipamentos: ${this.passCheckResult.equipamentos?.length || 0}
    `;
    
    this.util.exibirMensagemPopUp(detalhes, false);
  }

  // =====================================================
  // MÉTODOS PARA CONTROLAR EXPANSÃO DAS SEÇÕES
  // =====================================================
  
  toggleSection(section: string): void {
    this.expandedSections[section] = !this.expandedSections[section];
  }

  isSectionExpanded(section: string): boolean {
    return this.expandedSections[section] !== false;
  }

  // =====================================================
  // MÉTODOS PARA VALIDAÇÃO DE DEMISSÃO
  // =====================================================
  
  formatDate(dateString: string): string {
    if (!dateString) return '';
    
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('pt-BR');
    } catch (error) {
      return dateString;
    }
  }

  isDataDemissaoHojeOuPassou(dateString: string): boolean {
    if (!dateString) return false;
    
    try {
      const dataDemissao = new Date(dateString);
      const hoje = new Date();
      
      // Comparar apenas as datas (sem hora)
      dataDemissao.setHours(0, 0, 0, 0);
      hoje.setHours(0, 0, 0, 0);
      
      return dataDemissao <= hoje;
    } catch (error) {
      return false;
    }
  }

  // =====================================================
  // MÉTODO PARA ORIENTAÇÕES DO VIGILANTE
  // =====================================================
  
  getOrientacaoVigilante(): string {
    if (!this.passCheckResult?.colaborador) {
      return '<span class="orientacao-erro">❌ Erro ao carregar dados do colaborador</span>';
    }

    const colaborador = this.passCheckResult.colaborador;
    const qtdEmPosse = this.getRecursosEmPosse().length;
    const qtdHistorico = this.getRecursosHistoricos().length;
    const qtdByod = this.getRecursosParticulares().length;
    const temRecursos = (qtdEmPosse + qtdHistorico + qtdByod) > 0;
    const temDataDemissao = colaborador.dtDemissao && this.isDataDemissaoHojeOuPassou(colaborador.dtDemissao);
    const situacaoAtiva = colaborador.situacao === 'A' || colaborador.situacao === 'Ativo';

    // Cabeçalho comum de contadores
    const resumoContadores = `
      <div class="orientacao-contadores">
        <span><strong>${qtdEmPosse}</strong> Em Posse</span>
        <span><strong>${qtdHistorico}</strong> Histórico</span>
        <span><strong>${qtdByod}</strong> BYOD</span>
      </div>
    `;

    // Cenário 1: Colaborador desligado ou com data de demissão
    if (temDataDemissao || !situacaoAtiva) {
      return `
        ${resumoContadores}
        <div class="orientacao-critica">
          <span class="orientacao-titulo">🚨 AÇÃO IMEDIATA REQUERIDA 🚨</span>
          <br><br>
          <strong>RECOLHA TODOS OS RECURSOS!</strong><br>
          <span class="orientacao-detalhe">
            ${temDataDemissao ? `• Data de demissão: ${this.formatDate(colaborador.dtDemissao)}` : ''}
            ${!situacaoAtiva ? `• Situação: ${colaborador.situacao}` : ''}
            <br>• <strong>NÃO DEIXE O COLABORADOR SAIR COM RECURSOS</strong>
            <br>• Encaminhe para RH ou TI para devolução
          </span>
        </div>
      `;
    }

    // Cenário 2: Colaborador ativo sem recursos
    if (!temRecursos) {
      return `
        ${resumoContadores}
        <div class="orientacao-liberado">
          <span class="orientacao-titulo">✅ LIBERADO</span>
          <br><br>
          <strong>Agradeça e libere o colaborador</strong><br>
          <span class="orientacao-detalhe">
            • Colaborador ativo sem recursos em posse
            <br>• Pode sair normalmente
          </span>
        </div>
      `;
    }

    // Cenário 3: Colaborador ativo com recursos - verificar assinatura
    const temPendenciaAssinatura = this.passCheckResult.motivosPendencia && 
      this.passCheckResult.motivosPendencia.some((motivo: string) => motivo.toLowerCase().includes('assinado') || motivo.toLowerCase().includes('assinatura'));

    if (temPendenciaAssinatura) {
      // Sem flag por recurso, considerar apenas os recursos EM POSSE como sem assinatura
      const recursosSemAssinaturaEmPosse = qtdEmPosse;
      return `
        ${resumoContadores}
        <div class="orientacao-atencao">
          <span class="orientacao-titulo">⚠️ PENDÊNCIA DE ASSINATURA</span>
          <br><br>
          <strong>Sugira assinar o termo de compromisso</strong><br>
          <span class="orientacao-detalhe">
            • Colaborador tem ${recursosSemAssinaturaEmPosse} recurso(s) em posse sem termo assinado
            <br>• Oriente para assinar o termo antes de sair
            <br>• Se recusar, use \"Sinalizar Suspeita\" para recolhimento
          </span>
        </div>
      `;
    }

    // Cenário 4: Colaborador ativo com recursos - verificar classificação de trânsito
    const recursosTransitoLivre = this.getRecursosTransitoLivre();
    const recursosRequerAutorizacao = this.getRecursosRequerAutorizacao();
    
    if (recursosRequerAutorizacao.length > 0) {
      const recursosLista = recursosRequerAutorizacao.map(r => r.tipoEquipamento).join(', ');
      
      return `
        ${resumoContadores}
        <div class="orientacao-atencao">
          <span class="orientacao-titulo">⚠️ VALIDAR RECURSOS POR CATEGORIA</span>
          <br><br>
          <strong>Recursos classificados por permissão de saída:</strong><br>
          <span class="orientacao-detalhe">
            <strong>✅ PODEM SAIR (${recursosTransitoLivre.length}):</strong> ${recursosTransitoLivre.map(r => r.tipoEquipamento).join(', ')}
            <br><strong>🚫 REQUEREM AUTORIZAÇÃO (${recursosRequerAutorizacao.length}):</strong> ${recursosLista}
            <br><br>
            <strong>INSTRUÇÕES PARA O VIGILANTE:</strong>
            <br>• Se o colaborador estiver saindo com recursos que "PODEM SAIR": ✅ LIBERE
            <br>• Se o colaborador estiver saindo com recursos que "REQUEREM AUTORIZAÇÃO": 🚫 RETENHA
            <br>• Verifique os recursos que o colaborador está carregando
            <br>• Consulte as seções abaixo para identificar cada categoria
          </span>
        </div>
      `;
    }

    // Cenário 5: Colaborador ativo com recursos e termo assinado - tudo liberado
    return `
      ${resumoContadores}
      <div class="orientacao-liberado">
        <span class="orientacao-titulo">✅ LIBERADO</span>
        <br><br>
        <strong>Agradeça e libere o colaborador</strong><br>
        <span class="orientacao-detalhe">
          • Colaborador ativo com ${qtdEmPosse} recurso(s) em posse
          <br>• Termo de compromisso assinado
          <br>• Todos os recursos com trânsito livre ou autorização válida
          <br>• Pode sair normalmente
        </span>
      </div>
    `;
  }

temRecursosSemTransitoLivre(): boolean {
    if (!this.passCheckResult?.recursos) return false;
    
    return this.passCheckResult.recursos.some(recurso => !recurso.tipoEquipamentoTransitoLivre);
  }
  
  getRecursosSemTransitoLivre(): any[] {
    if (!this.passCheckResult?.recursos) return [];
    
    return this.passCheckResult.recursos.filter(recurso => !recurso.tipoEquipamentoTransitoLivre);
  }

  // ✅ NOVO: Método para obter recursos com trânsito livre - APENAS ATIVOS
  getRecursosTransitoLivre(): any[] {
    if (!this.passCheckResult?.recursos) return [];
    
    return this.passCheckResult.recursos.filter(recurso => 
      recurso.tipoEquipamentoTransitoLivre && !recurso.isHistorico && !recurso.isRecursoParticular
    );
  }

  // ✅ NOVO: Método para obter recursos que requerem autorização - APENAS ATIVOS
  getRecursosRequerAutorizacao(): any[] {
    if (!this.passCheckResult?.recursos) return [];
    
    return this.passCheckResult.recursos.filter(recurso => 
      !recurso.tipoEquipamentoTransitoLivre && !recurso.isHistorico && !recurso.isRecursoParticular
    );
  }

  // ✅ NOVO: Método para obter recursos particulares (BYOD) - APENAS ATIVOS
  getRecursosParticulares(): any[] {
    if (!this.passCheckResult?.recursos) return [];
    
    return this.passCheckResult.recursos.filter(recurso => 
      recurso.isRecursoParticular && !recurso.isHistorico
    );
  }

  // ✅ NOVO: Método para obter recursos históricos
  getRecursosHistoricos(): any[] {
    if (!this.passCheckResult?.recursos) return [];
    
    return this.passCheckResult.recursos.filter(recurso => recurso.isHistorico);
  }

  // Recursos em posse (ativos com o colaborador)
  getRecursosEmPosse(): any[] {
    if (!this.passCheckResult?.recursos) return [];
    return this.passCheckResult.recursos.filter(recurso => !recurso.isHistorico);
  }

  // ✅ NOVO: Método para toggle dos detalhes do colaborador
  toggleColaboradorDetails(): void {
    this.colaboradorDetailsExpanded = !this.colaboradorDetailsExpanded;
  }

// ✅ NOVO: Método para extrair fabricante e modelo do tipoEquipamento
  getFabricanteModelo(tipoEquipamento: string): { fabricante: string, modelo: string } {
    if (!tipoEquipamento) return { fabricante: 'Não informado', modelo: 'Não informado' };
    
    // Para linhas telefônicas
    if (tipoEquipamento.toLowerCase().includes('linha telefônica') || tipoEquipamento.toLowerCase().includes('telefônica')) {
      return { fabricante: 'Linha Telefônica', modelo: 'Telefone' };
    }
    
    // Para notebooks e outros equipamentos
    const partes = tipoEquipamento.split(' ');
    
    if (partes.length >= 3 && partes[0].toLowerCase() === 'notebook') {
      // Formato: "Notebook Lenovo E14" ou "Notebook Dell Inspiron 5050"
      const fabricante = partes[1];
      const modelo = partes.slice(2).join(' ');
      return { fabricante, modelo };
    }
    
    // Para outros tipos de equipamentos
    if (partes.length >= 2) {
      const fabricante = partes[0];
      const modelo = partes.slice(1).join(' ');
      return { fabricante, modelo };
    }
    
    // Se não conseguir extrair, retorna o tipo completo
    return { fabricante: tipoEquipamento, modelo: 'Não especificado' };
  }

  // ✅ NOVO: Método para determinar status final baseado apenas em trânsito livre
  getStatusFinalLiberacao(): 'liberado' | 'bloqueado' | 'pendente' {
    if (!this.passCheckResult?.colaborador) {
      return 'pendente';
    }

    const colaborador = this.passCheckResult.colaborador;
    const temDataDemissao = colaborador.dtDemissao && this.isDataDemissaoHojeOuPassou(colaborador.dtDemissao);
    const situacaoAtiva = colaborador.situacao === 'A' || colaborador.situacao === 'Ativo';

    // Se colaborador desligado ou com data de demissão
    if (temDataDemissao || !situacaoAtiva) {
      return 'bloqueado';
    }

    // Se não tem recursos, está liberado
    if (!this.passCheckResult.recursos || this.passCheckResult.recursos.length === 0) {
      return 'liberado';
    }

    // Se tem recursos sem trânsito livre, precisa validar
    const recursosSemTransitoLivre = this.getRecursosSemTransitoLivre();
    if (recursosSemTransitoLivre.length > 0) {
      return 'pendente';
    }

    // Se todos os recursos têm trânsito livre, está liberado
    return 'liberado';
  }

  getColaboradorStatus(): 'Ativo' | 'Desligado' {
    const dt = this.passCheckResult?.colaborador?.dtDemissao;
    if (dt && this.isDataDemissaoHojeOuPassou(dt)) {
      return 'Desligado';
    }
    return 'Ativo';
  }

}
