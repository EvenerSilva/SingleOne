import { Component, OnInit, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'app-colaborador',
  templateUrl: './colaborador.component.html',
  styleUrls: ['./colaborador.component.scss']
})
export class ColaboradorComponent implements OnInit, OnChanges {

  @Input() colaborador: any = null;
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() colaboradorSalvo = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  private session:any = {};
  public colaboradorLocal:any = {};
  public empresas:any = [];
  public centrocustos:any = [];
  private preencherFormularioEmAndamento: boolean = false;
  public localidades:any = [];
  public filiais:any = [];
  public colaboradores:any = [];
  public colaboradoresFiltrados:any = [];
  public pesquisaSuperior = new FormControl('');
  
  // 🎯 PROPRIEDADES PARA DESLIGAMENTO
  public modalReprogramarAberto: boolean = false;
  public novaDataDesligamento: string = '';
  public motivoReprogramacao: string = '';
  public dataAtual: string = '';
  
  // 🎯 PROPRIEDADES PARA REATIVAÇÃO
  public modalReativarAberto: boolean = false;
  public novaDataTermino: string = '';
  public motivoReativacao: string = '';
  public form: FormGroup;
  public isModal = false;
  public carregando = false;
  
  // Novas propriedades para seções expansíveis
  public secaoAdicionalExpanded = false;
  public camposAvancadosExpanded = true; // ✅ CORREÇÃO: Expandir por padrão
  public mostrarCamposAvancados = true; // ✅ CORREÇÃO: Sempre mostrar campos avançados

  constructor(private fb: FormBuilder, private util: UtilService, private api: ColaboradorApiService,
    private apiCad: ConfiguracoesApiService, private ar: ActivatedRoute, private route: Router) {
      this.form = this.fb.group({
        nome: ['', Validators.required],
        cpf: ['', Validators.required],
        matricula: ['', Validators.required],
        email: ['', Validators.required],
        dtadmissao: ['', Validators.required],
        dtdemissao: [''],
        empresa: ['', Validators.required], // ✅ CORREÇÃO: Empresa é obrigatória
        centrocusto: ['', Validators.required], // ✅ OBRIGATÓRIO: Centro de custo é obrigatório
        localidade: ['', Validators.required], // ✅ OBRIGATÓRIO: Localidade é obrigatória
        cargo: ['', Validators.required], // ✅ CORREÇÃO: Cargo agora é obrigatório
        setor: [''],
        tipocolaborador: ['', Validators.required], // ✅ CORREÇÃO: Tipo agora é obrigatório
        situacao: [''],
        // Novos campos opcionais
        filial_id: [''],
        matriculasuperior: [''],
        cliente: ['']
      })
    }

ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Verificar se é modal (quando é chamado com atributo modo)
    this.isModal = this.modo !== undefined && this.modo !== null;
    this.dataAtual = new Date().toISOString().split('T')[0];
    
    // Inicializar colaborador local
    if (this.colaborador) {
      this.colaboradorLocal = { ...this.colaborador };
    } else {
      this.colaboradorLocal = {
        cliente: this.session.usuario.cliente,
        usuario: this.session.usuario.id,
        tipocolaborador: 'F'
      };
    }
    
    // ✅ CORREÇÃO: Garantir que o cliente sempre esteja definido
    this.colaboradorLocal.cliente = this.session.usuario.cliente;
    this.colaboradorLocal.usuario = this.session.usuario.id;
    
    // Se não é modal (tem ID na URL), buscar dados da URL
    if (!this.isModal) {
      this.colaboradorLocal.tipocolaborador = 'F';
    }

    this.carregarDadosIniciais();
    
    // ✅ CORREÇÃO: Adicionar listener para sincronizar empresa com colaboradorLocal
    this.form.get('empresa')?.valueChanges.subscribe(empresaId => {
      this.colaboradorLocal.empresa = empresaId;
    });

    // ✅ NOVO: Listener para rastrear mudanças no centro de custo
    this.form.get('centrocusto')?.valueChanges.subscribe(centrocustoId => {
    });

    // ✅ NOVO: Listener para pesquisa de superior imediato
    this.pesquisaSuperior.valueChanges.pipe(
      debounceTime(300)
    ).subscribe(termo => {
      this.filtrarColaboradores(termo);
    });

    // ✅ NOVO: Fechar dropdown ao clicar fora
    document.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;
      if (!target.closest('.superior-search-container') && !target.closest('.superior-results')) {
        this.pesquisaSuperior.setValue('');
      }
    });
    
    // Se não é modal, verificar ID da URL
    if (!this.isModal) {
      this.ar.paramMap.subscribe(param => {
        var parametro = param.get('id');
        if(parametro != null) {
          this.colaboradorLocal.id = parametro;
          this.buscarPorID();
        }
      });
    }
  }

  ngOnChanges(): void {
    if (this.colaborador) {
      if (this.colaborador.id) {
        this.colaboradorLocal = { ...this.colaborador };
        this.buscarPorID();
      } else {
        // Se não tem ID, apenas usar os dados passados
        this.colaboradorLocal = { ...this.colaborador };
      }
    }
  }

  // Novos métodos para controle das seções
  public toggleSecaoAdicional(): void {
    this.secaoAdicionalExpanded = !this.secaoAdicionalExpanded;
  }

  public toggleCamposAvancados(): void {
    this.camposAvancadosExpanded = !this.camposAvancadosExpanded;
  }

  public getClienteHerdado(): string {
    const empresaId = this.form.get('empresa')?.value;
    if (empresaId) {
      const empresa = this.empresas.find((e: any) => e.id === empresaId);
      if (empresa && empresa.cliente) {
        // Aqui você pode buscar o nome do cliente se necessário
        return `Cliente ID: ${empresa.cliente}`;
      }
    }
    return 'Não definido';
  }

  // ✅ NOVO: Método para debug das chaves do objeto
  public getObjectKeys(obj: any): string {
    if (!obj) return 'N/A';
    return Object.keys(obj).join(', ');
  }

  private carregarDadosIniciais(): void {
    this.util.aguardar(true);
    this.apiCad.listarEmpresas("null", this.colaboradorLocal.cliente, this.session.token).then(res => {
      if (res.status === 200 || res.status === 204) {
        this.empresas = res.data || [];
        if (this.empresas.length > 0) {
        }
      } else {
        console.error('❌ [COLABORADOR] Erro ao carregar empresas:', res);
        this.empresas = [];
        this.util.exibirMensagemToast('Erro ao carregar empresas', 5000);
      }
      
      // ✅ CORREÇÃO: Usar cliente da sessão se colaboradorLocal.cliente estiver vazio
      const clienteId = this.colaboradorLocal.cliente || this.session.usuario.cliente;
      this.apiCad.listarLocalidades(clienteId, this.session.token).then(res => {
        this.localidades = res.data || [];
        if (this.localidades.length > 0) {
        }
        
        // Carregar filiais também
        this.apiCad.listarFiliais("null", clienteId, this.session.token).then(res => {
          this.filiais = res.data;
          if (this.filiais.length > 0) {
          }
          
          // ✅ CORREÇÃO: Centros de custo serão carregados quando empresa for selecionada
          // this.carregarCentrosCustosIniciais();
          
          // ✅ NOVO: Carregar colaboradores para superior imediato
          this.carregarColaboradores();
          // Após carregar listas base, tentar atualizar o campo de pesquisa do superior
          this.atualizarPesquisaSuperiorAPartirDaMatricula();
          
          this.util.aguardar(false);
          
          // ✅ CORREÇÃO: Se temos dados do colaborador para editar, preencher o formulário agora
          if (this.colaboradorLocal && this.colaboradorLocal.id && this.colaboradorLocal.nome) {
          }
        }).catch(err => {
          console.error('Erro ao carregar filiais:', err);
          this.util.aguardar(false);
        });
      }).catch(err => {
        console.error('Erro ao carregar localidades:', err);
        this.util.aguardar(false);
      });
    }).catch(err => {
      console.error('❌ [COLABORADOR] Erro ao carregar empresas:', err);
      this.empresas = [];
      this.util.exibirMensagemToast('Erro ao carregar empresas: ' + (err.message || 'Erro desconhecido'), 5000);
      this.util.aguardar(false);
    });
  }

  private preencherFormulario(): void {
    // ✅ CORREÇÃO: Evitar chamadas duplicadas
    if (this.preencherFormularioEmAndamento) {
      return;
    }
    
    if (this.colaboradorLocal && this.form) {
      this.preencherFormularioEmAndamento = true;
      if (this.empresas.length === 0) {
        console.warn('⚠️ [COLABORADOR] Dados iniciais não carregados ainda, aguardando...');
        // ✅ CORREÇÃO: Removido - será chamado pelo buscarPorID() quando os dados estiverem prontos
        return;
      }
      
      // ✅ CORREÇÃO: Mapear empresa pelo nome para encontrar o ID
      let empresaId = '';
      if (this.colaboradorLocal.empresa && typeof this.colaboradorLocal.empresa === 'string') {
        const empresaEncontrada = this.empresas.find((emp: any) => emp.nome === this.colaboradorLocal.empresa);
        if (empresaEncontrada) {
          empresaId = empresaEncontrada.id;
        } else {
          console.warn('⚠️ [COLABORADOR] Empresa não encontrada na lista:', this.colaboradorLocal.empresa);
        }
      } else {
        empresaId = this.colaboradorLocal.empresa || '';
      }
      
      // ✅ CORREÇÃO: Mapear centro de custo
      let centrocustoId = '';
      
      // Primeiro, tentar usar o ID direto se disponível
      if (this.colaboradorLocal.centrocusto) {
        centrocustoId = this.colaboradorLocal.centrocusto;
      } else if (this.colaboradorLocal.centrocustoId) {
        centrocustoId = this.colaboradorLocal.centrocustoId;
      } else if (this.colaboradorLocal.nomeCentroCusto && typeof this.colaboradorLocal.nomeCentroCusto === 'string') {
        // Se não tem ID, mas tem nome, carregar centros de custo e mapear
        if (empresaId) {
          this.carregarCentrosCustosParaEmpresa(empresaId).then(() => {
            const centroEncontrado = this.centrocustos.find((cc: any) => cc.nome === this.colaboradorLocal.nomeCentroCusto);
            if (centroEncontrado) {
              this.form.patchValue({ centrocusto: centroEncontrado.id });
            } else {
              console.warn('⚠️ [COLABORADOR] Centro de custo não encontrado na lista:', this.colaboradorLocal.nomeCentroCusto);
            }
          });
        }
      }
      
      // ✅ CORREÇÃO: Mapear localidade usando os campos corretos do DTO
      let localidadeId = '';
      
      // Priorizar LocalidadeId (campo numérico do DTO)
      if (this.colaboradorLocal.localidadeId) {
        localidadeId = this.colaboradorLocal.localidadeId;
      } 
      // Fallback para localidade (se for numérico)
      else if (this.colaboradorLocal.localidade && !isNaN(Number(this.colaboradorLocal.localidade))) {
        localidadeId = this.colaboradorLocal.localidade;
      }
      // Fallback para localidade_id
      else if (this.colaboradorLocal.localidade_id) {
        localidadeId = this.colaboradorLocal.localidade_id;
      }
      
      // Se ainda não encontrou, tentar outros campos possíveis
      if (!localidadeId) {
        const possiveisLocalidades = [
          this.colaboradorLocal.localidade,
          this.colaboradorLocal.localidadeId,
          this.colaboradorLocal.localidade_id,
          this.colaboradorLocal.localidadeId_id
        ];
        
        for (const loc of possiveisLocalidades) {
          if (loc && !isNaN(Number(loc))) {
            localidadeId = loc;
            break;
          }
        }
      }

      // 🛠️ NOVO: Se ainda não conseguiu mapear por ID, tentar mapear pelo nome (descricao)
      if (!localidadeId || !this.localidades.find((l: any) => l.id == localidadeId)) {
        const nomeLocalidade = (this.colaboradorLocal.localidade || '').toString().toLowerCase();
        if (nomeLocalidade) {
          const localidadeEncontrada = this.localidades.find((l: any) => (l.descricao || '').toLowerCase() === nomeLocalidade);
          if (localidadeEncontrada) {
            localidadeId = localidadeEncontrada.id;
          } else {
            console.warn('⚠️ [COLABORADOR] Não foi possível mapear localidade por nome:', this.colaboradorLocal.localidade);
          }
        }
      }
      
      // ✅ CORREÇÃO: Verificar se cargo está presente
      let cargo = this.colaboradorLocal.cargo || '';
      if (!cargo) {
        console.warn('⚠️ [COLABORADOR] Campo cargo não encontrado no colaboradorLocal');
      }
      
      // ✅ CORREÇÃO: Verificar se setor está presente
      let setor = this.colaboradorLocal.setor || '';
      if (!setor) {
        console.warn('⚠️ [COLABORADOR] Campo setor não encontrado no colaboradorLocal');
      }
      
      this.form.patchValue({
        nome: this.colaboradorLocal.nome || '',
        cpf: this.colaboradorLocal.cpf || '',
        matricula: this.colaboradorLocal.matricula || '',
        email: this.colaboradorLocal.email || '',
        dtadmissao: this.colaboradorLocal.dtadmissao || '',
        dtdemissao: this.colaboradorLocal.dtdemissao || '',
        empresa: empresaId,
        centrocusto: (() => {
          const valor = centrocustoId || this.colaboradorLocal.centrocusto || '';
          return valor;
        })(),
        localidade: localidadeId || this.colaboradorLocal.localidadeId || '',
        cargo: cargo,
        setor: setor,
        tipocolaborador: this.colaboradorLocal.tipocolaborador || 'F',
        situacao: this.colaboradorLocal.situacao || '',
        filial_id: this.colaboradorLocal.filialId || this.colaboradorLocal.filial_id || '',
        matriculasuperior: this.colaboradorLocal.matriculasuperior || this.colaboradorLocal.Matriculasuperior || '',
        cliente: this.colaboradorLocal.cliente || ''
      });

      // 🔄 Atualizar o campo visual de pesquisa do superior com base na matrícula salva
      this.atualizarPesquisaSuperiorAPartirDaMatricula();
      
      // ✅ CORREÇÃO: Carregar centros de custo após preencher o formulário
      if (empresaId) {
        this.carregarCentrosCustosParaEmpresa(empresaId);
      }
      
      // ✅ CORREÇÃO: Reset da flag
      this.preencherFormularioEmAndamento = false;
    }
  }

  // 🔧 Atualiza o campo de busca visual do Superior (pesquisaSuperior) com base na matrícula salva
  private atualizarPesquisaSuperiorAPartirDaMatricula(): void {
    try {
      const matricula = (this.form.get('matriculasuperior')?.value || this.colaboradorLocal.Matriculasuperior || '').toString().trim();
      if (!matricula) {
        return;
      }

      // Tentar encontrar o colaborador pelo cadastro pré-carregado
      let display = '';
      const encontrado = this.colaboradores?.find((c: any) => (c.matricula || '').toString() === matricula);
      if (encontrado) {
        display = `${encontrado.nome} (${matricula})`;
      } else {
        // Fallback: exibir apenas a matrícula
        display = `(${matricula})`;
      }

      if (this.pesquisaSuperior.value !== display) {
        this.pesquisaSuperior.setValue(display);
      }
    } catch {}
  }

  buscarPorID(){
    this.util.aguardar(true);
    this.api.obterColaboradorPorID(this.colaboradorLocal.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.colaboradorLocal = res.data;
      if (this.empresas.length > 0 && this.localidades.length > 0) {
        this.preencherFormulario();
      } else {
        // Se os dados iniciais ainda não foram carregados, aguardar
        setTimeout(() => {
          this.preencherFormulario();
        }, 1000);
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('❌ [COLABORADOR] Erro ao carregar colaborador:', error);
      this.util.exibirMensagemToast('Erro ao carregar dados do colaborador', 5000);
    });
  }

  // ✅ NOVO MÉTODO: Carregar centros de custo iniciais
  private carregarCentrosCustosIniciais(): void {
    this.apiCad.listarCentroCusto("null", this.colaboradorLocal.cliente, this.session.token).then(res => {
      this.centrocustos = res.data;
    }).catch(error => {
      console.error('Erro ao carregar centros de custo iniciais:', error);
      this.centrocustos = [];
    });
  }

  // ✅ NOVO MÉTODO: Carregar colaboradores para superior imediato
  private carregarColaboradores(): void {
    this.api.listarColaboradores("null", this.colaboradorLocal.cliente, 1, this.session.token).then(res => {
      if (res.status === 200 || res.status === 204) {
        this.colaboradores = res.data?.results || [];
        this.colaboradoresFiltrados = [...this.colaboradores]; // Inicializar lista filtrada
        if (this.colaboradores.length > 0) {
        }

        // Reforçar atualização do campo visual após lista carregada
        this.atualizarPesquisaSuperiorAPartirDaMatricula();
      } else {
        console.error('❌ [COLABORADOR] Erro ao carregar colaboradores:', res);
        this.colaboradores = [];
        this.colaboradoresFiltrados = [];
      }
    }).catch(error => {
      console.error('❌ [COLABORADOR] Erro ao carregar colaboradores:', error);
      this.colaboradores = [];
      this.colaboradoresFiltrados = [];
    });
  }

  // ✅ NOVO MÉTODO: Filtrar colaboradores por pesquisa
  private filtrarColaboradores(termo: string): void {
    if (!termo || termo.trim() === '') {
      this.colaboradoresFiltrados = [...this.colaboradores];
      return;
    }

    const termoLower = termo.toLowerCase().trim();
    this.colaboradoresFiltrados = this.colaboradores.filter((colaborador: any) => {
      const nome = (colaborador.nome || '').toLowerCase();
      const matricula = (colaborador.matricula || '').toLowerCase();
      const email = (colaborador.email || '').toLowerCase();
      
      return nome.includes(termoLower) || 
             matricula.includes(termoLower) || 
             email.includes(termoLower);
    });
  }

  // ✅ NOVO MÉTODO: Selecionar superior imediato
  selecionarSuperior(colaborador: any): void {
    this.form.patchValue({ matriculasuperior: colaborador.matricula });
    this.pesquisaSuperior.setValue(`${colaborador.nome} (${colaborador.matricula})`);
    // ✅ Garantir persistência: manter no objeto também
    this.colaboradorLocal.Matriculasuperior = (colaborador.matricula || '').toString();
    this.colaboradorLocal.matriculasuperior = this.colaboradorLocal.Matriculasuperior;
  }

  // ✅ NOVO MÉTODO: Limpar pesquisa
  limparPesquisaSuperior(): void {
    this.pesquisaSuperior.setValue('');
    this.form.patchValue({ matriculasuperior: '' });
  }

  // ✅ NOVO: Handler do blur para extrair matrícula da pesquisa
  onSuperiorBlur(): void {
    const atual = (this.form.get('matriculasuperior')?.value || '').toString().trim();
    if (atual) return; // já definido
    const pesquisa = (this.pesquisaSuperior?.value || '').toString();
    const match = pesquisa.match(/\(([^)]+)\)/);
    const matricula = match && match[1] ? match[1].trim() : '';
    if (matricula) {
      this.form.patchValue({ matriculasuperior: matricula });
    }
  }

// ✅ NOVO MÉTODO: Carregar centros de custo para uma empresa específica
  private carregarCentrosCustosParaEmpresa(empresaId: any): Promise<void> {
    if (!empresaId) {
      console.warn('⚠️ [COLABORADOR] Nenhuma empresa fornecida');
      this.centrocustos = [];
      return Promise.resolve();
    }
    
    return this.apiCad.listarCentroCustoDaEmpresa(empresaId, this.session.token).then(res => {
      this.centrocustos = res.data || [];
      const centrocustoAtual = this.form.get('centrocusto')?.value || this.colaboradorLocal.centrocusto;
      if (centrocustoAtual && this.centrocustos.length > 0) {
        // Verificar se o centro de custo atual ainda existe na lista
        const centroExiste = this.centrocustos.find((cc: any) => cc.id == centrocustoAtual);
        if (centroExiste) {
          // Só atualizar se o valor for diferente
          if (this.form.get('centrocusto')?.value !== centrocustoAtual) {
            this.form.patchValue({ centrocusto: centrocustoAtual });
          } else {
          }
        } else {
          console.warn('⚠️ [COLABORADOR] Centro de custo anterior não encontrado na lista');
          // Não sobrescrever automaticamente, deixar o usuário escolher
        }
      }
    }).catch(error => {
      console.error('❌ [COLABORADOR] Erro ao carregar centros de custo:', error);
      this.centrocustos = [];
    });
  }

  // ✅ MÉTODO ATUALIZADO: Para mudanças de empresa no formulário
  listarCentrosCustos(){
    this.util.aguardar(true);
    
    // ✅ CORREÇÃO: Usar o valor do formulário em vez do objeto
    const empresaId = this.form.get('empresa')?.value;
    if (!empresaId) {
      console.warn('⚠️ [COLABORADOR] Nenhuma empresa selecionada, não carregando centros de custo');
      this.centrocustos = [];
      this.form.patchValue({ centrocusto: '' });
      this.util.aguardar(false);
      return;
    }
    
    this.apiCad.listarCentroCustoDaEmpresa(empresaId, this.session.token).then(res => {
      this.util.aguardar(false);
      this.centrocustos = res.data;
      if (!this.centrocustos || this.centrocustos.length === 0) {
        this.form.patchValue({ centrocusto: '' });
      }
    }).catch(error => {
      this.util.aguardar(false);
      console.error('Erro ao carregar centros de custo:', error);
      this.centrocustos = [];
      this.form.patchValue({ centrocusto: '' });
    });
  }

  salvar() {
    if(this.form.valid) {
      this.carregando = true;
      this.util.aguardar(true);
      
            // Sincronizar dados do formulário com o objeto colaboradorLocal
      const formValues = this.form.value;
      this.colaboradorLocal = {
        ...this.colaboradorLocal,
        ...formValues
      };

      // ✅ Forçar mapeamento direto a partir do formulário para evitar perda dos valores
      const filialFormValue = this.form.get('filial_id')?.value;
      this.colaboradorLocal.FilialId = filialFormValue ? parseInt(filialFormValue, 10) : null;
      // Remover chaves alternativas para evitar duplicidade no binder
      delete this.colaboradorLocal.filialId;
      delete this.colaboradorLocal.filial_id;

      const superiorFormValue = this.form.get('matriculasuperior')?.value;
      this.colaboradorLocal.Matriculasuperior = superiorFormValue && superiorFormValue !== '' ? superiorFormValue.toString() : null;
      delete this.colaboradorLocal.matriculasuperior;

      // 🛠️ Fallback: extrair matrícula do campo de pesquisa se o form estiver vazio (ex.: "Nome (123456)")
      if (!this.colaboradorLocal.Matriculasuperior) {
        const pesquisa = (this.pesquisaSuperior?.value || '').toString();
        const match = pesquisa.match(/\(([^)]+)\)/);
        if (match && match[1]) {
          this.colaboradorLocal.Matriculasuperior = match[1].trim();
        }
      }

      // ✅ Normalizar CPF: manter apenas dígitos para validação/envio (LGPD: banco segue como já está)
      if (this.colaboradorLocal.cpf) {
        const cpfDigits = (this.colaboradorLocal.cpf || '').toString().replace(/\D/g, '');
        this.colaboradorLocal.cpf = cpfDigits;
        if (cpfDigits.length !== 11) {
          this.carregando = false;
          this.util.aguardar(false);
          this.util.exibirMensagemToast('CPF deve conter 11 dígitos.', 5000);
          return;
        }
      }

      // ✅ CORREÇÃO: Garantir que cliente não esteja vazio
      if (!this.colaboradorLocal.cliente || this.colaboradorLocal.cliente === '') {
        this.colaboradorLocal.cliente = this.session.usuario.cliente;
      }

      // ✅ CORREÇÃO: Garantir que usuario não esteja vazio
      if (!this.colaboradorLocal.usuario || this.colaboradorLocal.usuario === '') {
        this.colaboradorLocal.usuario = this.session.usuario.id;
      }

      // ✅ CORREÇÃO: Garantir campos obrigatórios
      if (!this.colaboradorLocal.tipocolaborador) {
        this.colaboradorLocal.tipocolaborador = 'F';
      }

      if (!this.colaboradorLocal.situacao) {
        this.colaboradorLocal.situacao = 'A';
      }

      // ✅ CORREÇÃO: Converter campos numéricos
      if (this.colaboradorLocal.empresa && typeof this.colaboradorLocal.empresa === 'string') {
        this.colaboradorLocal.empresa = parseInt(this.colaboradorLocal.empresa);
      }

      if (this.colaboradorLocal.centrocusto && typeof this.colaboradorLocal.centrocusto === 'string') {
        this.colaboradorLocal.centrocusto = parseInt(this.colaboradorLocal.centrocusto);
      } else if (!this.colaboradorLocal.centrocusto) {
        // Se não tem centro de custo, usar o primeiro disponível
        if (this.centrocustos && this.centrocustos.length > 0) {
          this.colaboradorLocal.centrocusto = this.centrocustos[0].id;
        } else {
          // Fallback para um ID válido (8 é o primeiro centro de custo existente)
          this.colaboradorLocal.centrocusto = 8;
        }
      }

      // ✅ CORREÇÃO: Mapear corretamente filial_id (do form) para FilialId esperado pelo backend
      if (this.colaboradorLocal.filial_id) {
        const filialParsed = parseInt(this.colaboradorLocal.filial_id as string, 10);
        if (!isNaN(filialParsed)) {
          // Usar a propriedade com camelCase para o binder do ASP.NET (case-insensitive)
          this.colaboradorLocal.filialId = filialParsed;
        }
        delete this.colaboradorLocal.filial_id; // remover chave snake_case do payload
      }

      // ✅ Garantir que localidade não seja NULL
      if (!this.colaboradorLocal.localidade) {
        // Se não tem localidade, definir padrão (ID 1 = "Padrão")
        this.colaboradorLocal.localidade = 1;
      }
      
      // ✅ CORREÇÃO: Garantir que localidade seja sempre um número
      if (typeof this.colaboradorLocal.localidade === 'string') {
        this.colaboradorLocal.localidade = parseInt(this.colaboradorLocal.localidade);
      }
      
      // ✅ CORREÇÃO: Remover localidade_id se existir (usar apenas localidade)
      if (this.colaboradorLocal.localidade_id) {
        delete this.colaboradorLocal.localidade_id;
      }
      
      // ✅ CORREÇÃO: Garantir que todos os campos obrigatórios estejam preenchidos
      if (!this.colaboradorLocal.nome || this.colaboradorLocal.nome.trim() === '') {
        this.util.exibirMensagemToast('Nome é obrigatório', 5000);
        return;
      }
      
      if (!this.colaboradorLocal.cpf || this.colaboradorLocal.cpf.trim() === '') {
        this.util.exibirMensagemToast('CPF é obrigatório', 5000);
        return;
      }
      
      if (!this.colaboradorLocal.matricula || this.colaboradorLocal.matricula.trim() === '') {
        this.util.exibirMensagemToast('Matrícula é obrigatória', 5000);
        return;
      }
      
      if (!this.colaboradorLocal.email || this.colaboradorLocal.email.trim() === '') {
        this.util.exibirMensagemToast('Email é obrigatório', 5000);
        return;
      }
      
      if (!this.colaboradorLocal.cargo || this.colaboradorLocal.cargo.trim() === '') {
        this.util.exibirMensagemToast('Cargo é obrigatório', 5000);
        return;
      }
      
      if (!this.colaboradorLocal.dtadmissao) {
        this.util.exibirMensagemToast('Data de admissão é obrigatória', 5000);
        return;
      }
      
      // ✅ CORREÇÃO: Validar data de demissão para terceirizados e consultores
      if ((this.colaboradorLocal.tipocolaborador === 'T' || this.colaboradorLocal.tipocolaborador === 'C') && !this.colaboradorLocal.dtdemissao) {
        this.util.exibirMensagemToast('Data de término de contrato é obrigatória para Terceirizados e Consultores', 5000);
        return;
      }

      // ✅ CORREÇÃO: Tratar campo Matriculasuperior com a capitalização esperada pelo backend
      if (this.colaboradorLocal.matriculasuperior && this.colaboradorLocal.matriculasuperior !== '') {
        this.colaboradorLocal.Matriculasuperior = this.colaboradorLocal.matriculasuperior.toString();
      } else if (this.colaboradorLocal.Matriculasuperior && this.colaboradorLocal.Matriculasuperior !== '') {
        // já está com a capitalização correta
      } else {
        this.colaboradorLocal.Matriculasuperior = null;
      }
      // remover a chave em minúsculas para evitar duplicidade
      delete this.colaboradorLocal.matriculasuperior;
      this.api.salvarColaborador(this.colaboradorLocal, this.session.token).then(res => {
        this.carregando = false;
        this.util.aguardar(false);
        if(res.status !== undefined && res.status !== null && res.status === 200){
          if (this.isModal) {
            // Se é modal, emitir evento
            this.colaboradorSalvo.emit(this.colaboradorLocal);
          } else {
            // Se é página, navegar
            this.util.exibirMensagemToast('Colaborador salvo com sucesso!', 5000);
            this.route.navigate(['/colaboradores']);
          }
        }
        else if(res.response && res.response.status == 409) {
          this.util.exibirMensagemToast(res.response.data, 5000);
        }
        else  {
          this.util.exibirFalhaComunicacao();
        }        
      }).catch(error => {
        this.carregando = false;
        this.util.aguardar(false);
        console.error('Erro ao salvar colaborador:', error);
        
        // ✅ CORREÇÃO: Tratamento de erro mais detalhado
        if (error.response && error.response.data) {
          if (error.response.data.errors) {
            const errors = error.response.data.errors;
            let errorMessage = 'Erro de validação:\n';
            
            Object.keys(errors).forEach(key => {
              errorMessage += `- ${key}: ${errors[key].join(', ')}\n`;
            });
            
            this.util.exibirMensagemToast(errorMessage, 8000);
          } else {
            this.util.exibirMensagemToast('Erro ao salvar colaborador: ' + (error.response.data.title || error.message), 5000);
          }
        } else {
          this.util.exibirFalhaComunicacao();
        }
      });
    } else {
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios.', 5000);
    }
  }

  // 🎯 MÉTODO PARA CARREGAR CENTROS DE CUSTO QUANDO EMPRESA MUDAR
  onEmpresaChange(): void {
    const empresaId = this.form.get('empresa')?.value;
    if (empresaId) {
      // ✅ CORREÇÃO: Preservar o valor atual do centro de custo
      const centrocustoAtual = this.form.get('centrocusto')?.value;
      this.carregarCentrosCustosParaEmpresa(empresaId).then(() => {
        // Restaurar o centro de custo se ainda existir na nova lista
        if (centrocustoAtual && this.centrocustos.length > 0) {
          const centroExiste = this.centrocustos.find((cc: any) => cc.id == centrocustoAtual);
          if (centroExiste) {
            this.form.patchValue({ centrocusto: centrocustoAtual });
          } else {
            console.warn('⚠️ [COLABORADOR] Centro de custo anterior não existe na nova empresa');
          }
        }
      });

      // ✅ CASCATA: Carregar localidades da empresa
      this.apiCad.listarLocalidadesDaEmpresa(empresaId, this.session.token).then(res => {
        this.localidades = res.data || [];
        this.filiais = [];
        this.form.patchValue({ 
          localidade: '',
          filial: '' 
        });
      }).catch(err => {
        console.error('❌ [COLABORADOR-CASCATA] Erro ao carregar localidades da empresa:', err);
        this.localidades = [];
      });
      
    } else {
      // ✅ CASCATA: Limpar tudo se empresa foi desmarcada
      this.centrocustos = [];
      this.localidades = [];
      this.filiais = [];
      this.form.patchValue({
        centrocusto: '',
        localidade: '',
        filial: ''
      });
    }
  }

  // ✅ NOVO: MÉTODO PARA CARREGAR FILIAIS QUANDO LOCALIDADE MUDAR
  onLocalidadeChange(): void {
    const empresaId = this.form.get('empresa')?.value;
    const localidadeId = this.form.get('localidade')?.value;
    if (empresaId && localidadeId) {
      this.apiCad.listarFiliaisPorLocalidade(empresaId, localidadeId, this.session.token).then(res => {
        this.filiais = res.data || [];
        this.form.patchValue({ filial: '' });
      }).catch(err => {
        console.error('❌ [COLABORADOR-CASCATA] Erro ao carregar filiais da localidade:', err);
        this.filiais = [];
      });
    } else {
      // ✅ CASCATA: Limpar filiais se localidade foi desmarcada
      this.filiais = [];
      this.form.patchValue({ filial: '' });
    }
  }

  // 🎯 MÉTODO PARA CANCELAR MODAL
  cancelar() {
    if (this.isModal) {
      this.cancelado.emit();
    } else {
      this.route.navigate(['/colaboradores']);
    }
  }

  // 🎯 MÉTODO PARA OBTER TÍTULO
  getTitulo(): string {
    if (this.modo === 'criar') {
      return 'Novo Colaborador';
    } else {
      return `Editar Colaborador`;
    }
  }

  // 🎯 MÉTODO PARA OBTER TEXTO DO BOTÃO
  getBotaoTexto(): string {
    if (this.carregando) {
      return this.modo === 'criar' ? 'Criando...' : 'Salvando...';
    }
    return this.modo === 'criar' ? 'Criar Colaborador' : 'Salvar Alterações';
  }

  // 🎯 MÉTODOS PARA DESLIGAMENTO
  
  /**
   * Verifica se o colaborador tem data de desligamento (independente se passou ou não)
   */
  temDataDesligamento(): boolean {
    const dataDesligamento = this.form.get('dtdemissao')?.value || this.colaboradorLocal.dtdemissao;
    return dataDesligamento && dataDesligamento.trim() !== '';
  }

  /**
   * Verifica se a data de desligamento já passou
   */
  isDataDesligamentoPassada(): boolean {
    if (!this.temDataDesligamento()) return false;
    
    const dataDesligamento = this.form.get('dtdemissao')?.value || this.colaboradorLocal.dtdemissao;
    const dataDesligamentoObj = new Date(dataDesligamento);
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0); // Zerar horário para comparar apenas a data
    
    return dataDesligamentoObj < hoje;
  }

  /**
   * Verifica se a data de desligamento é futura
   */
  isDataDesligamentoFutura(): boolean {
    if (!this.temDataDesligamento()) return false;
    
    const dataDesligamento = this.form.get('dtdemissao')?.value || this.colaboradorLocal.dtdemissao;
    const dataDesligamentoObj = new Date(dataDesligamento);
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0); // Zerar horário para comparar apenas a data
    
    return dataDesligamentoObj >= hoje;
  }

  /**
   * Verifica se o colaborador está desligado (tem data de desligamento)
   * Mantido para compatibilidade
   */
  isColaboradorDesligado(): boolean {
    return this.temDataDesligamento();
  }
  
  /**
   * Desliga o colaborador imediatamente com data/hora atual
   */
  desligarColaborador(): void {
    const hoje = new Date().toLocaleDateString('pt-BR');
    
    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja desligar o colaborador <strong>${this.colaboradorLocal.nome}</strong> agora?<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação irá definir a data de desligamento para hoje (${hoje}) e não poderá ser desfeita.`,
      true
    ).then(res => {
      if (res) {
        const hojeISO = new Date().toISOString().split('T')[0];
        this.form.patchValue({ dtdemissao: hojeISO });
        
        // Atualizar colaboradorLocal
        this.colaboradorLocal.dtdemissao = hojeISO;
        
        // Salvar automaticamente
        this.salvar();
        
        this.util.exibirMensagemToast(
          `Colaborador ${this.colaboradorLocal.nome} desligado com sucesso!`, 
          5000
        );
      }
    });
  }

  /**
   * Reativa o colaborador removendo a data de desligamento
   * Para consultores e terceiros, abre modal para definir nova data
   */
  reativarColaborador(): void {
    const tipoColaborador = this.form.get('tipocolaborador')?.value || this.colaboradorLocal.tipocolaborador;
    
    // Se for consultor (C) ou terceiro (T), precisa definir nova data
    if (tipoColaborador === 'C' || tipoColaborador === 'T') {
      this.abrirModalReativar();
    } else {
      // Para funcionários (F), pode reativar sem data
      const dataDesligamento = this.form.get('dtdemissao')?.value || this.colaboradorLocal.dtdemissao;
      const dataFormatada = dataDesligamento ? new Date(dataDesligamento).toLocaleDateString('pt-BR') : 'N/A';
      
      this.util.exibirMensagemPopUp(
        `Tem certeza que deseja reativar o colaborador <strong>${this.colaboradorLocal.nome}</strong>?<br><br>` +
        `🔄 <strong>Esta ação irá:</strong><br>` +
        `• Remover a data de desligamento (${dataFormatada})<br>` +
        `• Reativar o colaborador no sistema<br>` +
        `• Permitir que ele continue trabalhando normalmente`,
        true
      ).then(res => {
        if (res) {
          this.form.patchValue({ dtdemissao: '' });
          this.colaboradorLocal.dtdemissao = '';
          
          // Salvar automaticamente
          this.salvar();
          
          this.util.exibirMensagemToast(
            `Colaborador ${this.colaboradorLocal.nome} reativado com sucesso!`, 
            5000
          );
        }
      });
    }
  }

  /**
   * Abre o modal para reprogramar desligamento
   */
  abrirModalReprogramar(): void {
    this.novaDataDesligamento = this.colaboradorLocal.dtdemissao || this.dataAtual;
    this.motivoReprogramacao = '';
    this.modalReprogramarAberto = true;
  }

  /**
   * Fecha o modal de reprogramação
   */
  fecharModalReprogramar(): void {
    this.modalReprogramarAberto = false;
    this.novaDataDesligamento = '';
    this.motivoReprogramacao = '';
  }

  /**
   * Abre o modal para reativar colaborador (consultores e terceiros)
   */
  abrirModalReativar(): void {
    const dataFutura = new Date();
    dataFutura.setDate(dataFutura.getDate() + 30);
    this.novaDataTermino = dataFutura.toISOString().split('T')[0];
    this.motivoReativacao = '';
    this.modalReativarAberto = true;
  }

  /**
   * Fecha o modal de reativação
   */
  fecharModalReativar(): void {
    this.modalReativarAberto = false;
    this.novaDataTermino = '';
    this.motivoReativacao = '';
  }

  /**
   * Confirma a reativação do colaborador com nova data
   */
  confirmarReativacao(): void {
    if (!this.novaDataTermino) {
      this.util.exibirMensagemToast('Por favor, informe a nova data de término do contrato.', 5000);
      return;
    }

    // Validar se a data é futura
    const dataTermino = new Date(this.novaDataTermino);
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    
    if (dataTermino <= hoje) {
      this.util.exibirMensagemToast('A nova data de término deve ser futura.', 5000);
      return;
    }

    const dataFormatada = dataTermino.toLocaleDateString('pt-BR');
    const tipoColaborador = this.form.get('tipocolaborador')?.value || this.colaboradorLocal.tipocolaborador;
    const tipoTexto = tipoColaborador === 'C' ? 'Consultor' : 'Terceirizado';

    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja reativar o ${tipoTexto.toLowerCase()} <strong>${this.colaboradorLocal.nome}</strong>?<br><br>` +
      `🔄 <strong>Esta ação irá:</strong><br>` +
      `• Reativar o colaborador no sistema<br>` +
      `• Definir nova data de término: <strong>${dataFormatada}</strong><br>` +
      `• Permitir que ele continue trabalhando até a nova data`,
      true
    ).then(res => {
      if (res) {
        this.form.patchValue({ dtdemissao: this.novaDataTermino });
        this.colaboradorLocal.dtdemissao = this.novaDataTermino;
        
        // Fechar modal
        this.fecharModalReativar();
        
        // Salvar automaticamente
        this.salvar();
        
        this.util.exibirMensagemToast(
          `${tipoTexto} ${this.colaboradorLocal.nome} reativado com nova data de término!`, 
          5000
        );
      }
    });
  }

  /**
   * Confirma a reprogramação do desligamento
   */
  confirmarReprogramacao(): void {
    if (!this.novaDataDesligamento) {
      this.util.exibirMensagemToast('Por favor, selecione uma data válida.', 3000);
      return;
    }

    const dataAtual = new Date(this.dataAtual);
    const novaData = new Date(this.novaDataDesligamento);
    
    if (novaData < dataAtual) {
      this.util.exibirMensagemToast('A nova data não pode ser anterior à data atual.', 3000);
      return;
    }

    const motivoTexto = this.motivoReprogramacao ? `<br><strong>Motivo:</strong> ${this.motivoReprogramacao}` : '';
    
    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja reprogramar o desligamento do colaborador <strong>${this.colaboradorLocal.nome}</strong>?<br><br>` +
      `📅 <strong>Nova data:</strong> ${novaData.toLocaleDateString('pt-BR')}${motivoTexto}`,
      true
    ).then(res => {
      if (res) {
        this.form.patchValue({ dtdemissao: this.novaDataDesligamento });
        this.colaboradorLocal.dtdemissao = this.novaDataDesligamento;
        
        // Fechar modal
        this.fecharModalReprogramar();
        
        // Salvar automaticamente
        this.salvar();
        
        this.util.exibirMensagemToast(
          `Desligamento reprogramado para ${novaData.toLocaleDateString('pt-BR')} com sucesso!`, 
          5000
        );
      }
    });
  }

  // 🎯 MÉTODO PARA ALTERAÇÃO DO TIPO DE COLABORADOR
  onTipoColaboradorChange(): void {
    const tipo = this.form.get('tipocolaborador')?.value;
    if (tipo === 'F') {
      this.form.patchValue({ dtdemissao: '' });
    }
  }

  // 🎯 MÉTODO PARA VERIFICAR SE DATA DE DEMISSÃO É OBRIGATÓRIA
  isDataDemissaoObrigatoria(): boolean {
    const tipo = this.form.get('tipocolaborador')?.value;
    return tipo === 'T' || tipo === 'C';
  }

}
