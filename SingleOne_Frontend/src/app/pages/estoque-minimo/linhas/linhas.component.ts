import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { UtilService } from '../../../util/util.service';
import { EstoqueMinimoApiService, EstoqueMinimoLinha, EstoqueAlerta } from '../../../api/estoque-minimo/estoque-minimo-api.service';
import { ConfiguracoesApiService } from '../../../api/configuracoes/configuracoes-api.service';
import { TelefoniaApiService } from '../../../api/telefonia/telefonia-api.service';

@Component({
  selector: 'app-linhas',
  templateUrl: './linhas.component.html',
  styleUrls: ['./linhas.component.scss']
})
export class LinhasComponent implements OnInit, AfterViewInit {

  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  
  public session: any = {};
  public loading = false;
  public linhas: EstoqueAlerta[] = [];
  public linhasFiltradas: EstoqueAlerta[] = [];
  public dataSource: MatTableDataSource<EstoqueAlerta>;
  public operadoras: any[] = [];
  public localidades: any[] = [];
  public filtroAtivo: string = 'TODOS'; // 'TODOS', 'ALERTA', 'EXCESSO', 'OK'

  // Modal de cadastro/ediÃ§Ã£o
  public mostrarModal = false;
  public modo = 'criar'; // 'criar' ou 'editar'
  public linhaSelecionada: any = null;
  public form: FormGroup;
  public salvando = false;

  // Dados para os dropdowns
  public planos: any[] = [];
  public localidadesCompletas: any[] = [];
  
  // Propriedades computadas para os filtros
  get totalLinhas(): number {
    return this.linhas.length;
  }
  
  get linhasEmAlerta(): number {
    return this.linhas.filter(l => l.statusEstoque === 'ALERTA').length;
  }
  
  get linhasEmExcesso(): number {
    return this.linhas.filter(l => l.statusEstoque === 'EXCESSO').length;
  }
  
  get linhasOk(): number {
    return this.linhas.filter(l => l.statusEstoque === 'OK').length;
  }

  // Propriedades para exibição na tabela (usando paginação)
  get linhasParaExibicao(): EstoqueAlerta[] {
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
    private telefoniaApi: TelefoniaApiService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.dataSource = new MatTableDataSource<EstoqueAlerta>([]);
    // Inicializar formulário
    this.form = this.fb.group({
      planoId: ['', Validators.required],
      localidadeId: ['', Validators.required],
      estoqueMinimo: ['', [Validators.required, Validators.min(0)]],
      estoqueMaximo: ['', [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Debug da sessão
    if (!this.session.usuario?.cliente) {
      console.error('❌ [ERRO] Cliente não encontrado na sessão');
      this.util.exibirMensagemToast('Erro: Cliente não encontrado na sessão', 5000);
      return;
    }
    
    // Carregar dados de referÃªncia primeiro, depois as linhas
    this.carregarDadosIniciais();
    
    // Listener para o evento do botÃ£o Nova do header principal
    window.addEventListener('novaLinha', () => {
      this.novaLinha();
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
      // Carregar planos e localidades primeiro
      await Promise.all([
        this.carregarPlanos(),
        this.carregarLocalidades()
      ]);
      
      // Depois carregar as linhas
      await this.carregarLinhas();
      
    } catch (error) {
      console.error('Erro ao carregar dados iniciais:', error);
      this.util.exibirMensagemToast('Erro ao carregar dados iniciais', 5000);
    }
  }

  // ðŸ“Š CARREGAR LINHAS
  async carregarLinhas() {
    try {
      this.loading = true;
      
      // Verificar se cliente existe
      if (!this.session.usuario?.cliente) {
        console.error('❌ [ERRO] Cliente não encontrado para carregar linhas');
        this.util.exibirMensagemToast('Erro: Cliente não encontrado', 5000);
        return;
      }
      const response = await this.estoqueApi.listarLinhas(this.session.usuario.cliente);
      this.linhas = (response.data || []).map((linha: any) => ({
        id: linha.id ?? linha.Id,
        cliente: linha.cliente ?? linha.Cliente,
        operadora: linha.operadora ?? linha.Operadora,
        plano: linha.plano ?? linha.Plano,
        localidade: linha.localidade ?? linha.Localidade,
        quantidadeMinima: linha.quantidadeMinima ?? linha.QuantidadeMinima ?? linha.quantidademinima,
        quantidadeMaxima: linha.quantidadeMaxima ?? linha.QuantidadeMaxima ?? linha.quantidademaxima,
        perfilUso: linha.perfilUso ?? linha.PerfilUso ?? '',
        ativo: linha.ativo ?? linha.Ativo,
        dtCriacao: linha.dtCriacao ?? linha.DtCriacao ?? linha.dtcriacao,
        usuarioCriacao: linha.usuarioCriacao ?? linha.UsuarioCriacao ?? linha.usuariocriacao,
        dtAtualizacao: linha.dtAtualizacao ?? linha.DtAtualizacao ?? linha.dtatualizacao,
        usuarioAtualizacao: linha.usuarioAtualizacao ?? linha.UsuarioAtualizacao ?? linha.usuarioatualizacao,
        // Dados de navegação
        operadoraDescricao: linha.operadoraNavigation?.nome || linha.OperadoraNavigation?.Nome || linha.operadora || 'N/A',
        planoDescricao: linha.planoNavigation?.nome || linha.PlanoNavigation?.Nome || linha.plano || 'N/A',
        localidadeDescricao: linha.localidadeNavigation?.descricao || linha.LocalidadeNavigation?.Descricao || linha.localidade || 'N/A',
        // Dados calculados
        totalLancado: linha.quantidadeTotalLancada ?? linha.QuantidadeTotalLancada ?? linha.quantidadetotallancada ?? 0,
        quantidadeTotalLancada: linha.quantidadeTotalLancada ?? linha.QuantidadeTotalLancada ?? linha.quantidadetotallancada ?? 0,
        quantidadetotallancada: linha.quantidadeTotalLancada ?? linha.QuantidadeTotalLancada ?? linha.quantidadetotallancada ?? 0,
        estoqueAtual: linha.estoqueAtual ?? linha.EstoqueAtual ?? 0,
        percentualUtilizacao: linha.percentualUtilizacao ?? linha.PercentualUtilizacao ?? 0,
        statusEstoque: linha.statusEstoque ?? linha.StatusEstoque ?? 'OK'
      }));
      
      this.linhasFiltradas = [...this.linhas];
      
      // Atualizar dataSource
      this.dataSource = new MatTableDataSource<EstoqueAlerta>(this.linhasFiltradas);
      setTimeout(() => {
        this.configurarPaginador();
      }, 0);
    } catch (error) {
      console.error('Erro ao carregar linhas:', error);
      this.util.exibirMensagemToast('Erro ao carregar linhas', 5000);
    } finally {
      this.loading = false;
    }
  }

  // ðŸ“ CARREGAR OPERADORAS
  async carregarOperadoras() {
    try {
      const response = await this.telefoniaApi.listarOperadoras(this.session.token);
      this.operadoras = response.data || [];
    } catch (error) {
      console.error('Erro ao carregar operadoras:', error);
      this.util.exibirMensagemToast('Erro ao carregar operadoras', 3000);
    }
  }

  // ðŸ“ CARREGAR PLANOS

  async carregarPlanos() {
    try {
      const response = await this.telefoniaApi.listarTodosPlanos(this.session.token);
      const dados = response.data || [];
      
      if (Array.isArray(dados) && dados.length > 0) {
        // Remover duplicatas por ID
        const uniqueById = new Map<number, any>();
        dados.forEach((plano: any) => {
          const planoId = plano.id || plano.Id || plano.ID;
          if (planoId && !uniqueById.has(planoId)) {
            uniqueById.set(planoId, plano);
          }
        });
        
        // Remover duplicatas por nome normalizado
        const uniqueByNome = new Map<string, any>();
        Array.from(uniqueById.values()).forEach((plano: any) => {
          const planoNome = (plano.plano || plano.Plano || plano.nome || plano.Nome || '').trim().toLowerCase();
          if (planoNome && !uniqueByNome.has(planoNome)) {
            uniqueByNome.set(planoNome, plano);
          }
        });
        
        // Ordenar alfabeticamente
        this.planos = Array.from(uniqueByNome.values()).sort((a, b) => {
          const nomeA = (a.plano || a.Plano || a.nome || a.Nome || '').toLowerCase();
          const nomeB = (b.plano || b.Plano || b.nome || b.Nome || '').toLowerCase();
          return nomeA.localeCompare(nomeB);
        });
      } else {
        this.planos = [];
      }
    } catch (error) {
      console.error('Erro ao carregar planos:', error);
      this.util.exibirMensagemToast('Erro ao carregar planos', 3000);
      this.planos = [];
    }
  }

  // ðŸ“ CARREGAR LOCALIDADES
  async carregarLocalidades() {
    try {
      const response = await this.configuracoesApi.listarLocalidades(this.session.usuario.cliente, this.session.token);
      this.localidadesCompletas = response.data || [];
    } catch (error) {
      console.error('Erro ao carregar localidades:', error);
      this.util.exibirMensagemToast('Erro ao carregar localidades', 3000);
    }
  }

  // 🎯 FILTRAR POR CARD
  filtrarPorCard(tipo: string) {
    this.filtroAtivo = tipo;
    if (tipo === 'TODOS') {
      this.linhasFiltradas = [...this.linhas];
    } else {
      this.linhasFiltradas = this.linhas.filter(l => l.statusEstoque === tipo);
    }
    
    // Atualizar dataSource
    this.dataSource = new MatTableDataSource<EstoqueAlerta>(this.linhasFiltradas);
    setTimeout(() => {
      this.configurarPaginador();
    }, 0);
  }

  // âœ… VERIFICAR SE CARD ESTÃ ATIVO
  isCardAtivo(tipo: string): boolean {
    return this.filtroAtivo === tipo;
  }

  // 🧹 LIMPAR FILTROS
  limparFiltros() {
    this.filtroAtivo = 'TODOS';
    this.linhasFiltradas = [...this.linhas];
    
    // Atualizar dataSource
    this.dataSource = new MatTableDataSource<EstoqueAlerta>(this.linhasFiltradas);
    setTimeout(() => {
      this.configurarPaginador();
    }, 0);
  }

  // 🎯 MÉTODOS AUXILIARES PARA EXIBIÇÃO
  getOperadoraDescricaoGrid(linha: any): string {
    return linha.operadoraDescricao || linha.operadora || 'N/A';
  }

  getLocalidadeDescricaoGrid(linha: any): string {
    return linha.localidadeDescricao || linha.localidade || 'N/A';
  }

  getStatusLinha(linha: any): string {
    return linha.statusEstoque || linha.status || 'OK';
  }

  getPercentualUtilizacao(linha: any): number {
    const percentual = linha.percentualUtilizacao || linha.percentualutilizacao || 0;
    return Math.round(percentual);
  }

  // ðŸ§­ NAVEGAÃ‡ÃƒO PARA LINHAS FILTRADAS
  navegarParaLinhasFiltradas(linha: any) {
    this.util.exibirMensagemToast('Funcionalidade em desenvolvimento', 3000);
  }

  // âž• NOVA LINHA
  novaLinha() {
    this.modo = 'criar';
    this.linhaSelecionada = null;
    this.form.reset();
    // Não limpar planos - eles já foram carregados
    this.mostrarModal = true;
  }

  // âœï¸ EDITAR LINHA
  async editarLinha(linha: any) {
    this.modo = 'editar';
    this.linhaSelecionada = linha;
    
    // Preencher formulário com dados da linha
    this.form.patchValue({
      planoId: linha.planoId || linha.plano,
      localidadeId: linha.localidadeId || linha.localidade,
      estoqueMinimo: linha.quantidadeMinima || linha.quantidademinima || 0,
      estoqueMaximo: linha.quantidadeMaxima || linha.quantidademaxima || 0
    });
    
    this.mostrarModal = true;
  }

  // ðŸ’¾ SALVAR LINHA
  async salvarLinha() {
    if (this.form.invalid) {
      this.util.exibirMensagemToast('Por favor, preencha todos os campos obrigatÃ³rios', 3000);
      return;
    }

    try {
      this.salvando = true;
      
      const dadosLinha: EstoqueMinimoLinha = {
        id: this.modo === 'editar' ? this.linhaSelecionada.id : 0,
        cliente: this.session.usuario.cliente,
        operadora: 0, // Não usado mais
        plano: this.form.value.planoId,
        localidade: this.form.value.localidadeId,
        perfilUso: '', // Campo removido do formulário
        quantidadeMinima: this.form.value.estoqueMinimo,
        quantidadeMaxima: this.form.value.estoqueMaximo,
        ativo: true,
        dtCriacao: this.modo === 'criar' ? new Date().toISOString() : this.linhaSelecionada.dtcriacao,
        usuarioCriacao: this.session.usuario.id,
        dtAtualizacao: this.modo === 'editar' ? new Date().toISOString() : undefined,
        usuarioAtualizacao: this.modo === 'editar' ? this.session.usuario.id : undefined
      };
      await this.estoqueApi.salvarLinha(dadosLinha);
      
      this.util.exibirMensagemToast(
        `Linha ${this.modo === 'criar' ? 'cadastrada' : 'atualizada'} com sucesso!`, 
        3000
      );
      this.fecharModal();
      await this.carregarLinhas();
      
    } catch (error) {
      console.error('Erro ao salvar linha:', error);
      this.util.exibirMensagemToast('Erro ao salvar linha', 5000);
    } finally {
      this.salvando = false;
    }
  }

  // âŒ FECHAR MODAL
  fecharModal() {
    this.mostrarModal = false;
    this.form.reset();
    this.linhaSelecionada = null;
    this.modo = 'criar';
    // Não limpar planos - eles devem permanecer carregados
  }

// ðŸ—‘ï¸ EXCLUIR LINHA
  async excluirLinha(linha: any) {
    if (confirm('Tem certeza que deseja excluir esta linha?')) {
      try {
        await this.estoqueApi.excluirLinha(linha.id);
        this.util.exibirMensagemToast('Linha excluÃ­da com sucesso', 3000);
        this.carregarLinhas();
      } catch (error) {
        console.error('Erro ao excluir linha:', error);
        this.util.exibirMensagemToast('Erro ao excluir linha', 5000);
      }
    }
  }
}
