import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-parametros',
  templateUrl: './parametros.component.html',
  styleUrls: ['./parametros.component.scss']
})
export class ParametrosComponent implements OnInit {

  private session:any = {};
  public parametro:any = {};
  public form: FormGroup;
  public isAdmin: boolean = false;

  constructor(private fb: FormBuilder, private util: UtilService, private api: ConfiguracoesApiService, public route: Router) {
    this.form = this.fb.group({
      emailReporte: [''],
      emailDescontosEnabled: [false],
      smtpEnabled: [false],
      smtpHost: [''],
      smtpPort: ['', [Validators.required, Validators.min(1), Validators.max(65535)]],
      smtpLogin: [''],
      smtpPassword: [''],
      smtpEnableSSL: [false],
      smtpEmailFrom: [''],
      // Configurações de 2FA
      twoFactorEnabled: [{value: false, disabled: false}],
      twoFactorType: ['email'],
      twoFactorExpirationMinutes: [5, [Validators.required, Validators.min(1), Validators.max(60)]],
      twoFactorMaxAttempts: [3, [Validators.required, Validators.min(1), Validators.max(10)]],
      twoFactorLockoutMinutes: [15, [Validators.required, Validators.min(5), Validators.max(1440)]],
      twoFactorEmailTemplate: ['Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos.', [Validators.required]]
    })
    
    // Adicionar validações condicionais para E-mail de Descontos
    this.form.get('emailDescontosEnabled')?.valueChanges.subscribe(enabled => {
      this.atualizarValidacoesEmailDescontos();
    });

    // Adicionar validações condicionais para SMTP
    this.form.get('smtpEnableSSL')?.valueChanges.subscribe(sslEnabled => {
      this.atualizarValidacoesSMTP();
    });

    // Adicionar validações condicionais para habilitação SMTP
    this.form.get('smtpEnabled')?.valueChanges.subscribe(smtpEnabled => {
      this.atualizarValidacoesSMTP();
      this.atualizarValidacoes2FA();
    });

    // Adicionar validações condicionais para 2FA
    this.form.get('twoFactorEnabled')?.valueChanges.subscribe(twoFactorEnabled => {
      this.atualizarValidacoes2FA();
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.parametro.cliente = this.session.usuario.cliente;
    
    // Verificar se o usuário é administrador
    this.isAdmin = this.session.usuario.adm === true || this.session.usuario.su === true;
    
    // Inicializar valores padrão para SMTP
    this.parametro.smtpEnabled = false;
    this.parametro.smtpEnableSSL = false;
    this.parametro.smtpPort = 587;
    
    // Inicializar valores padrão para 2FA
    this.parametro.twoFactorEnabled = false;
    this.parametro.twoFactorType = 'email';
    this.parametro.twoFactorExpirationMinutes = 5;
    this.parametro.twoFactorMaxAttempts = 3;
    this.parametro.twoFactorLockoutMinutes = 15;
    this.parametro.twoFactorEmailTemplate = 'Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos.';
    
    // Atualizar o formulário com os valores padrão
    this.form.patchValue({
      smtpEnabled: false,
      smtpEnableSSL: false,
      smtpPort: 587,
      twoFactorEnabled: false,
      twoFactorType: 'email',
      twoFactorExpirationMinutes: 5,
      twoFactorMaxAttempts: 3,
      twoFactorLockoutMinutes: 15,
      twoFactorEmailTemplate: 'Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos.'
    });
    
    // Aplicar validações iniciais
    this.atualizarValidacoesSMTP();
    this.atualizarValidacoes2FA();
    
    this.buscarParametros();
  }

  buscarParametros(){
    this.util.aguardar(true);
    this.api.obterParametros(this.parametro.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else if(res.data != '') {
        this.parametro = res.data;
        
        // Garantir que os valores padrão sejam aplicados se não existirem
        if (this.parametro.emailDescontosEnabled === undefined || this.parametro.emailDescontosEnabled === null) {
          this.parametro.emailDescontosEnabled = false;
        }
        if (this.parametro.smtpEnabled === undefined || this.parametro.smtpEnabled === null) {
          this.parametro.smtpEnabled = false;
        }
        if (this.parametro.smtpEnableSSL === undefined || this.parametro.smtpEnableSSL === null) {
          this.parametro.smtpEnableSSL = false;
        }
        if (!this.parametro.smtpPort) {
          this.parametro.smtpPort = this.parametro.smtpEnableSSL ? 465 : 587;
        }
        
        // Garantir que os valores padrão de 2FA sejam aplicados se não existirem
        if (this.parametro.twoFactorEnabled === undefined || this.parametro.twoFactorEnabled === null) {
          this.parametro.twoFactorEnabled = false;
        }
        if (!this.parametro.twoFactorType) {
          this.parametro.twoFactorType = 'email';
        }
        if (!this.parametro.twoFactorExpirationMinutes) {
          this.parametro.twoFactorExpirationMinutes = 5;
        }
        if (!this.parametro.twoFactorMaxAttempts) {
          this.parametro.twoFactorMaxAttempts = 3;
        }
        if (!this.parametro.twoFactorLockoutMinutes) {
          this.parametro.twoFactorLockoutMinutes = 15;
        }
        if (!this.parametro.twoFactorEmailTemplate) {
          this.parametro.twoFactorEmailTemplate = 'Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos.';
        }
        
        // Atualizar o formulário com os valores
        this.form.patchValue({
          emailReporte: this.parametro.emailreporte, // Corrigido: usar emailreporte (lowercase)
          emailDescontosEnabled: this.parametro.emailDescontosEnabled,
          smtpEnabled: this.parametro.smtpEnabled,
          smtpEnableSSL: this.parametro.smtpEnableSSL,
          smtpPort: this.parametro.smtpPort,
          smtpHost: this.parametro.smtpHost,
          smtpLogin: this.parametro.smtpLogin,
          smtpPassword: this.parametro.smtpPassword,
          smtpEmailFrom: this.parametro.smtpEmailFrom,
          twoFactorEnabled: this.parametro.twoFactorEnabled,
          twoFactorType: this.parametro.twoFactorType,
          twoFactorExpirationMinutes: this.parametro.twoFactorExpirationMinutes,
          twoFactorMaxAttempts: this.parametro.twoFactorMaxAttempts,
          twoFactorLockoutMinutes: this.parametro.twoFactorLockoutMinutes,
          twoFactorEmailTemplate: this.parametro.twoFactorEmailTemplate
        });
        // Aplicar validações após carregar os dados
        this.atualizarValidacoesEmailDescontos();
        this.atualizarValidacoesSMTP();
        this.atualizarValidacoes2FA();
      }
    })
  }

  // Método para atualizar a porta baseada no SSL
  onSslChange() {
    // O FormControl já atualiza o valor automaticamente
    // Apenas atualizar as validações
    this.atualizarValidacoesSMTP();
  }

  // Método para quando a habilitação de E-mail para Descontos é alterada
  onEmailDescontosEnabledChange() {
    // O FormControl já atualiza o valor automaticamente
    // Apenas atualizar as validações
    this.atualizarValidacoesEmailDescontos();
  }

  // Método para quando a habilitação SMTP é alterada
  onSmtpEnabledChange() {
    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    const twoFactorEnabled = this.form.get('twoFactorEnabled')?.value;
    
    // ✅ VALIDAÇÃO: Se SMTP for desabilitado, desativar 2FA automaticamente
    if (!smtpEnabled && twoFactorEnabled) {
      this.form.patchValue({ twoFactorEnabled: false });
      this.util.exibirMensagemToast('2FA foi desativado automaticamente porque SMTP foi desabilitado.', 4000);
    }
    
    // Atualizar as validações
    this.atualizarValidacoesSMTP();
    this.atualizarValidacoes2FA();
  }

  // Método para quando a habilitação 2FA é alterada
  onTwoFactorEnabledChange() {
    const twoFactorEnabled = this.form.get('twoFactorEnabled')?.value;
    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    
    // ✅ VALIDAÇÃO: Não permitir ativar 2FA se SMTP não estiver habilitado
    if (twoFactorEnabled && !smtpEnabled) {
      this.form.patchValue({ twoFactorEnabled: false });
      this.util.exibirMensagemToast('SMTP deve estar habilitado para ativar 2FA.', 3000);
      return;
    }
    
    // Atualizar as validações
    this.atualizarValidacoes2FA();
  }

  // Método para atualizar validações condicionais do E-mail para Descontos
  private atualizarValidacoesEmailDescontos() {
    const emailReporteControl = this.form.get('emailReporte');
    
    // Usar o valor do FormControl em vez do modelo
    const emailDescontosEnabled = this.form.get('emailDescontosEnabled')?.value;
    
    if (emailDescontosEnabled) {
      // Se E-mail para Descontos estiver habilitado, tornar o campo obrigatório
      emailReporteControl?.setValidators([Validators.required, Validators.email]);
    } else {
      // Se E-mail para Descontos estiver desabilitado, tornar o campo opcional
      emailReporteControl?.clearValidators();
    }
    
    // Atualizar validações
    emailReporteControl?.updateValueAndValidity();
    
    // Atualizar o status do formulário
    this.form.updateValueAndValidity();
  }

  // Método para atualizar validações condicionais do SMTP
  private atualizarValidacoesSMTP() {
    const smtpHostControl = this.form.get('smtpHost');
    const smtpPortControl = this.form.get('smtpPort');
    const smtpLoginControl = this.form.get('smtpLogin');
    const smtpPasswordControl = this.form.get('smtpPassword');
    const smtpEmailFromControl = this.form.get('smtpEmailFrom');
    
    // Usar o valor do FormControl em vez do modelo
    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    
    if (smtpEnabled) {
      // Se SMTP estiver habilitado, tornar os campos obrigatórios
      smtpHostControl?.setValidators([Validators.required]);
      smtpPortControl?.setValidators([Validators.required, Validators.min(1), Validators.max(65535)]);
      smtpLoginControl?.setValidators([Validators.required]);
      smtpPasswordControl?.setValidators([Validators.required]);
      smtpEmailFromControl?.setValidators([Validators.required, Validators.email]);
    } else {
      // Se SMTP estiver desabilitado, tornar os campos opcionais
      smtpHostControl?.clearValidators();
      smtpPortControl?.clearValidators();
      smtpLoginControl?.clearValidators();
      smtpPasswordControl?.clearValidators();
      smtpEmailFromControl?.clearValidators();
    }
    
    // Atualizar validações
    smtpHostControl?.updateValueAndValidity();
    smtpPortControl?.updateValueAndValidity();
    smtpLoginControl?.updateValueAndValidity();
    smtpPasswordControl?.updateValueAndValidity();
    smtpEmailFromControl?.updateValueAndValidity();
    
    // Atualizar o status do formulário
    this.form.updateValueAndValidity();
  }

  // Método para atualizar validações condicionais do 2FA
  private atualizarValidacoes2FA() {
    const twoFactorTypeControl = this.form.get('twoFactorType');
    const twoFactorExpirationMinutesControl = this.form.get('twoFactorExpirationMinutes');
    const twoFactorMaxAttemptsControl = this.form.get('twoFactorMaxAttempts');
    const twoFactorLockoutMinutesControl = this.form.get('twoFactorLockoutMinutes');
    const twoFactorEmailTemplateControl = this.form.get('twoFactorEmailTemplate');
    const twoFactorEnabledControl = this.form.get('twoFactorEnabled');
    
    // Usar o valor do FormControl em vez do modelo
    const twoFactorEnabled = this.form.get('twoFactorEnabled')?.value;
    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    
    // Controlar o estado disabled do campo 2FA baseado no SMTP
    // 2FA só pode estar ativo se SMTP estiver habilitado
    // Usar emitEvent: false para evitar loop infinito
    if (twoFactorEnabledControl) {
      if (!smtpEnabled) {
        // SMTP desabilitado: desabilitar campo 2FA e desativar 2FA se estiver ativo
        twoFactorEnabledControl.disable({ emitEvent: false });
        if (twoFactorEnabled) {
          // Se 2FA estiver ativo e SMTP for desabilitado, desativar 2FA automaticamente
          this.form.patchValue({ twoFactorEnabled: false }, { emitEvent: false });
        }
      } else {
        // SMTP habilitado: permitir ativar/desativar 2FA
        twoFactorEnabledControl.enable({ emitEvent: false });
      }
    }
    
    if (twoFactorEnabled && smtpEnabled) {
      // Se 2FA estiver habilitado e SMTP também, tornar os campos obrigatórios
      twoFactorTypeControl?.setValidators([Validators.required]);
      twoFactorExpirationMinutesControl?.setValidators([Validators.required, Validators.min(1), Validators.max(60)]);
      twoFactorMaxAttemptsControl?.setValidators([Validators.required, Validators.min(1), Validators.max(10)]);
      twoFactorLockoutMinutesControl?.setValidators([Validators.required, Validators.min(5), Validators.max(1440)]);
      twoFactorEmailTemplateControl?.setValidators([Validators.required]);
    } else {
      // Se 2FA estiver desabilitado ou SMTP não estiver habilitado, tornar os campos opcionais
      twoFactorTypeControl?.clearValidators();
      twoFactorExpirationMinutesControl?.clearValidators();
      twoFactorMaxAttemptsControl?.clearValidators();
      twoFactorLockoutMinutesControl?.clearValidators();
      twoFactorEmailTemplateControl?.clearValidators();
    }
    
    // Atualizar validações
    twoFactorTypeControl?.updateValueAndValidity();
    twoFactorExpirationMinutesControl?.updateValueAndValidity();
    twoFactorMaxAttemptsControl?.updateValueAndValidity();
    twoFactorLockoutMinutesControl?.updateValueAndValidity();
    twoFactorEmailTemplateControl?.updateValueAndValidity();
    
    // Atualizar o status do formulário
    this.form.updateValueAndValidity();
  }

  // Método para verificar se o formulário SMTP é válido
  isFormSMTPValido(): boolean {
    // Usar o valor do FormControl em vez do modelo
    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    
    if (!smtpEnabled) {
      return true; // Se SMTP não estiver habilitado, não precisa validar
    }
    
    // Verificar se todos os campos obrigatórios estão preenchidos
    const smtpHost = this.form.get('smtpHost')?.value;
    const smtpPort = this.form.get('smtpPort')?.value;
    const smtpLogin = this.form.get('smtpLogin')?.value;
    const smtpPassword = this.form.get('smtpPassword')?.value;
    const smtpEmailFrom = this.form.get('smtpEmailFrom')?.value;
    
    return smtpHost && smtpPort && smtpLogin && smtpPassword && smtpEmailFrom;
  }

  // Método para testar a conexão SMTP
  testarConexaoSMTP() {
    // Usar o valor do FormControl em vez do modelo
    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    
    if (!smtpEnabled) {
      this.util.exibirMensagemToast('Habilite o SMTP para testar a conexão.', 3000);
      return;
    }

    if (!this.isFormSMTPValido()) {
      this.util.exibirMensagemToast('Preencha todos os campos obrigatórios antes de testar.', 3000);
      return;
    }

    // Criar objeto temporário com os valores do FormControl para o teste
    const parametrosTeste = {
      smtpEnabled: this.form.get('smtpEnabled')?.value,
      smtpHost: this.form.get('smtpHost')?.value,
      smtpPort: this.form.get('smtpPort')?.value,
      smtpLogin: this.form.get('smtpLogin')?.value,
      smtpPassword: this.form.get('smtpPassword')?.value,
      smtpEmailFrom: this.form.get('smtpEmailFrom')?.value,
      smtpEnableSSL: this.form.get('smtpEnableSSL')?.value
    };

    this.util.aguardar(true);
    
    // Chamar o endpoint real para testar a conexão SMTP
    this.api.testarConexaoSMTP(parametrosTeste, this.session.token).then(res => {
      this.util.aguardar(false);
      
      if (res.status === 200) {
        // Sucesso
        this.util.exibirMensagemToast(res.data.message || 'Conexão SMTP testada com sucesso!', 5000);
      } else {
        // Erro na comunicação
        this.util.exibirFalhaComunicacao();
      }
    }).catch(err => {
      this.util.aguardar(false);
      
      if (err.response && err.response.status === 400) {
        // Erro de validação ou configuração SMTP
        const errorMessage = err.response.data?.message || 'Erro ao testar conexão SMTP';
        this.util.exibirMensagemToast(errorMessage, 5000);
      } else {
        // Erro de comunicação
        this.util.exibirFalhaComunicacao();
      }
    });
  }

  // 🚀 SALVAR SEÇÕES INDIVIDUAIS
  salvarEmailDescontos() {
    // Validar seção de e-mail para descontos
    if (this.form.get('emailDescontosEnabled')?.value && !this.form.get('emailReporte')?.value) {
      this.util.exibirMensagemToast('Preencha o e-mail para descontos antes de salvar.', 3000);
      return;
    }

    // Preparar payload para e-mail de descontos
    // ✅ Busca sempre pelo Cliente (não depende do ID, funciona em novas implantações)
    const payloadEmailDescontos = {
      id: this.parametro?.id || 0, // Opcional: usado apenas como fallback
      cliente: this.session?.usuario?.cliente, // ✅ PRIORIDADE: busca sempre pelo Cliente
      emailDescontosEnabled: this.form.get('emailDescontosEnabled')?.value,
      emailreporte: this.form.get('emailReporte')?.value || '',
      emailReporte: this.form.get('emailReporte')?.value || '' // compatibilidade
    };

    this.util.aguardar(true);
    this.api.salvarParametro(payloadEmailDescontos, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        const mensagem = payloadEmailDescontos.emailDescontosEnabled 
          ? 'Configurações de e-mail para descontos salvas com sucesso!' 
          : 'E-mail para descontos desabilitado com sucesso!';
        this.util.exibirMensagemToast(mensagem, 5000);
        // Atualizar o objeto local (incluindo ID se retornado)
        if (res.data?.id) {
          this.parametro.id = res.data.id;
        }
        this.parametro.emailDescontosEnabled = payloadEmailDescontos.emailDescontosEnabled;
        this.parametro.emailreporte = payloadEmailDescontos.emailreporte;
        this.parametro.emailReporte = payloadEmailDescontos.emailReporte;
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(err => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  salvarSMTP() {
    // Validar seção SMTP
    if (this.form.get('smtpEnabled')?.value && !this.isFormSMTPValido()) {
      this.util.exibirMensagemToast('Preencha todos os campos SMTP obrigatórios antes de salvar.', 3000);
      return;
    }

    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    const twoFactorEnabled = this.form.get('twoFactorEnabled')?.value;

    // Se SMTP for desabilitado e 2FA estiver ativo, desativar 2FA automaticamente
    if (!smtpEnabled && twoFactorEnabled) {
      this.form.patchValue({ twoFactorEnabled: false });
      this.util.exibirMensagemToast('2FA foi desativado automaticamente porque SMTP foi desabilitado.', 4000);
    }

    // Preparar payload para SMTP
    // ✅ Busca sempre pelo Cliente (não depende do ID, funciona em novas implantações)
    const payloadSMTP = {
      id: this.parametro?.id || 0, // Opcional: usado apenas como fallback
      cliente: this.session?.usuario?.cliente, // ✅ PRIORIDADE: busca sempre pelo Cliente
      smtpEnabled: smtpEnabled,
      smtpEnableSSL: this.form.get('smtpEnableSSL')?.value || false,
      smtpHost: this.form.get('smtpHost')?.value || '',
      smtpPort: this.form.get('smtpPort')?.value || 587,
      smtpLogin: this.form.get('smtpLogin')?.value || '',
      smtpPassword: this.form.get('smtpPassword')?.value || '',
      smtpEmailFrom: this.form.get('smtpEmailFrom')?.value || '',
      // Se SMTP for desabilitado, também desativar 2FA
      twoFactorEnabled: smtpEnabled ? twoFactorEnabled : false
    };

    this.util.aguardar(true);
    this.api.salvarParametro(payloadSMTP, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        const mensagem = payloadSMTP.smtpEnabled 
          ? 'Configurações SMTP salvas com sucesso!' 
          : 'SMTP desabilitado com sucesso!';
        this.util.exibirMensagemToast(mensagem, 5000);
        // Atualizar o objeto local (incluindo ID se retornado)
        if (res.data?.id) {
          this.parametro.id = res.data.id;
        }
        Object.assign(this.parametro, payloadSMTP);
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(err => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  salvar2FA() {
    // Validar seção 2FA
    const twoFactorEnabled = this.form.get('twoFactorEnabled')?.value;
    const smtpEnabled = this.form.get('smtpEnabled')?.value;

    if (twoFactorEnabled && !smtpEnabled) {
      this.util.exibirMensagemToast('SMTP deve estar habilitado para ativar 2FA.', 3000);
      return;
    }

    if (twoFactorEnabled && smtpEnabled) {
      const twoFactorType = this.form.get('twoFactorType')?.value;
      const twoFactorExpirationMinutes = this.form.get('twoFactorExpirationMinutes')?.value;
      const twoFactorMaxAttempts = this.form.get('twoFactorMaxAttempts')?.value;
      const twoFactorLockoutMinutes = this.form.get('twoFactorLockoutMinutes')?.value;
      const twoFactorEmailTemplate = this.form.get('twoFactorEmailTemplate')?.value;

      if (!twoFactorType || !twoFactorExpirationMinutes || 
          !twoFactorMaxAttempts || !twoFactorLockoutMinutes || 
          !twoFactorEmailTemplate) {
        this.util.exibirMensagemToast('Preencha todos os campos de 2FA obrigatórios antes de salvar.', 3000);
        return;
      }
    }

    // Preparar payload para 2FA
    // ✅ Busca sempre pelo Cliente (não depende do ID, funciona em novas implantações)
    const payload2FA = {
      id: this.parametro?.id || 0, // Opcional: usado apenas como fallback
      cliente: this.session?.usuario?.cliente, // ✅ PRIORIDADE: busca sempre pelo Cliente
      twoFactorEnabled: twoFactorEnabled,
      twoFactorType: this.form.get('twoFactorType')?.value || 'email',
      twoFactorExpirationMinutes: this.form.get('twoFactorExpirationMinutes')?.value || 5,
      twoFactorMaxAttempts: this.form.get('twoFactorMaxAttempts')?.value || 3,
      twoFactorLockoutMinutes: this.form.get('twoFactorLockoutMinutes')?.value || 15,
      twoFactorEmailTemplate: this.form.get('twoFactorEmailTemplate')?.value || 'Seu código de verificação é: {CODE}. Este código expira em {EXPIRATION} minutos.'
    };

    this.util.aguardar(true);
    this.api.salvarParametro(payload2FA, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        const mensagem = payload2FA.twoFactorEnabled 
          ? 'Configurações de 2FA salvas com sucesso!' 
          : '2FA desabilitado com sucesso!';
        this.util.exibirMensagemToast(mensagem, 5000);
        // Atualizar o objeto local (incluindo ID se retornado)
        if (res.data?.id) {
          this.parametro.id = res.data.id;
        }
        Object.assign(this.parametro, payload2FA);
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(err => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

  // 🔄 SALVAR GERAL (mantido para compatibilidade)
  salvar() {
    // Validar formulário completo
    if (!this.form.valid) {
      this.util.exibirMensagemToast('Por favor, corrija os erros no formulário antes de salvar.', 3000);
      return;
    }

    const smtpEnabled = this.form.get('smtpEnabled')?.value;
    let twoFactorEnabled = this.form.get('twoFactorEnabled')?.value;

    // ✅ VALIDAÇÃO: 2FA só pode estar ativo se SMTP estiver habilitado
    if (twoFactorEnabled && !smtpEnabled) {
      this.util.exibirMensagemToast('SMTP deve estar habilitado para ativar 2FA.', 3000);
      return;
    }

    // Se SMTP for desabilitado, garantir que 2FA também esteja desabilitado
    if (!smtpEnabled) {
      twoFactorEnabled = false;
    }

    // Preparar payload completo
    // ✅ Busca sempre pelo Cliente (não depende do ID, funciona em novas implantações)
    const payloadCompleto = {
      id: this.parametro?.id || 0, // Opcional: usado apenas como fallback
      cliente: this.session?.usuario?.cliente, // ✅ PRIORIDADE: busca sempre pelo Cliente
      emailDescontosEnabled: this.form.get('emailDescontosEnabled')?.value,
      emailreporte: this.form.get('emailReporte')?.value,
      emailReporte: this.form.get('emailReporte')?.value,
      smtpEnabled: smtpEnabled,
      smtpEnableSSL: this.form.get('smtpEnableSSL')?.value,
      smtpHost: this.form.get('smtpHost')?.value,
      smtpPort: this.form.get('smtpPort')?.value,
      smtpLogin: this.form.get('smtpLogin')?.value,
      smtpPassword: this.form.get('smtpPassword')?.value,
      smtpEmailFrom: this.form.get('smtpEmailFrom')?.value,
      twoFactorEnabled: twoFactorEnabled,
      twoFactorType: this.form.get('twoFactorType')?.value,
      twoFactorExpirationMinutes: this.form.get('twoFactorExpirationMinutes')?.value,
      twoFactorMaxAttempts: this.form.get('twoFactorMaxAttempts')?.value,
      twoFactorLockoutMinutes: this.form.get('twoFactorLockoutMinutes')?.value,
      twoFactorEmailTemplate: this.form.get('twoFactorEmailTemplate')?.value
    };

    this.util.aguardar(true);
    this.api.salvarParametro(payloadCompleto, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status === 200) {
        this.util.exibirMensagemToast('Todas as configurações foram salvas com sucesso!', 5000);
        // Atualizar o objeto local (incluindo ID se retornado)
        if (res.data?.id) {
          this.parametro.id = res.data.id;
        }
        Object.assign(this.parametro, payloadCompleto);
      } else {
        this.util.exibirFalhaComunicacao();
      }
    }).catch(err => {
      this.util.aguardar(false);
      this.util.exibirFalhaComunicacao();
    });
  }

}
