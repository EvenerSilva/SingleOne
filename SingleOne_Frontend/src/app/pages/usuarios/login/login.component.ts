import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UsuarioApiService } from '../../../api/usuarios/usuario-api.service';
import { ConfiguracoesApiService } from '../../../api/configuracoes/configuracoes-api.service';
import { UtilService } from '../../../util/util.service';
import { TwoFactorAuthService } from '../../../services/two-factor-auth.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  public usuario:any = {};
  public frmLogin: FormGroup;
  public tentativas:number = 0;
  private logoCarregada: boolean = false; // Evita recarregar logo desnecessariamente
  
  public showTwoFactorModal: boolean = false;
  
  public twoFactorErrorMessage: string = ''; // Mensagem de erro do 2FA
  public twoFactorSuccessMessage: string = ''; // Mensagem de sucesso do 2FA
  
  // Gerenciamento de mensagens do modal 2FA
  private setErrorMessage(message: string) {
    this.ngZone.run(() => {
      this.twoFactorErrorMessage = message;
      this.twoFactorSuccessMessage = '';
      this.cdr.detectChanges();
      
      // Forçar atualização via DOM para garantir visibilidade
      setTimeout(() => {
        const alertEl = document.getElementById('alert-2fa-message');
        if (alertEl) {
          alertEl.style.display = 'flex';
          alertEl.className = 'alert-2fa alert-error';
          alertEl.innerHTML = `<span class="icon">⚠️</span><span>${message}</span>`;
        }
      }, 10);
    });
  }
  
  private setSuccessMessage(message: string) {
    this.ngZone.run(() => {
      this.twoFactorSuccessMessage = message;
      this.twoFactorErrorMessage = '';
      this.cdr.detectChanges();
      
      // Forçar atualização via DOM para garantir visibilidade
      setTimeout(() => {
        const alertEl = document.getElementById('alert-2fa-message');
        if (alertEl) {
          alertEl.style.display = 'flex';
          alertEl.className = 'alert-2fa alert-success';
          alertEl.innerHTML = `<span class="icon">✅</span><span>${message}</span>`;
        }
      }, 10);
    });
  }
  
  private clearMessages() {
    this.ngZone.run(() => {
      this.twoFactorErrorMessage = '';
      this.twoFactorSuccessMessage = '';
      
      const alertEl = document.getElementById('alert-2fa-message');
      if (alertEl) {
        alertEl.style.display = 'none';
      }
    });
  }
  
  public clienteLogo: string | null = null; // URL da logo do cliente
  public showPassword: boolean = false; // Controla visibilidade da senha

  constructor(
    private fb: FormBuilder, 
    private route: Router, 
    private api: UsuarioApiService,
    private configuracoesApi: ConfiguracoesApiService,
    private util: UtilService,
    public twoFactorService: TwoFactorAuthService,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) { }

  ngOnInit(): void {
    this.frmLogin = this.fb.group({
      usuario: ['', Validators.required],
      senha: ['', Validators.required]
    });
    
    this.carregarLogoCliente();
  }

  private async carregarLogoCliente() {
    // Se já carregou com sucesso, reutilizar
    if (this.logoCarregada && this.clienteLogo) {
      console.log('[LOGIN] ✅ Logo já carregada, usando cache em memória:', this.clienteLogo);
      return;
    }

    // 1) Tentar cache do localStorage (até 1h) para evitar tela sem logo
    const logoCache = localStorage.getItem('cliente_logo_url');
    const logoTimestamp = localStorage.getItem('cliente_logo_timestamp');
    if (logoCache && logoTimestamp) {
      const cacheAge = Date.now() - parseInt(logoTimestamp);
      if (cacheAge < 3600000) { // 1 hora
        console.log('[LOGIN] ✅ Usando logo do cache (localStorage):', logoCache);
        this.clienteLogo = logoCache;
        this.logoCarregada = true;
        this.cdr.detectChanges();
        return;
      }
    }

    try {
      console.log('[LOGIN] 🔍 Iniciando busca da logo...');
      const response = await this.configuracoesApi.buscarLogoCliente();
      
      console.log('[LOGIN] 📦 Resposta recebida:', response);
      console.log('[LOGIN] 📦 response.data:', response?.data);
      
      // A resposta do axios vem em response.data
      // O backend retorna: { Logo: "/api/logos/{fileName}", ClienteNome: "...", Mensagem: "..." }
      // O axios já extrai o body da resposta HTTP, então response.data já é o objeto JSON
      const logoData = response?.data;
      
      console.log('[LOGIN] 📦 logoData:', logoData);
      
      // Aceitar tanto maiúsculas quanto minúsculas, e priorizar logoUrl (com timestamp) se disponível
      if (logoData && (logoData.Logo || logoData.logo || logoData.LogoUrl || logoData.logoUrl)) {
        // Priorizar logoUrl (com timestamp) se disponível, senão usar Logo/logo
        let logoUrl = logoData.LogoUrl || logoData.logoUrl || logoData.Logo || logoData.logo;
        
        console.log('[LOGIN] 🔗 URL da logo (antes):', logoUrl);
        
        // Se a URL já começa com /api/, verificar se precisa adicionar baseURL
        if (logoUrl && logoUrl.startsWith('/api/')) {
          // Se estiver em desenvolvimento (ng serve), usar baseURL completo
          // O proxy.conf.json já faz o proxy, mas para a tag <img> precisamos da URL completa
          if (!environment.production && environment.apiUrl) {
            // Remover /api do baseURL se existir e construir URL completa
            const baseUrl = environment.apiUrl.replace('/api', '');
            logoUrl = baseUrl + logoUrl;
            console.log('[LOGIN] 🔗 URL da logo (desenvolvimento):', logoUrl);
          } else {
            // Em produção, manter URL relativa (nginx faz proxy)
            // Garantir que começa com /api/
            if (!logoUrl.startsWith('/')) {
              logoUrl = '/' + logoUrl;
            }
            console.log('[LOGIN] 🔗 URL da logo (produção, relativa):', logoUrl);
            console.log('[LOGIN] 🔗 URL completa será:', window.location.origin + logoUrl);
          }
        } else if (logoUrl && !logoUrl.startsWith('http') && !logoUrl.startsWith('/')) {
          // Se não começa com /api/ nem com http, adicionar /api/logos/
          logoUrl = '/api/logos/' + logoUrl;
          console.log('[LOGIN] 🔗 URL corrigida para:', logoUrl);
        }
        
        console.log('[LOGIN] ✅ Logo definida:', logoUrl);
        this.clienteLogo = logoUrl;
        this.logoCarregada = true;

        // Salvar no cache para próximas visitas ao login
        if (logoUrl) {
          try {
            localStorage.setItem('cliente_logo_url', logoUrl);
            localStorage.setItem('cliente_logo_timestamp', Date.now().toString());
          } catch (e) {
            console.warn('[LOGIN] ⚠️ Não foi possível salvar logo no localStorage:', e);
          }
        }

        this.cdr.detectChanges();
      } else {
        console.warn('[LOGIN] ⚠️ Nenhuma logo encontrada na resposta');
        console.warn('[LOGIN] ⚠️ logoData:', logoData);
        this.clienteLogo = null;
      }
    } catch (error) {
      console.error('[LOGIN] ❌ Erro ao carregar logo do cliente:', error);
      console.error('[LOGIN] ❌ Detalhes do erro:', error);
      // Tentativa de último recurso: usar cache de até 24h
      if (logoCache && logoTimestamp) {
        const cacheAge = Date.now() - parseInt(logoTimestamp);
        if (cacheAge < 86400000) { // 24 horas
          console.log('[LOGIN] ✅ Usando logo do cache após erro:', logoCache);
          this.clienteLogo = logoCache;
          this.logoCarregada = true;
          this.cdr.detectChanges();
          return;
        }
      }

      this.clienteLogo = null;
    }
  }
  
  onLogoError(event: any): void {
    console.error('[LOGIN] ❌ Erro ao carregar imagem da logo:', event);
    console.error('[LOGIN] ❌ URL da logo que falhou:', this.clienteLogo);
    console.error('[LOGIN] ❌ Target do erro:', event?.target);
    
    // Tentar usar logoUrl com timestamp se disponível
    if (this.clienteLogo && !this.clienteLogo.includes('?v=')) {
      console.log('[LOGIN] 🔄 Tentando adicionar timestamp à URL...');
      const newUrl = `${this.clienteLogo}?v=${Date.now()}`;
      console.log('[LOGIN] 🔄 Nova URL:', newUrl);
      this.clienteLogo = newUrl;
      this.cdr.detectChanges();
    } else {
      // Se já tentou com timestamp e ainda falhou, limpar para mostrar fallback
      console.warn('[LOGIN] ⚠️ Logo falhou mesmo com timestamp, usando fallback');
      this.clienteLogo = null;
      this.cdr.detectChanges();
    }
  }
  
  onLogoLoad(): void {
    console.log('[LOGIN] ✅ Logo do cliente carregada com sucesso:', this.clienteLogo);
  }

  async entrar() {
    if(this.frmLogin.valid) {
      this.util.aguardar(true);
      
      const formValues = this.frmLogin.getRawValue();
      
      if (!formValues.usuario || !formValues.usuario.trim()) {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Por favor, preencha o campo de email', 5000);
        return;
      }
      
      const dadosLogin = {
        Email: formValues.usuario,
        Senha: formValues.senha
      };
      
      if (!formValues.senha || !formValues.senha.trim()) {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Por favor, preencha o campo de senha', 5000);
        return;
      }
      
      this.api.entrar(dadosLogin).then(res => {
        this.util.aguardar(false);
        
        if (res && res.data) {
          const twoFactorResult = this.twoFactorService.processLoginResponse(res.data);
          
          if (twoFactorResult.requires2FA) {
            const usuario = this.twoFactorService.getCurrentUser();
            this.showTwoFactorModal = true;
            this.cdr.detectChanges();
            
            setTimeout(() => {
              const modal = document.getElementById('modal-2fa-overlay');
              if (modal) {
                modal.style.cssText = `
                  position: fixed !important;
                  top: 0 !important;
                  left: 0 !important;
                  width: 100vw !important;
                  height: 100vh !important;
                  z-index: 2147483647 !important;
                  display: flex !important;
                  visibility: visible !important;
                  opacity: 1 !important;
                `;
              }
            }, 50);
            
            this.enviarCodigo2FAAutomaticamente(usuario);
          } else {
            if (res.data.token) {
              localStorage.setItem('token', res.data.token);
            }
            
            this.util.salvarSessao('usuario', res.data);
            this.util.registrarStatus(true);
            this.util.montarMenuDeAcesso(res.data.usuario);
            this.route.navigate(['/dashboard']);
            this.recarregarPagina();
            this.util.exibirMensagemToast('Bem vindo ' + res.data.usuario.nome, 5000);
          }
        } else {
          this.util.exibirFalhaComunicacao();
        }
      }).catch(err => {
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      })
    }
  }

  recarregarPagina() {
    location.replace("/dashboard");
  }

  closeTwoFactorModal() {
    this.showTwoFactorModal = false;
    this.clearMessages();
  }

  async verifyTwoFactorCode(code: string) {
    this.clearMessages();
    
    if (!code || code.trim().length < 6) {
      this.setErrorMessage('Por favor, digite um código de 6 dígitos');
      return false;
    }

    try {
      const usuario = this.twoFactorService.getCurrentUser();
      if (!usuario) {
        return false;
      }
      
      const request = {
        UserId: usuario.id,
        Code: code.trim(),
        VerificationType: 'email'
      };
      
      const response = await this.api.verifyTwoFactorCode(request);
      
      if (response && response.success) {
        this.setSuccessMessage('Código verificado! Redirecionando...');
        
        localStorage.setItem('token', response.token);
        
        if (response.usuario) {
          localStorage.setItem('usuario', JSON.stringify(response.usuario));
          this.util.salvarSessao('usuario', { usuario: response.usuario, token: response.token });
          this.util.montarMenuDeAcesso(response.usuario);
        }
        
        this.showTwoFactorModal = false;
        
        try {
          await this.route.navigate(['/dashboard']);
        } catch (navError) {
          window.location.href = '/dashboard';
        }
        
        return true;
      } else {
        this.setErrorMessage(response?.message || 'Código inválido');
        return false;
      }
      
    } catch (error: any) {
      const errorMessage = error?.response?.data?.message || 
                          error?.message || 
                          'Erro ao verificar código. Tente novamente.';
      
      this.setErrorMessage(errorMessage);
      return false;
    }
  }

  async resendTwoFactorCode() {
    this.clearMessages();
    
    try {
      const usuario = this.twoFactorService.getCurrentUser();
      if (!usuario) {
        this.setErrorMessage('Erro ao reenviar código. Por favor, faça login novamente.');
        return;
      }
      
      const request = {
        UserId: usuario.id,
        Email: usuario.email
      };
      
      const response = await this.api.sendTwoFactorCode(request);
      
      if (response && response.success) {
        this.setSuccessMessage('Novo código enviado! Verifique seu e-mail.');
      } else {
        this.setErrorMessage(response?.message || 'Erro ao reenviar código');
      }
      
    } catch (error) {
      this.setErrorMessage('Erro ao reenviar código. Tente novamente.');
    }
  }

  private async enviarCodigo2FAAutomaticamente(usuario: any) {
    try {
      const request = {
        UserId: usuario.id,
        Email: usuario.email
      };
      
      const response = await this.api.sendTwoFactorCode(request);
      if (response && response.success) {
        this.util.exibirMensagemToast('Código de verificação enviado para seu e-mail!', 5000);
      } else {
        this.util.exibirMensagemToast('Erro ao enviar código. Clique em "Reenviar Código".', 5000);
      }
    } catch (error) {
      this.util.exibirMensagemToast('Erro ao enviar código. Clique em "Reenviar Código".', 5000);
    }
  }

  // Método para alternar visibilidade da senha
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

}
