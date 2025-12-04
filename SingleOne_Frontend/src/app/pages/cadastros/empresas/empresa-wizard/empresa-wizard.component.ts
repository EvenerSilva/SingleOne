import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-empresa-wizard',
  templateUrl: './empresa-wizard.component.html',
  styleUrls: ['./empresa-wizard.component.scss']
})
export class EmpresaWizardComponent implements OnInit {
  
  // Progress bar
  public currentStep = 1;
  public totalSteps = 3;
  
  // Formulário principal
  public wizardForm: FormGroup;
  
  // Dados para os dropdowns
  public localizacoes: any[] = [];
  
  // Estados de carregamento
  public loading = false;
  public submitting = false;
  public cnpjCarregando = false;
  
  // 🆕 Propriedades para os modais
  public mostrarModalLocalizacao = false;
  public novaLocalizacao = { descricao: '', cidade: '', estado: '' };
  
  // 🏢 Dados da empresa
  public empresa: any = {
    filiais: [],
    centrosCusto: []
  };
  
  // 🏢 Nova filial
  public novaFilial = { nome: '', localizacaoId: '', centroCusto: '' };
  
  // 💰 Novo centro de custo
  public novoCentroCusto = { nome: '', descricao: '' };
  
  // 🔐 Sessão do usuário
  private session: any = {};

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private configuracoesApi: ConfiguracoesApiService,
    private util: UtilService
  ) {
    this.wizardForm = this.fb.group({
      razaosocial: ['', Validators.required],
      cnpj: ['', Validators.required],
      localizacaoId: ['', Validators.required],
      centroCustoPrincipal: ['']
    });
  }

  ngOnInit() {
    this.session = this.util.getSession('usuario');
    this.empresa.cliente = this.session.usuario.cliente;
    this.carregarLocalizacoes();
  }

  /**
   * Carrega as localizações disponíveis
   */
  private async carregarLocalizacoes() {
    try {
      this.loading = true;
      
      const response = await this.configuracoesApi.listarLocalidades(this.empresa.cliente, this.session.token);
      if (response && response.data) {
        // Filtrar apenas localizações ativas e remover duplicatas por ID e descrição
        const localizacoesAtivas = response.data.filter((loc: any) => loc.ativo);
        
        // Remover duplicatas usando Map - primeiro por ID, depois por descrição
        const localizacoesUnicasPorId = new Map();
        localizacoesAtivas.forEach((loc: any) => {
          if (!localizacoesUnicasPorId.has(loc.id)) {
            localizacoesUnicasPorId.set(loc.id, loc);
          }
        });
        
        // Remover duplicatas por descrição (caso haja IDs diferentes com mesma descrição)
        const localizacoesUnicasPorDescricao = new Map();
        Array.from(localizacoesUnicasPorId.values()).forEach((loc: any) => {
          const descricaoNormalizada = (loc.descricao || '').trim().toLowerCase();
          if (!localizacoesUnicasPorDescricao.has(descricaoNormalizada)) {
            localizacoesUnicasPorDescricao.set(descricaoNormalizada, loc);
          }
        });
        
        // Converter Map para Array e ordenar por descrição
        this.localizacoes = Array.from(localizacoesUnicasPorDescricao.values())
          .sort((a: any, b: any) => {
            const descA = (a.descricao || '').toLowerCase();
            const descB = (b.descricao || '').toLowerCase();
            return descA.localeCompare(descB);
          });
      }
    } catch (error) {
      console.error('[WIZARD-EMPRESA] Erro ao carregar localizações:', error);
      this.util.exibirFalhaComunicacao();
    } finally {
      this.loading = false;
    }
  }

  /**
   * Verifica se um passo está completo
   */
  public isStepComplete(step: number): boolean {
    switch (step) {
      case 1:
        return this.wizardForm.get('razaosocial')?.valid && 
               this.wizardForm.get('cnpj')?.valid && 
               this.wizardForm.get('localizacaoId')?.valid;
      case 2:
        return true; // Filiais e centros de custo são opcionais
      case 3:
        return this.wizardForm.valid;
      default:
        return false;
    }
  }

  /**
   * Verifica se pode ir para o próximo passo
   */
  public canGoToNextStep(): boolean {
    return this.isStepComplete(this.currentStep);
  }

  /**
   * Vai para o próximo passo
   */
  public nextStep() {
    if (this.canGoToNextStep() && this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  /**
   * Vai para o passo anterior
   */
  public previousStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  /**
   * Adiciona uma nova filial
   */
  public adicionarFilial() {
    if (this.novaFilial.nome.trim() && this.novaFilial.localizacaoId) {
      const localizacao = this.localizacoes.find(loc => loc.id === this.novaFilial.localizacaoId);
      const filial = {
        id: null,
        nome: this.novaFilial.nome.trim(),
        localizacaoId: this.novaFilial.localizacaoId,
        localizacao: localizacao ? `${localizacao.descricao} - ${localizacao.cidade}${localizacao.estado ? ', ' + localizacao.estado : ''}` : '',
        centroCusto: this.novaFilial.centroCusto.trim() || null,
        ativo: true,
        empresa: null, // Será definido quando salvar a empresa
        cliente: this.empresa.cliente
      };
      
      this.empresa.filiais.push(filial);
      this.limparNovaFilial();
    }
  }

  /**
   * Remove uma filial
   */
  public removerFilial(index: number) {
    this.empresa.filiais.splice(index, 1);
  }

  /**
   * Limpa o formulário de nova filial
   */
  private limparNovaFilial() {
    this.novaFilial = { nome: '', localizacaoId: '', centroCusto: '' };
  }

  /**
   * Adiciona um novo centro de custo global
   */
  public adicionarCentroCusto() {
    if (this.novoCentroCusto.nome.trim()) {
      const centro = {
        id: null,
        nome: this.novoCentroCusto.nome.trim(),
        descricao: this.novoCentroCusto.descricao.trim() || null,
        ativo: true,
        empresa: null, // Será definido quando salvar a empresa
        cliente: this.empresa.cliente
      };
      
      this.empresa.centrosCusto.push(centro);
      this.limparNovoCentroCusto();
    }
  }

  /**
   * Remove um centro de custo global
   */
  public removerCentroCusto(index: number) {
    this.empresa.centrosCusto.splice(index, 1);
  }

  /**
   * Limpa o formulário de novo centro de custo
   */
  private limparNovoCentroCusto() {
    this.novoCentroCusto = { nome: '', descricao: '' };
  }

  /**
   * Abre o modal para nova localização
   */
  public abrirModalNovaLocalizacao() {
    this.mostrarModalLocalizacao = true;
    this.novaLocalizacao = { descricao: '', cidade: '', estado: '' };
  }

  /**
   * Fecha o modal de nova localização
   */
  public fecharModalLocalizacao() {
    this.mostrarModalLocalizacao = false;
  }

  /**
   * Salva uma nova localização
   */
  public async salvarNovaLocalizacao() {
    if (this.novaLocalizacao.descricao.trim() && this.novaLocalizacao.cidade.trim() && this.novaLocalizacao.estado.trim()) {
      try {
        this.loading = true;
        
        const localizacao = {
          descricao: this.novaLocalizacao.descricao.trim(),
          cidade: this.novaLocalizacao.cidade.trim(),
          estado: this.novaLocalizacao.estado.trim(),
          ativo: true,
          cliente: this.empresa.cliente
        };
        
        const response = await this.configuracoesApi.salvarLocalidade(localizacao, this.session.token);
        
        if (response && response.data) {
          // Adiciona a nova localização à lista
          this.localizacoes.push(response.data);
          
          // Seleciona automaticamente a nova localização
          this.wizardForm.patchValue({ localizacaoId: response.data.id });
          
          this.fecharModalLocalizacao();
          this.util.exibirMensagemToast('Localização cadastrada com sucesso!', 3000);
        }
      } catch (error) {
        console.error('[WIZARD-EMPRESA] Erro ao salvar localização:', error);
        this.util.exibirFalhaComunicacao();
      } finally {
        this.loading = false;
      }
    }
  }

  /**
   * Formata a exibição de uma localização para o dropdown
   * Usa APENAS a descrição (que já vem formatada: "Cidade/Estado")
   */
  public formatarLocalizacaoDropdown(loc: any): string {
    if (!loc) return '';
    
    // IMPORTANTE: cidade e estado são IDs numéricos, não textos!
    // Usar APENAS a descrição que já vem formatada
    if (loc.descricao && loc.descricao.trim()) {
      return loc.descricao.trim();
    }
    
    return 'Localização sem nome';
  }

  /**
   * Formata a exibição de uma localização (versão genérica)
   */
  public formatarLocalizacao(loc: any): string {
    return this.formatarLocalizacaoDropdown(loc);
  }

  /**
   * Obtém o nome da localização selecionada
   */
  public getLocalizacaoNome(): string {
    const localizacaoId = this.wizardForm.get('localizacaoId')?.value;
    if (localizacaoId) {
      const localizacao = this.localizacoes.find(loc => loc.id == localizacaoId);
      if (localizacao) {
        return this.formatarLocalizacao(localizacao);
      }
    }
    return 'Não selecionada';
  }

  /**
   * Obtém o nome da localização por ID
   */
  public getLocalizacaoNomeById(localizacaoId: any): string {
    if (localizacaoId) {
      const localizacao = this.localizacoes.find(loc => loc.id == localizacaoId);
      if (localizacao) {
        return this.formatarLocalizacao(localizacao);
      }
    }
    return 'Não selecionada';
  }

  /**
   * Busca dados do CNPJ (simulação)
   */
  public async buscarDadosCNPJ() {
    const cnpj = this.wizardForm.get('cnpj')?.value;
    if (cnpj && cnpj.length === 18) { // CNPJ formatado tem 18 caracteres
      try {
        this.cnpjCarregando = true;
        // Aqui você pode implementar uma chamada para API de consulta de CNPJ
        // Por enquanto, apenas simula um delay
        await new Promise(resolve => setTimeout(resolve, 1000));
        
        // Simula preenchimento automático baseado no CNPJ
        // Em uma implementação real, você faria uma chamada para a API da Receita Federal
      } catch (error) {
        console.error('[WIZARD-EMPRESA] Erro ao buscar dados do CNPJ:', error);
      } finally {
        this.cnpjCarregando = false;
      }
    }
  }

  /**
   * Submete o formulário final
   */
  public async submitForm() {
    if (this.wizardForm.valid) {
      try {
        this.submitting = true;
        const formValues = this.wizardForm.value;
        const dadosParaSalvar = {
          nome: formValues.razaosocial,
          razaosocial: formValues.razaosocial,
          cnpj: formValues.cnpj,
          localizacao: formValues.localizacaoId,
          centroCustoPrincipal: formValues.centroCustoPrincipal || 'Centro Principal',
          cliente: this.empresa.cliente,
          filiais: this.empresa.filiais,
          centrosCusto: this.empresa.centrosCusto
        };
        
        const response = await this.configuracoesApi.salvarEmpresa(dadosParaSalvar, this.session.token);
        if (response && response.status === 200) {
          this.util.exibirMensagemToast('✅ Empresa cadastrada com sucesso!', 5000);
          setTimeout(() => {
            this.router.navigate(['/empresas']);
          }, 1500);
        } else {
          console.error('[WIZARD-EMPRESA] ❌ Resposta inesperada da API:', response);
          this.util.exibirMensagemPopUp('Erro ao salvar empresa. Verifique os dados e tente novamente.', false);
        }
      } catch (error: any) {
        console.error('[WIZARD-EMPRESA] ===== ERRO AO SALVAR =====');
        console.error('Erro completo:', error);
        console.error('Mensagem:', error?.message);
        console.error('Error response:', error?.error);
        
        // Mensagem mais específica
        let mensagemErro = 'Erro ao salvar empresa. ';
        if (error?.error?.mensagem) {
          mensagemErro += error.error.mensagem;
        } else if (error?.message) {
          mensagemErro += error.message;
        } else {
          mensagemErro += 'Verifique os dados e tente novamente.';
        }
        
        this.util.exibirMensagemPopUp(mensagemErro, false);
      } finally {
        this.submitting = false;
      }
    } else {
      console.warn('[WIZARD-EMPRESA] ⚠️ Formulário inválido!');
      console.warn('Campos com erro:');
      Object.keys(this.wizardForm.controls).forEach(key => {
        const control = this.wizardForm.get(key);
        if (control?.invalid) {
          console.warn(`  - ${key}:`, control.errors);
        }
      });
      
      this.util.exibirMensagemPopUp('Por favor, preencha todos os campos obrigatórios (Razão Social, CNPJ e Localização)', false);
    }
  }

  /**
   * Cancela o wizard
   */
  public cancel() {
    this.router.navigate(['/empresas']);
  }
}
