import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { UtilService } from '../../../util/util.service';
import { EstoqueMinimoApiService, EstoqueMinimoEquipamento, EstoqueAlerta } from '../../../api/estoque-minimo/estoque-minimo-api.service';
import { ConfiguracoesApiService } from '../../../api/configuracoes/configuracoes-api.service';

@Component({
  selector: 'app-equipamentos',
  templateUrl: './equipamentos.component.html',
  styleUrls: ['./equipamentos.component.scss']
})
export class EquipamentosComponent implements OnInit, AfterViewInit {

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  
  public session: any = {};
  public loading = false;
  public equipamentos: EstoqueAlerta[] = [];
  public equipamentosFiltrados: EstoqueAlerta[] = [];
  public dataSource: MatTableDataSource<EstoqueAlerta>;
  public localidades: any[] = [];
  public filtroAtivo: string = 'TODOS'; // 'TODOS', 'ALERTA', 'EXCESSO', 'OK'

  // Modal de cadastro/edição
  public mostrarModal = false;
  public modo = 'criar'; // 'criar' ou 'editar'
  public equipamentoSelecionado: any = null;
  public form: FormGroup;
  public salvando = false;

  // Dados para os dropdowns
  public modelos: any[] = [];
  public localidadesCompletas: any[] = [];
  
  // Propriedades computadas para os filtros
  get totalEquipamentos(): number {
    return this.equipamentos.length;
  }
  
  get equipamentosEmAlerta(): number {
    return this.equipamentos.filter(e => e.statusEstoque === 'ALERTA').length;
  }
  
  get equipamentosEmExcesso(): number {
    return this.equipamentos.filter(e => e.statusEstoque === 'EXCESSO').length;
  }
  
  get equipamentosOk(): number {
    return this.equipamentos.filter(e => e.statusEstoque === 'OK').length;
  }

  // Propriedades para exibição na tabela (usando paginação)
  get equipamentosParaExibicao(): EstoqueAlerta[] {
    if (!this.dataSource || !this.dataSource.paginator) {
      return this.dataSource?.data || [];
    }
    
    const startIndex = this.dataSource.paginator.pageIndex * this.dataSource.paginator.pageSize;
    const endIndex = startIndex + this.dataSource.paginator.pageSize;
    return this.dataSource.data.slice(startIndex, endIndex);
  }

  constructor(
    private util: UtilService,
    private estoqueApi: EstoqueMinimoApiService,
    private configuracoesApi: ConfiguracoesApiService,
    private fb: FormBuilder,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.dataSource = new MatTableDataSource<EstoqueAlerta>([]);
    this.form = this.fb.group({
      modeloId: ['', Validators.required],
      localidadeId: ['', Validators.required],
      estoqueMinimo: ['', [Validators.required, Validators.min(0)]],
      estoqueMaximo: ['', [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Determinar cliente baseado na estrutura da sessão
    this.session.cliente = this.session?.usuario?.cliente || this.session?.usuario?.Cliente;
    
    // Carregar dados de referência primeiro, depois os equipamentos
    this.carregarDadosIniciais();
    
    // Listener para o evento do botão Novo do header principal
    window.addEventListener('novoEquipamento', () => {
      this.novoEquipamento();
    });
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  // 🔧 MÉTODO AUXILIAR PARA CONFIGURAR PAGINADOR (igual ao padrão de usuários)
  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      return;
    }
    
    // CONFIGURAÇÃO SIMPLES E DIRETA
    this.dataSource.paginator = this.paginator;
    
    // CONFIGURAR TAMANHO INICIAL
    this.paginator.pageSize = 10;
    this.paginator.pageIndex = 0;
    
    // ADICIONAR LISTENER PARA MUDANÇAS
    this.paginator.page.subscribe(() => {
      // FORÇAR ATUALIZAÇÃO DA VIEW
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    });
  }

  // 🚀 CARREGAR DADOS INICIAIS
  async carregarDadosIniciais() {
    try {
      // Carregar modelos e localidades primeiro
      await Promise.all([
        this.carregarModelos(),
        this.carregarLocalidades()
      ]);
      
      // Depois carregar equipamentos (que dependem dos dados de referência)
      await this.carregarEquipamentos();
      
    } catch (error) {
      console.error('Erro ao carregar dados iniciais:', error);
      this.util.exibirMensagemToast('Erro ao carregar dados iniciais', 5000);
    }
  }

  // 📊 CARREGAR EQUIPAMENTOS
  async carregarEquipamentos() {
    try {
      this.loading = true;
      
      // Carregar configurações de estoque mínimo de equipamentos com dados calculados
      const response = await this.estoqueApi.listarEquipamentos(this.session.cliente);
      this.equipamentos = response.data || [];
      
      // Inicializar equipamentos filtrados com todos os equipamentos
      this.equipamentosFiltrados = [...this.equipamentos];
      
      // Atualizar dataSource
      this.dataSource = new MatTableDataSource<EstoqueAlerta>(this.equipamentosFiltrados);
      setTimeout(() => {
        this.configurarPaginador();
      }, 0);
    } catch (error) {
      console.error('Erro ao carregar equipamentos:', error);
      this.util.exibirMensagemToast('Erro ao carregar equipamentos', 5000);
    } finally {
      this.loading = false;
    }
  }

  // 📍 CARREGAR LOCALIDADES
  async carregarLocalidades() {
    try {
      const response = await this.configuracoesApi.listarLocalidades(this.session.cliente, this.session.token);
      
      if (response.status === 200 || response.status === 204) {
        // Filtrar apenas localidades ativas (excluir ID 1 que é padrão)
        const localidadesFiltradas = (response.data || []).filter(localidade => localidade.id !== 1);
        this.localidades = localidadesFiltradas;
        this.localidadesCompletas = [...localidadesFiltradas];
      } else {
        this.localidades = [];
        this.localidadesCompletas = [];
      }
    } catch (error) {
      console.error('Erro ao carregar localidades:', error);
      this.util.exibirMensagemToast('Erro ao carregar localidades', 5000);
      this.localidades = [];
      this.localidadesCompletas = [];
    }
  }

  // 📱 CARREGAR MODELOS
  async carregarModelos() {
    try {
      const response = await this.configuracoesApi.listarModelos('', this.session.cliente, this.session.token);
      
      if (response.status === 200 || response.status === 204) {
        let dados = response.data;
        
        // Extrair dados se estiver em propriedade .dados
        if (dados && dados.dados) {
          dados = dados.dados;
        }
        
        if (dados && Array.isArray(dados)) {
          // Filtrar apenas modelos ativos
          const modelosAtivos = dados.filter((modelo: any) => modelo.ativo);
          
          // Remover duplicatas por ID
          const uniqueById = new Map<number, any>();
          modelosAtivos.forEach((modelo: any) => {
            if (!uniqueById.has(modelo.id)) {
              uniqueById.set(modelo.id, modelo);
            }
          });
          
          // Remover duplicatas por descrição normalizada
          const uniqueByDescription = new Map<string, any>();
          Array.from(uniqueById.values()).forEach((modelo: any) => {
            const displayText = this.getModeloDisplayText(modelo);
            const normalizedText = displayText.trim().toLowerCase();
            if (!uniqueByDescription.has(normalizedText)) {
              uniqueByDescription.set(normalizedText, modelo);
            }
          });
          
          // Ordenar alfabeticamente
          this.modelos = Array.from(uniqueByDescription.values()).sort((a, b) => {
            const textA = this.getModeloDisplayText(a).toLowerCase();
            const textB = this.getModeloDisplayText(b).toLowerCase();
            return textA.localeCompare(textB);
          });
        } else {
          this.modelos = [];
        }
      } else {
        this.modelos = [];
      }
    } catch (error) {
      console.error('Erro ao carregar modelos:', error);
      this.util.exibirMensagemToast('Erro ao carregar modelos', 5000);
      this.modelos = [];
    }
  }

  // 🔍 FILTRAR POR CARD
  filtrarPorCard(tipo: string) {
    this.filtroAtivo = tipo;
    
    if (tipo === 'TODOS') {
      this.equipamentosFiltrados = [...this.equipamentos];
    } else {
      this.equipamentosFiltrados = this.equipamentos.filter(e => e.statusEstoque === tipo);
    }
    
    // Atualizar dataSource
    this.dataSource = new MatTableDataSource<EstoqueAlerta>(this.equipamentosFiltrados);
    setTimeout(() => {
      this.configurarPaginador();
    }, 0);
  }

  // 🔍 VERIFICAR SE CARD ESTÁ ATIVO
  isCardAtivo(tipo: string): boolean {
    return this.filtroAtivo === tipo;
  }

  // 📝 FORMATAR TEXTO DO MODELO PARA EXIBIÇÃO
  getModeloDisplayText(modelo: any): string {
    if (!modelo) return '';
    
    const partes = [];
    
    // 1. TIPO DE EQUIPAMENTO - primeiro (se disponível)
    if (modelo.fabricanteNavigation && modelo.fabricanteNavigation.tipoequipamento && modelo.fabricanteNavigation.tipoequipamentoNavigation) {
      partes.push(modelo.fabricanteNavigation.tipoequipamentoNavigation.descricao);
    } else if (modelo.tipoEquipamentoDescricao) {
      partes.push(modelo.tipoEquipamentoDescricao);
    } else if (modelo.tipoEquipamento) {
      partes.push(modelo.tipoEquipamento);
    }
    
    // 2. FABRICANTE - segundo
    if (modelo.fabricanteNavigation && modelo.fabricanteNavigation.descricao) {
      partes.push(modelo.fabricanteNavigation.descricao);
    } else if (modelo.fabricanteDescricao) {
      partes.push(modelo.fabricanteDescricao);
    } else if (modelo.fabricante && typeof modelo.fabricante !== 'number') {
      partes.push(modelo.fabricante);
    }
    
    // 3. MODELO - terceiro
    if (modelo.descricao) {
      partes.push(modelo.descricao);
    }
    
    // Se não tem informações adicionais, retorna só a descrição
    if (partes.length === 0) {
      return modelo.descricao || 'Modelo sem descrição';
    }
    
    // Verificar se não estamos exibindo apenas IDs
    const resultado = partes.join(' - ');
    if (resultado.match(/^\d+$/)) {
      return modelo.descricao || `Modelo ${resultado}`;
    }
    
    return resultado;
  }

  // 📝 FORMATAR TEXTO DA LOCALIDADE PARA EXIBIÇÃO
  getLocalidadeDisplayText(localidade: any): string {
    if (!localidade) return '';
    
    const partes = [];
    
    // Descrição da Localidade - sempre usar descrição
    if (localidade.descricao) {
      partes.push(localidade.descricao);
    }
    
    // Cidade - verificar se não é ID (string que contém apenas números)
    if (localidade.cidade && typeof localidade.cidade === 'string' && localidade.cidade.trim() !== '' && !localidade.cidade.match(/^\d+$/)) {
      partes.push(localidade.cidade);
    }
    
    // Estado - verificar se não é ID (string que contém apenas números)
    if (localidade.estado && typeof localidade.estado === 'string' && localidade.estado.trim() !== '' && !localidade.estado.match(/^\d+$/)) {
      partes.push(localidade.estado);
    }
    
    // Se não tem informações adicionais, retorna só a descrição
    if (partes.length === 0) {
      return localidade.descricao || 'Localidade sem descrição';
    }
    
    // Verificar se não estamos exibindo apenas IDs
    const resultado = partes.join(' - ');
    if (resultado.match(/^\d+$/)) {
      return localidade.descricao || `Localidade ${resultado}`;
    }
    
    return resultado;
  }

  // 🔄 LIMPAR FILTROS
  limparFiltros() {
    this.filtroAtivo = 'TODOS';
    this.equipamentosFiltrados = [...this.equipamentos];
    
    // Atualizar dataSource
    this.dataSource = new MatTableDataSource<EstoqueAlerta>(this.equipamentosFiltrados);
    setTimeout(() => {
      this.configurarPaginador();
    }, 0);
  }

  // 🏷️ FUNÇÕES PARA EXIBIÇÃO NA GRID
  getModeloDescricaoGrid(equipamento: any): string {
    if (!equipamento) return 'N/A';
    
    // Usar dados calculados do backend se disponíveis
    if (equipamento.modeloDescricao && equipamento.fabricanteDescricao) {
      return `${equipamento.fabricanteDescricao} - ${equipamento.modeloDescricao}`;
    }
    
    // Usar modeloId ou modelo (backend usa 'modelo')
    const modeloId = equipamento.modeloId || equipamento.modelo;
    if (!modeloId) return 'N/A';
    
    // Buscar modelo com comparação flexível de tipos
    const modelo = this.modelos.find(m => 
      m.id == modeloId || // Comparação flexível
      m.id === Number(modeloId) || // Converter para número
      String(m.id) === String(modeloId) // Converter para string
    );
    return modelo ? this.getModeloDisplayText(modelo) : `Modelo ID: ${modeloId}`;
  }

  getLocalidadeDescricaoGrid(equipamento: any): string {
    if (!equipamento) return 'N/A';
    
    // Usar dados calculados do backend se disponíveis
    if (equipamento.localidadeDescricao) {
      return equipamento.localidadeDescricao;
    }
    
    // Usar localidadeId ou localidade (backend usa 'localidade')
    const localidadeId = equipamento.localidadeId || equipamento.localidade;
    if (!localidadeId) return 'N/A';
    
    // Buscar localidade com comparação flexível de tipos
    const localidade = this.localidadesCompletas.find(l => 
      l.id == localidadeId || // Comparação flexível
      l.id === Number(localidadeId) || // Converter para número
      String(l.id) === String(localidadeId) // Converter para string
    );
    return localidade ? this.getLocalidadeDisplayText(localidade) : `Localidade ID: ${localidadeId}`;
  }

  // 📊 CALCULAR STATUS DO EQUIPAMENTO
  getStatusEquipamento(equipamento: any): string {
    if (!equipamento) return 'OK';
    
    // Usar status calculado do backend se disponível
    if (equipamento.statusEstoque) {
      return equipamento.statusEstoque;
    }
    
    // Fallback: calcular localmente usando nomes de campos do backend
    const estoqueAtual = equipamento.estoqueAtual || equipamento.quantidadetotallancada || 0;
    const estoqueMinimo = equipamento.quantidadeMinima || equipamento.quantidademinima || 0;
    const estoqueMaximo = equipamento.quantidadeMaxima || equipamento.quantidademaxima || 0;
    
    if (estoqueAtual <= estoqueMinimo) {
      return 'ALERTA';
    } else if (estoqueAtual >= estoqueMaximo) {
      return 'EXCESSO';
    } else {
      return 'OK';
    }
  }

  // 📊 CALCULAR PERCENTUAL DE UTILIZAÇÃO
  getPercentualUtilizacao(equipamento: any): number {
    if (!equipamento) return 0;
    
    // Usar percentual calculado do backend se disponível
    if (equipamento.percentualUtilizacao !== undefined) {
      return equipamento.percentualUtilizacao;
    }
    
    // Fallback: calcular localmente
    const estoqueAtual = equipamento.estoqueAtual || equipamento.quantidadetotallancada || 0;
    const estoqueMaximo = equipamento.quantidadeMaxima || equipamento.quantidademaxima || 1; // Evitar divisão por zero
    
    return Math.round((estoqueAtual / estoqueMaximo) * 100);
  }

  // ➕ NOVO EQUIPAMENTO
  novoEquipamento() {
    this.modo = 'criar';
    this.equipamentoSelecionado = null;
    this.form.reset();
    this.mostrarModal = true;
    
    // Recarregar modelos se não estiverem carregados
    if (this.modelos.length === 0) {
      this.carregarModelos();
    }
  }

  // ✏️ EDITAR EQUIPAMENTO
  editarEquipamento(equipamento: any) {
    this.modo = 'editar';
    this.equipamentoSelecionado = equipamento;
    this.mostrarModal = true;
    
    // Verificar se os dados de referência estão carregados
    if (this.modelos.length === 0 || this.localidadesCompletas.length === 0) {
      this.carregarDadosIniciais().then(() => {
        this.preencherFormularioEdicao(equipamento);
      });
    } else {
      this.preencherFormularioEdicao(equipamento);
    }
  }

  private preencherFormularioEdicao(equipamento: any) {
    // Aguardar um tick para garantir que o modal esteja renderizado
    setTimeout(() => {
      // Usar nomes de campos do backend (camelCase) e converter para string
      const valoresForm = {
        modeloId: String(equipamento.modelo || equipamento.modeloId || ''),
        localidadeId: String(equipamento.localidade || equipamento.localidadeId || ''),
        estoqueMinimo: equipamento.quantidadeMinima || equipamento.estoqueMinimo || 0,
        estoqueMaximo: equipamento.quantidadeMaxima || equipamento.estoqueMaximo || 0
      };
      
      this.form.patchValue(valoresForm);
    }, 100);
  }

  // 🔒 FECHAR MODAL
  fecharModal() {
    this.mostrarModal = false;
    this.form.reset();
    this.equipamentoSelecionado = null;
  }

  // 💾 SALVAR EQUIPAMENTO
  async salvarEquipamento() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatórios', 3000);
      return;
    }

    const dados = this.form.value;
    
    try {
      this.salvando = true;
      
      const dadosCompletos = {
        ...dados,
        clienteId: this.session.cliente,
        token: this.session.token,
        usuarioId: this.session.usuario.id || 1
      };
      if (this.modo === 'criar') {
        await this.estoqueApi.cadastrarEquipamento(dadosCompletos);
        this.util.exibirMensagemToast('Recurso cadastrado com sucesso!', 3000);
      } else {
        await this.estoqueApi.atualizarEquipamento(this.equipamentoSelecionado.id, dadosCompletos);
        this.util.exibirMensagemToast('Recurso atualizado com sucesso!', 3000);
      }
      
      this.fecharModal();
      this.carregarEquipamentos();
      
    } catch (error: any) {
      console.error('Erro ao salvar recurso:', error);
      
      // Tratar diferentes tipos de erro
      let mensagemErro = 'Erro ao salvar recurso';
      
      if (error?.response?.data?.mensagem) {
        mensagemErro = error.response.data.mensagem;
      } else if (error?.message?.includes('duplicate') || error?.message?.includes('duplicat')) {
        mensagemErro = 'Já existe um registro com essas informações. Não é possível cadastrar duplicatas.';
      } else if (error?.response?.status === 409) {
        mensagemErro = 'Conflito: Já existe um registro com essas informações.';
      } else if (error?.response?.status === 400) {
        mensagemErro = 'Dados inválidos. Verifique as informações e tente novamente.';
      }
      
      this.util.exibirMensagemToast(mensagemErro, 5000);
    } finally {
      this.salvando = false;
    }
  }

  // 🗑️ EXCLUIR EQUIPAMENTO
  async excluirEquipamento(equipamento: any) {
    if (confirm('Tem certeza que deseja excluir este recurso?')) {
      try {
        await this.estoqueApi.excluirEquipamento(equipamento.id);
        this.util.exibirMensagemToast('Recurso excluído com sucesso', 3000);
        this.carregarEquipamentos();
      } catch (error) {
        console.error('Erro ao excluir recurso:', error);
        this.util.exibirMensagemToast('Erro ao excluir recurso', 5000);
      }
    }
  }

  // 🎯 NOVO: NAVEGAR PARA RECURSOS COM FILTRO
  navegarParaRecursosFiltrados(equipamento: any) {
    try {
      // Construir filtro baseado no modelo e localidade
      const filtroRecursos = {
        modeloId: equipamento.modelo || equipamento.modeloId,
        localidadeId: equipamento.localidade || equipamento.localidadeId,
        clienteId: this.session.cliente,
        // Adicionar informações para exibição
        modeloDescricao: equipamento.modeloDescricao || 'N/A',
        fabricanteDescricao: equipamento.fabricanteDescricao || 'N/A',
        localidadeDescricao: equipamento.localidadeDescricao || 'N/A'
      };
      this.router.navigate(['/recursos'], {
        queryParams: {
          modeloId: filtroRecursos.modeloId,
          localidadeId: filtroRecursos.localidadeId,
          clienteId: filtroRecursos.clienteId,
          // Parâmetros para exibição de informações
          modeloDescricao: filtroRecursos.modeloDescricao,
          fabricanteDescricao: filtroRecursos.fabricanteDescricao,
          localidadeDescricao: filtroRecursos.localidadeDescricao,
          // Flag para indicar que é um filtro do estoque mínimo
          origemEstoqueMinimo: true
        },
        skipLocationChange: false
      });
      
      // Exibir mensagem informativa
      this.util.exibirMensagemToast(
        `Filtrando recursos: ${filtroRecursos.fabricanteDescricao} - ${filtroRecursos.modeloDescricao} em ${filtroRecursos.localidadeDescricao}`,
        4000
      );
    } catch (error) {
      console.error('❌ [NAVEGAÇÃO] Erro na navegação:', error);
      this.util.exibirMensagemToast('Erro ao navegar para recursos', 3000);
    }
  }

}