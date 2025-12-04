import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';
import { CnpjValidatorService } from 'src/app/util/cnpj-validator.service';

@Component({
  selector: 'app-fornecedor',
  templateUrl: './fornecedor.component.html',
  styleUrls: ['./fornecedor.component.scss']
})
export class FornecedorComponent implements OnInit {

  @Input() fornecedor: any = {};
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() fornecedorSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session: any = {};
  public form: FormGroup;
  public cep: any = {};
  public carregando: boolean = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: ConfiguracoesApiService,
    private cnpjValidator: CnpjValidatorService) {
      this.form = this.fb.group({
        razaosocial: ['', Validators.required],
        cnpj: ['', Validators.required],
        destinadorResiduos: [false] // NOVO: Campo para marcar se é destinador de resíduos
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    if (!this.fornecedor) {
      this.fornecedor = {};
    }
    
    // Definir cliente se for criação
    if (this.modo === 'criar') {
      this.fornecedor.cliente = this.session.usuario.cliente;
    }
    
    // Se for edição, preencher o formulário
    if (this.modo === 'editar' && this.fornecedor?.id) {
      this.form.patchValue({
        razaosocial: this.fornecedor.nome || this.fornecedor.razaosocial,
        cnpj: this.fornecedor.cnpj,
        destinadorResiduos: this.fornecedor.destinadorResiduos || false // NOVO
      });
    }
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Novo Fornecedor' : 'Editar Fornecedor';
  }

  getBotaoTexto(): string {
    return this.modo === 'criar' ? 'Criar' : 'Salvar';
  }

  formatarCNPJ(event: any) {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length <= 14) {
      value = value.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
      this.form.patchValue({ cnpj: value });
    }
  }

  cancelar() {
    this.cancelado.emit();
  }

  salvar() {
    if(this.form.valid) {
      // Validação de CNPJ antes de enviar para a API
      const cnpj = this.form.get('cnpj')?.value;
      if (cnpj && !this.cnpjValidator.isValid(cnpj)) {
        const errorMessage = this.cnpjValidator.getErrorMessage(cnpj);
        this.util.exibirMensagemToast(`CNPJ inválido: ${errorMessage}`, 5000);
        return;
      }
      
      this.carregando = true;
      // Preparar dados para envio
      const dadosParaSalvar = {
        id: this.fornecedor.id || 0,
        nome: this.form.get('razaosocial')?.value,
        cnpj: this.form.get('cnpj')?.value,
        cliente: this.fornecedor.cliente,
        ativo: true,
        destinadorResiduos: this.form.get('destinadorResiduos')?.value || false // NOVO
      };
      
      this.api.salvarFornecedor(dadosParaSalvar, this.session.token).then(res => {
        this.carregando = false;
        
        // Se chegou aqui, é sucesso (status 200)
        if (res.data && res.data.Mensagem) {
          this.util.exibirMensagemToast(res.data.Mensagem, 5000);
        } else {
          this.util.exibirMensagemToast('Fornecedor salvo com sucesso!', 5000);
        }
        
        // Emitir evento de sucesso
        this.fornecedorSalvo.emit(res.data);
      }).catch(err => {
        console.error('[FORNECEDOR] ❌ Erro ao salvar:', err);
        console.error('[FORNECEDOR] ❌ Erro completo:', err.response || err);
        console.error('[FORNECEDOR] ❌ Status do erro:', err.response?.status);
        console.error('[FORNECEDOR] ❌ Mensagem do erro:', err.response?.data);
        
        this.carregando = false;
        
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
      })
    }
  }

}
