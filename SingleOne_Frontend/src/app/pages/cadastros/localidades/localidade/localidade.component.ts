import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesApiService } from '../../../../api/configuracoes/configuracoes-api.service';
import { Localidade } from '../../../../models/localidade.interface';
import { UtilService } from '../../../../util/util.service';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-localidade',
  templateUrl: './localidade.component.html',
  styleUrls: ['./localidade.component.scss']
})
export class LocalidadeComponent implements OnInit, OnDestroy {

  @Input() localidade: Localidade | null = null;
  @Input() modo: 'criar' | 'editar' = 'criar';
  @Output() localidadeSalva = new EventEmitter<any>();
  @Output() cancelado = new EventEmitter<void>();

  public localidadeForm: FormGroup;
  public salvando = false;
  public errorMessage = '';
  public session: any;
  
  // Arrays para dropdowns
  public estados: any[] = [];
  public cidades: any[] = [];
  public carregandoEstados = false;
  public carregandoCidades = false;
  
  // Arrays filtrados para busca
  public estadosFiltrados: any[] = [];
  public cidadesFiltradas: any[] = [];
  
  // Campos de busca
  public buscaEstado = '';
  public buscaCidade = '';
  
  // Controle de exibição dos dropdowns
  public mostrarDropdownEstado = false;
  public mostrarDropdownCidade = false;
  
  // Subjects para debounce da busca
  private buscaEstadoSubject = new Subject<string>();
  private buscaCidadeSubject = new Subject<string>();

  constructor(
    private fb: FormBuilder,
    private api: ConfiguracoesApiService,
    private util: UtilService
  ) {
    this.localidadeForm = this.fb.group({
      id: [0],
      descricao: ['', [Validators.required]],
      estado: [''],           // Campo estado (opcional)
      cidade: [''],           // Campo cidade (opcional)
      ativo: [true],
      cliente: [0],
      migrateid: ['']
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Inicializar localidade se for null
    if (!this.localidade) {
      this.localidade = {} as any;
    }
    
    // Definir cliente se for criação
    if (this.modo === 'criar') {
      (this.localidade as any).cliente = this.session.usuario.cliente;
      // ✅ CORREÇÃO: Preencher o formulário com o cliente
      this.localidadeForm.patchValue({
        cliente: this.session.usuario.cliente
      });
    }
    
    // Carregar estados
    this.carregarEstados();
    
    // Se for edição, preencher o formulário
    if (this.modo === 'editar' && this.localidade?.id) {
      this.preencherFormulario();
    }
    
    // Configurar listener para mudança de estado
    this.localidadeForm.get('estado')?.valueChanges.subscribe(estadoId => {
      if (estadoId) {
        // Limpar cidade quando estado muda
        this.localidadeForm.patchValue({ cidade: '' });
        this.buscaCidade = '';
        this.cidadesFiltradas = [];
        // NÃO carregar cidades aqui - será feito na seleção
      } else {
        this.cidades = [];
        this.cidadesFiltradas = [];
      }
    });
    
    // Configurar debounce para busca de estado
    this.buscaEstadoSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(termo => {
      this.filtrarEstados(termo);
    });
    
    // Configurar debounce para busca de cidade
    this.buscaCidadeSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(termo => {
      this.filtrarCidades(termo);
    });
    
    // Adicionar listener global para fechar dropdowns
    document.addEventListener('click', this.onDocumentClick.bind(this));
  }

  getTitulo(): string {
    return this.modo === 'criar' ? 'Nova Localidade' : 'Editar Localidade';
  }

  getBotaoTexto(): string {
    return this.modo === 'criar' ? 'Criar' : 'Salvar';
  }

  async carregarEstados() {
    try {
      this.carregandoEstados = true;
      // TODO: Implementar método no backend
      // this.estados = await this.api.listarEstados();
      
      // Dados mockados temporariamente
      this.estados = [
        { id: 1, sigla: 'SP', nome: 'São Paulo' },
        { id: 2, sigla: 'RJ', nome: 'Rio de Janeiro' },
        { id: 3, sigla: 'MG', nome: 'Minas Gerais' },
        { id: 4, sigla: 'PR', nome: 'Paraná' },
        { id: 5, sigla: 'RS', nome: 'Rio Grande do Sul' },
        { id: 6, sigla: 'SC', nome: 'Santa Catarina' },
        { id: 7, sigla: 'GO', nome: 'Goiás' },
        { id: 8, sigla: 'BA', nome: 'Bahia' },
        { id: 9, sigla: 'CE', nome: 'Ceará' },
        { id: 10, sigla: 'PE', nome: 'Pernambuco' }
      ];
      
      this.estadosFiltrados = [...this.estados];
    } catch (error) {
      console.error('Erro ao carregar estados:', error);
    } finally {
      this.carregandoEstados = false;
    }
  }

  async carregarCidadesPorEstado(estadoId: number) {
    try {
      this.carregandoCidades = true;
      // TODO: Implementar método no backend
      // this.cidades = await this.api.listarCidadesPorEstado(estadoId);
      
      // Dados mockados temporariamente
      if (estadoId === 1) { // SP
        this.cidades = [
          { id: 1, nome: 'São Paulo' },
          { id: 2, nome: 'Guarulhos' },
          { id: 3, nome: 'Campinas' },
          { id: 4, nome: 'Santo André' },
          { id: 5, nome: 'Osasco' },
          { id: 6, nome: 'Ribeirão Preto' },
          { id: 7, nome: 'Sorocaba' },
          { id: 8, nome: 'Mauá' },
          { id: 9, nome: 'São José dos Campos' },
          { id: 10, nome: 'Mogi das Cruzes' }
        ];
      } else if (estadoId === 2) { // RJ
        this.cidades = [
          { id: 11, nome: 'Rio de Janeiro' },
          { id: 12, nome: 'São Gonçalo' },
          { id: 13, nome: 'Duque de Caxias' },
          { id: 14, nome: 'Nova Iguaçu' },
          { id: 15, nome: 'Niterói' },
          { id: 16, nome: 'Belford Roxo' },
          { id: 17, nome: 'São João de Meriti' },
          { id: 18, nome: 'Petrópolis' },
          { id: 19, nome: 'Campos dos Goytacazes' },
          { id: 20, nome: 'Volta Redonda' }
        ];
      } else if (estadoId === 3) { // MG
        this.cidades = [
          { id: 21, nome: 'Belo Horizonte' },
          { id: 22, nome: 'Uberlândia' },
          { id: 23, nome: 'Contagem' },
          { id: 24, nome: 'Betim' },
          { id: 25, nome: 'Montes Claros' },
          { id: 26, nome: 'Ribeirão das Neves' },
          { id: 27, nome: 'Uberaba' },
          { id: 28, nome: 'Governador Valadares' },
          { id: 29, nome: 'Ipatinga' },
          { id: 30, nome: 'Sete Lagoas' }
        ];
      } else if (estadoId === 4) { // PR
        this.cidades = [
          { id: 31, nome: 'Curitiba' },
          { id: 32, nome: 'Londrina' },
          { id: 33, nome: 'Maringá' },
          { id: 34, nome: 'Ponta Grossa' },
          { id: 35, nome: 'Cascavel' },
          { id: 36, nome: 'São José dos Pinhais' },
          { id: 37, nome: 'Foz do Iguaçu' },
          { id: 38, nome: 'Colombo' },
          { id: 39, nome: 'Guarapuava' },
          { id: 40, nome: 'Paranaguá' }
        ];
      } else if (estadoId === 5) { // RS
        this.cidades = [
          { id: 41, nome: 'Porto Alegre' },
          { id: 42, nome: 'Caxias do Sul' },
          { id: 43, nome: 'Pelotas' },
          { id: 44, nome: 'Canoas' },
          { id: 45, nome: 'Santa Maria' },
          { id: 46, nome: 'Gravataí' },
          { id: 47, nome: 'Viamão' },
          { id: 48, nome: 'Novo Hamburgo' },
          { id: 49, nome: 'São Leopoldo' },
          { id: 50, nome: 'Rio Grande' }
        ];
      } else if (estadoId === 6) { // SC
        this.cidades = [
          { id: 51, nome: 'Florianópolis' },
          { id: 52, nome: 'Joinville' },
          { id: 53, nome: 'Blumenau' },
          { id: 54, nome: 'Criciúma' },
          { id: 55, nome: 'São José' },
          { id: 56, nome: 'Lages' },
          { id: 57, nome: 'Itajaí' },
          { id: 58, nome: 'Chapecó' },
          { id: 59, nome: 'Jaraguá do Sul' },
          { id: 60, nome: 'Palhoça' }
        ];
      } else if (estadoId === 7) { // GO
        this.cidades = [
          { id: 61, nome: 'Goiânia' },
          { id: 62, nome: 'Aparecida de Goiânia' },
          { id: 63, nome: 'Anápolis' },
          { id: 64, nome: 'Rio Verde' },
          { id: 65, nome: 'Luziânia' },
          { id: 66, nome: 'Águas Lindas de Goiás' },
          { id: 67, nome: 'Valparaíso de Goiás' },
          { id: 68, nome: 'Trindade' },
          { id: 69, nome: 'Formosa' },
          { id: 70, nome: 'Novo Gama' }
        ];
      } else if (estadoId === 8) { // BA
        this.cidades = [
          { id: 71, nome: 'Salvador' },
          { id: 72, nome: 'Feira de Santana' },
          { id: 73, nome: 'Vitória da Conquista' },
          { id: 74, nome: 'Camaçari' },
          { id: 75, nome: 'Itabuna' },
          { id: 76, nome: 'Juazeiro' },
          { id: 77, nome: 'Lauro de Freitas' },
          { id: 78, nome: 'Ilhéus' },
          { id: 79, nome: 'Jequié' },
          { id: 80, nome: 'Alagoinhas' }
        ];
      } else if (estadoId === 9) { // CE
        this.cidades = [
          { id: 81, nome: 'Fortaleza' },
          { id: 82, nome: 'Caucaia' },
          { id: 83, nome: 'Sobral' },
          { id: 84, nome: 'Juazeiro do Norte' },
          { id: 85, nome: 'Maracanaú' },
          { id: 86, nome: 'Crato' },
          { id: 87, nome: 'Iguatu' },
          { id: 88, nome: 'Quixadá' },
          { id: 89, nome: 'Pacatuba' },
          { id: 90, nome: 'Aquiraz' },
          { id: 266, nome: 'Eusébio' }  // ← ADICIONADO EUSÉBIO!
        ];
      } else if (estadoId === 10) { // PE
        this.cidades = [
          { id: 91, nome: 'Recife' },
          { id: 92, nome: 'Jaboatão dos Guararapes' },
          { id: 93, nome: 'Olinda' },
          { id: 94, nome: 'Caruaru' },
          { id: 95, nome: 'Petrolina' },
          { id: 96, nome: 'Paulista' },
          { id: 97, nome: 'Cabo de Santo Agostinho' },
          { id: 98, nome: 'Camaragibe' },
          { id: 99, nome: 'Garanhuns' },
          { id: 100, nome: 'Vitória de Santo Antão' }
        ];
      } else {
        this.cidades = [];
      }
      
      this.cidadesFiltradas = [...this.cidades];
    } catch (error) {
      console.error('Erro ao carregar cidades:', error);
      this.cidades = [];
      this.cidadesFiltradas = [];
    } finally {
      this.carregandoCidades = false;
    }
  }

  // Métodos de busca
  onBuscaEstadoChange(termo: any) {
    this.buscaEstado = termo.target.value;
    this.buscaEstadoSubject.next(termo.target.value);
  }

  onBuscaCidadeChange(termo: any) {
    this.buscaCidade = termo.target.value;
    this.buscaCidadeSubject.next(termo.target.value);
  }

  filtrarEstados(termo: string) {
    if (!termo.trim()) {
      this.estadosFiltrados = [...this.estados];
    } else {
      const termoLower = termo.toLowerCase();
      this.estadosFiltrados = this.estados.filter(estado => 
        estado.nome.toLowerCase().includes(termoLower) ||
        estado.sigla.toLowerCase().includes(termoLower)
      );
    }
  }

  filtrarCidades(termo: string) {
    if (!termo.trim()) {
      this.cidadesFiltradas = [...this.cidades];
    } else {
      const termoLower = termo.toLowerCase();
      this.cidadesFiltradas = this.cidades.filter(cidade => 
        cidade.nome.toLowerCase().includes(termoLower)
      );
    }
  }

  // Seleção de estado
  selecionarEstado(estado: any) {
    this.localidadeForm.patchValue({ estado: estado.id });
    this.buscaEstado = `${estado.sigla} - ${estado.nome}`;
    this.mostrarDropdownEstado = false;
    
    // Carregar cidades para este estado
    this.carregarCidadesPorEstado(estado.id);
  }

  // Seleção de cidade
  selecionarCidade(cidade: any) {
    this.localidadeForm.patchValue({ cidade: cidade.id });
    this.buscaCidade = cidade.nome;
    this.mostrarDropdownCidade = false;
  }

  // Controle de foco dos campos
  onEstadoFocus() {
    this.mostrarDropdownEstado = true;
    this.estadosFiltrados = [...this.estados];
  }

  onCidadeFocus() {
    if (this.localidadeForm.get('estado')?.value) {
      this.mostrarDropdownCidade = true;
      this.cidadesFiltradas = [...this.cidades];
    }
  }

  // Fechar dropdowns ao clicar fora
  onDocumentClick(event: any) {
    const target = event.target as HTMLElement;
    
    if (!target.closest('.estado-dropdown-container')) {
      this.mostrarDropdownEstado = false;
    }
    
    if (!target.closest('.cidade-dropdown-container')) {
      this.mostrarDropdownCidade = false;
    }
  }

  preencherFormulario() {
    if (this.localidade) {
      // ✅ CORREÇÃO: Garantir que cliente seja sempre preenchido
      const clienteId = (this.localidade as any).cliente || this.session.usuario.cliente;
      this.localidadeForm.patchValue({
        id: this.localidade.id,
        descricao: this.localidade.descricao,
        estado: this.localidade.estado || '',
        cidade: this.localidade.cidade || '',
        ativo: this.localidade.ativo,
        cliente: clienteId,
        migrateid: this.localidade.migrateid || ''
      });
      
      // Se tem estado, carregar cidades e preencher busca
      if (this.localidade.estado) {
        const estado = this.estados.find(e => e.id === Number(this.localidade.estado));
        if (estado) {
          this.buscaEstado = `${estado.sigla} - ${estado.nome}`;
          this.carregarCidadesPorEstado(Number(this.localidade.estado));
          
          // Se tem cidade, preencher busca após carregar cidades
          if (this.localidade.cidade) {
            setTimeout(() => {
              const cidade = this.cidades.find(c => c.id === Number(this.localidade.cidade));
              if (cidade) {
                this.buscaCidade = cidade.nome;
              }
            }, 300);
          }
        }
      }
    }
  }

  async salvar() {
    if (this.localidadeForm.invalid) {
      this.marcarCamposInvalidos();
      return;
    }

    try {
      this.salvando = true;
      this.errorMessage = '';

      const localidadeData = this.localidadeForm.value;
      const token = this.session.token;

      if (!token) {
        this.errorMessage = 'Token não encontrado';
        return;
      }

      // Log dos dados que serão enviados
      const response = await this.api.salvarLocalidade(localidadeData, token);
      if (response.status === 200) {
        this.localidadeSalva.emit(response.data);
      } else {
        this.errorMessage = response.data?.mensagem || 'Erro ao salvar localidade';
      }
    } catch (error) {
      console.error('Erro ao salvar localidade:', error);
      this.errorMessage = 'Erro ao salvar localidade. Tente novamente.';
    } finally {
      this.salvando = false;
    }
  }

  marcarCamposInvalidos() {
    Object.keys(this.localidadeForm.controls).forEach(key => {
      const control = this.localidadeForm.get(key);
      if (control?.invalid) {
        control.markAsTouched();
      }
    });
  }

  cancelar() {
    this.cancelado.emit();
  }

  ngOnDestroy() {
    // Remover listener global
    document.removeEventListener('click', this.onDocumentClick.bind(this));
  }
}
