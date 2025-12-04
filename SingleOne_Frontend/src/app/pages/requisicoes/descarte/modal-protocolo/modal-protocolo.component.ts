import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ProtocoloDescarte, TipoDescarteEnum, MtrEmitidoPorEnum, MTR_EMITIDO_POR_LABELS, Fornecedor } from '../../../../models/protocolo-descarte.model';
import { ProtocoloDescarteApiService } from '../../../../api/protocolo-descarte/protocolo-descarte-api.service';
import { UtilService } from '../../../../util/util.service';

@Component({
  selector: 'app-modal-protocolo',
  templateUrl: './modal-protocolo.component.html',
  styleUrls: ['./modal-protocolo.component.scss']
})
export class ModalProtocoloComponent implements OnInit {

  protocoloForm: FormGroup;
  tiposDescarte = [
    { value: TipoDescarteEnum.DOACAO, label: 'Doação' },
    { value: TipoDescarteEnum.VENDA, label: 'Venda' },
    { value: TipoDescarteEnum.DEVOLUCAO, label: 'Devolução' },
    { value: TipoDescarteEnum.LOGISTICA_REVERSA, label: 'Logística Reversa' },
    { value: TipoDescarteEnum.DESCARTE_FINAL, label: 'Descarte Geral (destruição)' }
  ];

  mtrEmitidoPor = [
    { value: MtrEmitidoPorEnum.GERADOR, label: MTR_EMITIDO_POR_LABELS[MtrEmitidoPorEnum.GERADOR] },
    { value: MtrEmitidoPorEnum.TRANSPORTADOR, label: MTR_EMITIDO_POR_LABELS[MtrEmitidoPorEnum.TRANSPORTADOR] },
    { value: MtrEmitidoPorEnum.DESTINADOR, label: MTR_EMITIDO_POR_LABELS[MtrEmitidoPorEnum.DESTINADOR] }
  ];

  // Fornecedores destinadores
  fornecedoresDestinadores: Fornecedor[] = [];
  fornecedorSelecionado: Fornecedor | null = null;

  isEditMode = false;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ModalProtocoloComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { protocolo?: ProtocoloDescarte, clienteId: number },
    private protocoloApi: ProtocoloDescarteApiService,
    private util: UtilService
  ) {
    this.isEditMode = !!this.data.protocolo;
    this.initializeForm();
  }

  ngOnInit(): void {
    if (this.isEditMode && this.data.protocolo) {
      this.populateForm(this.data.protocolo);
    } else {
      this.gerarNumeroProtocolo();
    }
    
    // Carregar fornecedores destinadores
    this.carregarFornecedoresDestinadores();
  }

  private initializeForm(): void {
    this.protocoloForm = this.fb.group({
      protocolo: [{ value: '', disabled: true }],
      tipoDescarte: ['', Validators.required],
      motivoDescarte: [''],
      destinoFinal: [''],
      fornecedorDestinador: [''], // NOVO: Campo para seleção de fornecedor
      empresaDestinoFinal: [''],
      cnpjDestinoFinal: [''],
      certificadoDescarte: [''],
      valorTotalEstimado: [0],
      observacoes: [''],
      
      // Campos MTR
      mtrObrigatorio: [false],
      mtrNumero: [''],
      mtrEmitidoPor: [''],
      mtrDataEmissao: [''],
      mtrValidade: [''],
      mtrArquivo: [''],
      mtrEmpresaTransportadora: [''],
      mtrCnpjTransportadora: [''],
      mtrPlacaVeiculo: [''],
      mtrMotorista: [''],
      mtrCpfMotorista: ['']
    });
  }

  private populateForm(protocolo: ProtocoloDescarte): void {
    this.protocoloForm.patchValue({
      protocolo: protocolo.protocolo,
      tipoDescarte: protocolo.tipoDescarte,
      motivoDescarte: protocolo.motivoDescarte,
      destinoFinal: protocolo.destinoFinal,
      // fornecedorDestinador será preenchido automaticamente se encontrar correspondência
      empresaDestinoFinal: protocolo.empresaDestinoFinal,
      cnpjDestinoFinal: protocolo.cnpjDestinoFinal,
      certificadoDescarte: protocolo.certificadoDescarte,
      valorTotalEstimado: protocolo.valorTotalEstimado,
      observacoes: protocolo.observacoes,
      
      // Campos MTR
      mtrObrigatorio: protocolo.mtrObrigatorio || false,
      mtrNumero: protocolo.mtrNumero,
      mtrEmitidoPor: protocolo.mtrEmitidoPor,
      mtrDataEmissao: protocolo.mtrDataEmissao,
      mtrValidade: protocolo.mtrValidade,
      mtrArquivo: protocolo.mtrArquivo,
      mtrEmpresaTransportadora: protocolo.mtrEmpresaTransportadora,
      mtrCnpjTransportadora: protocolo.mtrCnpjTransportadora,
      mtrPlacaVeiculo: protocolo.mtrPlacaVeiculo,
      mtrMotorista: protocolo.mtrMotorista,
      mtrCpfMotorista: protocolo.mtrCpfMotorista
    });
  }

  private gerarNumeroProtocolo(): void {
    const session = this.util.getSession('usuario');
    if (session?.token) {
      this.protocoloApi.gerarNumeroProtocolo(session.token).subscribe({
        next: (response) => {
          this.protocoloForm.patchValue({
            protocolo: response.protocolo
          });
        },
        error: (error) => {
          console.error('Erro ao gerar número do protocolo:', error);
          this.util.exibirMensagemToast('Erro ao gerar número do protocolo', 5000);
        }
      });
    }
  }

  salvar(): void {
    if (this.protocoloForm.valid) {
      this.loading = true;
      const formValue = this.protocoloForm.getRawValue(); // Pega valores incluindo campos desabilitados
      const session = this.util.getSession('usuario');

      if (!session?.token) {
        this.util.exibirMensagemToast('Sessão inválida', 5000);
        this.loading = false;
        return;
      }

      // Remove campos nulos/vazios para evitar erro no backend
      const protocoloData: any = {
        cliente: this.data.clienteId,
        tipoDescarte: formValue.tipoDescarte,
        responsavelProtocolo: session.usuario.id,
        documentoGerado: false
      };

      // Adiciona campos opcionais apenas se tiverem valor
      if (this.isEditMode && this.data.protocolo?.id) {
        protocoloData.id = this.data.protocolo.id;
      }
      
      if (formValue.motivoDescarte) {
        protocoloData.motivoDescarte = formValue.motivoDescarte;
      }
      
      if (formValue.destinoFinal) {
        protocoloData.destinoFinal = formValue.destinoFinal;
      }
      
      if (formValue.empresaDestinoFinal) {
        protocoloData.empresaDestinoFinal = formValue.empresaDestinoFinal;
      }
      
      if (formValue.cnpjDestinoFinal) {
        protocoloData.cnpjDestinoFinal = formValue.cnpjDestinoFinal;
      }
      
      if (formValue.certificadoDescarte) {
        protocoloData.certificadoDescarte = formValue.certificadoDescarte;
      }
      
      if (formValue.valorTotalEstimado && formValue.valorTotalEstimado > 0) {
        protocoloData.valorTotalEstimado = formValue.valorTotalEstimado;
      }
      
          if (formValue.observacoes) {
            protocoloData.observacoes = formValue.observacoes;
          }

          // Não enviar o campo fornecedorDestinador para o backend
          // Os campos empresaDestinoFinal e cnpjDestinoFinal já foram preenchidos automaticamente

          // Campos MTR
      if (formValue.mtrObrigatorio !== undefined) {
        protocoloData.mtrObrigatorio = formValue.mtrObrigatorio;
      }
      
      if (formValue.mtrNumero) {
        protocoloData.mtrNumero = formValue.mtrNumero;
      }
      
      if (formValue.mtrEmitidoPor) {
        protocoloData.mtrEmitidoPor = formValue.mtrEmitidoPor;
      }
      
      if (formValue.mtrDataEmissao) {
        protocoloData.mtrDataEmissao = formValue.mtrDataEmissao;
      }
      
      if (formValue.mtrValidade) {
        protocoloData.mtrValidade = formValue.mtrValidade;
      }
      
      if (formValue.mtrArquivo) {
        protocoloData.mtrArquivo = formValue.mtrArquivo;
      }
      
      if (formValue.mtrEmpresaTransportadora) {
        protocoloData.mtrEmpresaTransportadora = formValue.mtrEmpresaTransportadora;
      }
      
      if (formValue.mtrCnpjTransportadora) {
        protocoloData.mtrCnpjTransportadora = formValue.mtrCnpjTransportadora;
      }
      
      if (formValue.mtrPlacaVeiculo) {
        protocoloData.mtrPlacaVeiculo = formValue.mtrPlacaVeiculo;
      }
      
      if (formValue.mtrMotorista) {
        protocoloData.mtrMotorista = formValue.mtrMotorista;
      }
      
      if (formValue.mtrCpfMotorista) {
        protocoloData.mtrCpfMotorista = formValue.mtrCpfMotorista;
      }
      const request = this.isEditMode 
        ? this.protocoloApi.atualizarProtocolo(this.data.protocolo!.id!, protocoloData, session.token)
        : this.protocoloApi.criarProtocolo(protocoloData, session.token);

      request.subscribe({
        next: (protocolo) => {
          this.loading = false;
          this.util.exibirMensagemToast(
            `Protocolo ${this.isEditMode ? 'atualizado' : 'criado'} com sucesso!`, 
            5000
          );
          this.dialogRef.close(protocolo);
        },
        error: (error) => {
          this.loading = false;
          console.error('❌ Erro ao salvar protocolo:', error);
          console.error('Detalhes do erro:', {
            status: error.status,
            statusText: error.statusText,
            message: error.message,
            error: error.error
          });
          
          let mensagemErro = 'Erro ao salvar protocolo';
          if (error.error && error.error.mensagem) {
            mensagemErro = error.error.mensagem;
          } else if (error.error && typeof error.error === 'string') {
            mensagemErro = error.error;
          }
          
          this.util.exibirMensagemToast(mensagemErro, 5000);
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  cancelar(): void {
    this.dialogRef.close();
  }

  private markFormGroupTouched(): void {
    Object.keys(this.protocoloForm.controls).forEach(key => {
      const control = this.protocoloForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const field = this.protocoloForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors['required']) {
        return `${this.getFieldLabel(fieldName)} é obrigatório`;
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      tipoDescarte: 'Tipo de Descarte',
      motivoDescarte: 'Motivo do Descarte',
      destinoFinal: 'Destino Final',
      valorTotalEstimado: 'Valor Total Estimado',
      observacoes: 'Observações'
    };
    return labels[fieldName] || fieldName;
  }

  onFileSelected(event: any, fieldName: string): void {
    const file = event.target.files[0];
    if (file) {
      // Por enquanto, apenas armazenar o nome do arquivo
      // Em uma implementação completa, você faria upload do arquivo para o servidor
      this.protocoloForm.patchValue({
        [fieldName]: file.name
      });
      
      this.util.exibirMensagemToast(`Arquivo selecionado: ${file.name}`, 3000);
    }
  }

  /**
   * Carregar fornecedores destinadores de resíduos
   */
  private carregarFornecedoresDestinadores(): void {
    const session = this.util.getSession('usuario');
    
    if (!session?.token || !this.data.clienteId) {
      console.warn('Sessão ou cliente inválido para carregar fornecedores');
      return;
    }
    
    this.protocoloApi.buscarFornecedoresDestinadores(this.data.clienteId, session.token).subscribe({
      next: (fornecedores) => {
        this.fornecedoresDestinadores = fornecedores;
        
        // Se estiver editando um protocolo existente, tentar encontrar correspondência
        if (this.isEditMode && this.data.protocolo) {
          setTimeout(() => {
            this.encontrarFornecedorCorrespondente(
              this.data.protocolo!.empresaDestinoFinal || '',
              this.data.protocolo!.cnpjDestinoFinal || ''
            );
          }, 100);
        }
      },
      error: (error) => {
        console.error('Erro ao carregar fornecedores destinadores:', error);
        this.util.exibirMensagemToast('Erro ao carregar fornecedores destinadores', 3000);
      }
    });
  }

  /**
   * Quando um fornecedor é selecionado no dropdown
   */
  onFornecedorSelecionado(fornecedorId: number): void {
    this.fornecedorSelecionado = this.fornecedoresDestinadores.find(f => f.id === fornecedorId) || null;
    
    if (this.fornecedorSelecionado) {
      // Preencher automaticamente os campos de destino final
      this.protocoloForm.patchValue({
        empresaDestinoFinal: this.fornecedorSelecionado.nome,
        cnpjDestinoFinal: this.fornecedorSelecionado.cnpj
      });
    }
  }

  /**
   * Tentar encontrar fornecedor correspondente aos dados existentes
   */
  private encontrarFornecedorCorrespondente(empresaDestinoFinal: string, cnpjDestinoFinal: string): void {
    if (!empresaDestinoFinal && !cnpjDestinoFinal) return;

    // Tentar encontrar por CNPJ primeiro (mais preciso)
    if (cnpjDestinoFinal) {
      const fornecedorPorCnpj = this.fornecedoresDestinadores.find(f => 
        f.cnpj === cnpjDestinoFinal
      );
      if (fornecedorPorCnpj) {
        this.protocoloForm.patchValue({
          fornecedorDestinador: fornecedorPorCnpj.id
        });
        this.fornecedorSelecionado = fornecedorPorCnpj;
        return;
      }
    }

    // Tentar encontrar por nome da empresa
    if (empresaDestinoFinal) {
      const fornecedorPorNome = this.fornecedoresDestinadores.find(f => 
        f.nome.toLowerCase().includes(empresaDestinoFinal.toLowerCase()) ||
        empresaDestinoFinal.toLowerCase().includes(f.nome.toLowerCase())
      );
      if (fornecedorPorNome) {
        this.protocoloForm.patchValue({
          fornecedorDestinador: fornecedorPorNome.id
        });
        this.fornecedorSelecionado = fornecedorPorNome;
        return;
      }
    }
  }
}
