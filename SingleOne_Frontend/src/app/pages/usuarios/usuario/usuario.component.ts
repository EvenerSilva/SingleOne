import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UsuarioApiService } from 'src/app/api/usuarios/usuario-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-usuario',
  templateUrl: './usuario.component.html',
  styleUrls: ['./usuario.component.scss']
})
export class UsuarioComponent implements OnInit {

  public session:any = {};
  public usuario:any = {};
  public form: FormGroup;
  public clientes:any = [];
  public showPassword: boolean = false; // Controla visibilidade da senha
  public passwordStrength: 'weak' | 'medium' | 'strong' | null = null; // Força da senha
  public passwordStrengthText: string = ''; // Texto descritivo da força
  public passwordStrengthColor: string = ''; // Cor do indicador

  constructor(private fb: FormBuilder, private util: UtilService, private api: UsuarioApiService,
    private apiCli: ConfiguracoesApiService, private ar: ActivatedRoute, private route: Router) {
      this.form = this.fb.group({
        nome: ['', Validators.required],
        email: ['', Validators.required],
        senha: ['', Validators.required], // Será ajustado dinamicamente
        tipo: ['', Validators.required],
        adm: [''],
        operador: [''],
        consulta: [''],
        cliente: [''],
        twoFactorEnabled: [false]
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // - Super Usuário (su): pode gerenciar usuários de TODOS os clientes
    // - Administrador (adm): pode gerenciar usuários do seu próprio cliente
    // - Usuário comum: não pode gerenciar usuários
    this.usuario.Cliente = this.session.usuario.Cliente || this.session.usuario.cliente;
    this.util.aguardar(true);
    this.apiCli.listarClientes("null", this.session.token).then(res => {
      this.util.aguardar(false);
      this.clientes = res.data;
      if(this.route.url == "/meu-usuario") {
        this.usuario.id = this.session.usuario.Id || this.session.usuario.id;
        this.buscarPorId();
      }
      else {
        this.ar.paramMap.subscribe(param => {
          var parametro = param.get('id');
          if(parametro != null) {
            this.usuario.id = parametro;
            this.buscarPorId();
          } else {
            if (this.session.usuario.Su || this.session.usuario.su) {
              // Super usuário pode selecionar qualquer cliente
            } else if (this.session.usuario.Adm || this.session.usuario.adm) {
              // Administrador usa seu próprio cliente
              this.usuario.Cliente = this.session.usuario.Cliente || this.session.usuario.cliente;
            }
            
                  // Atualizar formulário com valores padrão para novo usuário
      this.form.patchValue({
        nome: '',
        email: '',
        senha: '',
        tipo: 'C', // Padrão: Consulta
        adm: false,
        operador: false,
        consulta: true,
        cliente: this.usuario.Cliente,
        twoFactorEnabled: false
      });
      
      // Para novos usuários, senha é obrigatória
      this.form.get('senha').setValidators([Validators.required]);
      this.form.get('senha').updateValueAndValidity();
          }
        })
      }
    })
  }

  buscarPorId() {
    this.util.aguardar(true);
    this.api.buscarPorId(this.usuario.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.usuario = res.data;
      
      // Debug das propriedades
      if(this.usuario.Adm || this.usuario.adm) {
        this.usuario.tipo = 'A';
      }
      else if(this.usuario.Operador || this.usuario.operador) {
        this.usuario.tipo = 'O';
      }
      else if(this.usuario.Consulta || this.usuario.consulta) {
        this.usuario.tipo = 'C';
      }
      this.form.patchValue({
        nome: this.usuario.Nome || this.usuario.nome || '',
        email: this.usuario.Email || this.usuario.email || '',
        senha: '', // Senha não é exibida por segurança
        tipo: this.usuario.tipo || '',
        adm: this.usuario.Adm || this.usuario.adm || false,
        operador: this.usuario.Operador || this.usuario.operador || false,
        consulta: this.usuario.Consulta || this.usuario.consulta || false,
        cliente: this.usuario.Cliente || this.usuario.cliente || '',
        twoFactorEnabled: this.usuario.TwoFactorEnabled || this.usuario.twoFactorEnabled || false
      });
      
      // Para usuários existentes, senha é opcional (pode ser deixada em branco para não alterar)
      this.form.get('senha').clearValidators();
      this.form.get('senha').updateValueAndValidity();
    }).catch(err => {
      console.error('❌ ERRO ao buscar usuário:', err);
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  salvar() {
  const senhaDigitada = this.form.get('senha').value;
  const isEditing = !!this.usuario.id;
  
  // Se é edição e senha está vazia, remover validação obrigatória temporariamente
  if (isEditing && (!senhaDigitada || senhaDigitada.trim() === '')) {
    this.form.get('senha').clearValidators();
    this.form.get('senha').updateValueAndValidity();
  } else if (!isEditing && (!senhaDigitada || senhaDigitada.trim() === '')) {
    // Se é novo usuário e senha está vazia, adicionar validação obrigatória
    this.form.get('senha').setValidators([Validators.required]);
    this.form.get('senha').updateValueAndValidity();
  }
  
  // Verificar se a senha fornecida é forte o suficiente (apenas para novos usuários)
  if (!isEditing && senhaDigitada && senhaDigitada.trim() !== '') {
    this.checkPasswordStrength(senhaDigitada);
    if (this.passwordStrength === 'weak') {
      this.util.exibirMensagemToast('Sua senha é muito fraca. Considere torná-la mais forte.', 5000);
    }
  }
    
    if(this.form.valid) {
      // Determinar permissões baseado no tipo
      if(this.usuario.tipo == 'A'){
        this.usuario.Adm = true;
        this.usuario.Operador = false;
        this.usuario.Consulta = false;
      }
      else if(this.usuario.tipo == 'O') {
        this.usuario.Operador = true;
        this.usuario.Adm = false;
        this.usuario.Consulta = false;
      }
      else if(this.usuario.tipo == 'C') {
        this.usuario.Consulta = true;
        this.usuario.Operador = false;
        this.usuario.Adm = false;
      }
      
      // Incluir status de 2FA
      this.usuario.TwoFactorEnabled = this.form.get('twoFactorEnabled').value;
      
      // Garantir que todas as propriedades estejam definidas
      this.usuario.Nome = this.form.get('nome').value;
      this.usuario.Email = this.form.get('email').value;
      
      // Só atualizar senha se foi fornecida (para usuários existentes)
      const senhaDigitada = this.form.get('senha').value;
      if (senhaDigitada && senhaDigitada.trim() !== '') {
        this.usuario.Senha = btoa(senhaDigitada); // Criptografar senha
      }
      
      // Definir cliente baseado na hierarquia
      if (this.session.usuario.Su || this.session.usuario.su) {
        // Super usuário pode selecionar cliente
        this.usuario.Cliente = this.form.get('cliente').value;
      } else if (this.session.usuario.Adm || this.session.usuario.adm) {
        // Administrador usa seu próprio cliente
        this.usuario.Cliente = this.session.usuario.Cliente || this.session.usuario.cliente;
      }
      this.util.aguardar(true);
      this.api.Salvar(this.usuario, this.session.token).then(res => {
        this.util.aguardar(false);
        
        // Verificar se é sucesso (200) ou erro (qualquer outro status)
        // O backend retorna HTTP 200 mas com status no corpo da resposta
        const statusResponse = res.data?.Status || res.data?.status;
        if(String(statusResponse) == "200") {
          this.util.exibirMensagemToast('Usuário salvo com sucesso!', 5000);
          this.route.navigate(['/usuarios']);
        } else {
          console.error('❌ Status não é 200:', statusResponse);
          
          // Verificar se é erro de e-mail duplicado
          if(String(statusResponse) == "200.1") {
            const mensagem = res.data?.Messagem || res.data?.mensagem || 'E-mail já cadastrado!';
            this.util.exibirMensagemToast(mensagem, 5000);
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }
      }).catch(err => {
        console.error('❌ ERRO ao salvar usuário:', err);
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
    } else {
      this.util.exibirMensagemToast('Você precisa preencher todo formulário antes de salvar.', 5000);
    }
  }

  // Método para alternar visibilidade da senha
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  // Método para verificar a força da senha
  checkPasswordStrength(password: string): void {
    if (!password || password.trim() === '') {
      this.passwordStrength = null;
      this.passwordStrengthText = '';
      this.passwordStrengthColor = '';
      return;
    }

    let score = 0;
    const feedback: string[] = [];

    // Critérios de força
    if (password.length >= 8) score += 1;
    else feedback.push('Mínimo 8 caracteres');

    if (/[a-z]/.test(password)) score += 1;
    else feedback.push('Adicione letras minúsculas');

    if (/[A-Z]/.test(password)) score += 1;
    else feedback.push('Adicione letras maiúsculas');

    if (/[0-9]/.test(password)) score += 1;
    else feedback.push('Adicione números');

    if (/[^A-Za-z0-9]/.test(password)) score += 1;
    else feedback.push('Adicione caracteres especiais');

    // Determinar força baseada no score
    if (score <= 2) {
      this.passwordStrength = 'weak';
      this.passwordStrengthText = 'Senha fraca';
      this.passwordStrengthColor = '#dc3545';
    } else if (score <= 4) {
      this.passwordStrength = 'medium';
      this.passwordStrengthText = 'Senha média';
      this.passwordStrengthColor = '#ffc107';
    } else {
      this.passwordStrength = 'strong';
      this.passwordStrengthText = 'Senha forte';
      this.passwordStrengthColor = '#28a745';
    }
  }

  // Método para obter dicas de melhoria da senha
  getPasswordTips(): string[] {
    const tips = [
      'Use pelo menos 8 caracteres',
      'Combine letras maiúsculas e minúsculas',
      'Inclua números',
      'Adicione caracteres especiais (!@#$%^&*)',
      'Evite informações pessoais óbvias'
    ];
    return tips;
  }

}
