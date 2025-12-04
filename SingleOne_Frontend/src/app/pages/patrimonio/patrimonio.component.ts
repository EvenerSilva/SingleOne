import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PatrimonioService } from '../../api/patrimonio/patrimonio.service';
import { UtilService } from '../../util/util.service';
import { environment } from '../../../environments/environment';
import { ActivatedRoute, Router } from '@angular/router';
import { Contestacao, ContestacaoStatus } from '../../models/contestacao.interface';

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
  selector: 'app-patrimonio',
  templateUrl: './patrimonio.component.html',
  styleUrls: ['./patrimonio.component.scss']
})
export class PatrimonioComponent implements OnInit {
  // Formulários
  patrimonioForm: FormGroup;
  contestacaoForm: FormGroup;
  autoInventarioForm: FormGroup;
  
  // Estados da aplicação
  loading = false;
  showContestacaoModal = false;
  showAutoInventarioModal = false;
  
  // Dados do Meu Patrimônio
  patrimonioData: any = null;
  patrimonioError: string = '';
  
  // Estados de autenticação
  isAuthenticated = false;
  userToken: string = '';
  
  // Dados para contestações
  equipamentosParaContestar: any[] = [];
  minhasContestoes: Contestacao[] = [];
  
  // Controle de expansão das seções
  expandedSections = {
    byod: false,
    emPosse: false,
    historico: false,
    transitoLivre: false,
    requerAutorizacao: false
  };
  motivosContestacao = [
    'Não reconheço este recurso',
    'Não recebi este recurso',
    'Recurso está danificado',
    'Recurso não funciona',
    'Outro motivo'
  ];

  // Equipamento selecionado (modal de contestação)
  selectedEquipment: any | null = null;

  // Flag para habilitar novo layout (agora padrão)
  newLayoutEnabled = true;

  // Colapso dos detalhes do colaborador (novo layout)
  colaboradorDetailsExpanded = false;

  constructor(
    private fb: FormBuilder,
    private patrimonioService: PatrimonioService,
    private util: UtilService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    
    // ✅ CORREÇÃO: Sempre forçar logout ao acessar a página
    this.forceLogout();
    
    // Garantir que o formulário seja exibido
    this.patrimonioData = null;
    this.patrimonioError = '';

    this.route.queryParamMap.subscribe(params => {
      const flag = params.get('novoLayout');
      if (flag === '0') {
        this.newLayoutEnabled = false;
      }
    });

    // ✅ NOVO: Escutar quando a aba volta ao foco (após assinar termo em outra aba)
    window.addEventListener('focus', async () => {
      const termoAssinadoFlag = localStorage.getItem('termo_assinado_sucesso');
      if (termoAssinadoFlag && this.isAuthenticated) {
        localStorage.removeItem('termo_assinado_sucesso');
        localStorage.removeItem('termo_assinado_payload');
        
        // Recarregar dados do patrimônio
        await this.loadMeuPatrimonio();
        
        // Forçar detecção de mudanças
        this.cdr.detectChanges();
        
        // Mostrar mensagem de sucesso
        this.util.exibirMensagemToast('✅ Termo assinado com sucesso! Dados atualizados.', 'n');
      }
    });

    // Escutar evento de assinatura concluída vindo de outra aba (localStorage)
    try {
      window.addEventListener('storage', async (event: StorageEvent) => {
        if (event.key === 'termo_assinado_sucesso' && event.newValue) {
          localStorage.removeItem('termo_assinado_sucesso');
          localStorage.removeItem('termo_assinado_payload');
          
          // Recarregar dados do patrimônio se autenticado
          if (this.isAuthenticated) {
            await this.loadMeuPatrimonio();
            // Forçar detecção de mudanças
            this.cdr.detectChanges();
            // Mostrar mensagem de sucesso
            this.util.exibirMensagemToast('✅ Termo assinado com sucesso! Dados atualizados.', 'n');
          }
        }
      });
    } catch (error) {
      console.error('❌ [PATRIMÔNIO] Erro ao configurar listener de storage:', error);
    }

    // ✅ NOVO: Usar BroadcastChannel para comunicação entre abas (mais robusto)
    try {
      const channel = new BroadcastChannel('patrimonio_updates');
      channel.onmessage = async (event) => {
        if (event.data.type === 'termo_assinado' && this.isAuthenticated) {
          await this.loadMeuPatrimonio();
          this.cdr.detectChanges();
          this.util.exibirMensagemToast('✅ Termo assinado com sucesso! Dados atualizados.', 'n');
        }
      };
    } catch (error) {
      console.warn('⚠️ [PATRIMÔNIO] BroadcastChannel não suportado:', error);
    }

    // ✅ NOVO: Escutar postMessage de janelas filhas (quando termo é aberto em nova aba)
    window.addEventListener('message', async (event: MessageEvent) => {
      // Validação de segurança básica
      if (event.data && event.data.type === 'termo_assinado' && this.isAuthenticated) {
        await this.loadMeuPatrimonio();
        this.cdr.detectChanges();
        this.util.exibirMensagemToast('✅ Termo assinado com sucesso! Dados atualizados.', 'n');
      }
    });
  }

  private initializeForms(): void {
    // Formulário do Meu Patrimônio - CPF + Email ou CPF + Matrícula
    this.patrimonioForm = this.fb.group({
      cpf: ['', [Validators.required, Validators.minLength(11)]],
      email: ['', []],
      matricula: [''],
      tipoAutenticacao: ['email', Validators.required] // 'email' ou 'matricula'
    });

    // Formulário de contestação
    this.contestacaoForm = this.fb.group({
      equipamentoId: ['', Validators.required],
      motivo: ['', Validators.required],
      descricao: ['', Validators.required]
    });

    // Formulário de Auto Inventário
    this.autoInventarioForm = this.fb.group({
      numeroSerie: ['', Validators.required],
      observacoes: ['']
    });

    // Validações dinâmicas conforme tipo de autenticação
    const applyTipoValidators = (tipo: 'email' | 'matricula') => {
      const emailCtrl = this.patrimonioForm.get('email');
      const matriculaCtrl = this.patrimonioForm.get('matricula');
      if (tipo === 'email') {
        emailCtrl?.setValidators([Validators.required, Validators.email]);
        matriculaCtrl?.clearValidators();
        matriculaCtrl?.setValue('');
      } else {
        matriculaCtrl?.setValidators([Validators.required, Validators.minLength(3)]);
        emailCtrl?.clearValidators();
        emailCtrl?.setValue('');
      }
      emailCtrl?.updateValueAndValidity({ emitEvent: false });
      matriculaCtrl?.updateValueAndValidity({ emitEvent: false });
    };

    // Aplicar no início e reagir a mudanças
    applyTipoValidators(this.patrimonioForm.get('tipoAutenticacao')?.value);
    this.patrimonioForm.get('tipoAutenticacao')?.valueChanges.subscribe((tipo) => applyTipoValidators(tipo));
  }

  private forceLogout(): void {
    // Limpar todos os dados de autenticação para forçar login
    this.isAuthenticated = false;
    this.userToken = '';
    this.patrimonioData = null;
    this.patrimonioError = '';
    
    // Limpar dados específicos do patrimônio
    localStorage.removeItem('patrimonio_token');
    localStorage.removeItem('patrimonio_data');
    
    // Limpar também dados gerais de autenticação (opcional, para garantir)
    // localStorage.removeItem('token');
    // localStorage.removeItem('usuario');
    
  }

  // ✅ MÉTODO REMOVIDO: checkAuthentication() não é mais necessário
  // pois sempre forçamos logout ao acessar a página

// =====================================================
  // MÉTODOS DO MEU PATRIMÔNIO
  // =====================================================
  
  async autenticarPatrimonio(): Promise<void> {
    // Validar formulário reativo
    if (this.patrimonioForm.invalid) {
      this.markFormGroupTouched(this.patrimonioForm);
      this.patrimonioError = 'Preencha os campos obrigatórios corretamente.';
      return;
    }

    this.loading = true;
    this.patrimonioError = '';

    try {
      const formValue = this.patrimonioForm.value;
      const authData = {
        cpf: formValue.cpf,
        email: formValue.tipoAutenticacao === 'email' ? formValue.email : null,
        matricula: formValue.tipoAutenticacao === 'matricula' ? formValue.matricula : null,
        tipoAutenticacao: formValue.tipoAutenticacao
      };

      const result = await this.patrimonioService.autenticar(authData).toPromise();
      
      if (result?.sucesso) {
        this.isAuthenticated = true;
        this.userToken = result.token;
        // Salvar token específico do patrimônio
        localStorage.setItem('patrimonio_token', result.token);
        
        // Atribuir os dados retornados da autenticação
        this.patrimonioData = result;
        this.equipamentosParaContestar = result.equipamentos.filter((e: any) => e.podeContestar);
        
        // Salvar dados no localStorage para persistência
        localStorage.setItem('patrimonio_data', JSON.stringify(result));
        
        // Carregar contestações do colaborador após autenticação
        await this.carregarMinhasContestoes();
        
      } else {
        // ✅ CORREÇÃO: Não redirecionar, apenas mostrar erro
        this.patrimonioError = result?.mensagem || 'Colaborador não identificado';
        this.isAuthenticated = false;
        this.patrimonioData = null;
      }
    } catch (error: any) {
      console.error('Erro na autenticação:', error);
      // ✅ CORREÇÃO: Não redirecionar, apenas mostrar erro
      this.patrimonioError = error.error?.mensagem || 'Colaborador não identificado';
      this.isAuthenticated = false;
      this.patrimonioData = null;
    } finally {
      this.loading = false;
    }
  }

  async loadMeuPatrimonio(): Promise<void> {
    if (!this.isAuthenticated || !this.patrimonioData?.colaborador?.id) return;

    this.loading = true;
    this.patrimonioError = '';

    try {
      const colaboradorId = this.patrimonioData.colaborador.id;
      const result = await this.patrimonioService.obterMeuPatrimonio(colaboradorId).toPromise();
      
      if (result?.sucesso) {
        this.patrimonioData = result;
        // Após recarregar, reavaliar pendências e fechar avisos se necessário
        this.cdr.detectChanges();
        this.equipamentosParaContestar = result.equipamentos.filter((e: any) => e.podeContestar);
        
        // Carregar contestações do colaborador
        await this.carregarMinhasContestoes();
        
      } else {
        this.patrimonioError = result?.mensagem || 'Erro ao carregar patrimônio';
      }
    } catch (error: any) {
      console.error('Erro ao carregar patrimônio:', error);
      this.patrimonioError = error.error?.mensagem || 'Erro ao carregar patrimônio';
    } finally {
      this.loading = false;
    }
  }

  // =====================================================
  // MÉTODOS DE CONTESTAÇÃO
  // =====================================================
  
  async criarContestacao(): Promise<void> {
    
    if (this.contestacaoForm.invalid) {
      this.markFormGroupTouched(this.contestacaoForm);
      this.util.exibirMensagemPopUp('Por favor, preencha todos os campos obrigatórios (Motivo e Descrição)', false);
      return;
    }

    if (!this.selectedEquipment) {
      console.error('🚀 Nenhum equipamento selecionado');
      return;
    }

    this.loading = true;

    try {
      const contestacaoData = {
        colaboradorId: this.patrimonioData?.colaborador?.id || 0,
        equipamentoId: this.selectedEquipment.id,
        motivo: this.contestacaoForm.value.motivo,
        descricao: this.contestacaoForm.value.descricao
      };

const result = await this.patrimonioService.criarContestacao(contestacaoData).toPromise();
      
      if (result?.sucesso) {
        this.util.exibirMensagemPopUp('Contestação criada com sucesso!', false);
        this.contestacaoForm.reset();
        this.fecharModalContestacao();
        await this.loadMeuPatrimonio(); // Recarregar dados
      } else {
        console.error('Erro ao criar contestação:', result?.mensagem);
        this.util.exibirMensagemPopUp('Erro ao criar contestação: ' + (result?.mensagem || 'Erro desconhecido'), false);
      }
    } catch (error: any) {
      console.error('Erro ao criar contestação:', error);
      this.util.exibirMensagemPopUp('Erro ao criar contestação: ' + error.message, false);
    } finally {
      this.loading = false;
    }
  }

  async cancelarContestacao(equipamento: any): Promise<void> {
    try {
      const mensagem = 'Tem certeza que deseja cancelar esta contestação? Esta ação não pode ser desfeita.';
      const confirmar: any = await this.util.exibirMensagemPopUp(mensagem, true);
      if (!confirmar) return;

      const colaboradorId = this.patrimonioData?.colaborador?.id || 0;
      if (colaboradorId === 0) {
        this.util.exibirMensagemPopUp('Erro: Colaborador não identificado. Por favor, faça login novamente.', false);
        return;
      }

      const payload: any = {
        colaboradorId: colaboradorId,
        equipamentoId: equipamento.id,
        justificativa: 'Cancelado pelo colaborador'
      };
      if (equipamento.contestacaoId) {
        payload.contestacaoId = equipamento.contestacaoId;
      }
      this.loading = true;
      const resp = await this.patrimonioService.cancelarContestacao(payload).toPromise();
      if (resp?.sucesso) {
        this.util.exibirMensagemPopUp('Contestação cancelada com sucesso!', false);
        await this.loadMeuPatrimonio();
      } else {
        this.util.exibirMensagemPopUp(resp?.mensagem || 'Falha ao cancelar contestação', false);
      }
    } catch (e: any) {
      console.error('[CANCELAR] Erro ao cancelar contestação:', e);
      console.error('[CANCELAR] Status:', e?.status);
      console.error('[CANCELAR] Error:', e?.error);
      this.util.exibirMensagemPopUp('Erro ao cancelar contestação: ' + (e?.error?.mensagem || e?.message || 'Erro desconhecido'), false);
    } finally {
      this.loading = false;
    }
  }

  // =====================================================
  // MÉTODOS DE UTILIDADE
  // =====================================================
  
  private clearResults(): void {
    this.patrimonioError = '';
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

  formatDate(date: string): string {
    if (!date) return '';
    // Espera formato yyyy-MM-dd; formata manualmente para evitar offset de timezone
    const parts = date.split('-');
    if (parts.length !== 3) {
      return date;
    }
    const [year, month, day] = parts;
    const dd = day.padStart(2, '0');
    const mm = month.padStart(2, '0');
    return `${dd}/${mm}/${year}`;
  }

  /**
   * Formata data para exibição (igual à portaria)
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

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'liberado': return 'success';
      case 'pendências': return 'warning';
      case 'pendente': return 'warning';
      case 'aprovada': return 'success';
      case 'cancelada': return 'warning';
      default: return 'secondary';
    }
  }

  logout(): void {
    // Usar o mesmo método de limpeza
    this.forceLogout();
  }

  toggleColaboradorDetails(): void {
    this.colaboradorDetailsExpanded = !this.colaboradorDetailsExpanded;
  }

// =====================================================
  // MÉTODOS DO MODAL DE CONTESTAÇÃO
  // =====================================================
  
  abrirModalContestacao(equipamento?: any): void {
    
    if (equipamento) {
      this.contestacaoForm.patchValue({
        equipamentoId: equipamento.id
      });
      this.selectedEquipment = equipamento;
    } else {
    }
    
    this.showContestacaoModal = true;
    
    // Forçar detecção de mudanças
    this.cdr.detectChanges();
    
    setTimeout(() => {
      this.cdr.detectChanges();
    }, 100);
  }

  fecharModalContestacao(): void {
    this.showContestacaoModal = false;
    this.contestacaoForm.reset();
    this.selectedEquipment = null;
  }

  // =====================================================
  // MÉTODOS DO MODAL DE AUTO INVENTÁRIO
  // =====================================================
  
  abrirModalAutoInventario(): void {
    
    this.autoInventarioForm.reset();
    this.showAutoInventarioModal = true;

// Forçar detecção de mudanças
    this.cdr.detectChanges();
  }

  fecharModalAutoInventario(): void {
    this.showAutoInventarioModal = false;
    this.autoInventarioForm.reset();
  }

  async criarAutoInventario(): Promise<void> {
    
    if (this.autoInventarioForm.invalid) {
      this.markFormGroupTouched(this.autoInventarioForm);
      this.util.exibirMensagemPopUp('Por favor, preencha o número de série do recurso', false);
      return;
    }

    // Verificar se o colaborador está logado
    if (!this.patrimonioData?.colaborador?.id) {
      console.error('🚀 ERRO: Colaborador não está logado ou não tem ID');
      this.util.exibirMensagemPopUp('Erro: Colaborador não está logado. Faça login novamente.', false);
      return;
    }

    this.loading = true;

    try {
      const autoInventarioData = {
        colaboradorId: this.patrimonioData.colaborador.id,
        numeroSerie: this.autoInventarioForm.value.numeroSerie,
        observacoes: this.autoInventarioForm.value.observacoes || ''
      };

const result = await this.patrimonioService.criarAutoInventario(autoInventarioData).toPromise();
      
      if (result?.sucesso) {
        this.util.exibirMensagemPopUp('Solicitação de Auto Inventário criada com sucesso!', true);
        this.autoInventarioForm.reset();
        this.fecharModalAutoInventario();
      } else {
        console.error('❌ Erro ao criar Auto Inventário:', result?.mensagem);
        this.util.exibirMensagemPopUp('Erro ao criar solicitação: ' + (result?.mensagem || 'Erro desconhecido'), false);
      }
    } catch (error) {
      console.error('❌ Erro na requisição:', error);
      console.error('❌ Detalhes do erro:', error);
      console.error('❌ Status:', error.status);
      console.error('❌ Status Text:', error.statusText);
      console.error('❌ Error:', error.error);
      console.error('❌ Headers:', error.headers);
      
      if (error.status === 0) {
        console.error('❌ Erro de conexão - servidor não está respondendo');
        this.util.exibirMensagemPopUp('Erro de conexão. Verifique se o servidor está rodando na porta 5000.', false);
      } else if (error.error) {
        console.error('❌ Erro do servidor:', error.error);
        this.util.exibirMensagemPopUp('Erro do servidor: ' + (error.error?.mensagem || error.message || 'Erro desconhecido'), false);
      } else {
        console.error('❌ Erro desconhecido:', error.message);
        this.util.exibirMensagemPopUp('Erro: ' + (error.message || 'Erro desconhecido'), false);
      }
    } finally {
      this.loading = false;
    }
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
    
    // Para Meu Patrimônio (não tem statusLiberacao, mas tem colaborador.situacao)
    if (resultado.colaborador?.situacao) {
      if (resultado.colaborador.situacao === 'A' || resultado.colaborador.situacao === 'Ativo') {
        return 'liberado';
      } else if (resultado.colaborador.situacao === 'I' || resultado.colaborador.situacao === 'Inativo') {
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
    
    // Para Meu Patrimônio (não tem statusLiberacao, mas tem colaborador.situacao)
    if (resultado.colaborador?.situacao) {
      if (resultado.colaborador.situacao === 'A' || resultado.colaborador.situacao === 'Ativo') {
        return 'check_circle';
      } else if (resultado.colaborador.situacao === 'I' || resultado.colaborador.situacao === 'Inativo') {
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
    
    // Para Meu Patrimônio (não tem statusLiberacao, mas tem colaborador.situacao)
    if (resultado.colaborador?.situacao) {
      // Converter abreviação para texto completo
      if (resultado.colaborador.situacao === 'A') {
        return 'Ativo';
      } else if (resultado.colaborador.situacao === 'I') {
        return 'Inativo';
      }
      return resultado.colaborador.situacao;
    }
    
    return 'Desconhecido';
  }

  // =====================================================
  // MÉTODOS PARA CONTROLAR EXPANSÃO DAS SEÇÕES
  // =====================================================
  
  toggleSection(section: 'byod' | 'emPosse' | 'historico' | 'transitoLivre' | 'requerAutorizacao'): void {
    this.expandedSections[section] = !this.expandedSections[section];
  }

  isSectionExpanded(section: 'byod' | 'emPosse' | 'historico' | 'transitoLivre' | 'requerAutorizacao'): boolean {
    return this.expandedSections[section];
  }

  getEquipamentosEmPosse(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    const equipamentosEmPosse = this.patrimonioData.equipamentos.filter((equipamento: any) => 
      equipamento.status === 'Entregue'
    );
    // Log específico para equipamento 4
    const equipamento4 = equipamentosEmPosse.find(e => e.id === 4);
    if (equipamento4) {
    } else {
    }
    return equipamentosEmPosse;
  }

  getEquipamentosHistorico(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    const equipamentosHistorico = this.patrimonioData.equipamentos.filter((equipamento: any) => 
      equipamento.status !== 'Entregue'
    );
    // Log específico para equipamento 4 no histórico
    const equipamento4Historico = equipamentosHistorico.find(e => e.id === 4);
    if (equipamento4Historico) {
    }
    
    return equipamentosHistorico;
  }

  getByodEquipamentos(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    // Considerar aquisições próprias como BYOD; filtrar apenas em posse (status Entregue) se aplicável
    return this.patrimonioData.equipamentos.filter((equipamento: any) => {
      const byod = (equipamento.tipoAquisicao?.toLowerCase?.() === 'próprio' || equipamento.isByod === true);
      const emPosse = equipamento.status === 'Entregue';
      return byod && emPosse;
    });
  }

  /**
   * Envia termo por email para o colaborador
   */
  async enviarTermoPorEmail(equipamento: any): Promise<void> {

    try {
      this.loading = true;
      const colaboradorId = this.patrimonioData?.colaborador?.id || 0;
      
      // Aqui você pode implementar a chamada para o endpoint de envio de email
      // Por enquanto, vou simular uma resposta de sucesso
      await new Promise(resolve => setTimeout(resolve, 1000)); // Simular delay
      
      this.util.exibirMensagemPopUp('Termo enviado por email com sucesso!', false);
    } catch (e: any) {
      console.error('Erro ao enviar termo por email:', e);
      this.util.exibirMensagemPopUp('Erro ao enviar termo por email: ' + (e?.message || 'Erro desconhecido'), false);
    } finally {
      this.loading = false;
    }
  }

  /**
   * Navega para a tela de assinatura do termo
   */
  async assinarTermo(equipamento: any): Promise<void> {
    try {
      // Verificar se o equipamento tem hash da requisição
      if (!equipamento.hashRequisicao) {
        this.util.exibirMensagemPopUp('Erro: Hash da requisição não encontrado. Não é possível assinar o termo.', false);
        return;
      }

      // Confirmar ação
      const confirmar = await this.util.exibirMensagemPopUp(
        `Deseja assinar o termo de responsabilidade para o equipamento ${equipamento.patrimonio || equipamento.numeroSerie}?`, 
        true
      );
      if (!confirmar) {
        return;
      }

      // Determinar se é BYOD baseado no tipo de aquisição
      const isByod = equipamento.isByod === true || equipamento.tipoAquisicao?.toLowerCase() === 'próprio';
      
      // Navegar para a tela de assinatura
      const hash = equipamento.hashRequisicao;
      const routePath = isByod ? `/termos/${hash}/true` : `/termos/${hash}`;
      this.util.exibirMensagemPopUp('Redirecionando para a tela de assinatura do termo...', false);
      
      // Usar Router do Angular para navegação mais robusta
      // Abrir em nova aba para não perder o contexto do patrimônio
      const url = this.router.serializeUrl(this.router.createUrlTree([routePath]));
      window.open(url, '_blank');
      
    } catch (e: any) {
      console.error('❌ [PATRIMÔNIO] Erro ao navegar para assinatura:', e);
      this.util.exibirMensagemPopUp('Erro ao abrir tela de assinatura: ' + (e?.message || 'Erro desconhecido'), false);
    }
  }

  /**
   * Libera equipamento para assinatura do termo (método legado)
   */
  async liberarParaAssinar(equipamento: any): Promise<void> {
    try {
      // Confirmar ação
      const confirmar = await this.util.exibirMensagemPopUp(
        `Deseja liberar o equipamento ${equipamento.patrimonio || equipamento.numeroSerie} para assinatura do termo?`, 
        true
      );
      if (!confirmar) {
        return;
      }

      this.loading = true;
      const colaboradorId = this.patrimonioData?.colaborador?.id || 0;
      
      // Chamar endpoint para liberar para assinatura
      const result = await this.patrimonioService.liberarParaAssinatura(equipamento.id, colaboradorId).toPromise();
      
      if (result?.sucesso) {
        this.util.exibirMensagemPopUp('Equipamento liberado para assinatura com sucesso! Verifique seu email para assinar o termo.', false);
        await this.loadMeuPatrimonio(); // Recarregar dados
      } else {
        this.util.exibirMensagemPopUp('Erro ao liberar para assinatura: ' + (result?.mensagem || 'Erro desconhecido'), false);
      }
    } catch (e: any) {
      console.error('❌ [PATRIMÔNIO] Erro ao liberar para assinatura:', e);
      this.util.exibirMensagemPopUp('Erro ao liberar para assinatura: ' + (e?.message || 'Erro desconhecido'), false);
    } finally {
      this.loading = false;
    }
  }

  /**
   * Gera um novo termo para o equipamento (método legado)
   */
  async gerarTermo(equipamento: any): Promise<void> {
    try {
      this.loading = true;
      const colaboradorId = this.patrimonioData?.colaborador?.id || 0;
      
      // Chamar endpoint para gerar novo termo
      const result = await this.patrimonioService.gerarTermo(equipamento.id, colaboradorId).toPromise();
      
      if (result?.sucesso) {
        this.util.exibirMensagemPopUp('Termo gerado com sucesso! Verifique seu email.', false);
        await this.loadMeuPatrimonio(); // Recarregar dados
      } else {
        this.util.exibirMensagemPopUp('Erro ao gerar termo: ' + (result?.mensagem || 'Erro desconhecido'), false);
      }
    } catch (e: any) {
      console.error('❌ [PATRIMÔNIO] Erro ao gerar termo:', e);
      this.util.exibirMensagemPopUp('Erro ao gerar termo: ' + (e?.message || 'Erro desconhecido'), false);
    } finally {
      this.loading = false;
    }
  }

  /**
   * Baixa o PDF do termo
   */
  async baixarTermoPDF(equipamento: any): Promise<void> {
    try {
      this.loading = true;
      
      const colaboradorId = this.patrimonioData?.colaborador?.id;
      const pdfBlob = await this.patrimonioService.gerarTermoPDF(equipamento.id, colaboradorId).toPromise();
      
      if (pdfBlob) {
        // Criar URL do blob
        const url = window.URL.createObjectURL(pdfBlob);
        
        // Criar elemento de download
        const link = document.createElement('a');
        link.href = url;
        link.download = `Termo_${equipamento.patrimonio || equipamento.numeroSerie}_${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.pdf`;
        
        // Adicionar ao DOM, clicar e remover
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        // Limpar URL do blob
        window.URL.revokeObjectURL(url);
        
        this.util.exibirMensagemPopUp('PDF baixado com sucesso!', false);
      } else {
        this.util.exibirMensagemPopUp('Erro ao gerar PDF do termo', false);
        console.error('❌ [PATRIMÔNIO] PDF retornado vazio');
      }
    } catch (e: any) {
      console.error('❌ [PATRIMÔNIO] Erro ao baixar PDF:', e);
      this.util.exibirMensagemPopUp('Erro ao baixar PDF: ' + (e?.message || 'Erro desconhecido'), false);
    } finally {
      this.loading = false;
    }
  }

  // =====================================================
  // MÉTODOS DE CLASSIFICAÇÃO DE RECURSOS (IGUAL PORTARIA)
  // =====================================================

  /**
   * Obtém recursos com trânsito livre - APENAS ATIVOS EM POSSE
   * Implementação baseada na lógica da portaria
   */
  getRecursosTransitoLivre(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    
    // Usar apenas o dado real do backend, evitando duplicidades
    const recursos = this.patrimonioData.equipamentos.filter(recurso => {
      const temTransitoLivre = recurso.tipoEquipamentoTransitoLivre === true;
      const naoEhHistorico = recurso.status === 'Entregue' || recurso.status === 'Ativo';
      const naoEhByod = recurso.isByod !== true && recurso.isRecursoParticular !== true;
      return temTransitoLivre && naoEhHistorico && naoEhByod;
    });
    
    return recursos;
  }

  /**
   * Obtém recursos que requerem autorização - APENAS ATIVOS EM POSSE
   */
  getRecursosRequerAutorizacao(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    
    return this.patrimonioData.equipamentos.filter(recurso => {
      // Não é de trânsito livre (comparação estrita evita undefined contar como false)
      const naoTemTransitoLivre = recurso.tipoEquipamentoTransitoLivre === false;
      
      // Não é histórico (está ativo/entregue)
      const naoEhHistorico = recurso.status === 'Entregue' || recurso.status === 'Ativo';
      
      // Não é BYOD (recurso particular)
      const naoEhByod = !recurso.isByod;
      
      return naoTemTransitoLivre && naoEhHistorico && naoEhByod;
    });
  }

  /**
   * Obtém recursos particulares (BYOD) - APENAS ATIVOS EM POSSE
   * Implementação baseada na lógica da portaria
   */
  getRecursosParticulares(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    
    // Lógica corrigida: Usar o campo IsByod que está sendo retornado pelo backend
    const recursosByod = this.patrimonioData.equipamentos.filter(recurso => {
      const isByod = recurso.isByod === true;
      const naoEhHistorico = recurso.status === 'Entregue' || recurso.status === 'Ativo';
      
      return isByod && naoEhHistorico;
    });
    
    return recursosByod;
  }

  /**
   * Obtém recursos históricos (devolvidos ou não ativos)
   */
  getRecursosHistoricos(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    
    return this.patrimonioData.equipamentos.filter(recurso => 
      recurso.isHistorico || recurso.status !== 'Entregue'
    );
  }

  /**
   * Define se o recurso pode ser contestado
   * - Apenas recursos Corporativos/Alugados (não BYOD)
   * - Apenas ativos em posse (status Entregue/Ativo)
   */
  podeContestarRecurso(recurso: any): boolean {
    if (!recurso) return false;
    const naoEhByod = recurso.isByod !== true && recurso.isRecursoParticular !== true && (recurso.tipoAquisicao?.toLowerCase?.() !== 'próprio');
    const ativoEmPosse = recurso.status === 'Entregue' || recurso.status === 'Ativo';
    return naoEhByod && ativoEmPosse;
  }

  /**
   * Verifica se tem recursos sem trânsito livre
   */
  temRecursosSemTransitoLivre(): boolean {
    if (!this.patrimonioData?.equipamentos) return false;
    
    return this.patrimonioData.equipamentos.some(recurso => 
      !recurso.tipoEquipamentoTransitoLivre && 
      recurso.status === 'Entregue' && 
      !recurso.isHistorico && 
      !recurso.isRecursoParticular
    );
  }

  /**
   * Obtém recursos sem trânsito livre
   */
  getRecursosSemTransitoLivre(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    
    return this.patrimonioData.equipamentos.filter(recurso => 
      !recurso.tipoEquipamentoTransitoLivre && 
      recurso.status === 'Entregue' && 
      !recurso.isHistorico && 
      !recurso.isRecursoParticular
    );
  }

  /**
   * Obtém orientações para o colaborador (similar à portaria)
   */
  getOrientacaoColaborador(): string {
    if (!this.patrimonioData?.colaborador) {
      return '<span class="orientacao-erro">❌ Erro ao carregar dados do colaborador</span>';
    }

    const colaborador = this.patrimonioData.colaborador;
    const qtdEmPosse = this.getEquipamentosEmPosse().length;
    const qtdHistorico = this.getEquipamentosHistorico().length;
    const qtdByod = this.getRecursosParticulares().length;
    const temRecursos = (qtdEmPosse + qtdHistorico + qtdByod) > 0;
    const situacaoAtiva = colaborador.situacao === 'A' || colaborador.situacao === 'Ativo';

    // Cabeçalho comum de contadores
    const resumoContadores = `
      <div class="orientacao-contadores">
        <span><strong>${qtdEmPosse}</strong> Em Posse</span>
        <span><strong>${qtdHistorico}</strong> Histórico</span>
        <span><strong>${qtdByod}</strong> BYOD</span>
      </div>
    `;

    // Se não tem recursos, orientação simples
    if (!temRecursos) {
      return `
        ${resumoContadores}
        <div class="orientacao-liberado">
          <span class="orientacao-titulo">✅ Nenhum recurso em posse</span>
          <br><br>
          <strong>Seu patrimônio está atualizado</strong><br>
          <span class="orientacao-detalhe">
            • Você não possui recursos em sua posse no momento
            <br>• Use o "Auto Inventário" se tiver algum recurso não listado
          </span>
        </div>
      `;
    }

    // Se tem recursos, mostrar classificação
    const recursosTransitoLivre = this.getRecursosTransitoLivre();
    const recursosRequerAutorizacao = this.getRecursosRequerAutorizacao();
    
    if (recursosRequerAutorizacao.length > 0) {
      const recursosLista = recursosRequerAutorizacao.map(r => r.tipoEquipamento).join(', ');
      
      return `
        ${resumoContadores}
        <div class="orientacao-atencao">
          <span class="orientacao-titulo">⚠️ ATENÇÃO: Recursos que requerem autorização</span>
          <br><br>
          <strong>Recursos classificados por permissão de saída:</strong><br>
          <span class="orientacao-detalhe">
            <strong>✅ PODEM SAIR LIVREMENTE (${recursosTransitoLivre.length}):</strong> ${recursosTransitoLivre.map(r => r.tipoEquipamento).join(', ')}
            <br><strong>🚫 REQUEREM AUTORIZAÇÃO (${recursosRequerAutorizacao.length}):</strong> ${recursosLista}
            <br><br>
            <strong>IMPORTANTE:</strong>
            <br>• Recursos com trânsito livre podem sair da empresa normalmente
            <br>• Recursos que requerem autorização precisam de aprovação prévia
            <br>• Consulte as seções abaixo para ver detalhes de cada categoria
            <br>• Se você possui algum recurso em posse que não aparece listado acima, utilize o botão <strong>Auto Inventário</strong> para solicitar a regularização.
          </span>
        </div>
      `;
    }

    // Se todos os recursos têm trânsito livre, está liberado
    return `
      ${resumoContadores}
      <div class="orientacao-liberado">
        <span class="orientacao-titulo">✅ Todos os recursos com trânsito livre</span>
        <br><br>
        <strong>Seus recursos podem sair livremente</strong><br>
        <span class="orientacao-detalhe">
          • Você possui ${qtdEmPosse} recurso(s) em posse
          <br>• Todos os recursos têm trânsito livre
          <br>• Pode sair da empresa normalmente com seus equipamentos
          <br>• Se você possui algum recurso não listado, utilize o botão <strong>Auto Inventário</strong> para nos informar.
        </span>
      </div>
    `;
  }

  /**
   * Verifica se data de demissão é hoje ou passou (igual à portaria)
   */
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

  /**
   * Obtém status do colaborador (igual à portaria)
   */
  getColaboradorStatus(): 'Ativo' | 'Desligado' {
    const dt = this.patrimonioData?.colaborador?.dtDemissao;
    if (dt && this.isDataDemissaoHojeOuPassou(dt)) {
      return 'Desligado';
    }
    return 'Ativo';
  }

  /**
   * Limpa o campo CPF (igual à portaria)
   */
  limparCpf(): void {
    this.patrimonioForm.get('cpf')?.setValue('');
  }

  /**
   * Carrega as contestações do colaborador
   */
  async carregarMinhasContestoes(): Promise<void> {
    if (!this.patrimonioData?.colaborador?.id) {
      return;
    }

    try {
      const colaboradorId = this.patrimonioData.colaborador.id;
      const result = await this.patrimonioService.obterContestoesColaborador(colaboradorId).toPromise();
      
      if (result?.sucesso && result?.data) {
        this.minhasContestoes = result.data.map((item: any) => ({
          id: item.id,
          patrimonioId: item.patrimonioId,
          colaboradorId: item.colaboradorId,
          dataContestacao: new Date(item.dataContestacao),
          motivo: item.motivo,
          descricao: item.descricao,
          status: this.mapearStatusContestacao(item.status),
          statusId: item.statusId,
          usuarioAbertura: item.usuarioAbertura,
          tecnicoResponsavel: item.tecnicoResponsavel,
          tecnicoResponsavelId: item.tecnicoResponsavelId,
          usuarioResolucao: item.usuarioResolucao,
          dataResolucao: item.dataResolucao ? new Date(item.dataResolucao) : undefined,
          observacoesResolucao: item.observacoesResolucao,
          cliente: item.cliente,
          hashContestacao: item.hashContestacao,
          tipoContestacao: item.tipoContestacao,
          equipamento: {
            id: item.equipamento?.id,
            nome: item.equipamento?.nome || item.equipamento?.tipoEquipamento,
            numeroSerie: item.equipamento?.numeroSerie,
            tipoEquipamento: item.equipamento?.tipoEquipamento
          },
          colaborador: {
            id: item.colaborador?.id,
            nome: item.colaborador?.nome,
            cpf: item.colaborador?.cpf,
            email: item.colaborador?.email
          }
        }));
      } else {
        this.minhasContestoes = [];
      }
    } catch (error: any) {
      console.error('Erro ao carregar contestações:', error);
      this.minhasContestoes = [];
    }
  }

/**
   * Mapeia o status da contestação para o enum
   */
  private mapearStatusContestacao(status: string): ContestacaoStatus {
    switch (status?.toLowerCase()) {
      case 'pendente':
      case 'aberta':
        return ContestacaoStatus.ABERTA;
      case 'em_analise':
      case 'em análise':
        return ContestacaoStatus.EM_ANALISE;
      case 'resolvida':
        return ContestacaoStatus.RESOLVIDA;
      case 'cancelada':
        return ContestacaoStatus.CANCELADA;
      case 'negada':
        return ContestacaoStatus.NEGADA;
      case 'pendente_colaborador':
        return ContestacaoStatus.PENDENTE_COLABORADOR;
      default:
        return ContestacaoStatus.ABERTA;
    }
  }

  /**
   * Obtém recursos corporativos com pendência de assinatura de termo
   */
  getRecursosComPendenciaAssinatura(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    
    return this.patrimonioData.equipamentos.filter(recurso => {
      // Apenas recursos corporativos (não BYOD)
      const naoEhByod = !recurso.isByod;
      
      // Apenas recursos ativos em posse
      const naoEhHistorico = recurso.status === 'Entregue' || recurso.status === 'Ativo';
      
      // Não tem assinatura (assinado === false ou undefined)
      const naoTemAssinatura = recurso.assinado !== true;
      
      return naoEhByod && naoEhHistorico && naoTemAssinatura;
    });
  }

  /**
   * Obtém recursos BYOD com pendência de assinatura de termo
   */
  getRecursosByodComPendenciaAssinatura(): any[] {
    if (!this.patrimonioData?.equipamentos) return [];
    
    return this.patrimonioData.equipamentos.filter(recurso => {
      // Apenas recursos BYOD
      const ehByod = recurso.isByod === true;
      
      // Apenas recursos ativos em posse
      const naoEhHistorico = recurso.status === 'Entregue' || recurso.status === 'Ativo';
      
      // Não tem assinatura (assinado === false ou undefined)
      const naoTemAssinatura = recurso.assinado !== true;
      
      return ehByod && naoEhHistorico && naoTemAssinatura;
    });
  }

  /**
   * Verifica se há pendências de assinatura de termo
   */
  temPendenciasAssinatura(): boolean {
    const corporativos = this.getRecursosComPendenciaAssinatura().length;
    const byod = this.getRecursosByodComPendenciaAssinatura().length;
    return corporativos > 0 || byod > 0;
  }

  /**
   * Obtém total de recursos com pendência de assinatura
   */
  getTotalPendenciasAssinatura(): number {
    return this.getRecursosComPendenciaAssinatura().length + this.getRecursosByodComPendenciaAssinatura().length;
  }
}
