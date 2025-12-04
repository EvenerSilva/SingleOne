import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesApiService } from '../../../../api/configuracoes/configuracoes-api.service';
import { Filial, FilialForm } from '../../../../models/filial.interface';
import { UtilService } from '../../../../util/util.service';
import { CnpjValidatorService } from '../../../../util/cnpj-validator.service';

@Component({
  selector: 'app-filial',
  templateUrl: './filial.component.html',
  styleUrls: ['./filial.component.scss']
})
export class FilialComponent implements OnInit {
  
  @Input() filial: Filial | null = null;
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() filialSalva = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  filialForm: FormGroup;
  empresas: any[] = [];
  localidades: any[] = [];
  loading: boolean = false;
  saving: boolean = false;
  errorMessage: string = '';
  isEditMode: boolean = false;
  titulo: string = '';
  session: any;

  constructor(
    private fb: FormBuilder,
    private api: ConfiguracoesApiService,
    private util: UtilService,
    private cnpjValidator: CnpjValidatorService
  ) {
    this.filialForm = this.fb.group({
      id: [0],
      nome: ['', [Validators.required, Validators.maxLength(100)]],
      empresaId: ['', Validators.required],
      localidadeId: ['', Validators.required],
      cnpj: ['', [Validators.maxLength(18)]],
      endereco: [''],
      telefone: ['', [Validators.maxLength(20)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      ativo: [true]
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    if (!this.filial) {
      this.filial = {} as any;
    }
    
    // Definir cliente se for criação
    if (this.modo === 'criar') {
      (this.filial as any).cliente = this.session.usuario.cliente;
    }
    
    // Carregar dados iniciais
    this.carregarDadosIniciais();
    
    // ✅ CORREÇÃO: Não preencher formulário aqui, aguardar dados serem carregados
    // Se for edição, será preenchido após carregarDadosIniciais()
  }

  carregarDadosIniciais() {
    this.loading = true;
    
    const token = this.session.token;
    const cliente = this.session.usuario.cliente;
    if (!token || !cliente) {
      console.error('[FILIAL-COMPONENT] ❌ Sessão inválida');
      this.loading = false;
      return;
    }
    this.api.listarEmpresas("null", cliente, token)
      .then(res => {
        if (res.status === 200 || res.status === 204) {
          this.empresas = res.data || [];
        } else {
          console.error('[FILIAL-COMPONENT] ❌ Erro ao carregar empresas:', res);
        }
      })
      .catch(err => {
        console.error('[FILIAL-COMPONENT] ❌ Erro ao carregar empresas:', err);
      });

    // Carregar localidades
    this.api.listarLocalidades(cliente, token)
      .then(res => {
        this.loading = false;
        if (res.status === 200 || res.status === 204) {
          this.localidades = res.data || [];
          if (this.localidades.length > 0) {
            const primeiraLocalidade = this.localidades[0];
          }
          
          // ✅ CORREÇÃO: Preencher formulário após dados serem carregados
          if (this.modo === 'editar' && this.filial?.id) {
            this.preencherFormulario();
          } else {
          }
        } else {
          console.error('[FILIAL-COMPONENT] ❌ Erro ao carregar localidades:', res);
          this.localidades = [];
        }
      })
      .catch(err => {
        this.loading = false;
        console.error('[FILIAL-COMPONENT] ❌ Erro ao carregar localidades:', err);
        this.localidades = [];
      });
  }

  verificarModoEdicao() {
    this.isEditMode = this.modo === 'editar';
    this.titulo = this.isEditMode ? 'Editar Filial' : 'Nova Filial';
    
    if (this.isEditMode && this.filial) {
      this.preencherFormulario();
    }
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Nova Filial' : 'Editar Filial';
  }

  getBotaoTexto(): string {
    return this.modo === 'criar' ? 'Criar' : 'Salvar';
  }

  carregarFilial(id: number) {
    this.loading = true;
    const token = this.util.getSession('usuario')?.token;
    
    if (!token) {
      console.error('Token não encontrado');
      this.loading = false;
      return;
    }
    
    this.api.obterFilialPorId(id, token)
      .then(res => {
        this.filial = res.data;
        this.preencherFormulario();
        this.loading = false;
      })
      .catch(err => {
        this.errorMessage = 'Erro ao carregar filial';
        this.loading = false;
        console.error('Erro ao carregar filial:', err);
      });
  }

  preencherFormulario() {
    if (this.filial) {
      if (this.filial.localidadeId === undefined || this.filial.localidadeId === null) {
        console.warn('[FILIAL-COMPONENT] ⚠️ ATENÇÃO: localidadeId está undefined/null!');
        console.warn('[FILIAL-COMPONENT] Estrutura completa da filial:', this.filial);
        console.warn('[FILIAL-COMPONENT] Chaves disponíveis:', Object.keys(this.filial));
      }
      
      this.filialForm.patchValue({
        id: this.filial.id,
        nome: this.filial.nome,
        empresaId: this.filial.empresaId,
        localidadeId: this.filial.localidadeId,
        cnpj: this.filial.cnpj || '',
        endereco: this.filial.endereco || '',
        telefone: this.filial.telefone || '',
        email: this.filial.email || '',
        ativo: this.filial.ativo !== undefined ? this.filial.ativo : true
      });
    }
  }

  salvar() {
    if (this.filialForm.valid) {
      // Validação de CNPJ antes de enviar para a API
      const cnpj = this.filialForm.get('cnpj')?.value;
      if (cnpj && !this.cnpjValidator.isValid(cnpj)) {
        const errorMessage = this.cnpjValidator.getErrorMessage(cnpj);
        this.util.exibirMensagemToast(`CNPJ inválido: ${errorMessage}`, 5000);
        return;
      }
      
      this.saving = true;
      this.errorMessage = '';

      const filialData: FilialForm = this.filialForm.value;
      
      // 🔧 CORREÇÃO: Garantir que o campo ativo seja sempre enviado
      if (filialData.ativo === undefined || filialData.ativo === null) {
        filialData.ativo = true;
      }
      if (!filialData.cnpj || filialData.cnpj.trim() === '') {
        filialData.cnpj = '';
      }

      const token = this.session.token;
      
      if (!token) {
        this.errorMessage = 'Token não encontrado';
        this.saving = false;
        return;
      }
      
      this.api.salvarFilial(filialData, token)
        .then(res => {
          this.saving = false;
          
          // Se chegou aqui, é sucesso (status 200)
          if (res.data && res.data.Mensagem) {
            this.util.exibirMensagemToast(res.data.Mensagem, 5000);
          } else {
            this.util.exibirMensagemToast('Filial salva com sucesso!', 5000);
          }
          
          this.filialSalva.emit(res.data);
        })
        .catch(err => {
          console.error('[FILIAL] ❌ Erro ao salvar:', err);
          console.error('[FILIAL] ❌ Erro completo:', err.response || err);
          console.error('[FILIAL] ❌ Status do erro:', err.response?.status);
          console.error('[FILIAL] ❌ Mensagem do erro:', err.response?.data);
          
          this.saving = false;
          
          // Verificar se é erro 400 (CNPJ inválido ou duplicado)
          if (err.response?.status === 400) {
            if (err.response.data && err.response.data.Mensagem) {
              this.util.exibirMensagemToast(err.response.data.Mensagem, 5000);
            } else {
              this.util.exibirMensagemToast('Erro de validação. Verifique os dados informados.', 5000);
            }
          }
          // Verificar se é erro 500 (problema no backend)
          else if (err.response?.status === 500) {
            this.util.exibirMensagemToast('Erro interno do servidor. Verifique o backend.', 5000);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        });
    } else {
      this.marcarCamposInvalidos();
    }
  }

  marcarCamposInvalidos() {
    Object.keys(this.filialForm.controls).forEach(key => {
      const control = this.filialForm.get(key);
      if (control?.invalid) {
        control.markAsTouched();
      }
    });
  }

  cancelar() {
    this.cancelado.emit();
  }

  formatarCNPJ(event: any) {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length <= 14) {
      value = value.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
      this.filialForm.patchValue({ cnpj: value });
    }
  }

  limparCNPJ() {
    this.filialForm.patchValue({ cnpj: '' });
  }

  getErrorMessage(controlName: string): string {
    const control = this.filialForm.get(controlName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) return 'Campo obrigatório';
      if (control.errors['maxlength']) return `Máximo de ${control.errors['maxlength'].requiredLength} caracteres`;
      if (control.errors['email']) return 'Email inválido';
    }
    return '';
  }

  isFieldInvalid(controlName: string): boolean {
    const control = this.filialForm.get(controlName);
    return !!(control?.invalid && control.touched);
  }
}
