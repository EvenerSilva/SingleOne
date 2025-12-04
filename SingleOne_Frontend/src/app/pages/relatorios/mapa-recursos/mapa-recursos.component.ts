import { Component, OnInit, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { RelatorioApiService } from '../../../api/relatorios/relatorio-api.service';
import { UtilService } from '../../../util/util.service';
import * as XLSX from 'xlsx';

interface NoMapa {
  id: number;
  nome: string;
  tipo: string;
  descricao: string;
  totalColaboradores: number;
  totalRecursos: number;
  icone: string;
  cor: string;
  temFilhos: boolean;
  isExpandido?: boolean;
  filhos?: NoMapa[];
}

interface ColaboradorMapa {
  id: number;
  nome: string;
  email: string;
  cargo: string;
  tipoColaborador: string;
  empresa: string;
  localidade: string;
  filial: string;
  centroCusto: string;
  totalRecursos: number;
  totalRecursosAtivos: number;
  recursos: RecursoDetalhe[];
  historicoRecursos: RecursoDetalhe[];
}

interface RecursoDetalhe {
  id: number;
  tipo: string;
  modelo: string;
  numeroSerie: string;
  patrimonio: string;
  status: string;
  statusDescricao: string;
  dataEntrega: string;
  dataDevolucao: string;
  cor: string;
  icone: string;
}

@Component({
  selector: 'app-mapa-recursos',
  templateUrl: './mapa-recursos.component.html',
  styleUrls: ['./mapa-recursos.component.scss'],
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ height: 0, opacity: 0, overflow: 'hidden' }),
        animate('300ms ease-out', style({ height: '*', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ height: 0, opacity: 0, overflow: 'hidden' }))
      ])
    ])
  ]
})
export class MapaRecursosComponent implements OnInit {
  @ViewChild('mapaContainer', { static: false }) mapaContainer: ElementRef;
  
  session: any;
  carregando = false;
  exportando = false; // 🆕 Flag para mostrar loading durante exportação
  
  // Filtros
  empresas: any[] = [];
  localidades: any[] = [];
  centrosCusto: any[] = [];
  
  // 🆕 Listas completas (sem filtro) para cascata
  todasLocalidades: any[] = [];
  todosCentrosCusto: any[] = [];
  
  empresaSelecionada: number = null;
  localidadeSelecionada: number = null;
  centroCustoSelecionado: number = null;
  
  // Toggles
  mostrarSemRecursos = false;
  mostrarHistorico = false;
  
  // Dados do mapa
  hierarquia: NoMapa = null;
  colaboradores: ColaboradorMapa[] = [];
  colaboradoresSemRecursos: ColaboradorMapa[] = [];
  colaboradoresLista: any[] = []; // 🆕 Lista plana para modo lista horizontal
  metricas: any = {};
  
  // Estado da visualização
  modoVisualizacao: 'arvore' | 'lista' = 'arvore';
  breadcrumb: any[] = [];

  constructor(
    private relatorioApi: RelatorioApiService,
    private util: UtilService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    if (!this.session) {
      this.util.exibirMensagemToast('Sessão expirada. Faça login novamente.', 5000);
      return;
    }
    this.carregarEmpresas();
  }

  /**
   * ✅ NOVA: Carregar hierarquia completa para popular TODOS os dropdowns de uma vez
   */
  async carregarEmpresas() {
    try {
      this.carregando = true;

// Buscar hierarquia completa SEM filtros para popular todos os dropdowns
      const filtros = {
        clienteId: this.session.usuario.cliente,
        empresaId: null,
        localidadeId: null,
        centroCustoId: null,
        incluirColaboradoresSemRecursos: false,
        incluirHistoricoRecursos: false
      };
      
      const response = await this.relatorioApi.obterMapaRecursos(filtros, this.session.token);

      if (response && response.data && response.data.data) {
        const dados = response.data.data;
        
        // Extrair TODOS os dropdowns da hierarquia completa
        this.atualizarDropdownsCascata(dados.raizHierarquia);
        
      }
    } catch (error) {
      this.util.exibirMensagemToast('Erro ao carregar dados iniciais', 5000);
    } finally {
      this.carregando = false;
    }
  }

  /**
   * ✅ NOVA LÓGICA: Ao selecionar empresa, filtrar localidades e CCs
   */
  onEmpresaChange() {
    
    // Resetar filtros subsequentes
    this.localidadeSelecionada = null;
    this.centroCustoSelecionado = null;
    
    // Filtrar localidades e CCs com base na empresa selecionada
    this.filtrarDropdownsPorEmpresa();
    
    // Limpar mapa (usuário deve clicar em "Buscar" novamente)
    this.hierarquia = null;
    this.colaboradores = [];
    this.colaboradoresSemRecursos = [];
    this.metricas = {};
  }

  /**
   * ✅ NOVA LÓGICA: Ao selecionar localidade, filtrar CCs
   */
  onLocalidadeChange() {
    
    // Resetar filtro subsequente
    this.centroCustoSelecionado = null;
    
    // Filtrar CCs com base na localidade selecionada
    this.filtrarDropdownsPorLocalidade();
    
    // Limpar mapa (usuário deve clicar em "Buscar" novamente)
    this.hierarquia = null;
    this.colaboradores = [];
    this.colaboradoresSemRecursos = [];
    this.metricas = {};
  }

  /**
   * ✅ NOVA LÓGICA: Ao selecionar centro de custo
   */
  onCentroCustoChange() {
    
    // Limpar mapa (usuário deve clicar em "Buscar" novamente)
    this.hierarquia = null;
    this.colaboradores = [];
    this.colaboradoresSemRecursos = [];
    this.metricas = {};
  }

  /**
   * Buscar/Gerar mapa de recursos (acionado pelo botão)
   * Não é mais necessário selecionar empresa - mostra hierarquia completa desde a raiz
   */
  async buscarMapaRecursos() {
    await this.carregarMapaRecursos();
  }

  /**
   * Carregar dados do mapa de recursos
   */
  private async carregarMapaRecursos() {
    try {
      this.carregando = true;
      
      const filtros = {
        clienteId: this.session.usuario.cliente,
        empresaId: this.empresaSelecionada || null,
        localidadeId: this.localidadeSelecionada || null,
        centroCustoId: this.centroCustoSelecionado || null,
        incluirColaboradoresSemRecursos: this.mostrarSemRecursos,
        incluirHistoricoRecursos: this.mostrarHistorico
      };

      const response = await this.relatorioApi.obterMapaRecursos(filtros, this.session.token);

      if (response && response.data && response.data.data) {
        const dados = response.data.data;
        
        this.hierarquia = dados.raizHierarquia;
        this.colaboradores = dados.colaboradores || [];
        this.colaboradoresSemRecursos = dados.colaboradoresSemRecursos || [];
        this.metricas = dados.metricas || {};
        
        // Atualizar dropdowns em cascata
        this.atualizarDropdownsCascata(dados.raizHierarquia);
        
        // 🆕 Extrair colaboradores para modo lista horizontal
        if (this.hierarquia) {
          this.colaboradoresLista = this.extrairColaboradoresDaHierarquia(this.hierarquia);
        }
        
        // Forçar detecção de mudanças
        this.cdr.detectChanges();
      }
    } catch (error) {
      console.error('Erro ao carregar mapa de recursos:', error);
      this.util.exibirMensagemToast('Erro ao carregar mapa de recursos', 5000);
    } finally {
      this.carregando = false;
    }
  }

  /**
   * ✅ NOVA: Extrair TODOS os dropdowns da hierarquia completa E salvar listas completas
   */
  atualizarDropdownsCascata(hierarquia: NoMapa) {
    if (!hierarquia || !hierarquia.filhos) return;

// 1️⃣ Extrair EMPRESAS (sempre no nível 1)
    if (hierarquia.filhos[0]?.tipo === 'empresa') {
      this.empresas = hierarquia.filhos.map(n => ({ id: n.id, nome: n.nome }));
      
      // 2️⃣ Extrair LOCALIDADES com referência à empresa (para filtro em cascata)
      const localidadesComEmpresa = new Map<number, any>();
      hierarquia.filhos.forEach(empresa => {
        if (empresa.filhos) {
          empresa.filhos.forEach(localidade => {
            if (localidade.tipo === 'localidade' && !localidadesComEmpresa.has(localidade.id)) {
              localidadesComEmpresa.set(localidade.id, { 
                id: localidade.id, 
                descricao: localidade.nome,
                empresaId: empresa.id // 🆕 Guardar empresa
              });
            }
          });
        }
      });
      this.todasLocalidades = Array.from(localidadesComEmpresa.values());
      this.localidades = [...this.todasLocalidades]; // Inicialmente mostra todas
      
      // 3️⃣ Extrair CENTROS DE CUSTO com referência à empresa E localidade
      const ccsComReferencia = new Map<number, any>();
      hierarquia.filhos.forEach(empresa => {
        if (empresa.filhos) {
          empresa.filhos.forEach(localidade => {
            if (localidade.filhos) {
              localidade.filhos.forEach(cc => {
                if (cc.tipo === 'centroCusto' && !ccsComReferencia.has(cc.id)) {
                  ccsComReferencia.set(cc.id, { 
                    id: cc.id, 
                    nome: cc.nome,
                    empresaId: empresa.id, // 🆕 Guardar empresa
                    localidadeId: localidade.id // 🆕 Guardar localidade
                  });
                }
              });
            }
          });
        }
      });
      this.todosCentrosCusto = Array.from(ccsComReferencia.values());
      this.centrosCusto = [...this.todosCentrosCusto]; // Inicialmente mostra todos
    }
  }
  
  /**
   * 🆕 Filtrar localidades e CCs com base na empresa selecionada
   */
  filtrarDropdownsPorEmpresa() {
    if (!this.empresaSelecionada) {
      // Sem empresa selecionada → mostrar todas
      this.localidades = [...this.todasLocalidades];
      this.centrosCusto = [...this.todosCentrosCusto];
    } else {
      // Filtrar localidades da empresa selecionada
      this.localidades = this.todasLocalidades.filter(loc => loc.empresaId === this.empresaSelecionada);
      
      // Filtrar CCs da empresa selecionada
      this.centrosCusto = this.todosCentrosCusto.filter(cc => cc.empresaId === this.empresaSelecionada);
      
    }
  }
  
  /**
   * 🆕 Filtrar CCs com base na localidade selecionada
   */
  filtrarDropdownsPorLocalidade() {
    if (!this.localidadeSelecionada) {
      // Sem localidade selecionada → mostrar CCs da empresa
      if (this.empresaSelecionada) {
        this.centrosCusto = this.todosCentrosCusto.filter(cc => cc.empresaId === this.empresaSelecionada);
      } else {
        this.centrosCusto = [...this.todosCentrosCusto];
      }
    } else {
      // Filtrar CCs da empresa E localidade selecionadas
      this.centrosCusto = this.todosCentrosCusto.filter(cc => 
        cc.empresaId === this.empresaSelecionada && 
        cc.localidadeId === this.localidadeSelecionada
      );
    }
  }

  /**
   * Expandir/colapsar nó da árvore
   */
  async toggleNo(no: NoMapa) {
    no.isExpandido = !no.isExpandido;
    
    // Se está expandindo e não tem filhos carregados, fazer drilldown
    if (no.isExpandido && no.temFilhos && (!no.filhos || no.filhos.length === 0)) {
      await this.carregarFilhosNo(no);
    }
  }

  /**
   * Carregar filhos de um nó específico
   */
  async carregarFilhosNo(no: NoMapa) {
    try {
      
      // Encontrar o caminho completo até este nó
      const caminho = this.encontrarCaminhoNo(this.hierarquia, no);
      
      if (!caminho || caminho.length === 0) {
        return;
      }

// Montar filtros com base no caminho (respeitando toggles)
      const filtros: any = {
        clienteId: this.session.usuario.cliente,
        incluirColaboradoresSemRecursos: this.mostrarSemRecursos,
        incluirHistoricoRecursos: this.mostrarHistorico
      };

      // Extrair IDs do caminho (sem filiais)
      for (const noPath of caminho) {
        switch (noPath.tipo) {
          case 'empresa':
            filtros.empresaId = noPath.id;
            break;
          case 'localidade':
            filtros.localidadeId = noPath.id;
            break;
          case 'centroCusto':
            filtros.centroCustoId = noPath.id;
            break;
        }
      }

const response = await this.relatorioApi.obterMapaRecursos(filtros, this.session.token);

      if (response && response.data && response.data.data) {
        const dados = response.data.data;
        
        // ✅ SEMPRE usar raizHierarquia.filhos (backend já filtrou corretamente)
        if (dados.raizHierarquia && dados.raizHierarquia.filhos) {
          no.filhos = dados.raizHierarquia.filhos;
        }
        
        // Forçar detecção de mudanças
        this.cdr.detectChanges();
      }
    } catch (error) {
      console.error('Erro ao carregar filhos do nó:', error);
      this.util.exibirMensagemToast('Erro ao carregar detalhes', 3000);
    }
  }

  /**
   * Encontrar o caminho completo até um nó específico na hierarquia
   */
  private encontrarCaminhoNo(raiz: NoMapa, noAlvo: NoMapa, caminhoAtual: NoMapa[] = []): NoMapa[] | null {
    if (!raiz) return null;

    // Se encontrou o nó, retorna o caminho incluindo ele
    if (raiz.id === noAlvo.id && raiz.tipo === noAlvo.tipo) {
      return [...caminhoAtual, raiz];
    }

    // Buscar nos filhos
    if (raiz.filhos && raiz.filhos.length > 0) {
      for (const filho of raiz.filhos) {
        const resultado = this.encontrarCaminhoNo(filho, noAlvo, [...caminhoAtual, raiz]);
        if (resultado) {
          return resultado;
        }
      }
    }

    return null;
  }

  /**
   * Montar nós de colaboradores
   */
  private montarNosColaboradores(colaboradores: any[]): NoMapa[] {
    return colaboradores.map(c => ({
      id: c.id,
      nome: c.nome,
      tipo: 'colaborador',
      descricao: c.cargo,
      totalColaboradores: 0,
      totalRecursos: c.totalRecursos,
      icone: 'cil-user',
      cor: c.totalRecursosAtivos > 0 ? '#4CAF50' : '#FF9800',
      temFilhos: c.recursos && c.recursos.length > 0,
      filhos: this.montarNosRecursos(c.recursos),
      isExpandido: false
    }));
  }

  /**
   * Montar nós de recursos
   */
  private montarNosRecursos(recursos: any[]): NoMapa[] {
    if (!recursos) return [];
    
    return recursos.map(r => ({
      id: r.id,
      nome: `${r.tipo} - ${r.modelo}`,
      tipo: 'recurso',
      descricao: r.patrimonio || r.numeroSerie,
      totalColaboradores: 0,
      totalRecursos: 0,
      icone: r.icone,
      cor: r.cor,
      temFilhos: false,
      filhos: [],
      isExpandido: false
    }));
  }

  /**
   * Alternar visualização entre árvore e lista
   */
  toggleVisualizacao() {
    this.modoVisualizacao = this.modoVisualizacao === 'arvore' ? 'lista' : 'arvore';
  }
  
  /**
   * 🆕 Extrair lista plana de colaboradores da hierarquia (para modo lista)
   */
  extrairColaboradoresDaHierarquia(hierarquia: NoMapa): any[] {
    const colaboradores: any[] = [];
    
    const percorrer = (no: NoMapa, caminho: any[] = []) => {
      if (!no) return;
      
      // Se for colaborador, adicionar à lista com o caminho completo
      if (no.tipo === 'colaborador') {
        colaboradores.push({
          ...no,
          caminho: [...caminho],
          empresa: caminho.find(n => n.tipo === 'empresa')?.nome || '',
          localidade: caminho.find(n => n.tipo === 'localidade')?.nome || '',
          centroCusto: caminho.find(n => n.tipo === 'centroCusto')?.nome || ''
        });
      }
      
      // Continuar percorrendo os filhos
      if (no.filhos && no.filhos.length > 0) {
        const novoCaminho = no.tipo !== 'raiz' ? [...caminho, { tipo: no.tipo, nome: no.nome, id: no.id }] : caminho;
        no.filhos.forEach(filho => percorrer(filho, novoCaminho));
      }
    };
    
    percorrer(hierarquia);
    return colaboradores;
  }
  
  /**
   * 🆕 Verificar se algum nó está expandido (para uso no template)
   */
  temNosExpandidos(nodes: NoMapa[]): boolean {
    if (!nodes) return false;
    return nodes.some(n => n.isExpandido && n.filhos && n.filhos.length > 0);
  }

  /**
   * Alternar mostrar colaboradores sem recursos
   */
  async toggleMostrarSemRecursos() {
    // Se já tem dados carregados, recarregar automaticamente
    if (this.hierarquia || this.colaboradores.length > 0) {
      await this.carregarMapaRecursos();
    }
  }

  /**
   * Alternar mostrar histórico de recursos
   */
  async toggleMostrarHistorico() {
    // Se já tem dados carregados, recarregar automaticamente
    if (this.hierarquia || this.colaboradores.length > 0) {
      await this.carregarMapaRecursos();
    }
  }

  /**
   * Limpar filtros
   */
  limparFiltros() {
    
    this.empresaSelecionada = null;
    this.localidadeSelecionada = null;
    this.centroCustoSelecionado = null;
    
    // 🆕 Restaurar listas completas (sem filtro)
    this.localidades = [...this.todasLocalidades];
    this.centrosCusto = [...this.todosCentrosCusto];
    
    this.hierarquia = null;
    this.colaboradores = [];
    this.colaboradoresSemRecursos = [];
    this.colaboradoresLista = [];
    this.metricas = {};
    
  }

  /**
   * Obter cor do badge de status
   */
  getCorBadgeStatus(total: number): string {
    if (total === 0) return 'success';
    if (total <= 3) return 'warning';
    return 'danger';
  }

  /**
   * Formatar data
   */
  formatarData(data: string): string {
    if (!data) return '-';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  /**
   * Obter gradiente para nó do mapa mental
   */
  getNodeGradient(cor: string): string {
    const gradients = {
      '#2196F3': 'linear-gradient(135deg, #2196F3 0%, #1976D2 100%)', // Azul - Colaboradores com recursos
      '#FF9800': 'linear-gradient(135deg, #FF9800 0%, #F57C00 100%)', // Laranja - Localidades
      '#9C27B0': 'linear-gradient(135deg, #9C27B0 0%, #7B1FA2 100%)', // Roxo - Organização
      '#4CAF50': 'linear-gradient(135deg, #4CAF50 0%, #388E3C 100%)', // Verde - Centros de Custo / Recursos ativos
      '#9E9E9E': 'linear-gradient(135deg, #9E9E9E 0%, #757575 100%)', // Cinza - Colaboradores sem recursos / Recursos devolvidos
    };
    return gradients[cor] || `linear-gradient(135deg, ${cor} 0%, ${cor} 100%)`;
  }

  // ============================================
  // 🆕 MÉTODOS DE EXPORTAÇÃO
  // ============================================

  /**
   * 📄 Exportar como PDF - Gera HTML em nova janela para impressão
   */
  exportarPDF() {
    try {

      if (!this.hierarquia) {
        this.util.exibirMensagemToast('Nenhum mapa para exportar', 3000);
        return;
      }

      this.exportando = true;
      
      // Gerar HTML do mapa
      const htmlContent = this.gerarHTMLParaImpressao();
      
      // Abrir nova janela
      const janelaImpressao = window.open('', '_blank', 'width=800,height=600');
      
      if (!janelaImpressao) {
        this.util.exibirMensagemToast('Bloqueador de pop-up ativo. Permita pop-ups para esta página.', 5000);
        this.exportando = false;
        return;
      }
      
      // Escrever HTML na nova janela
      janelaImpressao.document.write(htmlContent);
      janelaImpressao.document.close();
      
      // Aguardar carregar e imprimir
      janelaImpressao.onload = () => {
        setTimeout(() => {
          janelaImpressao.print();
          this.exportando = false;
        }, 500);
      };

      this.util.exibirMensagemToast('Janela de impressão aberta. Escolha "Salvar como PDF".', 5000);
    } catch (error) {
      this.util.exibirMensagemToast('Erro ao preparar PDF', 5000);
      this.exportando = false;
    }
  }
  
  /**
   * 🆕 Gerar HTML completo para impressão (hierarquia expandida)
   */
  private gerarHTMLParaImpressao(): string {
    const dataAtual = new Date().toLocaleString('pt-BR');
    const nomeEmpresa = this.empresaSelecionada 
      ? this.empresas.find(e => e.id === this.empresaSelecionada)?.nome 
      : 'Todas as Empresas';
    
    let html = `
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <title>Mapa de Recursos - ${nomeEmpresa}</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      padding: 20px;
      background: white;
    }
    h1 {
      color: #080039;
      border-bottom: 3px solid #FF3A0F;
      padding-bottom: 10px;
      margin-bottom: 20px;
    }
    .info {
      color: #6c757d;
      margin-bottom: 30px;
      font-size: 14px;
    }
    .hierarquia {
      padding-left: 20px;
    }
    .nivel {
      margin: 10px 0;
      padding-left: 20px;
      border-left: 2px solid #e0e0e0;
    }
    .empresa {
      font-weight: bold;
      color: #2196F3;
      font-size: 16px;
      margin: 15px 0 5px 0;
    }
    .localidade {
      font-weight: bold;
      color: #FF9800;
      font-size: 15px;
      margin: 10px 0 5px 0;
    }
    .cc {
      font-weight: bold;
      color: #4CAF50;
      font-size: 14px;
      margin: 8px 0 5px 0;
    }
    .colaborador {
      color: #2196F3;
      font-size: 13px;
      margin: 5px 0;
    }
    .colaborador.sem-recursos {
      color: #9E9E9E;
    }
    .recurso {
      color: #4CAF50;
      font-size: 12px;
      margin-left: 20px;
      padding: 3px 0;
    }
    .recurso.devolvido {
      color: #9E9E9E;
      text-decoration: line-through;
    }
    .icone {
      margin-right: 5px;
    }
  </style>
</head>
<body>
  <h1>📊 Mapa de Recursos</h1>
  <div class="info">
    <strong>Empresa:</strong> ${nomeEmpresa}<br>
    <strong>Data/Hora:</strong> ${dataAtual}
  </div>
  <div class="hierarquia">
`;
    
    // Percorrer hierarquia e gerar HTML
    html += this.gerarHTMLNo(this.hierarquia, 0);
    
    html += `
  </div>
</body>
</html>`;
    
    return html;
  }
  
  /**
   * 🆕 Gerar HTML de um nó recursivamente
   */
  private gerarHTMLNo(no: NoMapa, nivel: number): string {
    if (!no) return '';
    
    let html = '';
    const indent = '  '.repeat(nivel);
    
    if (no.tipo === 'raiz') {
      // Raiz - processar filhos
      if (no.filhos && no.filhos.length > 0) {
        no.filhos.forEach(filho => {
          html += this.gerarHTMLNo(filho, nivel);
        });
      }
    } else if (no.tipo === 'empresa') {
      html += `${indent}<div class="empresa">🏭 ${no.nome} (${no.totalColaboradores || 0} colaboradores)</div>\n`;
      html += `${indent}<div class="nivel">\n`;
      if (no.filhos && no.filhos.length > 0) {
        no.filhos.forEach(filho => {
          html += this.gerarHTMLNo(filho, nivel + 1);
        });
      }
      html += `${indent}</div>\n`;
    } else if (no.tipo === 'localidade') {
      html += `${indent}<div class="localidade">📍 ${no.nome}</div>\n`;
      html += `${indent}<div class="nivel">\n`;
      if (no.filhos && no.filhos.length > 0) {
        no.filhos.forEach(filho => {
          html += this.gerarHTMLNo(filho, nivel + 1);
        });
      }
      html += `${indent}</div>\n`;
    } else if (no.tipo === 'centroCusto') {
      html += `${indent}<div class="cc">📁 ${no.nome}</div>\n`;
      html += `${indent}<div class="nivel">\n`;
      if (no.filhos && no.filhos.length > 0) {
        no.filhos.forEach(filho => {
          html += this.gerarHTMLNo(filho, nivel + 1);
        });
      }
      html += `${indent}</div>\n`;
    } else if (no.tipo === 'colaborador') {
      const classe = (!no.filhos || no.filhos.length === 0) ? 'colaborador sem-recursos' : 'colaborador';
      const recursos = no.totalRecursos || 0;
      html += `${indent}<div class="${classe}">👤 ${no.nome} (${recursos} recurso${recursos !== 1 ? 's' : ''})</div>\n`;
      if (no.filhos && no.filhos.length > 0) {
        no.filhos.forEach(filho => {
          html += this.gerarHTMLNo(filho, nivel + 1);
        });
      }
    } else if (no.tipo === 'recurso') {
      const classe = no.cor === '#9E9E9E' ? 'recurso devolvido' : 'recurso';
      const icone = this.getIconeTexto(no.icone);
      html += `${indent}<div class="${classe}">${icone} ${no.nome} (${no.descricao || 'N/A'})</div>\n`;
    }
    
    return html;
  }
  
  /**
   * 🆕 Converter classe de ícone em emoji/texto
   */
  private getIconeTexto(iconeClasse: string): string {
    const icones = {
      'cil-laptop': '💻',
      'cil-monitor': '🖥️',
      'cil-phone': '📞',
      'cil-mobile': '📱',
      'cil-mouse': '🖱️',
      'cil-keyboard': '⌨️',
      'cil-tv': '📺',
      'cil-user': '👤',
      'cil-user-follow': '👤',
    };
    return icones[iconeClasse] || '📦';
  }

  /**
   * 🖼️ Exportar como Imagem - Copiar para área de transferência
   */
  exportarImagem() {
    try {
      
      this.util.exibirMensagemToast(
        'Use Print Screen (PrtScn) ou ferramenta de captura do Windows (Win + Shift + S) para capturar a tela', 
        7000
      );
    } catch (error) {
    }
  }

  /**
   * 📊 Exportar como Excel
   */
  exportarExcel() {
    try {
      this.exportando = true;

      if (!this.hierarquia) {
        this.util.exibirMensagemToast('Nenhum dado para exportar', 3000);
        return;
      }

      // Extrair dados tabulados da hierarquia
      const dados = this.extrairDadosTabulados();

      if (dados.length === 0) {
        this.util.exibirMensagemToast('Nenhum colaborador encontrado', 3000);
        return;
      }

      // Criar planilha
      const worksheet = XLSX.utils.json_to_sheet(dados);
      const workbook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(workbook, worksheet, 'Mapa de Recursos');

      // Ajustar largura das colunas
      const colWidths = [
        { wch: 25 }, // Empresa
        { wch: 20 }, // Localidade
        { wch: 30 }, // Centro de Custo
        { wch: 30 }, // Colaborador
        { wch: 20 }, // Tipo Recurso
        { wch: 40 }, // Modelo
        { wch: 20 }, // Patrimônio/NS
        { wch: 15 }  // Status
      ];
      worksheet['!cols'] = colWidths;

      const nomeArquivo = `mapa-recursos-${this.getNomeArquivo()}.xlsx`;
      XLSX.writeFile(workbook, nomeArquivo);

      this.util.exibirMensagemToast('Excel exportado com sucesso!', 3000);
    } catch (error) {
      this.util.exibirMensagemToast('Erro ao exportar Excel', 5000);
    } finally {
      this.exportando = false;
    }
  }

  /**
   * 🆕 Extrair dados tabulados da hierarquia para Excel
   */
  private extrairDadosTabulados(): any[] {
    const dados: any[] = [];

    const percorrer = (no: NoMapa, caminho: any = {}) => {
      if (!no) return;

      // Atualizar caminho
      if (no.tipo === 'empresa') caminho.empresa = no.nome;
      if (no.tipo === 'localidade') caminho.localidade = no.nome;
      if (no.tipo === 'centroCusto') caminho.centroCusto = no.nome;
      if (no.tipo === 'colaborador') caminho.colaborador = no.nome;

      // Se for recurso, adicionar linha na planilha
      if (no.tipo === 'recurso') {
        dados.push({
          'Empresa': caminho.empresa || '',
          'Localidade': caminho.localidade || '',
          'Centro de Custo': caminho.centroCusto || '',
          'Colaborador': caminho.colaborador || '',
          'Tipo Recurso': no.nome.split(' - ')[0] || '',
          'Modelo/Descrição': no.nome.split(' - ')[1] || no.nome,
          'Patrimônio/Número': no.descricao || '',
          'Status': no.cor === '#4CAF50' ? 'Ativo' : 'Devolvido'
        });
      }

      // Se for colaborador SEM recursos, adicionar linha vazia
      if (no.tipo === 'colaborador' && (!no.filhos || no.filhos.length === 0)) {
        dados.push({
          'Empresa': caminho.empresa || '',
          'Localidade': caminho.localidade || '',
          'Centro de Custo': caminho.centroCusto || '',
          'Colaborador': no.nome,
          'Tipo Recurso': 'Sem recursos',
          'Modelo/Descrição': '-',
          'Patrimônio/Número': '-',
          'Status': '-'
        });
      }

      // Percorrer filhos
      if (no.filhos && no.filhos.length > 0) {
        no.filhos.forEach(filho => percorrer(filho, { ...caminho }));
      }
    };

    percorrer(this.hierarquia);
    return dados;
  }

  /**
   * 🆕 Gerar nome de arquivo baseado nos filtros
   */
  private getNomeArquivo(): string {
    const dataHora = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
    const filtros: string[] = [];

    if (this.empresaSelecionada) {
      const empresa = this.empresas.find(e => e.id === this.empresaSelecionada);
      if (empresa) filtros.push(empresa.nome.replace(/\s+/g, '-'));
    }

    if (this.localidadeSelecionada) {
      const loc = this.localidades.find(l => l.id === this.localidadeSelecionada);
      if (loc) filtros.push(loc.descricao.replace(/\s+/g, '-'));
    }

    if (this.centroCustoSelecionado) {
      filtros.push(`CC${this.centroCustoSelecionado}`);
    }

    const filtroStr = filtros.length > 0 ? filtros.join('_') : 'completo';
    return `${filtroStr}_${dataHora}`;
  }
}

