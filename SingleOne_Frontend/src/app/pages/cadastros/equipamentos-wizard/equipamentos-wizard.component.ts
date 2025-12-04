import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoriasApiService } from '../../../api/categorias/categorias-api.service';
import { ConfiguracoesApiService } from '../../../api/configuracoes/configuracoes-api.service';
import { UtilService } from '../../../util/util.service';

@Component({
  selector: 'app-equipamentos-wizard',
  templateUrl: './equipamentos-wizard.component.html',
  styleUrls: ['./equipamentos-wizard.component.scss']
})
export class EquipamentosWizardComponent implements OnInit {
  
  // Progress bar
  public currentStep = 1;
  public totalSteps = 3;
  
  // Formulário principal
  public wizardForm: FormGroup;
  
  // Dados para os dropdowns
  public categorias: any[] = [];
  public tiposRecursos: any[] = [];
  public fabricantes: any[] = [];
  
  // Estados de carregamento
  public loading = false;
  public submitting = false;
  
  // 🆕 Propriedades para os modais
  public mostrarModalTipo = false;
  public mostrarModalFabricante = false;
  public novoTipo = { descricao: '' };
  public novoFabricante = { descricao: '' };
  
  // 🔐 Sessão do usuário
  private session: any = {};

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private categoriasApi: CategoriasApiService,
    private configuracoesApi: ConfiguracoesApiService,
    private util: UtilService
  ) {
    this.wizardForm = this.fb.group({
      categoriaId: ['', Validators.required],
      tipoRecursoId: ['', Validators.required],
      fabricanteId: ['', Validators.required],
      modelo: ['', Validators.required],
      descricao: ['']
    });
  }

  ngOnInit() {
    this.session = this.util.getSession('usuario');
    this.carregarCategorias();
    this.configurarValidacoes();
  }

  /**
   * Carrega as categorias disponíveis (apenas liberadas)
   */
  private async carregarCategorias() {
    try {
      this.loading = true;
      
      const response = await this.categoriasApi.listarCategorias();
      if (response && response.data) {
        if (response.data.dados) {
          // Filtrar apenas categorias ativas/liberadas
          this.categorias = response.data.dados.filter((cat: any) => cat.ativo);
        } else {
          console.warn('[WIZARD] response.data.dados não encontrado');
          // Tentar usar response.data diretamente
          if (Array.isArray(response.data)) {
            this.categorias = response.data.filter((cat: any) => cat.ativo);
          }
        }
      } else {
        console.warn('[WIZARD] Resposta inválida da API');
      }
    } catch (error) {
      console.error('[WIZARD] Erro ao carregar categorias:', error);
      // Para debug, vamos criar dados de teste
      this.categorias = [
        { id: 1, nome: 'Teste Categoria 1', ativo: true },
        { id: 2, nome: 'Teste Categoria 2', ativo: true }
      ];
    } finally {
      this.loading = false;
    }
  }

  /**
   * Configura validações condicionais do formulário
   */
  private configurarValidacoes() {
    // Categoria é sempre obrigatória (primeira etapa)
    this.wizardForm.get('categoriaId')?.setValidators([Validators.required]);

    // Tipo de recurso só é obrigatório se categoria for selecionada
    this.wizardForm.get('categoriaId')?.valueChanges.subscribe(categoriaId => {
      if (categoriaId) {
        this.wizardForm.get('tipoRecursoId')?.setValidators([Validators.required]);
        this.carregarTiposRecursos(categoriaId);
      } else {
        this.wizardForm.get('tipoRecursoId')?.clearValidators();
        this.tiposRecursos = [];
      }
      this.wizardForm.get('tipoRecursoId')?.updateValueAndValidity();
    });

    // Fabricante só é obrigatório se tipo de recurso for selecionado
    this.wizardForm.get('tipoRecursoId')?.valueChanges.subscribe(tipoId => {
      if (tipoId) {
        this.wizardForm.get('fabricanteId')?.setValidators([Validators.required]);
        this.carregarFabricantes(tipoId);
      } else {
        this.wizardForm.get('fabricanteId')?.clearValidators();
        this.fabricantes = [];
      }
      this.wizardForm.get('fabricanteId')?.updateValueAndValidity();
    });
  }

  /**
   * Carrega tipos de recursos baseado na categoria selecionada
   */
  private async carregarTiposRecursos(categoriaId: number) {
    try {
      const response = await this.configuracoesApi.listarTiposRecursos("null", this.session.usuario.cliente, this.session.token);
      if (response && response.status === 200) {
        const todosTipos = response.data || [];
         if (categoriaId) {
                       // Filtrar tipos que pertencem à categoria selecionada
            this.tiposRecursos = todosTipos.filter((tipo: any) => {
              // ✅ CORRIGIDO: Usar categoriaId (com c minúsculo) e converter para number
              return Number(tipo.categoriaId) === Number(categoriaId);
            });
           
                       // Se não encontrou tipos para esta categoria, limpar seleções
            if (this.tiposRecursos.length === 0) {
              // Limpar seleção de tipo e fabricante
              this.wizardForm.patchValue({ 
                tipoRecursoId: '', 
                fabricanteId: '' 
              });
              this.fabricantes = [];
            }
         } else {
           this.tiposRecursos = todosTipos;
         }
      } else {
        console.warn('[WIZARD] Erro ao carregar tipos de recursos:', response);
        this.tiposRecursos = [];
      }
    } catch (error) {
      console.error('[WIZARD] Erro ao carregar tipos de recursos:', error);
      console.error('[WIZARD] Stack trace:', error.stack);
      this.tiposRecursos = [];
    }
  }

  /**
   * Carrega fabricantes baseado no tipo de recurso selecionado
   */
  private async carregarFabricantes(tipoId: number) {
    try {
      const response = await this.configuracoesApi.listarFabricantesPorTipoRecurso(tipoId, this.session.usuario.cliente, this.session.token);
      
      if (response && response.status === 200) {
        this.fabricantes = response.data || [];
      } else {
        console.warn('[WIZARD] Erro ao carregar fabricantes:', response);
        this.fabricantes = [];
      }
    } catch (error) {
      console.error('[WIZARD] Erro ao carregar fabricantes:', error);
      this.fabricantes = [];
    }
  }

  /**
   * Avança para o próximo passo
   */
  public nextStep() {
    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  /**
   * Volta para o passo anterior
   */
  public previousStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  /**
   * Vai para um passo específico
   */
  public goToStep(step: number) {
    if (step >= 1 && step <= this.totalSteps) {
      this.currentStep = step;
    }
  }

  /**
   * Verifica se pode avançar para o próximo passo
   */
  public canGoToNextStep(): boolean {
    switch (this.currentStep) {
      case 1:
        return this.wizardForm.get('categoriaId')?.valid || false;
      case 2:
        return this.wizardForm.get('tipoRecursoId')?.valid || false;
      case 3:
        return this.wizardForm.get('fabricanteId')?.valid || false;
      default:
        return false;
    }
  }

  /**
   * Verifica se o passo atual está completo
   */
  public isStepComplete(step: number): boolean {
    switch (step) {
      case 1:
        // Passo 1 só é completo se já passamos por ele E o campo está válido
        return this.currentStep > 1 && this.wizardForm.get('categoriaId')?.valid || false;
      case 2:
        // Passo 2 só é completo se já passamos por ele E o campo está válido
        return this.currentStep > 2 && this.wizardForm.get('tipoRecursoId')?.valid || false;
      case 3:
        // Passo 3 só é completo se já passamos por ele E o campo está válido
        return this.currentStep > 3 && this.wizardForm.get('fabricanteId')?.valid || false;
      default:
        return false;
    }
  }

  /**
   * Submete o formulário final
   */
  public async submitForm() {
    if (this.wizardForm.valid) {
      try {
        this.submitting = true;
        const dadosRecurso = {
          cliente: this.session.usuario.cliente,
          fabricante: this.wizardForm.get('fabricanteId')?.value,
          descricao: this.wizardForm.get('modelo')?.value,
          ativo: true
        };
        const response = await this.configuracoesApi.salvarModelo(dadosRecurso, this.session.token);
        
        if (response && response.status === 200) {
          this.util.exibirMensagemToast('Recurso cadastrado com sucesso!', 5000);
          
          // Redirecionar para cadastros
          this.router.navigate(['/cadastros']);
          
        } else {
          console.error('[WIZARD] Erro ao salvar modelo:', response);
          this.util.exibirFalhaComunicacao();
        }
        
      } catch (error) {
        console.error('[WIZARD] Erro ao salvar:', error);
        this.util.exibirFalhaComunicacao();
      } finally {
        this.submitting = false;
      }
    } else {
      this.wizardForm.markAllAsTouched();
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios.', 5000);
    }
  }

  /**
   * Cancela o wizard e volta para cadastros
   */
  public cancel() {
    this.router.navigate(['/cadastros']);
  }

  // 🆕 MÉTODOS DOS MODAIS

  /**
   * Abre o modal para novo tipo de recurso
   */
  public abrirModalNovoTipo() {
    this.novoTipo = { descricao: '' };
    this.mostrarModalTipo = true;
  }

  /**
   * Fecha o modal de novo tipo
   */
  public fecharModalTipo() {
    this.mostrarModalTipo = false;
    this.novoTipo = { descricao: '' };
  }

  /**
   * Salva um novo tipo de recurso
   */
  public async salvarNovoTipo() {
    if (!this.novoTipo.descricao.trim()) {
      return;
    }

    try {
      const dadosTipo = {
        descricao: this.novoTipo.descricao.trim(),
        categoriaId: this.wizardForm.get('categoriaId')?.value, // 🆕 INCLUIR CATEGORIA
        tipoequipamentosclientes: [{ cliente: this.session.usuario.cliente }]
      };
      const response = await this.configuracoesApi.salvarTipoRecurso(dadosTipo, this.session.token);
      
      if (response && response.status === 200) {
        await this.carregarTiposRecursos(this.wizardForm.get('categoriaId')?.value);
        
        // Selecionar o tipo recém-criado
        const tipoCriado = this.tiposRecursos.find(t => t.descricao === this.novoTipo.descricao.trim());
        if (tipoCriado) {
          this.wizardForm.patchValue({ tipoRecursoId: tipoCriado.id });
        }
        
        // Fechar modal
        this.fecharModalTipo();
        
        // Exibir mensagem de sucesso
        this.util.exibirMensagemToast('Tipo de recurso criado com sucesso!', 5000);
        
      } else {
        console.error('[WIZARD] Erro ao salvar tipo:', response);
        this.util.exibirFalhaComunicacao();
      }
      
    } catch (error) {
      console.error('[WIZARD] Erro ao salvar tipo:', error);
      this.util.exibirFalhaComunicacao();
    }
  }

  /**
   * Abre o modal para novo fabricante
   */
  public abrirModalNovoFabricante() {
    this.novoFabricante = { descricao: '' };
    this.mostrarModalFabricante = true;
  }

  /**
   * Fecha o modal de novo fabricante
   */
  public fecharModalFabricante() {
    this.mostrarModalFabricante = false;
    this.novoFabricante = { descricao: '' };
  }

  /**
   * Salva um novo fabricante
   */
  public async salvarNovoFabricante() {
    if (!this.novoFabricante.descricao.trim()) {
      return;
    }

    try {
      const dadosFabricante = {
        cliente: this.session.usuario.cliente,
        tipoequipamento: this.wizardForm.get('tipoRecursoId')?.value,
        descricao: this.novoFabricante.descricao.trim(),
        ativo: true
      };
      const response = await this.configuracoesApi.salvarFabricante(dadosFabricante, this.session.token);
      
      if (response && response.status === 200) {
        await this.carregarFabricantes(this.wizardForm.get('tipoRecursoId')?.value);
        
        // Selecionar o fabricante recém-criado
        const fabricanteCriado = this.fabricantes.find(f => f.descricao === this.novoFabricante.descricao.trim());
        if (fabricanteCriado) {
          this.wizardForm.patchValue({ fabricanteId: fabricanteCriado.id });
        }
        
        // Fechar modal
        this.fecharModalFabricante();
        
        // Exibir mensagem de sucesso
        this.util.exibirMensagemToast('Fabricante criado com sucesso!', 5000);
        
      } else {
        console.error('[WIZARD] Erro ao salvar fabricante:', response);
        this.util.exibirFalhaComunicacao();
      }
      
    } catch (error) {
      console.error('[WIZARD] Erro ao salvar fabricante:', error);
      this.util.exibirFalhaComunicacao();
    }
  }
}
