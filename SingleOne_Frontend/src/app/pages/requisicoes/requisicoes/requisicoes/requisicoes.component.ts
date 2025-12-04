import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-requisicoes',
  templateUrl: './requisicoes.component.html',
  styleUrls: ['./requisicoes.component.scss']
})
export class RequisicoesComponent implements OnInit, AfterViewInit {

  private session: any = {};
  public colunas = ['id', 'status', 'usuabertura', 'usuresponsavel', 'recursos', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public statusFiltro: string = 'Ativa';
  
  // ✅ NOVO: Propriedades para estatísticas totais (sem filtro)
  public estatisticasTotais = {
    total: 0,
    ativas: 0,
    processadas: 0,
    canceladas: 0
  };
  
  // ✅ NOVO: Dados completos para estatísticas
  private dadosCompletos: any[] = [];
  
  // ✅ NOVO: Cache para informações de linhas telefônicas
  private cacheLinhasTelefonicas: Map<number, any> = new Map();

  // ✅ NOVO: Propriedades para estatísticas em tempo real
  public estatisticas = {
    total: 0,
    ativas: 0,
    processadas: 0,
    canceladas: 0
  };

  // Getter para dados paginados
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.paginator) {
      return this.dataSource?.data || [];
    }
    
    const startIndex = this.dataSource.paginator.pageIndex * this.dataSource.paginator.pageSize;
    const endIndex = startIndex + this.dataSource.paginator.pageSize;
    return this.dataSource.data.slice(startIndex, endIndex);
  }

  constructor(
    private util: UtilService, 
    private api: RequisicaoApiService, 
    private telefoniaApi: TelefoniaApiService,
    private route: Router,
    private activatedRoute: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.session = this.util.getSession('usuario');
    if (this.session && this.session.usuario) {
      this.cliente = this.session.usuario.cliente;
    } else {
      console.error('[REQUISICOES] Sessão ou usuário inválido');
      this.cliente = 1; // Valor padrão para evitar erro
    }
    
    // ✅ CORREÇÃO: Validar se o cliente é válido antes de fazer a requisição
    if (!this.cliente || this.cliente <= 0) {
      console.error('[REQUISICOES] Cliente inválido:', this.cliente);
      this.util.exibirMensagemToast('Cliente inválido. Por favor, faça login novamente.', 5000);
      this.route.navigate(['/']);
      return;
    }
    
    // ✅ NOVO: Processar query parameters para busca automática
    this.activatedRoute.queryParams.subscribe(params => {
      if (params['requisicaoId'] && params['source'] === 'recursos') {
        const requisicaoId = params['requisicaoId'];
        this.aplicarFiltroAutomaticoRequisicao(requisicaoId);
        
        // ✅ NOVO: Mostrar mensagem explicativa
        this.util.exibirMensagemToast(
          `Filtro automático aplicado: Mostrando requisição ${requisicaoId} com todos os seus recursos`, 
          5000
        );
      } else if (params['search']) {
        const searchTerm = params['search'];
        this.consulta.setValue(searchTerm);
        this.buscar(searchTerm);
        
        // ✅ NOVO: Mostrar mensagem de feedback para o usuário
        this.util.exibirMensagemToast(
          `Filtro aplicado: Buscando requisições com recurso "${searchTerm}"`, 
          3000
        );
      } else {
        // Se não há parâmetro de busca, carregar todas as requisições
        this.listar(null);
      }
    });
    
    // ✅ NOVO: Configurar subscription para o campo de busca
    this.consulta.valueChanges.pipe(
      debounceTime(500), // Aguardar 500ms após o usuário parar de digitar
      tap(valor => {
        if (valor && valor.trim()) {
          this.buscar(valor.trim());
        } else if (valor === '' || valor === null) {
          // Se o campo estiver vazio, recarregar todas as requisições
          this.listar(null);
        }
      })
    ).subscribe();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  // 🔧 MÉTODO AUXILIAR PARA CONFIGURAR PAGINADOR
  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      console.warn('[REQUISICOES] Paginador ou dataSource não disponível para configuração');
      return;
    }
    
    // CONFIGURAÇÃO SIMPLES E DIRETA
    this.dataSource.paginator = this.paginator;
    
    // CONFIGURAR TAMANHO INICIAL
    this.paginator.pageSize = 10;
    this.paginator.pageIndex = 0;
    
    // ADICIONAR LISTENER PARA MUDANÇAS
    this.paginator.page.subscribe(() => {
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    });
  }

  listar(event: PageEvent): Promise<void> {
    // ✅ CORREÇÃO: Validar sessão antes de fazer a requisição
    if (!this.session || !this.session.token) {
      console.error('[REQUISICOES] Sessão inválida ao listar');
      this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
      this.route.navigate(['/']);
      return;
    }
    
    this.util.aguardar(true);
    const pagina = ((event == null) ? 1 : event.pageIndex + 1);
    
    // ✅ CORREÇÃO: Usar filtro que o backend entenda para buscar todas as requisições
    const filtro = 'null'; // O backend verifica se pesquisa != "null" para aplicar filtros
    return this.api.listarRequisicoes(filtro, this.cliente, pagina, this.session.token).then(res => {
      this.util.aguardar(false);
      if (!res || res.error) {
        console.error('[REQUISICOES] Resposta com erro:', res);
        this.util.exibirFalhaComunicacao();
        return;
      }
      if (res.status != 200 && res.status != 204) {
        console.error('[REQUISICOES] Status inválido:', res.status);
        this.util.exibirFalhaComunicacao();
      } else {
        // ✅ CORREÇÃO: Verificar se data tem a estrutura esperada
        if (res.data && (res.data.results || Array.isArray(res.data))) {
          const results = res.data.results || res.data;
          
          // ✅ NOVO: Salvar TODOS os dados para histórico completo
          this.dadosCompletos = [...results];
          
          // ✅ DEBUG: Verificar estrutura dos dados recebidos
          if (results.length > 0) {
            // ✅ NOVO: Verificar se há outras propriedades que podem conter os dados
          }
          
          // ✅ NOVO: Calcular estatísticas com TODOS os dados
          this.calcularEstatisticas(results);
          
          // ✅ OTIMIZAÇÃO: Pré-carregar apenas linhas telefônicas da página atual
          this.preCarregarLinhasTelefonicas(results).then(() => {
            this.cdr.detectChanges();
          });
          
          this.dataSource = new MatTableDataSource(results);
          
          // ✅ CORREÇÃO: Aplicar filtro padrão "Ativa" após carregar dados
          setTimeout(() => {
            this.filtrarPorStatusCard('Ativa');
          }, 100);
          
          // ✅ CORREÇÃO: Configurar paginador corretamente
          if (this.paginator) {
            const currentPage = res.data.currentPage || 1;
            const rowCount = res.data.rowCount || results.length;
            
            this.paginator.pageIndex = currentPage - 1;
            this.paginator.pageSize = 10;
            this.paginator.length = rowCount;
            
            // ✅ CORREÇÃO CRÍTICA: Configurar paginador após atualizar propriedades
            this.configurarPaginador();
          } else {
            console.warn('[REQUISICOES] Paginador não disponível, dados carregados sem paginação');
          }
          this.util.exibirMensagemToast(
            `Carregadas ${results.length} requisições (incluindo histórico completo)`, 
            3000
          );
        } else {
          console.warn('[REQUISICOES] Estrutura de dados inválida:', res.data);
          this.dataSource = new MatTableDataSource([]);
          this.dadosCompletos = [];
          // ✅ NOVO: Calcular estatísticas com dados vazios
          this.calcularEstatisticas([]);
        }
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('[REQUISICOES] Erro na requisição:', err);
      
      // ✅ CORREÇÃO: Verificar diferentes tipos de erro
      const errorStatus = err?.response?.status || err?.status || err?.statusCode;
      
      if (errorStatus === 401) {
        this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
        this.route.navigate(['/']);
      } else {
        this.util.exibirFalhaComunicacao();
      }
    });
  }

  buscar(valor) {
    // ✅ CORREÇÃO: Validar sessão antes de fazer a requisição
    if (!this.session || !this.session.token) {
      console.error('[REQUISICOES] Sessão inválida ao buscar');
      this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
      this.route.navigate(['/']);
      return;
    }
    
    if (valor != '' && valor != null && valor != 'null') {
      this.util.aguardar(true);
      const isNumeroTelefone = this.isNumeroTelefone(valor);
      const isNumeroSerie = !isNumeroTelefone && this.isNumeroSerieEquipamento(valor);
      
      // ✅ CORREÇÃO: Usar sempre busca geral que agora inclui número de série e telefone
      const apiCall = this.api.listarRequisicoes(valor, this.cliente, 1, this.session.token);
      
      apiCall.then(res => {
        // ✅ CORREÇÃO: Usar método centralizado para processar resultado
        this.processarResultadoBusca(res, valor, isNumeroSerie, isNumeroTelefone);
        if (res.status != 200 && res.status != 204) {
          console.error('[REQUISICOES] Status inválido na busca:', res.status);
          this.util.exibirFalhaComunicacao();
        } else {
        // ✅ CORREÇÃO: Verificar se data tem a estrutura esperada
        if (res.data && (res.data.results || Array.isArray(res.data))) {
          let results = res.data.results || res.data;
          
          // ✅ NOVO: Se for busca por número de série e não foi endpoint específico, filtrar localmente
          if (isNumeroSerie && !res.data.filtradoPorEquipamento) {
            results = this.filtrarRequisicoesPorEquipamento(results, valor);
          }
          
          this.dataSource = new MatTableDataSource(results);
          
          // ✅ NOVO: Calcular estatísticas em tempo real
          this.calcularEstatisticas(results);
            
            // ✅ CORREÇÃO: Validar paginador antes de atribuí-lo
            if (this.paginator) {
              // ✅ CORREÇÃO CRÍTICA: Configurar paginador após criar novo dataSource
              this.configurarPaginador();
            } else {
              console.warn('[REQUISICOES] Paginador não disponível para busca');
            }
            if (results.length === 0) {
              this.util.exibirMensagemToast(
                'Nenhuma requisição encontrada com os critérios informados', 
                3000
              );
            } else {
              this.util.exibirMensagemToast(
                `Busca realizada: ${results.length} requisições encontradas`, 
                3000
              );
            }
          } else {
            console.warn('[REQUISICOES] Dados inválidos na busca:', res.data);
            this.dataSource = new MatTableDataSource([]);
            // ✅ NOVO: Calcular estatísticas com dados vazios
            this.calcularEstatisticas([]);
            
            // ✅ NOVO: Mensagem sobre dados inválidos
            this.util.exibirMensagemToast(
              'Erro ao processar resultados da busca', 
              3000
            );
          }
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('[REQUISICOES] Erro na busca:', err);
        
        // ✅ CORREÇÃO: Verificar diferentes tipos de erro
        const errorStatus = err?.response?.status || err?.status || err?.statusCode;
        
        if (errorStatus === 401) {
          this.util.exibirMensagemToast('Sessão expirada. Por favor, faça login novamente.', 5000);
          this.route.navigate(['/']);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      });
    } else {
      // ✅ CORREÇÃO: Quando não há valor de busca, recarregar todas as requisições
      this.listar(null);
    }
  }

  filtrarPorStatus() {
    this.listar(null);
  }

  limparFiltro() {
    this.statusFiltro = '';
    this.listar(null);
  }

  // 📊 MÉTODOS PARA ESTATÍSTICAS
  getTotalRequisicoes(): number {
    return this.estatisticasTotais.total;
  }

  getRequisicoesAtivas(): number {
    return this.estatisticasTotais.ativas;
  }

  getRequisicoesProcessadas(): number {
    return this.estatisticasTotais.processadas;
  }

  getRequisicoesCanceladas(): number {
    return this.estatisticasTotais.canceladas;
  }

  // ✅ NOVO: Métodos para novos status
  getRequisicoesPendentes(): number {
    return 0; // Status não existe no sistema
  }

  getRequisicoesAprovadas(): number {
    return 0; // Status não existe no sistema
  }

  getRequisicoesRejeitadas(): number {
    return 0; // Status não existe no sistema
  }

  // 🎨 MÉTODOS PARA CLASSES DE STATUS
  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'ativa':
        return 'status-active';
      case 'processada':
        return 'status-processed';
      case 'cancelada':
        return 'status-cancelled';
      default:
        return 'status-default';
    }
  }

  // ✅ NOVO: Métodos getter para acessar dados da requisição de forma segura
  getRequisicaoId(row: any): number {
    return row?.requisicao?.id || row?.Requisicao?.Id || row?.Requisicao?.id || 0;
  }

  getRequisicaoStatus(row: any): string {
    return row?.requisicao?.requisicaostatus || row?.Requisicao?.Requisicaostatus || 'Desconhecido';
  }

  getRequisicaoStatusId(row: any): number {
    return row?.requisicao?.requisicaostatusid || row?.Requisicao?.Requisicaostatusid || 0;
  }

  getRequisicaoUsuario(row: any): string {
    return row?.requisicao?.usuariorequisicao || row?.Requisicao?.Usuariorequisicao || 'N/A';
  }

  getRequisicaoTecnico(row: any): string {
    return row?.requisicao?.tecnicoresponsavel || row?.Requisicao?.Tecnicoresponsavel || 'N/A';
  }

  // ✅ NOVO: Método para obter nome/descrição da requisição
  getRequisicaoNome(row: any): string {
    const id = this.getRequisicaoId(row);
    return `Requisição ${id}`;
  }

  getRequisicaoDataCriacao(row: any): string {
    const data = row?.requisicao?.dtsolicitacao;
    if (data) {
      try {
        return new Date(data).toLocaleDateString('pt-BR');
      } catch (error) {
        return 'Data inválida';
      }
    }
    return 'N/A';
  }

  // Verifica se a requisição está acima do prazo (5 dias) E está ativa
  isRequisicaoAtrasada(row: any): boolean {
    // ✅ CORREÇÃO: Só mostrar alerta para requisições ativas (status ID = 1)
    const statusId = this.getRequisicaoStatusId(row);
    if (statusId !== 1) {
      return false; // Não é ativa, não mostrar alerta
    }

    const data = row?.requisicao?.dtsolicitacao;
    if (data) {
      try {
        const dataCriacao = new Date(data);
        const hoje = new Date();
        const diffTime = Math.abs(hoje.getTime() - dataCriacao.getTime());
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return diffDays > 5;
      } catch (error) {
        return false;
      }
    }
    return false;
  }

  // Calcula quantos dias tem a requisição
  getDiasRequisicao(row: any): number {
    const data = row?.requisicao?.dtsolicitacao;
    if (data) {
      try {
        const dataCriacao = new Date(data);
        const hoje = new Date();
        const diffTime = Math.abs(hoje.getTime() - dataCriacao.getTime());
        return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      } catch (error) {
        return 0;
      }
    }
    return 0;
  }

  // Retorna a mensagem de alerta para o tooltip (apenas para requisições ativas)
  getAlertaRequisicao(row: any): string {
    // ✅ CORREÇÃO: Só mostrar alerta para requisições ativas (status ID = 1)
    const statusId = this.getRequisicaoStatusId(row);
    if (statusId !== 1) {
      return ''; // Não é ativa, não mostrar alerta
    }

    const dias = this.getDiasRequisicao(row);
    if (dias > 5) {
      return `⚠️ ALERTA: Esta requisição ATIVA está há ${dias} dias sem entrega!\n\nPrazo padrão: 5 dias\nStatus: ATRASADA\n\nRecomendação: Priorizar entrega imediatamente.`;
    }
    return '';
  }

  getEquipamentosRequisicao(row: any): any[] {
    // ✅ Retornar equipamentos se existirem
    let equipamentos = row?.equipamentosRequisicao || row?.EquipamentosRequisicao || [];
    
    // ✅ NOVO: Verificar se os equipamentos estão em RequisicaoItens
    if (!equipamentos || equipamentos.length === 0) {
      const requisicaoItens = row?.RequisicaoItens || row?.requisicaoItens || [];
      if (requisicaoItens && requisicaoItens.length > 0) {
        // ✅ Filtrar APENAS equipamentos (não linhas telefônicas)
        equipamentos = requisicaoItens.filter((item: any) => {
          const isEquipamento = item?.Equipamento || item?.equipamento;
          return isEquipamento;
        });
        
        // ✅ Mapear APENAS equipamentos para o formato esperado
        equipamentos = equipamentos.map((item: any) => {
          const isEquipamento = item?.Equipamento || item?.equipamento;
          
          if (isEquipamento) {
            return {
              equipamento: isEquipamento,
              numeroserie: item?.Numeroserie || item?.numeroserie || 'N/A',
              tipo: 'equipamento'
            };
          }
          return null;
        }).filter(item => item !== null);
      }
    }
    return Array.isArray(equipamentos) ? equipamentos : [];
  }

  // ✅ Método para pré-carregar informações das linhas telefônicas
  private async preCarregarLinhasTelefonicas(dados: any[]): Promise<void> {
    // ✅ LIMPEZA: Limpar cache antigo se ficar muito grande
    if (this.cacheLinhasTelefonicas.size > 100) {
      this.cacheLinhasTelefonicas.clear();
    }
    
    const linhasParaCarregar = new Set<number>();
    
    // ✅ Coletar todos os IDs de linhas telefônicas não cacheadas
    dados.forEach(row => {
      const requisicaoItens = row?.RequisicaoItens || row?.requisicaoItens || [];
      requisicaoItens.forEach((item: any) => {
        const linhaId = item?.Linhatelefonica || item?.linhatelefonica;
        if (linhaId && !this.cacheLinhasTelefonicas.has(linhaId)) {
          linhasParaCarregar.add(linhaId);
        }
      });
    });
    
    if (linhasParaCarregar.size === 0) {
      return;
    }
    const promises = Array.from(linhasParaCarregar).map(async (linhaId) => {
      try {
        const response = await this.telefoniaApi.buscarLinhaPorId(linhaId, this.session.token);
        
        if (response && response.status === 200 && response.data) {
          const linha = response.data;
          this.cacheLinhasTelefonicas.set(linhaId, {
            numero: linha.numero || linhaId,
            operadora: linha.planoNavigation?.contratoNavigation?.operadoraNavigation?.nome || 'N/A',
            plano: linha.planoNavigation?.nome || 'N/A'
          });
        }
      } catch (error) {
        console.error('[REQUISICOES] Erro ao buscar linha:', linhaId, error);
        this.cacheLinhasTelefonicas.set(linhaId, {
          numero: linhaId,
          operadora: 'N/A',
          plano: 'N/A'
        });
      }
    });
    
    await Promise.all(promises);
  }

  // ✅ Método síncrono para o HTML
  getLinhasTelefonicas(row: any): any[] {
    // ✅ Retornar linhas telefônicas se existirem
    let linhas = row?.linhasTelefonicas || row?.LinhasTelefonicas || [];
    
    // ✅ NOVO: Verificar se as linhas estão em RequisicaoItens
    if (!linhas || linhas.length === 0) {
      const requisicaoItens = row?.RequisicaoItens || row?.requisicaoItens || [];
      if (requisicaoItens && requisicaoItens.length > 0) {
        // ✅ Filtrar apenas os itens que são linhas telefônicas
        const linhasItems = requisicaoItens.filter((item: any) => {
          const isLinhaTelefonica = item?.Linhatelefonica || item?.linhatelefonica;
          return isLinhaTelefonica;
        });
        
        // ✅ Mapear usando o cache
        linhas = linhasItems.map((item: any) => {
          const linhaId = item?.Linhatelefonica || item?.linhatelefonica;
          const linhaCacheada = this.cacheLinhasTelefonicas.get(linhaId);
          
          if (linhaCacheada) {
            return linhaCacheada;
          }
          
          // ✅ Fallback se não estiver no cache
          return {
            numero: linhaId || 'N/A',
            operadora: 'Carregando...',
            plano: 'Carregando...'
          };
        });
      }
    }
    return Array.isArray(linhas) ? linhas : [];
  }

  editar(obj) {
    this.route.navigate(['/recurso', btoa(JSON.stringify(obj))]);
  }

  excluir(req) {
    // ✅ CORREÇÃO: Validar se a requisição pode ser cancelada
    if (!req || (!req.requisicao && !req.Requisicao)) {
      this.util.exibirMensagemToast('Dados da requisição inválidos', 3000);
      return;
    }
    
    // ✅ CORREÇÃO: Usar estrutura correta (requisicao ou Requisicao)
    const requisicao = req.requisicao || req.Requisicao;
    const statusAtual = requisicao.requisicaostatus || requisicao.Requisicaostatus;
    const statusId = requisicao.requisicaostatusid || requisicao.Requisicaostatusid;
    const requisicaoId = this.getRequisicaoId(req);
    if (statusAtual === 'Processada' || statusId === 2) {
      this.util.exibirMensagemToast(
        'Requisição já foi processada e não pode ser cancelada. Ela permanecerá no histórico.', 
        5000
      );
      return;
    }
    
    if (statusAtual === 'Cancelada' || statusId === 3) {
      this.util.exibirMensagemToast(
        'Requisição já foi cancelada anteriormente.', 
        3000
      );
      return;
    }
    
    // ✅ CORREÇÃO: Confirmar cancelamento apenas para requisições ativas
    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja cancelar a requisição?<br><br>` +
      `📋 <strong>Requisição:</strong> #REQ${requisicaoId}<br>` +
      `📊 <strong>Status atual:</strong> ${statusAtual}<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação não pode ser desfeita e a requisição será movida para o histórico.`,
      true
    ).then(aceita => {
      if (aceita) {
      const requisicaoParaCancelar = {
        id: requisicao.id || requisicao.Id,
        requisicaostatus: 2, // ✅ CORREÇÃO: Status 2 = Cancelada (ID 2 = Cancelada no backend)
        usuarioRequisicao: requisicao.usuariorequisicaoid || requisicao.Usuariorequisicaoid,
        tecnicoResponsavel: requisicao.tecnicoresponsavelid || requisicao.Tecnicoresponsavelid,
        dtSolicitacao: requisicao.dtsolicitacao || requisicao.Dtsolicitacao,
        hashRequisicao: requisicao.hashrequisicao || requisicao.Hashrequisicao,
        requisicoesItens: [],
        cliente: requisicao.cliente || requisicao.Cliente
      };
      const equipamentosRequisicao = req.equipamentosRequisicao || req.EquipamentosRequisicao || [];
      const requisicaoItens = req.RequisicaoItens || req.requisicaoItens || [];
      
      // ✅ CORREÇÃO: Usar tanto equipamentos quanto itens
      if (equipamentosRequisicao && equipamentosRequisicao.length > 0) {
        equipamentosRequisicao.forEach(item => {
          const ri = { 
            id: item.id, 
            requisicao: item.requisicao, 
            equipamento: item.equipamentoid, 
            linhaTelefonica: item.linhatelefonica 
          };
          requisicaoParaCancelar.requisicoesItens.push(ri);
        });
      }
      
      // ✅ NOVO: Incluir também RequisicaoItens se existirem
      if (requisicaoItens && requisicaoItens.length > 0) {
        requisicaoItens.forEach(item => {
          const ri = { 
            id: item.Id || item.id, 
            requisicao: item.Requisicao || item.requisicao, 
            equipamento: item.Equipamento || item.equipamento, 
            linhaTelefonica: item.Linhatelefonica || item.linhatelefonica 
          };
          requisicaoParaCancelar.requisicoesItens.push(ri);
        });
      }
      this.util.aguardar(true);
      this.api.salvarRequisicao(requisicaoParaCancelar, this.session.token).then(res => {
        this.util.aguardar(false);
        if (!res || res.error) {
          console.error('[REQUISICOES] Resposta com erro ao salvar:', res);
          this.util.exibirFalhaComunicacao();
          return;
        }
        
        if (res.status != 200) {
          this.util.exibirFalhaComunicacao();
        } else {
          const retorno: any = res.data;
          if (retorno.Status == "200.1") {
            this.util.exibirMensagemToast(
              `Requisição #REQ${requisicaoId} cancelada com sucesso e movida para o histórico.`, 
              5000
            );
          } else {
            this.util.exibirMensagemToast(retorno.Mensagem || 'Requisição cancelada com sucesso', 5000);
          }
          
          // ✅ CORREÇÃO: Recarregar dados para atualizar estatísticas
          this.listar(null);
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('[REQUISICOES] Erro ao cancelar requisição:', err);
        this.util.exibirFalhaComunicacao();
      });
      }
    });
  }

  // 📊 MÉTODOS PARA ESTATÍSTICAS EM TEMPO REAL
  private calcularEstatisticas(dados: any[]): void {
    try {
      this.dadosCompletos = [...dados];
      if (!dados || dados.length === 0) {
        console.warn('[REQUISICOES] Nenhum dado para calcular estatísticas');
        this.estatisticasTotais = {
          total: 0,
          ativas: 0,
          processadas: 0,
          canceladas: 0
        };
        return;
      }

      // ✅ CALCULAR ESTATÍSTICAS TOTAIS (sem filtro)
      this.estatisticasTotais.total = dados.length;
      this.estatisticasTotais.ativas = dados.filter(item => {
        const status = this.obterStatusRequisicao(item);
        const isAtiva = status === 'Ativa' || status === 'ativa' || status === 'ATIVA';
        if (isAtiva) {
        }
        return isAtiva;
      }).length;
      
      this.estatisticasTotais.processadas = dados.filter(item => {
        const status = this.obterStatusRequisicao(item);
        const isProcessada = status === 'Processada' || status === 'processada' || status === 'PROCESSADA';
        if (isProcessada) {
        }
        return isProcessada;
      }).length;
      
      this.estatisticasTotais.canceladas = dados.filter(item => {
        const status = this.obterStatusRequisicao(item);
        const isCancelada = status === 'Cancelada' || status === 'cancelada' || status === 'CANCELADA';
        if (isCancelada) {
        }
        return isCancelada;
      }).length;
      
      // ✅ DEBUG: Log detalhado de cada item para debug
      dados.forEach((item, index) => {
        const status = this.obterStatusRequisicao(item);
      });
    } catch (error) {
      console.error('[REQUISICOES] Erro ao calcular estatísticas:', error);
      this.estatisticasTotais = {
        total: 0,
        ativas: 0,
        processadas: 0,
        canceladas: 0
      };
    }
  }

  // ✅ NOVO: Método para obter o status da requisição de forma segura
  private obterStatusRequisicao(item: any): string {
    try {
      if (item?.Requisicao?.Requisicaostatus) {
        return item.Requisicao.Requisicaostatus;
      }
      
      // ✅ CORREÇÃO: Estrutura alternativa - RequisicaoVM.Requisicao.Requisicaostatusid
      if (item?.Requisicao?.Requisicaostatusid) {
        const statusId = item.Requisicao.Requisicaostatusid;
        let statusString = '';
        
        switch (statusId) {
          case 1: statusString = 'Ativa'; break;
          case 2: statusString = 'Processada'; break;
          case 3: statusString = 'Cancelada'; break;
          case 4: statusString = 'Entregue'; break;
          default: statusString = 'Desconhecido'; break;
        }
        return statusString;
      }
      
      // ✅ CORREÇÃO: Estrutura direta (fallback)
      if (item?.Requisicaostatus) {
        return item.Requisicaostatus;
      }
      
      // ✅ NOVA CORREÇÃO: Estrutura alternativa - item.requisicao.requisicaostatus
      if (item?.requisicao?.requisicaostatus) {
        return item.requisicao.requisicaostatus;
      }
      
      // ✅ NOVA CORREÇÃO: Estrutura alternativa - item.requisicao.requisicaostatusid
      if (item?.requisicao?.requisicaostatusid) {
        const statusId = item.requisicao.requisicaostatusid;
        let statusString = '';
        
        switch (statusId) {
          case 1: statusString = 'Ativa'; break;
          case 2: statusString = 'Processada'; break;
          case 3: statusString = 'Cancelada'; break;
          case 4: statusString = 'Entregue'; break;
          default: statusString = 'Desconhecido'; break;
        }
        
        return statusString;
      }
      
      console.warn('[REQUISICOES] Estrutura de dados não reconhecida para status:', item);
      return 'Desconhecido';
      
    } catch (error) {
      console.error('[REQUISICOES] Erro ao obter status da requisição:', error);
      return 'Erro';
    }
  }

  // ✅ NOVO: Métodos para filtrar por status ao clicar nos cards
  filtrarPorStatusCard(status: string): void {
    try {
      if (!this.dadosCompletos || this.dadosCompletos.length === 0) {
        console.warn('[REQUISICOES] Nenhum dado disponível para filtrar');
        return;
      }
      
      // ✅ FILTRAR DADOS COMPLETOS POR STATUS
      let dadosFiltrados: any[] = [];
      
      if (status === 'Total') {
        dadosFiltrados = [...this.dadosCompletos];
        this.statusFiltro = '';
      } else {
        dadosFiltrados = this.dadosCompletos.filter(item => {
          const itemStatus = this.obterStatusRequisicao(item);
          const isMatch = itemStatus === status || 
                         itemStatus === status.toLowerCase() || 
                         itemStatus === status.toUpperCase();
          
          if (isMatch) {
          }
          
          return isMatch;
        });
        this.statusFiltro = status;
      }
      
      // ✅ ATUALIZAR DATASOURCE COM DADOS FILTRADOS
      this.dataSource = new MatTableDataSource(dadosFiltrados);
      
      // ✅ CONFIGURAR PAGINADOR
      if (this.paginator) {
        this.paginator.pageIndex = 0; // Voltar para primeira página
        // ✅ CORREÇÃO: Preservar o tamanho da página atual
        const pageSizeAtual = this.paginator.pageSize || 10;
        this.paginator.pageSize = pageSizeAtual;
        this.paginator.length = dadosFiltrados.length;
        
        // ✅ CORREÇÃO CRÍTICA: Reconectar o paginador ao novo dataSource
        this.configurarPaginador();
      }
      
      // ✅ NOVO: Mostrar mensagem informativa sobre o filtro aplicado
      if (status === 'Total') {
        this.util.exibirMensagemToast(
          `Mostrando histórico completo: ${dadosFiltrados.length} requisições`, 
          3000
        );
      } else if (status === 'Ativa' && this.statusFiltro === '') {
        // ✅ NOVO: Mensagem especial para filtro padrão
        this.util.exibirMensagemToast(
          `Filtro padrão aplicado: ${dadosFiltrados.length} requisições ativas`, 
          3000
        );
      } else {
        this.util.exibirMensagemToast(
          `Filtro aplicado: ${dadosFiltrados.length} requisições com status "${status}"`, 
          3000
        );
      }
    } catch (error) {
      console.error('[REQUISICOES] Erro ao filtrar por status:', error);
      this.util.exibirMensagemToast('Erro ao aplicar filtro', 3000);
    }
  }

  // ✅ NOVO: Método para filtrar por período (histórico)
  filtrarPorPeriodo(dias: number): void {
    try {
      if (!this.dadosCompletos || this.dadosCompletos.length === 0) {
        return;
      }
      
      const dataLimite = new Date();
      dataLimite.setDate(dataLimite.getDate() - dias);
      
      const dadosFiltrados = this.dadosCompletos.filter(item => {
        if (item?.requisicao?.dtsolicitacao) {
          const dataRequisicao = new Date(item.requisicao.dtsolicitacao);
          return dataRequisicao >= dataLimite;
        }
        return false;
      });
      
      this.dataSource = new MatTableDataSource(dadosFiltrados);
      this.statusFiltro = `Últimos ${dias} dias`;
      
      if (this.paginator) {
        this.paginator.pageIndex = 0;
        this.paginator.length = dadosFiltrados.length;
        this.configurarPaginador();
      }
      
      this.util.exibirMensagemToast(
        `Filtro aplicado: ${dadosFiltrados.length} requisições dos últimos ${dias} dias`, 
        3000
      );
    } catch (error) {
      console.error('[REQUISICOES] Erro ao filtrar por período:', error);
    }
  }

  // ✅ NOVO: Método para limpar filtro e mostrar padrão "Ativa"
  limparFiltroCard(): void {
    this.filtrarPorStatusCard('Ativa');
  }

  // ✅ NOVO: Método para limpar busca
  limparBusca(): void {
    this.consulta.setValue('');
    this.listar(null); // Recarregar com filtro padrão
  }

  // ✅ NOVO: Método para mostrar todas as requisições (histórico completo)
  mostrarTodasRequisicoes(): void {
    this.filtrarPorStatusCard('Total');
  }

  // ✅ NOVO: Método para verificar se há filtro ativo
  getFiltroAtivo(): string {
    if (!this.statusFiltro || this.statusFiltro === '') {
      return 'Padrão (Ativas)';
    }
    return this.statusFiltro;
  }

  // ✅ NOVO: Método para verificar se há filtro aplicado
  temFiltroAtivo(): boolean {
    // ✅ CORREÇÃO: Filtro padrão "Ativa" não é considerado filtro ativo
    return this.statusFiltro && this.statusFiltro !== '' && this.statusFiltro !== 'Ativa';
  }

  // ✅ NOVO: Método para identificar se a busca é por número de série
  private isNumeroSerieEquipamento(valor: string): boolean {
    if (!valor || typeof valor !== 'string') {
      return false;
    }
    
    // Padrões comuns de números de série de equipamentos (mais específicos)
    const patterns = [
      /^[A-Z]{3}-\d{1,}[A-Z0-9]{6,}$/,  // Ex: AUT-1UVBEGI935
      /^[A-Z0-9]{8,}$/,                  // Ex: ABC123456789 (mas não puramente numérico)
      /^[A-Z]{2,}\d{6,}$/,               // Ex: DE123456789
      /^\d{16,}$/,                       // Ex: 1234567890123456 (16+ dígitos - não telefone)
      /^[A-Z0-9]{6,}-[A-Z0-9]{6,}$/,    // Ex: ABC123-DEF456
      /^[A-Z]{1,}\d{8,}$/                // Ex: A123456789 (letra + 8+ dígitos)
    ];
    
    return patterns.some(pattern => pattern.test(valor.trim().toUpperCase()));
  }

  // ✅ NOVO: Método para identificar se a busca é por número de telefone
  private isNumeroTelefone(valor: string): boolean {
    if (!valor || typeof valor !== 'string') {
      return false;
    }
    
    // Padrões específicos para números de telefone brasileiros
    const patterns = [
      /^\d{10,11}$/,                     // Ex: 8590987654, 85909876543 (10-11 dígitos)
      /^\(\d{2}\)\s?\d{4,5}-?\d{4}$/,   // Ex: (85) 90987-6544
      /^\d{2}\s?\d{4,5}-?\d{4}$/,       // Ex: 85 90987-6544
      /^\+\d{2}\s?\d{2}\s?\d{4,5}-?\d{4}$/, // Ex: +55 85 90987-6544
      /^\d{2}\d{4,5}\d{4}$/             // Ex: 8590987654 (formato brasileiro específico)
    ];
    
    return patterns.some(pattern => pattern.test(valor.trim()));
  }

  // ✅ NOVO: Método para processar resultado da busca
  private processarResultadoBusca(res: any, valor: string, isNumeroSerie: boolean, isNumeroTelefone?: boolean): void {
    this.util.aguardar(false);
    if (!res || res.error) {
      console.error('[REQUISICOES] Resposta com erro na busca:', res);
      this.util.exibirFalhaComunicacao();
      return;
    }
    if (res.status != 200 && res.status != 204) {
      console.error('[REQUISICOES] Status inválido na busca:', res.status);
      this.util.exibirFalhaComunicacao();
      return;
    }
    
    // ✅ CORREÇÃO: Verificar se data tem a estrutura esperada
    if (res.data && (res.data.results || Array.isArray(res.data))) {
      let results = res.data.results || res.data;
      
      // ✅ DEBUG: Log das condições de detecção
      if (isNumeroSerie && !isNumeroTelefone && !res.data.filtradoPorEquipamento) {
        results = this.filtrarRequisicoesPorEquipamento(results, valor);
      }
      
      // ✅ DEBUG: Log para telefone
      if (isNumeroTelefone) {
      }
      
      this.dataSource = new MatTableDataSource(results);
      
      // ✅ NOVO: Calcular estatísticas em tempo real
      this.calcularEstatisticas(results);
      
      // ✅ CORREÇÃO: Validar paginador antes de atribuí-lo
      if (this.paginator) {
        // ✅ CORREÇÃO CRÍTICA: Configurar paginador após criar novo dataSource
        this.configurarPaginador();
      } else {
        console.warn('[REQUISICOES] Paginador não disponível para busca');
      }
      if (results.length === 0) {
        this.util.exibirMensagemToast(
          'Nenhuma requisição encontrada com os critérios informados', 
          3000
        );
      } else {
        let mensagem = `Encontradas ${results.length} requisição(ões)`;
        if (isNumeroSerie) {
          mensagem += ` com equipamento "${valor}"`;
        } else if (isNumeroTelefone) {
          mensagem += ` com telefone "${valor}"`;
        }
        this.util.exibirMensagemToast(mensagem, 2000);
      }
    } else {
      console.warn('[REQUISICOES] Estrutura de dados inválida:', res.data);
      this.dataSource = new MatTableDataSource([]);
      this.dadosCompletos = [];
      // ✅ NOVO: Calcular estatísticas com dados vazios
      this.calcularEstatisticas([]);
    }
  }

  // ✅ NOVO: Método para filtrar requisições por equipamento localmente
  private filtrarRequisicoesPorEquipamento(requisicoes: any[], numeroSerie: string): any[] {
    if (!requisicoes || !Array.isArray(requisicoes)) {
      return [];
    }

    const numeroSerieUpper = numeroSerie.trim().toUpperCase();
    return requisicoes.filter(requisicao => {
      // Verificar se a requisição tem equipamentos
      const equipamentos = this.getEquipamentosRequisicao(requisicao);
      
      if (!equipamentos || !Array.isArray(equipamentos)) {
        return false;
      }

      // Verificar se algum equipamento tem o número de série procurado
      const temEquipamento = equipamentos.some(eqp => {
        const eqpNumeroSerie = eqp.numeroserie || eqp.numeroSerie || eqp.NumeroSerie || '';
        const match = eqpNumeroSerie.toString().toUpperCase().includes(numeroSerieUpper);
        
        if (match) {
        }
        
        return match;
      });

      return temEquipamento;
    });
  }

  // ✅ NOVO: Método para aplicar filtro automático de requisição específica
  private aplicarFiltroAutomaticoRequisicao(requisicaoId: string): void {
    try {
      this.listar(null).then(() => {
        // ✅ Aguardar um pouco para os dados carregarem
        setTimeout(() => {
          if (this.dadosCompletos && this.dadosCompletos.length > 0) {
            // ✅ Filtrar apenas a requisição específica
            const requisicaoFiltrada = this.dadosCompletos.filter(item => {
              const id = this.getRequisicaoId(item);
              const match = id.toString() === requisicaoId;
              
              if (match) {
              }
              
              return match;
            });
            
            if (requisicaoFiltrada.length > 0) {
              // ✅ Aplicar filtro
              this.dataSource = new MatTableDataSource(requisicaoFiltrada);
              
              // ✅ Configurar paginador
              if (this.paginator) {
                this.paginator.pageIndex = 0;
                this.paginator.length = requisicaoFiltrada.length;
                this.configurarPaginador();
              }
              
              // ✅ Atualizar estatísticas
              this.calcularEstatisticas(requisicaoFiltrada);
              
              // ✅ Definir status do filtro
              this.statusFiltro = `Requisição ${requisicaoId}`;
              
              // ✅ Mostrar botão de "Ir para Entrega" destacado
              this.util.exibirMensagemToast(
                `Requisição ${requisicaoId} encontrada! Clique em "Entrega" para processar todos os recursos.`, 
                8000
              );
            } else {
              console.warn('[REQUISICOES] Requisição não encontrada para filtro automático:', requisicaoId);
              this.util.exibirMensagemToast(
                `Requisição ${requisicaoId} não encontrada. Mostrando todas as requisições.`, 
                5000
              );
            }
          } else {
            console.warn('[REQUISICOES] Dados não carregados para filtro automático');
            this.util.exibirMensagemToast(
              'Erro ao carregar dados para filtro automático', 
              3000
            );
          }
        }, 1000); // Aguardar 1 segundo para dados carregarem
      });
      
    } catch (error) {
      console.error('[REQUISICOES] Erro ao aplicar filtro automático:', error);
      this.util.exibirMensagemToast('Erro ao aplicar filtro automático', 3000);
    }
  }
}
