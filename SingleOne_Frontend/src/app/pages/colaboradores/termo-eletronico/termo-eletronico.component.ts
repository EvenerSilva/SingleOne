import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { FormControl, FormGroup, FormBuilder } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { CampanhaApiService } from 'src/app/api/campanhas/campanha-api.service';
import { UtilService } from 'src/app/util/util.service';
import { GeolocationService, LocationData } from 'src/app/services/geolocation.service';

interface CampanhaFiltros {
  empresas: number[];
  localidades: number[];
  centrosCusto: number[];
  tiposColaborador: string[];
  cargos: string[];
  setores: string[];
}

interface DashboardMetrics {
  totalColaboradores: number;
  totalAssinados: number;
  totalPendentes: number;
  percentualAssinado: number;
  ultimoEnvio: Date | null;
}

interface CampanhaResumo {
  id: number;
  nome: string;
  descricao: string;
  dataCriacao: Date;
  dataInicio: Date | null;
  dataFim: Date | null;
  status: string;
  statusDescricao: string;
  usuarioCriacaoNome: string;
  totalColaboradores: number;
  totalEnviados: number;
  totalAssinados: number;
  totalPendentes: number;
  percentualAdesao: number | null;
  dataUltimoEnvio: Date | null;
  dataConclusao: Date | null;
  filtrosJson: string;
}

interface ColaboradorPendente {
  colaboradorId: number;
  colaboradorNome: string;
  colaboradorCpf: string;
  colaboradorEmail: string;
  colaboradorCargo: string;
  empresaNome: string;
  localidadeNome: string;
  statusAssinatura: string;
  statusAssinaturaDescricao: string;
  dataInclusao: Date;
  dataEnvio: Date | null;
  dataUltimoEnvio: Date | null;
  totalEnvios: number;
  diasDesdeEnvio: number;
}

@Component({
  selector: 'app-termo-eletronico',
  templateUrl: './termo-eletronico.component.html',
  styleUrls: ['./termo-eletronico.component.scss']
})
export class TermoEletronicoComponent implements OnInit, AfterViewInit {

  private session: any = {};
  public colunas = ['selecao', 'colaborador', 'dtenvio', 'status', 'acao'];
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public filtro = "Em aberto";
  public locationData: LocationData | null = null;

  // 🆕 PROPRIEDADES PARA CAMPANHAS
  public mostrarModalCampanha = false;
  public campanhaAtiva: CampanhaResumo | null = null;
  public campanhas: CampanhaResumo[] = [];
  public campanhasFiltradas: CampanhaResumo[] = [];
  public colaboradoresSelecionados: Set<number> = new Set();
  public todosColaboradores: any[] = [];
  public mostrarSecaoCampanhas = false; // Toggle para mostrar/ocultar campanhas (inicia colapsado)
  
  // ✅ SIMPLIFICADO: Apenas 3 filtros essenciais
  public empresas: any[] = [];
  public localidades: any[] = [];
  public centrosCusto: any[] = [];
  public loadingFiltros = false;
  
  // Paginação local (como em Auditoria de Acessos)
  public dadosPagina: any[] = [];
  public totalLength = 0;
  public pageSize = 10;
  public currentPageIndex = 0;
  public loading = false;
  public loadingCampanhas = false;
  
  // Dashboard Metrics
  public metrics: DashboardMetrics = {
    totalColaboradores: 0,
    totalAssinados: 0,
    totalPendentes: 0,
    percentualAssinado: 0,
    ultimoEnvio: null
  };

  // Formulário de campanha
  public campanhaForm: FormGroup;
  public enviarAutomaticamente = false; // Controle do checkbox de envio automático
  public mostrarDataAgendamento = false; // Controle de exibição do campo de data

  constructor(
    private util: UtilService,
    private api: ColaboradorApiService,
    private campanhaApi: CampanhaApiService,
    private route: Router,
    private geolocationService: GeolocationService,
    private fb: FormBuilder
  ) {
    // ✅ SIMPLIFICADO: Apenas 3 filtros essenciais
    this.campanhaForm = this.fb.group({
      nomeCampanha: [''],
      descricao: [''],
      dataInicio: [null], // 📅 Data de início da campanha
      dataFim: [null], // 📅 Data de conclusão/prazo final da campanha
      empresas: [[]],
      localidades: [[]],
      centrosCusto: [[]],
      enviarAutomaticamente: [false],
      dataEnvioAgendado: [null]
    });
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    this.initializeLocation();
    this.listar();
    this.carregarCampanhas(); // Carregar campanhas ao inicializar
  }

  ngAfterViewInit(): void {
    this.inicializarDataSource();
  }

  private inicializarDataSource(): void {
    if (!this.dataSource) {
      this.dataSource = new MatTableDataSource<any>([]);
    }
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }
  }

  initializeLocation(): void {
    this.geolocationService.getCompleteLocationData().subscribe(
      (location: LocationData) => {
        this.locationData = location;
      },
      (error) => {
        console.error('Erro ao obter localização:', error);
      }
    );
  }

  listar() {
    this.loading = true;
    this.api.colaboradoresComTermoPorAssinar("null", this.cliente, this.filtro, this.session.token).then(res => {
      this.loading = false;
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.todosColaboradores = res.data || [];
        
        // Configurar paginação local (como em Auditoria de Acessos)
        this.totalLength = this.todosColaboradores.length;
        this.currentPageIndex = 0;
        this.atualizarPagina();
        
        this.atualizarMetricas();
      }
    }).catch(() => {
      this.loading = false;
      this.util.exibirFalhaComunicacao();
    });
  }

  buscar(valor) {
    if (valor != '') {
      this.loading = true;
      this.api.colaboradoresComTermoPorAssinar(valor, this.cliente, this.filtro, this.session.token).then(res => {
        this.loading = false;
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.todosColaboradores = res.data || [];
          
          // Configurar paginação local
          this.totalLength = this.todosColaboradores.length;
          this.currentPageIndex = 0;
          this.atualizarPagina();
          
          this.atualizarMetricas();
        }
      }).catch(() => {
        this.loading = false;
        this.util.exibirFalhaComunicacao();
      });
    }
    else {
      this.listar();
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.filtro = 'Em aberto';
    this.colaboradoresSelecionados.clear();
    this.listar();
  }

  // Método: Mudança de página (paginação local)
  onPageChange(event: PageEvent) {
    this.pageSize = event.pageSize;
    this.currentPageIndex = event.pageIndex;
    this.atualizarPagina();
  }

  // Método: Atualizar dados da página corrente (paginação local)
  private atualizarPagina() {
    const inicio = this.currentPageIndex * this.pageSize;
    const fim = inicio + this.pageSize;
    this.dadosPagina = this.todosColaboradores.slice(inicio, fim);
  }

  // 📊 ATUALIZAR MÉTRICAS DO DASHBOARD
  atualizarMetricas() {
    // Buscar todos (assinados + pendentes) para cálculo correto
    Promise.all([
      this.api.colaboradoresComTermoPorAssinar("null", this.cliente, "Em aberto", this.session.token),
      this.api.colaboradoresComTermoPorAssinar("null", this.cliente, "Assinado", this.session.token)
    ]).then(([pendentes, assinados]) => {
      const totalPendentes = (pendentes.status === 200 || pendentes.status === 204) ? (pendentes.data || []).length : 0;
      const totalAssinados = (assinados.status === 200 || assinados.status === 204) ? (assinados.data || []).length : 0;
      const total = totalPendentes + totalAssinados;

      this.metrics = {
        totalColaboradores: total,
        totalAssinados: totalAssinados,
        totalPendentes: totalPendentes,
        percentualAssinado: total > 0 ? Math.round((totalAssinados / total) * 100) : 0,
        ultimoEnvio: this.obterUltimoEnvio()
      };
    });
  }

  obterUltimoEnvio(): Date | null {
    if (!this.todosColaboradores || this.todosColaboradores.length === 0) return null;
    
    const datas = this.todosColaboradores
      .map(c => c.dtenviotermo)
      .filter(d => d != null)
      .map(d => new Date(d))
      .sort((a, b) => b.getTime() - a.getTime());
    
    return datas.length > 0 ? datas[0] : null;
  }

  // 🆕 ALTERNAR MODO DE VISUALIZAÇÃO (pelo select)
  alternarModoSelect() {
    this.listar();
    this.colaboradoresSelecionados.clear();
  }

  // 🎯 SELEÇÃO DE COLABORADORES
  toggleSelecao(colaboradorId: number) {
    if (this.colaboradoresSelecionados.has(colaboradorId)) {
      this.colaboradoresSelecionados.delete(colaboradorId);
    } else {
      this.colaboradoresSelecionados.add(colaboradorId);
    }
  }

  estaSelecionado(colaboradorId: number): boolean {
    return this.colaboradoresSelecionados.has(colaboradorId);
  }

  selecionarTodos() {
    if (this.todosColaboradores && this.todosColaboradores.length > 0) {
      this.todosColaboradores.forEach(c => {
        if (c.colaboradorfinalid) {
          this.colaboradoresSelecionados.add(c.colaboradorfinalid);
        }
      });
    }
  }

  deselecionarTodos() {
    this.colaboradoresSelecionados.clear();
  }

  toggleTodosSelecionados(event: any) {
    if (event.target.checked) {
      this.selecionarTodos();
    } else {
      this.deselecionarTodos();
    }
  }

  todosSelecionados(): boolean {
    if (!this.todosColaboradores || this.todosColaboradores.length === 0) return false;
    return this.todosColaboradores.every(c => this.colaboradoresSelecionados.has(c.colaboradorfinalid));
  }

  algunsSelecionados(): boolean {
    if (!this.todosColaboradores || this.todosColaboradores.length === 0) return false;
    const selecionados = this.todosColaboradores.filter(c => this.colaboradoresSelecionados.has(c.colaboradorfinalid)).length;
    return selecionados > 0 && selecionados < this.todosColaboradores.length;
  }

  // 📧 ENVIAR TERMO INDIVIDUAL
  enviar(colab) {
    if (!this.locationData) {
      this.util.exibirMensagemToast('Aguarde enquanto obtemos sua localização...', 3000);
      return;
    }

    const locationInfo = this.geolocationService.formatLocationForDisplay(this.locationData);
    const coordinates = this.geolocationService.formatCoordinates(this.locationData);

    const confirmMessage = `Enviar termo para <strong>${colab.colaboradorfinal}</strong>?<br><br>Esta ação será registrada com:<br>${locationInfo}<br>${coordinates}`;

    this.util.exibirMensagemPopUp(confirmMessage, true).then((confirmado: boolean) => {
      if (confirmado) {
        this.util.aguardar(true);

        this.logLocationData(colab, this.locationData);

        this.api.termoPorEmail(this.cliente, colab.colaboradorfinalid, this.session.usuario.id, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res.status != 200) {
            this.util.exibirFalhaComunicacao();
          }
          else {
            this.util.exibirMensagemToast(res.data.Mensagem + '\n\nLocalização registrada: ' + locationInfo, 5000);
            this.listar();
          }
        })
      }
    });
  }

  // 📧 ENVIAR EM MASSA
  enviarEmMassa() {
    if (this.colaboradoresSelecionados.size === 0) {
      this.util.exibirMensagemToast('Selecione pelo menos um colaborador', 3000);
      return;
    }

    if (!this.locationData) {
      this.util.exibirMensagemToast('Aguarde enquanto obtemos sua localização...', 3000);
      return;
    }

    const locationInfo = this.geolocationService.formatLocationForDisplay(this.locationData);
    const qtdSelecionados = this.colaboradoresSelecionados.size;

    const confirmMessage = `Enviar termo para <strong>${qtdSelecionados} colaborador(es) selecionado(s)</strong>?<br><br>Esta ação será registrada com:<br>${locationInfo}`;

    this.util.exibirMensagemPopUp(confirmMessage, true).then((confirmado: boolean) => {
      if (confirmado) {
        this.util.aguardar(true);

        let sucessos = 0;
        let falhas = 0;
        const promises: Promise<any>[] = [];

        this.colaboradoresSelecionados.forEach(colaboradorId => {
          const colab = this.todosColaboradores.find(c => c.colaboradorfinalid === colaboradorId);
          if (colab) {
            this.logLocationData(colab, this.locationData!);
            const promise = this.api.termoPorEmail(this.cliente, colaboradorId, this.session.usuario.id, this.session.token)
              .then(res => {
                if (res.status === 200) {
                  sucessos++;
                } else {
                  falhas++;
                }
              })
              .catch(() => {
                falhas++;
              });
            promises.push(promise);
          }
        });

        Promise.all(promises).then(() => {
          this.util.aguardar(false);
          this.util.exibirMensagemToast(
            `Envio concluído!\n✅ Sucessos: ${sucessos}\n❌ Falhas: ${falhas}`,
            5000
          );
          this.colaboradoresSelecionados.clear();
          this.listar();
        });
      }
    });
  }

  private logLocationData(colab: any, location: LocationData): void {
    const logData = {
      colaboradorId: colab.colaboradorfinalid,
      colaboradorNome: colab.colaboradorfinal,
      usuarioLogadoId: this.session.usuario.id,
      ip: location.ip,
      country: location.country,
      city: location.city,
      region: location.region,
      latitude: location.latitude,
      longitude: location.longitude,
      accuracy: location.accuracy,
      timestamp: location.timestamp,
      acao: 'ENVIO_TERMO_EMAIL'
    };

    this.api.registrarLocalizacaoAssinatura(logData, this.session.token).then(res => {
      if (res.status === 200) {
      }
    }).catch(err => {
      console.error('Erro ao registrar dados de localização:', err);
    });
  }

  // 🎯 MODAL DE NOVA CAMPANHA
  async abrirModalCampanha() {
    this.mostrarModalCampanha = true;
    this.enviarAutomaticamente = false;
    this.mostrarDataAgendamento = false;
    
    // ✅ SIMPLIFICADO: Apenas 3 filtros essenciais
    this.campanhaForm.reset({
      nomeCampanha: '',
      descricao: '',
      dataInicio: null,
      dataFim: null,
      empresas: [],
      localidades: [],
      centrosCusto: [],
      enviarAutomaticamente: false,
      dataEnvioAgendado: null
    });
    
    // Carregar opções dos filtros
    await this.carregarOpcoesFiltros();
  }

  // 📧 CONTROLE DE ENVIO AUTOMÁTICO
  toggleEnvioAutomatico() {
    this.enviarAutomaticamente = !this.enviarAutomaticamente;
    this.mostrarDataAgendamento = this.enviarAutomaticamente;
    this.campanhaForm.patchValue({ enviarAutomaticamente: this.enviarAutomaticamente });
    
    if (!this.enviarAutomaticamente) {
      this.campanhaForm.patchValue({ dataEnvioAgendado: null });
    }
  }

  /**
   * Retorna data/hora mínima para o datepicker (agora + 1 minuto)
   */
  getMinDateTime(): string {
    const agora = new Date();
    agora.setMinutes(agora.getMinutes() + 1); // Adiciona 1 minuto
    return agora.toISOString().slice(0, 16); // Formato: YYYY-MM-DDTHH:mm
  }

  fecharModalCampanha() {
    this.mostrarModalCampanha = false;
    this.campanhaForm.reset();
  }

  /**
   * ✅ CORRIGIDO: Carregar opções APENAS dos colaboradores que NÃO ASSINARAM (na lista atual)
   */
  async carregarOpcoesFiltros(): Promise<void> {
    try {
      this.loadingFiltros = true;
      
      const colaboradores = this.todosColaboradores;
      
      if (!colaboradores || colaboradores.length === 0) {
        this.empresas = [];
        this.localidades = [];
        this.centrosCusto = [];
        this.loadingFiltros = false;
        return;
      }
      // Vamos buscar 1 vez só para pegar empresas, localidades e centros de custo
      const idsColaboradores = colaboradores.map((c: any) => c.colaboradorfinalid).filter((id: any) => id);
      const todosAtivos = await this.api.listarColaboradoresAtivos('null', this.cliente, this.session.token);
      
      // Filtrar apenas os que estão na lista sem assinatura
      const colaboradoresCompletos = todosAtivos?.data?.filter((c: any) => 
        idsColaboradores.includes(c.id)
      ) || [];
      if (colaboradoresCompletos.length > 0) {
        // Extrair empresas únicas
        const empresasMap = new Map();
        colaboradoresCompletos.forEach((colab: any) => {
          if (colab.empresa && !empresasMap.has(colab.empresa)) {
            empresasMap.set(colab.empresa, {
              id: colab.empresa,
              nome: colab.empresaNavigation?.nome || `Empresa ${colab.empresa}`
            });
          }
        });
        this.empresas = Array.from(empresasMap.values());
        
        // Extrair localidades únicas
        const localidadesMap = new Map();
        colaboradoresCompletos.forEach((colab: any) => {
          if (colab.localidade && !localidadesMap.has(colab.localidade)) {
            localidadesMap.set(colab.localidade, {
              id: colab.localidade,
              nome: colab.localidadeNavigation?.descricao || `Localidade ${colab.localidade}`
            });
          }
        });
        this.localidades = Array.from(localidadesMap.values());
        
        // Extrair centros de custo únicos
        const centrosCustoMap = new Map();
        colaboradoresCompletos.forEach((colab: any) => {
          if (colab.centrocusto && !centrosCustoMap.has(colab.centrocusto)) {
            centrosCustoMap.set(colab.centrocusto, {
              id: colab.centrocusto,
              codigo: colab.centrocustoNavigation?.codigo || '',
              nome: colab.centrocustoNavigation?.nome || `Centro de Custo ${colab.centrocusto}`
            });
          }
        });
        this.centrosCusto = Array.from(centrosCustoMap.values());
      } else {
        console.warn('[FILTROS] ❌ Não foi possível carregar dados completos dos colaboradores');
        this.empresas = [];
        this.localidades = [];
        this.centrosCusto = [];
      }
      
      this.loadingFiltros = false;
    } catch (error) {
      this.loadingFiltros = false;
      console.error('[FILTROS] ❌ ERRO ao carregar opções:', error);
      this.util.exibirMensagemToast('Erro ao carregar opções de filtros', 3000);
    }
  }

  // ✅ SIMPLIFICADO: Métodos obsoletos removidos (carregarEmpresas, carregarLocalidades, etc)

  async criarCampanha() {
    const valores = this.campanhaForm.value;
    
    // Validações
    if (!valores.nomeCampanha || valores.nomeCampanha.trim() === '') {
      this.util.exibirMensagemToast('Por favor, informe o nome da campanha', 3000);
      return;
    }

    if (this.colaboradoresSelecionados.size === 0) {
      this.util.exibirMensagemToast('Por favor, selecione pelo menos um colaborador', 3000);
      return;
    }

    // Validar data de agendamento se estiver marcado
    if (valores.enviarAutomaticamente && valores.dataEnvioAgendado) {
      const dataAgendada = new Date(valores.dataEnvioAgendado);
      const agora = new Date();
      
      if (dataAgendada <= agora) {
        this.util.exibirMensagemToast('A data de agendamento deve ser futura', 3000);
        return;
      }
    }

    this.util.aguardar(true);

    try {
      // Preparar dados para envio
      const colaboradoresIds = Array.from(this.colaboradoresSelecionados);
      
      // ✅ SIMPLIFICADO: Apenas 3 filtros essenciais
      const filtrosJson = JSON.stringify({
        empresas: valores.empresas || [],
        localidades: valores.localidades || [],
        centrosCusto: valores.centrosCusto || []
      });

      const dadosCampanha = {
        clienteId: this.cliente,
        usuarioCriacaoId: this.session.usuario.id,
        nome: valores.nomeCampanha.trim(),
        descricao: valores.descricao || '',
        dataInicio: valores.dataInicio || null, // 📅 Data de início da campanha
        dataFim: valores.dataFim || null, // 📅 Data de conclusão/prazo final
        filtrosJson: filtrosJson,
        colaboradoresIds: colaboradoresIds,
        enviarAutomaticamente: valores.enviarAutomaticamente || false,
        dataEnvioAgendado: valores.dataEnvioAgendado || null // 📧 Data para agendar envio de emails
      };
      const resultado = await this.campanhaApi.criarCampanha(dadosCampanha, this.session.token);
      this.util.aguardar(false);

      // ✅ Melhor tratamento de resposta
      if (resultado && resultado.status === 200) {
        // Backend pode retornar o objeto campanha diretamente ou com { sucesso: true, campanha: {...} }
        const campanhaData = resultado.data?.campanha || resultado.data;
        const campanhaId = campanhaData?.id || campanhaData;
        
        let mensagemSucesso = `Campanha "${valores.nomeCampanha}" criada com sucesso! ${colaboradoresIds.length} colaborador(es) adicionado(s).`;
        
        // 📧 ENVIO AUTOMÁTICO OU AGENDADO
        if (valores.enviarAutomaticamente) {
          if (valores.dataEnvioAgendado) {
            // Envio agendado
            const dataFormatada = new Date(valores.dataEnvioAgendado).toLocaleString('pt-BR');
            mensagemSucesso += `\n📅 Envio agendado para: ${dataFormatada}`;
            this.util.exibirMensagemToast(mensagemSucesso, 7000);
          } else {
            // Envio imediato
            mensagemSucesso += '\n📧 Enviando emails...';
            this.util.exibirMensagemToast(mensagemSucesso, 5000);
            
            // Disparar envio em massa usando o método existente
            await this.enviarTermosEmMassaDaCampanha(campanhaId, colaboradoresIds);
          }
        } else {
          this.util.exibirMensagemToast(mensagemSucesso, 5000);
        }
        this.colaboradoresSelecionados.clear();
        
        // Fechar modal
        this.fecharModalCampanha();
        
        // Recarregar campanhas
        await this.carregarCampanhas();
      } else {
        // Tratamento de erro robusto
        const mensagemErro = resultado?.data?.mensagem 
          || resultado?.data?.message 
          || resultado?.message 
          || 'Erro ao criar campanha';
        
        console.error('[CAMPANHA] ❌ Erro:', mensagemErro);
        this.util.exibirMensagemToast(mensagemErro, 5000);
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('[CAMPANHA] Erro ao criar campanha:', error);
      this.util.exibirMensagemToast('Erro ao criar campanha. Verifique os dados e tente novamente.', 5000);
    }
  }

  /**
   * Enviar termos em massa após criar campanha
   */
  private async enviarTermosEmMassaDaCampanha(campanhaId: number, colaboradoresIds: number[]): Promise<void> {
    try {
      const dados = {
        usuarioEnvioId: this.session.usuario.id,
        colaboradoresIds: colaboradoresIds,
        ipEnvio: this.locationData?.ip || '',
        localizacaoEnvio: this.locationData ? JSON.stringify(this.locationData) : ''
      };

      const resultado = await this.campanhaApi.enviarTermosEmMassa(
        campanhaId,
        dados,
        this.session.token
      );
      if (resultado.status === 200 && resultado.data.sucesso) {
        this.util.exibirMensagemToast('Termo(s) enviado(s) com sucesso!', 5000);
      } else {
        console.warn('[CAMPANHA] ⚠️ Alguns envios falharam:', resultado.data);
      }
    } catch (error) {
      console.error('[CAMPANHA] ❌ Erro ao enviar emails:', error);
    }
  }

  // ==================== MÉTODOS DE CAMPANHAS ====================

  /**
   * Carregar campanhas do cliente
   */
  async carregarCampanhas(): Promise<void> {
    this.loadingCampanhas = true;
    
    try {
      const resultado = await this.campanhaApi.obterResumosCampanhas(this.cliente, this.session.token);
      
      if (resultado.status === 200 && resultado.data) {
        this.campanhas = resultado.data;
        this.campanhasFiltradas = resultado.data;
      } else {
        console.warn('[CAMPANHAS] Nenhuma campanha encontrada');
        this.campanhas = [];
        this.campanhasFiltradas = [];
      }
    } catch (error) {
      console.error('[CAMPANHAS] Erro ao carregar campanhas:', error);
      this.util.exibirMensagemToast('Erro ao carregar campanhas', 3000);
      this.campanhas = [];
      this.campanhasFiltradas = [];
    } finally {
      this.loadingCampanhas = false;
    }
  }

  /**
   * Processar manualmente campanhas vencidas (concluir automaticamente)
   */
  async processarCampanhasVencidas(): Promise<void> {
    try {
      this.util.exibirMensagemToast('Verificando campanhas vencidas...', 2000);
      
      const resultado = await this.campanhaApi.processarCampanhasVencidas(this.cliente, this.session.token);
      if (resultado.status === 200 && resultado.data) {
        // Suporte para camelCase e PascalCase
        const data = resultado.data;
        const totalEncontradas = data.totalEncontradas || data.TotalEncontradas || 0;
        const campanhas = data.campanhas || data.Campanhas || [];
        if (totalEncontradas === 0) {
          this.util.exibirMensagemToast('Nenhuma campanha vencida encontrada', 3000);
        } else {
          const sucesso = campanhas.filter((c: any) => c.status === 'Concluída').length;
          const erro = campanhas.filter((c: any) => c.status === 'Erro').length;
          
          let mensagem = `${sucesso} campanha(s) concluída(s) automaticamente`;
          if (erro > 0) {
            mensagem += ` (${erro} com erro)`;
          }
          
          this.util.exibirMensagemToast(mensagem, 5000);
          
          // Recarregar lista
          await this.carregarCampanhas();
        }
      } else {
        this.util.exibirMensagemToast('Erro ao processar campanhas vencidas', 3000);
      }
    } catch (error) {
      console.error('[CAMPANHAS-VENCIDAS] Erro:', error);
      this.util.exibirMensagemToast('Erro ao processar campanhas vencidas', 3000);
    }
  }

  /**
   * Filtrar campanhas por status
   */
  filtrarCampanhasPorStatus(status: string): void {
    if (status === 'TODAS') {
      this.campanhasFiltradas = this.campanhas;
    } else {
      this.campanhasFiltradas = this.campanhas.filter(c => c.status === status);
    }
  }

  /**
   * Selecionar campanha para visualização
   */
  async selecionarCampanha(campanha: CampanhaResumo): Promise<void> {
    this.campanhaAtiva = campanha;
    await this.carregarColaboradoresCampanha(campanha.id);
  }

  /**
   * Carregar colaboradores de uma campanha específica
   */
  async carregarColaboradoresCampanha(campanhaId: number): Promise<void> {
    this.loading = true;
    
    try {
      const resultado = await this.campanhaApi.obterColaboradoresDaCampanha(
        campanhaId, 
        null, // Todos os status
        this.session.token
      );
      if (resultado.status === 200 && resultado.data) {
        this.todosColaboradores = resultado.data.map((cc: any) => ({
          colaboradorfinalid: cc.colaboradorId,
          colaboradorfinal: cc.colaborador?.nome || 'Nome não disponível',
          dtenviotermo: cc.dataEnvio,
          situacao: this.obterDescricaoStatusAssinatura(cc.statusAssinatura),
          statusAssinatura: cc.statusAssinatura,
          dataUltimoEnvio: cc.dataUltimoEnvio,
          totalEnvios: cc.totalEnvios,
          // Dados extras para exibição
          empresaNome: cc.colaborador?.empresaNavigation?.nome,
          localidadeNome: cc.colaborador?.localidadeNavigation?.descricao,
          email: cc.colaborador?.email,
          cpf: cc.colaborador?.cpf,
          cargo: cc.colaborador?.cargo
        }));
        
        this.totalLength = this.todosColaboradores.length;
        this.currentPageIndex = 0;
        this.atualizarPagina();
      }
    } catch (error) {
      console.error('[CAMPANHA] Erro ao carregar colaboradores:', error);
      this.util.exibirMensagemToast('Erro ao carregar colaboradores da campanha', 3000);
    } finally {
      this.loading = false;
    }
  }

  /**
   * Converter status de assinatura para descrição
   */
  private obterDescricaoStatusAssinatura(status: string): string {
    const statusMap: { [key: string]: string } = {
      'P': 'Pendente',
      'E': 'Enviado',
      'A': 'Assinado',
      'R': 'Recusado'
    };
    return statusMap[status] || 'Desconhecido';
  }

  /**
   * Limpar seleção de campanha (voltar para visualização geral)
   */
  limparSelecaoCampanha(): void {
    this.campanhaAtiva = null;
    this.listar(); // Voltar para lista geral
  }

  /**
   * Obter classe CSS baseada no status da campanha
   */
  getStatusCampanhaClass(status: string): string {
    const classes: { [key: string]: string } = {
      'A': 'status-ativa',
      'I': 'status-inativa',
      'C': 'status-concluida',
      'G': 'status-agendada'
    };
    return classes[status] || 'status-default';
  }

  /**
   * Obter ícone baseado no status da campanha
   */
  getStatusCampanhaIcon(status: string): string {
    const icons: { [key: string]: string } = {
      'A': 'cil-media-play',
      'I': 'cil-media-pause',
      'C': 'cil-check-circle',
      'G': 'cil-clock'
    };
    return icons[status] || 'cil-info';
  }

  /**
   * Enviar termo para colaborador específico de uma campanha
   */
  async enviarTermoCampanha(colaborador: ColaboradorPendente): Promise<void> {
    if (!this.campanhaAtiva) {
      this.util.exibirMensagemToast('Nenhuma campanha selecionada', 3000);
      return;
    }

    this.util.aguardar(true);

    try {
      const dados = {
        usuarioEnvioId: this.session.usuario.id,
        ipEnvio: this.locationData?.ip || '',
        localizacaoEnvio: this.locationData ? JSON.stringify(this.locationData) : ''
      };

      const resultado = await this.campanhaApi.enviarTermo(
        this.campanhaAtiva.id,
        colaborador.colaboradorId,
        dados,
        this.session.token
      );
      this.util.aguardar(false);

      if (resultado.status === 200 && resultado.data.sucesso) {
        this.util.exibirMensagemToast('Termo enviado com sucesso!', 3000);
        
        // Recarregar colaboradores da campanha
        await this.carregarColaboradoresCampanha(this.campanhaAtiva.id);
        
        // Atualizar estatísticas da campanha
        await this.atualizarEstatisticasCampanha(this.campanhaAtiva.id);
      } else {
        this.util.exibirMensagemToast(
          resultado.data.mensagem || 'Erro ao enviar termo',
          3000
        );
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('[CAMPANHA] Erro ao enviar termo:', error);
      this.util.exibirMensagemToast('Erro ao enviar termo', 3000);
    }
  }

  /**
   * Enviar termos em massa para colaboradores selecionados
   */
  async enviarTermosEmMassa(): Promise<void> {
    if (!this.campanhaAtiva) {
      this.util.exibirMensagemToast('Nenhuma campanha selecionada', 3000);
      return;
    }

    if (this.colaboradoresSelecionados.size === 0) {
      this.util.exibirMensagemToast('Selecione pelo menos um colaborador', 3000);
      return;
    }

    this.util.aguardar(true);

    try {
      const dados = {
        usuarioEnvioId: this.session.usuario.id,
        colaboradoresIds: Array.from(this.colaboradoresSelecionados),
        ipEnvio: this.locationData?.ip || '',
        localizacaoEnvio: this.locationData ? JSON.stringify(this.locationData) : ''
      };

      const resultado = await this.campanhaApi.enviarTermosEmMassa(
        this.campanhaAtiva.id,
        dados,
        this.session.token
      );
      this.util.aguardar(false);

      if (resultado.status === 200 && resultado.data.sucesso) {
        this.util.exibirMensagemToast(
          `${dados.colaboradoresIds.length} termo(s) enviado(s) com sucesso!`,
          5000
        );
        // Limpar seleção
        this.colaboradoresSelecionados.clear();
        
        // Recarregar colaboradores da campanha
        await this.carregarColaboradoresCampanha(this.campanhaAtiva.id);
        
        // Atualizar estatísticas da campanha
        await this.atualizarEstatisticasCampanha(this.campanhaAtiva.id);
      } else {
        this.util.exibirMensagemToast(
          resultado.data.mensagem || 'Erro ao enviar termos',
          5000
        );
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('[CAMPANHA] Erro ao enviar termos em massa:', error);
      this.util.exibirMensagemToast('Erro ao enviar termos em massa', 5000);
    }
  }

  /**
   * Atualizar estatísticas de uma campanha
   */
  async atualizarEstatisticasCampanha(campanhaId: number): Promise<void> {
    try {
      const resultado = await this.campanhaApi.atualizarEstatisticas(
        campanhaId,
        this.session.token
      );
      if (resultado.status === 200 && resultado.data.sucesso) {
        // Recarregar lista de campanhas para atualizar estatísticas
        await this.carregarCampanhas();
        
        // Se houver campanha ativa, atualizar referência
        if (this.campanhaAtiva) {
          const campanhaAtualizada = this.campanhas.find(c => c.id === this.campanhaAtiva!.id);
          if (campanhaAtualizada) {
            this.campanhaAtiva = campanhaAtualizada;
          }
        }
      }
    } catch (error) {
      console.error('[CAMPANHA] Erro ao atualizar estatísticas:', error);
    }
  }

  /**
   * Inativar campanha
   */
  async inativarCampanha(campanhaId: number): Promise<void> {
    const confirmado = await this.util.exibirMensagemPopUp('Deseja realmente inativar esta campanha?', true);
    
    if (!confirmado) {
      return;
    }

    this.util.aguardar(true);

    try {
      const resultado = await this.campanhaApi.inativarCampanha(campanhaId, this.session.token);

      this.util.aguardar(false);

      if (resultado.status === 200 && resultado.data.sucesso) {
        this.util.exibirMensagemToast('Campanha inativada com sucesso!', 3000);
        await this.carregarCampanhas();
        
        // Se a campanha inativada era a ativa, limpar seleção
        if (this.campanhaAtiva && this.campanhaAtiva.id === campanhaId) {
          this.limparSelecaoCampanha();
        }
      } else {
        this.util.exibirMensagemToast(
          resultado.data.mensagem || 'Erro ao inativar campanha',
          3000
        );
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('[CAMPANHA] Erro ao inativar campanha:', error);
      this.util.exibirMensagemToast('Erro ao inativar campanha', 3000);
    }
  }

  /**
   * Concluir campanha
   */
  async concluirCampanha(campanhaId: number): Promise<void> {
    const confirmado = await this.util.exibirMensagemPopUp('Deseja realmente concluir esta campanha?', true);
    
    if (!confirmado) {
      return;
    }

    this.util.aguardar(true);

    try {
      const resultado = await this.campanhaApi.concluirCampanha(campanhaId, this.session.token);

      this.util.aguardar(false);

      if (resultado.status === 200 && resultado.data.sucesso) {
        this.util.exibirMensagemToast('Campanha concluída com sucesso!', 3000);
        await this.carregarCampanhas();
        
        // Se a campanha concluída era a ativa, limpar seleção
        if (this.campanhaAtiva && this.campanhaAtiva.id === campanhaId) {
          this.limparSelecaoCampanha();
        }
      } else {
        this.util.exibirMensagemToast(
          resultado.data.mensagem || 'Erro ao concluir campanha',
          3000
        );
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('[CAMPANHA] Erro ao concluir campanha:', error);
      this.util.exibirMensagemToast('Erro ao concluir campanha', 3000);
    }
  }

  /**
   * Toggle seção de campanhas
   */
  toggleSecaoCampanhas(): void {
    this.mostrarSecaoCampanhas = !this.mostrarSecaoCampanhas;
  }

  // 🎨 HELPER PARA CLASSES CSS
  getStatusClass(situacao: string): string {
    return situacao?.toLowerCase().includes('assinado') ? 'status-assinado' : 'status-pendente';
  }

  getStatusIcon(situacao: string): string {
    return situacao?.toLowerCase().includes('assinado') ? 'cil-check-circle' : 'cil-clock';
  }

  // 📅 MÉTODOS PARA PRAZO DE CONCLUSÃO
  
  /**
   * Calcular dias restantes até a data final
   */
  getDiasRestantes(dataFim: Date | string | null): number {
    if (!dataFim) return 0;
    
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    
    const dataFinal = new Date(dataFim);
    dataFinal.setHours(0, 0, 0, 0);
    
    const diffTime = dataFinal.getTime() - hoje.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return diffDays;
  }

  /**
   * Classe CSS condicional baseada nos dias restantes
   */
  getPrazoClass(dataFim: Date | string | null): string {
    const dias = this.getDiasRestantes(dataFim);
    
    if (dias < 0) return 'prazo-vencido'; // Vencido
    if (dias === 0) return 'prazo-hoje'; // Vence hoje
    if (dias <= 3) return 'prazo-urgente'; // 3 dias ou menos
    if (dias <= 7) return 'prazo-atencao'; // 7 dias ou menos
    return 'prazo-normal'; // Mais de 7 dias
  }

  /**
   * Ícone condicional baseado nos dias restantes
   */
  getPrazoIcon(dataFim: Date | string | null): string {
    const dias = this.getDiasRestantes(dataFim);
    
    if (dias < 0) return 'cil-x-circle'; // Vencido
    if (dias === 0) return 'cil-warning'; // Vence hoje
    if (dias <= 3) return 'cil-alarm'; // Urgente
    if (dias <= 7) return 'cil-clock'; // Atenção
    return 'cil-calendar-check'; // Normal
  }

  /**
   * Expor Math.abs para o template
   */
  Math = Math;
}
