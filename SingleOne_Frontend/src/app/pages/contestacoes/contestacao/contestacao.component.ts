import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ContestacaoApiService } from 'src/app/api/contestacoes/contestacao-api.service';
import { UtilService } from 'src/app/util/util.service';
import { Contestacao, ContestacaoStatus } from 'src/app/models/contestacao.interface';

@Component({
  selector: 'app-contestacao',
  templateUrl: './contestacao.component.html',
  styleUrls: ['./contestacao.component.scss']
})
export class ContestacaoComponent implements OnInit {

  private session: any = {};
  public contestacaoForm: FormGroup;
  public contestacao: Contestacao | null = null;
  public isEditMode = false;
  public isLoading = false;
  public contestacaoId: number | null = null;
  public observacoesResolucao: string = '';
  public observacoesTouched: boolean = false;
  public tipoContestacao: string = 'contestacao'; // 'contestacao' ou 'auto_inventario'

  // Status disponíveis
  public statusOptions = [
    { value: 1, label: 'Aberta' },
    { value: 2, label: 'Em Análise' },
    { value: 3, label: 'Resolvida' },
    { value: 4, label: 'Cancelada' },
    { value: 5, label: 'Negada' },
    { value: 6, label: 'Pendente Colaborador' }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private api: ContestacaoApiService,
    private util: UtilService
  ) {
    this.contestacaoForm = this.createForm();
  }

  ngOnInit() {
    this.session = this.util.getSession('usuario');
    
    if (!this.session || !this.session.token) {
      this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
      this.router.navigate(['/']);
      return;
    }

    // Capturar query parameter 'tipo' para definir o título
    this.route.queryParams.subscribe(queryParams => {
      if (queryParams['tipo']) {
        this.tipoContestacao = queryParams['tipo'];
      }
    });

    // Verificar se é edição ou criação
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.contestacaoId = +params['id'];
        this.isEditMode = true;
        this.carregarContestacao();
      } else {
        this.isEditMode = false;
        this.contestacaoForm.patchValue({
          statusId: 1, // Status padrão: Aberta
          cliente: this.session.usuario.cliente
        });
      }
    });
  }

  private createForm(): FormGroup {
    return this.fb.group({
      id: [null],
      patrimonioId: [null, Validators.required],
      colaboradorId: [null, Validators.required],
      dataContestacao: [new Date(), Validators.required],
      motivo: ['', [Validators.required, Validators.minLength(10)]],
      descricao: ['', [Validators.required, Validators.minLength(20)]],
      statusId: [1, Validators.required],
      tecnicoResponsavelId: [null],
      observacoesResolucao: [''],
      cliente: [null, Validators.required],
      hashContestacao: ['']
    });
  }

  private carregarContestacao(): void {
    if (!this.contestacaoId) return;

    this.isLoading = true;
    this.api.obterContestacao(this.contestacaoId, this.session.token).then(res => {
      this.isLoading = false;
      
      if (res && res.status === 200 && res.data) {
        this.contestacao = res.data;
        
        // 🎯 AUTO-PREENCHIMENTO: Se não há técnico responsável e está em atendimento,
        // preencher automaticamente com o usuário logado
        let tecnicoResponsavel = this.contestacao.tecnicoResponsavelId;
        
        if (!tecnicoResponsavel && this.session?.usuario?.id) {
          // Status 1 = Aberta, Status 2 = Em Análise
          const statusQuePermitemAutoAtribuicao = [1, 2];
          
          if (statusQuePermitemAutoAtribuicao.includes(this.contestacao.statusId)) {
            tecnicoResponsavel = this.session.usuario.id;
          }
        }
        
        this.contestacaoForm.patchValue({
          id: this.contestacao.id,
          patrimonioId: this.contestacao.patrimonioId,
          colaboradorId: this.contestacao.colaboradorId,
          dataContestacao: new Date(this.contestacao.dataContestacao),
          motivo: this.contestacao.motivo,
          descricao: this.contestacao.descricao,
          statusId: this.contestacao.statusId,
          tecnicoResponsavelId: tecnicoResponsavel,
          observacoesResolucao: this.contestacao.observacoesResolucao,
          cliente: this.contestacao.cliente,
          hashContestacao: this.contestacao.hashContestacao
        });
      } else {
        this.util.exibirFalhaComunicacao();
        this.router.navigate(['/movimentacoes/contestacoes']);
      }
    }).catch(err => {
      this.isLoading = false;
      console.error('[CONTESTACAO] Erro ao carregar contestação:', err);
      this.util.exibirFalhaComunicacao();
      this.router.navigate(['/movimentacoes/contestacoes']);
    });
  }

  public salvarContestacao(): void {
    if (this.contestacaoForm.invalid) {
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios corretamente.', 5000);
      return;
    }

    const formData = this.contestacaoForm.value;
    
    // Adicionar hash único se for nova contestação
    if (!this.isEditMode) {
      formData.hashContestacao = this.gerarHashContestacao();
    }

    this.isLoading = true;

    const apiCall = this.isEditMode 
      ? this.api.atualizarContestacao(this.contestacaoId!, formData, this.session.token)
      : this.api.criarContestacao(formData, this.session.token);

    apiCall.then(res => {
      this.isLoading = false;
      
      if (res && res.status === 200) {
        const mensagem = this.isEditMode ? 'Contestação atualizada com sucesso!' : 'Contestação criada com sucesso!';
        this.util.exibirMensagemToast(mensagem, 5000);
        this.router.navigate(['/movimentacoes/contestacoes']);
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(err => {
      this.isLoading = false;
      console.error('[CONTESTACAO] Erro ao salvar contestação:', err);
      this.util.exibirFalhaComunicacao();
    });
  }

  public cancelar(): void {
    this.router.navigate(['/movimentacoes/contestacoes']);
  }

  public resolverContestacao(): void {
    if (!this.contestacaoId) return;
    
    this.observacoesTouched = true;
    
    if (!this.observacoesResolucao || this.observacoesResolucao.trim().length < 10) {
      this.util.exibirMensagemToast('Por favor, preencha as observações da resolução (mínimo 10 caracteres).', 5000);
      return;
    }

    const tipoTexto = this.tipoContestacao === 'auto_inventario' ? 'solicitação de auto inventário' : 
                     this.tipoContestacao === 'inventario_forcado' ? 'inventário forçado' : 'contestação';

    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja marcar est${this.tipoContestacao === 'inventario_forcado' ? 'e' : 'a'} ${tipoTexto} como resolvid${this.tipoContestacao === 'inventario_forcado' ? 'o' : 'a'}?<br><br>` +
      `📋 <strong>ID:</strong> #${this.contestacaoId}<br>` +
      `👤 <strong>Colaborador:</strong> ${this.contestacao?.colaborador?.nome || 'N/A'}<br><br>` +
      'Esta ação irá finalizar o atendimento e não poderá ser desfeita.',
      true
    ).then(aceita => {
      if (aceita) {
        this.isLoading = true;
        const payload = {
          status: ContestacaoStatus.RESOLVIDA,
          observacaoResolucao: this.observacoesResolucao,
          usuarioResolucao: this.session?.usuario?.id || null
        };
        
        this.api.atualizarContestacao(this.contestacaoId!, payload, this.session.token).then(res => {
          this.isLoading = false;
          
          if (res && res.status === 200) {
            const mensagemSucesso = this.tipoContestacao === 'auto_inventario' ? 'Solicitação de auto inventário resolvida com sucesso!' :
                                   this.tipoContestacao === 'inventario_forcado' ? 'Inventário forçado resolvido com sucesso!' :
                                   'Contestação resolvida com sucesso!';
            this.util.exibirMensagemToast(mensagemSucesso, 5000);
            this.router.navigate(['/movimentacoes/contestacoes']);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(err => {
          this.isLoading = false;
          console.error('[CONTESTACAO] Erro ao resolver:', err);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  public negarContestacao(): void {
    if (!this.contestacaoId) return;
    
    this.observacoesTouched = true;
    
    if (!this.observacoesResolucao || this.observacoesResolucao.trim().length < 10) {
      this.util.exibirMensagemToast('Por favor, preencha as observações da resolução (mínimo 10 caracteres).', 5000);
      return;
    }

    this.util.exibirMensagemPopUp(
      'Tem certeza que deseja negar esta contestação?<br><br>' +
      `📋 <strong>ID:</strong> #${this.contestacaoId}<br>` +
      `👤 <strong>Colaborador:</strong> ${this.contestacao?.colaborador?.nome || 'N/A'}<br><br>` +
      'Esta ação marcará a contestação como negada pela equipe técnica e não poderá ser desfeita.',
      true
    ).then(aceita => {
      if (aceita) {
        this.isLoading = true;
        const payload = {
          status: ContestacaoStatus.NEGADA,
          observacaoResolucao: this.observacoesResolucao,
          usuarioResolucao: this.session?.usuario?.id || null
        };
        this.api.atualizarContestacao(this.contestacaoId!, payload, this.session.token).then(res => {
          this.isLoading = false;
          
          if (res && res.status === 200) {
            this.util.exibirMensagemToast('Contestação negada com sucesso!', 5000);
            this.router.navigate(['/movimentacoes/contestacoes']);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(err => {
          this.isLoading = false;
          console.error('[CONTESTACAO] Erro ao negar contestação:', err);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  public cancelarAutoInventario(): void {
    if (!this.contestacaoId) return;
    
    this.observacoesTouched = true;
    
    if (!this.observacoesResolucao || this.observacoesResolucao.trim().length < 10) {
      this.util.exibirMensagemToast('Por favor, preencha as observações do cancelamento (mínimo 10 caracteres).', 5000);
      return;
    }

    const tipoTexto = this.tipoContestacao === 'inventario_forcado' ? 'inventário forçado' : 'solicitação de auto inventário';

    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja cancelar est${this.tipoContestacao === 'inventario_forcado' ? 'e' : 'a'} ${tipoTexto}?<br><br>` +
      `📋 <strong>ID:</strong> #${this.contestacaoId}<br>` +
      `👤 <strong>Colaborador:</strong> ${this.contestacao?.colaborador?.nome || 'N/A'}<br><br>` +
      `Esta ação marcará ${this.tipoContestacao === 'inventario_forcado' ? 'o inventário' : 'a solicitação'} como cancelad${this.tipoContestacao === 'inventario_forcado' ? 'o' : 'a'} e não poderá ser desfeita.`,
      true
    ).then(aceita => {
      if (aceita) {
        this.isLoading = true;
        const payload = {
          status: ContestacaoStatus.CANCELADA,
          observacaoResolucao: this.observacoesResolucao,
          usuarioResolucao: this.session?.usuario?.id || null
        };
        this.api.atualizarContestacao(this.contestacaoId!, payload, this.session.token).then(res => {
          this.isLoading = false;
          
          if (res && res.status === 200) {
            const mensagemSucesso = this.tipoContestacao === 'inventario_forcado' ? 'Inventário forçado cancelado com sucesso!' : 'Solicitação de auto inventário cancelada com sucesso!';
            this.util.exibirMensagemToast(mensagemSucesso, 5000);
            this.router.navigate(['/movimentacoes/contestacoes']);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(err => {
          this.isLoading = false;
          console.error('[CONTESTACAO] Erro ao cancelar:', err);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  public podeResolver(): boolean {
    const statusId = this.contestacao?.statusId || this.contestacaoForm.get('statusId')?.value;
    return statusId === 1 || statusId === 2; // Aberta ou Em Análise
  }

  public podeNegar(): boolean {
    const statusId = this.contestacao?.statusId || this.contestacaoForm.get('statusId')?.value;
    return statusId === 1 || statusId === 2; // Aberta ou Em Análise
  }

  public podeEditar(): boolean {
    const statusId = this.contestacao?.statusId || this.contestacaoForm.get('statusId')?.value;
    return statusId === 1 || statusId === 2; // Aberta ou Em Análise
  }

  private gerarHashContestacao(): string {
    const timestamp = Date.now().toString();
    const random = Math.random().toString(36).substring(2);
    return `CONTEST_${timestamp}_${random}`.toUpperCase();
  }

  public getStatusLabel(statusId: number): string {
    const status = this.statusOptions.find(s => s.value === statusId);
    return status ? status.label : 'Desconhecido';
  }

  public getStatusClass(statusId: number): string {
    switch (statusId) {
      case 1: return 'status-open';
      case 2: return 'status-analyzing';
      case 3: return 'status-resolved';
      case 4: return 'status-cancelled';
      case 5: return 'status-denied';
      case 6: return 'status-pending';
      default: return 'status-default';
    }
  }

  public temErroObservacoes(): boolean {
    return this.observacoesTouched && (!this.observacoesResolucao || this.observacoesResolucao.trim().length < 10);
  }

  public getTituloAtendimento(): string {
    if (this.isEditMode) {
      if (this.tipoContestacao === 'auto_inventario') {
        return 'Atendimento Auto Inventário';
      } else if (this.tipoContestacao === 'inventario_forcado') {
        return 'Atendimento Inventário Forçado';
      }
      return 'Atendimento de Contestação';
    }
    return 'Nova Contestação';
  }

  public getSubtituloAtendimento(): string {
    if (this.isEditMode) {
      if (this.tipoContestacao === 'auto_inventario') {
        return 'Análise e resolução da solicitação de auto inventário';
      } else if (this.tipoContestacao === 'inventario_forcado') {
        return 'Análise e resolução do inventário forçado pela equipe de TI';
      }
      return 'Análise e resolução da contestação patrimonial';
    }
    return 'Criar nova contestação patrimonial';
  }

  public getBreadcrumbAtivo(): string {
    return this.isEditMode ? 'Atendimento' : 'Nova';
  }

  // Verifica se o equipamento é uma linha telefônica (número possui 10 ou 11 dígitos)
  public isLinhaTelefonica(): boolean {
    const nome = this.contestacao?.equipamento?.nome || '';
    return /^\d{10,11}$/.test(nome);
  }

  // Formata número de telefone para exibição
  public formatarNumeroTelefone(numero: string): string {
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

  // Retorna o label apropriado para o equipamento
  public getLabelEquipamento(): string {
    return this.isLinhaTelefonica() ? 'Linha Telefônica' : 'Equipamento';
  }

  // Retorna o valor formatado do equipamento
  public getValorEquipamento(): string {
    const nome = this.contestacao?.equipamento?.nome || 'N/A';
    return this.isLinhaTelefonica() ? this.formatarNumeroTelefone(nome) : nome;
  }

  // Retorna o label apropriado para número de série
  public getLabelNumeroSerie(): string {
    return this.isLinhaTelefonica() ? 'ICCID' : 'S/N';
  }
}
