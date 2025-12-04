import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';
import { CnpjValidatorService } from 'src/app/util/cnpj-validator.service';

@Component({
  selector: 'app-cliente',
  templateUrl: './cliente.component.html',
  styleUrls: ['./cliente.component.scss']
})
export class ClienteComponent implements OnInit {

  private session:any = {};
  public cliente:any = {};
  public form: FormGroup;
  public cep:any = {};
  public logoPreview: string | null = null;
  public selectedFile: File | null = null;

  constructor(private fb: FormBuilder, private util: UtilService, private api: ConfiguracoesApiService,
    private ar: ActivatedRoute, private route: Router, private cnpjValidator: CnpjValidatorService) {
      this.form = this.fb.group({
        razaosocial: ['', Validators.required],
        cnpj: ['', Validators.required]
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente.empresa = this.session.usuario.empresa;
    
    this.ar.paramMap.subscribe(param => {
      var parametro = param.get('id');
      if(parametro != null) {
        this.cliente = JSON.parse(atob(parametro));
      }
    })
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
      this.util.aguardar(true);
      this.api.salvarCliente(this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          console.error('Status diferente de 200:', res.status);
          this.util.exibirFalhaComunicacao();
        }
        else {
          if (this.selectedFile) {
            const clienteIdParaUpload = this.cliente.id || res.data?.Id || res.data?.id;
            this.uploadLogo(clienteIdParaUpload);
          } else {
            this.util.exibirMensagemToast('Cliente salvo com sucesso!', 5000);
            this.route.navigate(['/clientes']);
          }
        }
      }).catch(err => {
        console.error('=== ERRO NA API ===');
        console.error('Erro completo:', err);
        console.error('Mensagem:', err.message);
        console.error('Status:', err.response?.status);
        console.error('Dados do erro:', err.response?.data);
        console.error('Headers do erro:', err.response?.headers);
        
        this.util.aguardar(false);
        
        // Verificar se é erro 400 (validação falhou - CNPJ inválido ou duplicado)
        if (err.response?.status === 400) {
          if (err.response.data && err.response.data.Mensagem) {
            // Exibir mensagem específica do backend
            this.util.exibirMensagemToast(err.response.data.Mensagem, 5000);
          } else {
            this.util.exibirMensagemToast('Erro de validação. Verifique os dados informados.', 5000);
          }
        }
        // Verificar se é erro 500 (problema no backend)
        else if (err.response?.status === 500) {
          console.error('Erro 500 - Problema no backend');
          this.util.exibirMensagemToast('Erro interno do servidor. Verifique os logs do backend.', 5000);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      })
    } else {
    }
  }

  onLogoSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      // Validar tipo de arquivo
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
      if (!allowedTypes.includes(file.type)) {
        this.util.exibirMensagemToast('Tipo de arquivo não permitido. Use apenas JPG, PNG ou GIF.', 5000);
        return;
      }

      // Validar tamanho (máximo 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.util.exibirMensagemToast('Arquivo muito grande. Tamanho máximo: 5MB.', 5000);
        return;
      }

      this.selectedFile = file;
      
      // Criar preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.logoPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  async uploadLogo(clienteId: number) {
    if (!this.selectedFile) return;

    try {
      this.util.aguardar(true);
      
      // Criar FormData para upload
      const formData = new FormData();
      formData.append('logo', this.selectedFile);
      formData.append('clienteId', clienteId.toString());

      const res = await this.api.uploadLogoCliente(formData, this.session.token);
      if (res.status === 200) {
        this.util.exibirMensagemToast('Cliente e logo salvos com sucesso!', 5000);
        this.route.navigate(['/clientes']);
      } else {
        this.util.exibirMensagemToast('Cliente salvo, mas erro ao fazer upload da logo.', 5000);
        this.route.navigate(['/clientes']);
      }
    } catch (error) {
      console.error('=== ERRO NO UPLOAD ===');
      console.error('Erro completo:', error);
      console.error('Response:', error.response);
      console.error('Status:', error.response?.status);
      console.error('Data:', error.response?.data);
      
      if (error.response?.status === 400) {
        this.util.exibirMensagemToast(`Erro no upload: ${error.response.data?.Mensagem || 'Dados inválidos'}`, 5000);
      } else {
        this.util.exibirMensagemToast('Cliente salvo, mas erro ao fazer upload da logo.', 5000);
      }
      this.route.navigate(['/clientes']);
    } finally {
      this.util.aguardar(false);
    }
  }

  removerLogo() {
    if (confirm('Tem certeza que deseja remover a logo?')) {
      this.cliente.logo = null;
      this.logoPreview = null;
      this.selectedFile = null;
    }
  }

  getLogoUrl(fileName: string): string {
    if (!fileName) return '';
    return `/api/logos/${fileName}`;
  }

}
