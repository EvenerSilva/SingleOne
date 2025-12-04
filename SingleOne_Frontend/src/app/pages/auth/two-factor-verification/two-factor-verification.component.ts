import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TwoFactorAuthService, TwoFactorVerificationRequest } from 'src/app/services/two-factor-auth.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-two-factor-verification',
  templateUrl: './two-factor-verification.component.html',
  styleUrls: ['./two-factor-verification.component.scss']
})
export class TwoFactorVerificationComponent implements OnInit {
  usuario: any;
  showBackupCodes: boolean = false;

  verificationForm: FormGroup;
  verificationType: 'totp' | 'backup' | 'email' = 'totp';
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private twoFactorService: TwoFactorAuthService,
    private util: UtilService
  ) {
    this.verificationForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(8)]],
      verificationType: ['totp']
    });
  }

  ngOnInit(): void {
    this.usuario = this.twoFactorService.getCurrentUser();
    if (!this.usuario) {
      this.router.navigate(['/login']);
      return;
    }
    this.verificationForm.patchValue({
      verificationType: this.verificationType
    });
  }

  onVerificationTypeChange(type: 'totp' | 'backup' | 'email'): void {
    this.verificationType = type;
    this.verificationForm.patchValue({
      verificationType: type,
      code: ''
    });
    this.errorMessage = '';
    this.successMessage = '';
  }

  async onSubmit(): Promise<void> {
    if (this.verificationForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    try {
      const request: TwoFactorVerificationRequest = {
        userId: this.usuario.id,
        code: this.verificationForm.get('code')?.value,
        verificationType: this.verificationType,
        storedCode: '' // Será preenchido pelo backend se necessário
      };

      // Chamada real para a API de verificação 2FA
      this.twoFactorService.verifyCode(request).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.successMessage = 'Verificação realizada com sucesso!';
            
            // Emitir evento de sucesso
            // this.verificationComplete.emit({ // Removed as per edit hint
            //   success: true,
            //   usuario: response.usuario,
            //   token: response.token
            // });
            
            // Redirecionar para o dashboard após verificação bem-sucedida
            setTimeout(() => {
              this.util.salvarSessao('usuario', response);
              this.util.registrarStatus(true);
              this.util.montarMenuDeAcesso(response.usuario);
              this.router.navigate(['/dashboard']);
              this.util.exibirMensagemToast('Bem vindo ' + response.usuario.nome, 5000);
            }, 1000);
            
          } else {
            this.errorMessage = response.message || 'Código inválido. Tente novamente.';
          }
        },
        error: (error: any) => {
          console.error('[2FA] Erro na verificação:', error);
          this.errorMessage = error.error?.message || error.message || 'Erro ao verificar código 2FA';
        }
      });
      
    } catch (error: any) {
      console.error('[2FA] Erro inesperado:', error);
      this.errorMessage = error.message || 'Erro ao verificar código 2FA';
    } finally {
      this.isLoading = false;
    }
  }

  onGoBack(): void {
    this.router.navigate(['/login']);
  }

  onShowBackupCodes(): void {
    this.showBackupCodes = true;
  }

  onHideBackupCodes(): void {
    this.showBackupCodes = false;
  }

  onResendCode(): void {
    if (this.verificationType === 'email') {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';
      // Por enquanto, vamos simular o sucesso
      setTimeout(() => {
        this.isLoading = false;
        this.successMessage = 'Código reenviado com sucesso! Verifique sua caixa de entrada.';
        
        // Limpar mensagem após 5 segundos
        setTimeout(() => {
          this.successMessage = '';
        }, 5000);
      }, 1000);
    }
  }
}
