import { Component, OnInit, ViewChild, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { environment } from 'src/environments/environment';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { MatTabGroup } from '@angular/material/tabs';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import { debounceTime, tap, finalize } from 'rxjs/operators';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { RequisicaoApiService } from 'src/app/api/requisicoes/requisicao-api.service';
import { UtilService } from 'src/app/util/util.service';
import { AdicionarObservacaoComponent } from '../adicionar-observacao/adicionar-observacao.component';
import { AgendamentoComponent } from '../agendamento/agendamento.component';
import { ModalCompartilharItemComponent } from './modal-compartilhar-item/modal-compartilhar-item.component';

@Component({
  selector: 'app-entregas-devolucoes',
  templateUrl: './entregas-devolucoes.component.html',
  styleUrls: ['./entregas-devolucoes.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EntregasDevolucoesComponent implements OnInit {

  private session:any = {};
  public colunasEntregas = ['id', 'dtsolicitacao', 'requisitante', 'recursos', 'acao'];
  public colunasDevolucoes = ['colaborador'];
  @ViewChild(MatPaginator, { static: true }) paginatorEntrega: MatPaginator;
  @ViewChild(MatPaginator, { static: true }) paginatorDevolucao: MatPaginator;
  @ViewChild(MatPaginator, { static: true }) paginatorBYOD: MatPaginator;
  @ViewChild('abas') tabGroup: MatTabGroup;
  public dataSourceEntregas: MatTableDataSource<any>;
  public dataSourceDevolucoes: MatTableDataSource<any>;
  public dataSourceBYOD: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public requisicoes:any = [];
  public parametro = null;
  public expandedColaboradores: Set<number> = new Set(); // ✅ NOVO: Controla quais colaboradores estão expandidos
  public carregandoRequisicoes: Set<number> = new Set(); // ✅ NOVO: Controla quais colaboradores estão carregando requisições
  public raimundoNonato: any = null; // ✅ DEBUG: Referência para Raimundo Nonato
  
  // ✅ NOVO: Cache para dados carregados
  private cacheRequisicoes: Map<number, any[]> = new Map();
  private cacheCarregamento: Map<number, boolean> = new Map();
  
  // ✅ NOVO: Controle de carregamento geral
  public carregandoDados: boolean = false;
  // 🔎 DEBUG: controlar logs por requisição
  private debugLoggedReqIds: Set<number> = new Set();

  // ===================== COMPARTILHAMENTO POR ITEM (NÃO-BYOD) =====================
  public compartilhamentoAberto: Set<number> = new Set();
  public mapaCompartilhados: { [itemId: number]: any[] } = {};
  public mapaOpcoesCoResp: { [itemId: number]: any[] } = {};

  public isLinhaItem(eqp: any): boolean {
    const nome = (eqp?.equipamento || eqp?.nome || '').toString().toLowerCase();
    return nome.includes('linha');
  }

  public getNumeroLinha(eqp: any): string {
    // O número vem dentro do texto "Linha telefônica 11989363372"
    // Precisa extrair apenas o número
    if (this.isLinhaItem(eqp)) {
      const equipamentoTexto = eqp.equipamento || '';
      // Extrai números do final da string (após "Linha telefônica ")
      const match = equipamentoTexto.match(/\d+$/);
      if (match) {
        return match[0];
      }
    }
    return eqp.patrimonio || eqp.numero || eqp.numeroSerie || 'N/A';
  }

  public getOperadoraPlanoLinha(eqp: any): string {
    // O patrimonio contém "Claro - LINHA MÓVEL VOZ/DADOS"
    if (this.isLinhaItem(eqp)) {
      return eqp.patrimonio || 'N/A';
    }
    return '';
  }

  public getOperadoraLinha(eqp: any): string {
    // Extrai a operadora (antes do hífen)
    if (this.isLinhaItem(eqp)) {
      const patrimonio = eqp.patrimonio || '';
      const partes = patrimonio.split('-');
      if (partes.length > 0) {
        return partes[0].trim();
      }
    }
    return 'N/A';
  }

  public getPlanoLinha(eqp: any): string {
    // Extrai o plano (depois do hífen)
    if (this.isLinhaItem(eqp)) {
      const patrimonio = eqp.patrimonio || '';
      const partes = patrimonio.split('-');
      if (partes.length > 1) {
        return partes.slice(1).join('-').trim();
      }
    }
    return 'N/A';
  }

  public getReqItemId(eqp: any): number {
    return (
      eqp?.requisicaoItemId ||
      eqp?.requisicoesItemId ||
      eqp?.requisicaoItem ||
      eqp?.requisicoesitemid ||
      eqp?.requisicoesItemID ||
      eqp?.id || 0
    );
  }

  constructor(
    private util: UtilService, 
    private api: RequisicaoApiService, 
    private apiCol: ColaboradorApiService, 
    private route: Router,
    private ar: ActivatedRoute, 
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
    private apiEquip: EquipamentoApiService
  ) { 
    // ✅ CORREÇÃO: Inicializar MatTableDataSource
    this.dataSourceDevolucoes = new MatTableDataSource<any>([]);
    this.dataSourceBYOD = new MatTableDataSource<any>([]);
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    
    // 🔍 Configuração da busca com debounce
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscarDevolucoes(value))
    );
    this.resultado.subscribe();
    
    // 📋 Verificação de parâmetros da rota
    this.ar.paramMap.subscribe(param => {
      this.parametro = param.get('id');
      if(this.parametro != null) {
        setTimeout(() => {
          if(this.tabGroup) {
            this.tabGroup.selectedIndex = 0;
          }
        }, 100);
        // Aplicar filtro pela rota (id pode vir como nome do colaborador em cenários legados)
        this.consulta.setValue(this.parametro);
      }
    });

    // 🔎 Verificação de query params (preferencial)
    this.ar.queryParams.subscribe(params => {
      const search = params['search'];
      if (search && typeof search === 'string' && search.trim()) {
        // Focar na aba de devoluções e aplicar filtro imediatamente
        setTimeout(() => {
          if (this.tabGroup) {
            this.tabGroup.selectedIndex = 0;
          }
        }, 100);
        this.consulta.setValue(search.trim());
        // Dispara busca direta para refletir imediatamente
        this.buscarDevolucoes(search.trim());
      }
    });
    
    // 📊 Carregamento inicial dos dados - OTIMIZADO
    this.carregarDadosIniciais();
  }

  // ✅ NOVO: Método otimizado para carregar dados iniciais
  private carregarDadosIniciais(): void {
    this.carregandoDados = true;
    this.util.aguardar(true);
    
    // Carregar entregas e devoluções em paralelo
    const entregasPromise = this.api.listarEntregasDisponiveis(this.cliente, this.session.token);
    const devolucoesPromise = this.api.listarDevolucoesDisponivels("null", this.cliente, 1, this.session.token);
    const byodPromise = this.api.listarBYOD("null", this.cliente, 1, this.session.token);
    
    forkJoin([entregasPromise, devolucoesPromise, byodPromise])
      .pipe(
        finalize(() => {
          this.carregandoDados = false;
          this.util.aguardar(false);
          this.cdr.markForCheck(); // Forçar detecção de mudanças
        })
      )
      .subscribe({
        next: ([entregasRes, devolucoesRes, byodRes]) => {
          this.processarDadosEntregas(entregasRes);
          this.processarDadosDevolucoes(devolucoesRes);
          this.processarDadosBYOD(byodRes);
        },
        error: (error) => {
          console.error('[ENTREGAS-DEVOLUCOES] Erro ao carregar dados:', error);
          this.util.exibirFalhaComunicacao();
        }
      });
  }

  listarEntregas(event) {
    this.util.aguardar(true);
    var pagina = ((event == null) ? 1 : event.pageIndex+1);
    this.api.listarEntregasDisponiveis(this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      this.processarDadosEntregas(res);
    })
  }
  
  // ✅ NOVO: Método separado para processar dados de entregas
  private processarDadosEntregas(res: any): void {
    if(res.status != 200 && res.status != 204) {
      this.util.exibirFalhaComunicacao();
    }
    else {
      this.dataSourceEntregas = new MatTableDataSource<any>(res.data);
      this.dataSourceEntregas.paginator = this.paginatorEntrega;
    }
  }

  imprimirTermo(colaborador) {
    if (!colaborador || colaborador <= 0) {
      this.util.exibirMensagemToast('Erro: Colaborador inválido', 5000);
      return;
    }

    this.util.aguardar(true);
    this.apiCol.termoCompromisso(this.session.usuario.cliente, colaborador, this.session.usuario.id, this.session.token).then(res => {
      this.util.aguardar(false);
      
      // Verificar se é uma resposta de erro estruturada
      if (res && 'error' in res && res.error === true) {
        const errorMsg = (res as any).data || 'Erro ao gerar termo de compromisso';
        this.util.exibirMensagemToast(`Erro: ${errorMsg}`, 5000);
        console.error('[ENTREGAS-DEVOLUCOES] Erro retornado pela API:', res);
        return;
      }

      if (res && res.status === 200 && res.data) {
        // Verificar se res.data é uma string Base64 válida
        if (typeof res.data === 'string' && res.data.length > 0) {
          this.util.abrirArquivoNovaJanela(res.data);
        } else {
          this.util.exibirMensagemToast('Erro: Dados do termo inválidos', 5000);
          console.error('[ENTREGAS-DEVOLUCOES] Dados inválidos recebidos:', res.data);
        }
      } else {
        const errorMsg = res?.data || (res as any)?.response?.data || 'Erro ao gerar termo de compromisso';
        this.util.exibirMensagemToast(`Erro: ${errorMsg}`, 5000);
        console.error('[ENTREGAS-DEVOLUCOES] Resposta inválida:', res);
      }
    }).catch(err => {
      this.util.aguardar(false);
      const errorMsg = err?.response?.data || err?.message || 'Erro ao gerar termo de compromisso';
      this.util.exibirMensagemToast(`Erro: ${errorMsg}`, 5000);
      console.error('[ENTREGAS-DEVOLUCOES] Erro ao imprimir termo:', err);
    });
  }
  imprimirTermoBYOD(colaborador) {
    if (!colaborador || colaborador <= 0) {
      this.util.exibirMensagemToast('Erro: Colaborador inválido', 5000);
      return;
    }

    this.util.aguardar(true);
    this.apiCol.termoCompromissoBYOD(this.session.usuario.cliente, colaborador, this.session.usuario.id, this.session.token).then(res => {
      this.util.aguardar(false);
      
      // Verificar se é uma resposta de erro estruturada
      if (res && 'error' in res && res.error === true) {
        const errorMsg = (res as any).data || 'Erro ao gerar termo de compromisso BYOD';
        this.util.exibirMensagemToast(`Erro: ${errorMsg}`, 5000);
        console.error('[ENTREGAS-DEVOLUCOES] Erro retornado pela API:', res);
        return;
      }

      if (res && res.status === 200 && res.data) {
        // Verificar se res.data é uma string Base64 válida
        if (typeof res.data === 'string' && res.data.length > 0) {
          this.util.abrirArquivoNovaJanela(res.data);
        } else {
          this.util.exibirMensagemToast('Erro: Dados do termo inválidos', 5000);
          console.error('[ENTREGAS-DEVOLUCOES] Dados inválidos recebidos:', res.data);
        }
      } else {
        const errorMsg = res?.data || (res as any)?.response?.data || 'Erro ao gerar termo de compromisso BYOD';
        this.util.exibirMensagemToast(`Erro: ${errorMsg}`, 5000);
        console.error('[ENTREGAS-DEVOLUCOES] Resposta inválida:', res);
      }
    }).catch(err => {
      this.util.aguardar(false);
      const errorMsg = err?.response?.data || err?.message || 'Erro ao gerar termo de compromisso BYOD';
      this.util.exibirMensagemToast(`Erro: ${errorMsg}`, 5000);
      console.error('[ENTREGAS-DEVOLUCOES] Erro ao imprimir termo BYOD:', err);
    });
  }
  enviarTermo(colaborador){
    this.util.aguardar(true);
    this.apiCol.termoPorEmail(this.session.usuario.cliente, colaborador, this.session.usuario.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.util.exibirMensagemToast('Termo enviado para e-mail do colaborador', 5000);
    })
  }
  enviarTermoBYOD(colaborador){
    this.util.aguardar(true);
    this.apiCol.termoPorEmailBYOD(this.session.usuario.cliente, colaborador, this.session.usuario.id, this.session.token).then(res => {
      this.util.aguardar(false);
      this.util.exibirMensagemToast('Termo enviado para e-mail do colaborador', 5000);
    })
  }

  adicionarObservacao(eqp) {
    
    const modalObservacao = this.dialog.open(AdicionarObservacaoComponent, {
      width: '500px',
      data: {
        eqp
      }
    });

    modalObservacao.afterClosed().subscribe(r => {
      this.listarDevolucoes(null);
    })
  }
  agendamento(req) {
    const modalAgendamento = this.dialog.open(AgendamentoComponent, {
      width: '500px',
      data: {
        requisicao: req
      }
    });

    modalAgendamento.afterClosed().subscribe(result => {
      if (result) {
        this.listarDevolucoes(null);
      }
    });
  }
  devolverEquipamento(eqp, colaboradorLinha?: any) {
    const mensagem = `Está certo que deseja realizar a devolução do recurso ${eqp.equipamento}?<br><br>S/N: ${eqp.numeroSerie}<br>Patrimônio: ${eqp.patrimonio}`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        eqp.usuariodevolucaoid = this.session.usuario.id;

        this.util.aguardar(true);
        this.api.realizarDevolucaoEquipamento(eqp, this.session.token).then(res => {
          this.util.aguardar(false);
          if(res.status == 200) {
            this.listarDevolucoes(null);
            // ✅ NOVO: Checagem pós-devolução para emitir Nada Consta
            const colabId = eqp.colaboradorId || colaboradorLinha?.colaboradorId;
            const colabNome = eqp.colaboradorNome || colaboradorLinha?.colaborador;
            this.sugerirNadaConstaSeElegivel(colabId, colabNome);
          }
          else {
            this.util.exibirMensagemToast('Falha de comunicação com o serviço', 5000);
          }
        })
      }
    });
  }
  devolverEquipamentosColaborador(req) {
    const mensagem = `⚠️ <strong>ATENÇÃO</strong>: Devolução de Todos os Recursos<br><br>Está certo que deseja realizar a devolução de todos os recursos do colaborador ${req.colaborador}?<br><br>🔴 <strong>IMPORTANTE</strong> - Leia com atenção:<br><br>1. Certifique-se de que TODOS os recursos foram fisicamente recebidos<br>2. Uma vez confirmada a devolução, o colaborador não terá mais responsabilidade sobre os recursos<br>3. A responsabilidade será transferida para o operador que realizar a devolução<br>4. Esta ação não pode ser desfeita<br><br>Deseja continuar com a devolução?`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        var requisicoes = {colaboradorId: req.colaboradorId, usuariodevolucaoid: this.session.usuario.id};
        this.util.aguardar(true);
        this.api.realizarDevolucoesColaborador(requisicoes, this.session.token).then(res => {
          this.util.aguardar(false);
          if(res.status == 200) {
            this.listarDevolucoes(null);
            // ✅ NOVO: Checagem pós-devolução para emitir Nada Consta
            this.sugerirNadaConstaSeElegivel(req.colaboradorId, req.colaborador);
          }
          else {
            this.util.exibirMensagemToast('Falha de comunicação com o serviço', 5000);
          }
        })
      }
    });
  }

  // ✅ NOVO: Sugere emissão de Nada Consta se colaborador não tiver mais recursos
  private sugerirNadaConstaSeElegivel(colaboradorId: number, colaboradorNome: string) {
    if (!colaboradorId) return;
    try {
      // Consultar novamente as devoluções para garantir estado atualizado antes da checagem
      this.api.listarDevolucoesDisponivels("null", this.cliente, 1, this.session.token)
        .then((res: any) => {
          const lista = (res && res.data && (res.data.results || res.data)) || [];
          const aindaPossui = Array.isArray(lista) && lista.some((row: any) => row && row.colaboradorId === colaboradorId);
          if (aindaPossui) return;

          const msg = `O colaborador <b>${colaboradorNome || 'selecionado'}</b> não possui mais recursos pendentes após a devolução.<br><br>` +
                      `Deseja emitir o Nada Consta agora?`;

          this.util.exibirMensagemPopUp(msg, true).then((emitir) => {
            if (emitir) {
              this.route.navigate(['/nada-consta'], { queryParams: { search: colaboradorNome || '' } });
            }
          });
        })
        .catch(() => { /* silencioso */ });
    } catch (_) { }
  }

  devolverEquipamentosColaboradorBYOD(req) {
    const mensagem = `⚠️ <strong>ATENÇÃO</strong>: Devolução de Todos os Recursos BYOD<br><br>Está certo que deseja realizar a devolução de todos os recursos BYOD do colaborador ${req.colaborador}?<br><br>🔴 <strong>IMPORTANTE</strong> - Leia com atenção:<br><br>1. Certifique-se de que TODOS os recursos BYOD foram fisicamente recebidos<br>2. Uma vez confirmada a devolução, o colaborador não terá mais responsabilidade sobre os recursos<br>3. A responsabilidade será transferida para o operador que realizar a devolução<br>4. Esta ação não pode ser desfeita<br><br>Deseja continuar com a devolução?`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        var requisicoes = {colaboradorId: req.colaboradorId, usuariodevolucaoid: this.session.usuario.id};
        this.util.aguardar(true);
        this.api.realizarDevolucoesColaboradorBYOD(requisicoes, this.session.token).then(res => {
          this.util.aguardar(false);
          if(res.status == 200) {
            this.listarDevolucoes(null);
            // ✅ NOVO: Checagem pós-devolução para emitir Nada Consta
            this.sugerirNadaConstaSeElegivel(req.colaboradorId, req.colaborador);
          }
          else {
            this.util.exibirMensagemToast('Falha de comunicação com o serviço', 5000);
          }
        })
      }
    });
  }

  listarDevolucoes(event ){
    //Se houver parametro, pesquisar pelo nome do colaborador, do contrário, lista tudo.
    if(this.parametro != null) {
      this.consulta.setValue(this.parametro);
      this.buscarDevolucoes(this.parametro);
    }
    else {
      //this.tabGroup.selectedIndex = 0;
      this.util.aguardar(true);
      var pagina = ((event == null) ? 1 : event.pageIndex+1);
      
      // ✅ OTIMIZAÇÃO: Carregar devoluções e BYOD em paralelo
      const devolucoesPromise = this.api.listarDevolucoesDisponivels("null", this.cliente, pagina, this.session.token);
      const byodPromise = this.api.listarBYOD("null", this.cliente, pagina, this.session.token);
      
      forkJoin([devolucoesPromise, byodPromise])
        .pipe(
          finalize(() => {
            this.util.aguardar(false);
            this.cdr.markForCheck();
          })
        )
        .subscribe({
          next: ([devolucoesRes, byodRes]) => {
            this.processarDadosDevolucoes(devolucoesRes);
            this.processarDadosBYOD(byodRes);
          },
          error: (error) => {
            console.error('[ENTREGAS-DEVOLUCOES] Erro ao carregar devoluções:', error);
            this.util.aguardar(false);
            this.dataSourceDevolucoes.data = [];
            this.dataSourceBYOD.data = [];
          }
        });
    }
  }
  
  // ✅ NOVO: Método separado para processar dados de devoluções
  private processarDadosDevolucoes(res: any): void {
    // ✅ CORREÇÃO: Validação de dados para evitar erros
    if (res && res.data && res.data.results) {
      // 🔍 DEBUG opcional: logar primeiro item quando sem tipo de aquisição
      try {
        if (!environment.production) {
          const primeiro = (res.data.results[0]?.requisicoesColaborador?.[0]?.equipamentosRequisicao?.[0]) || res.data.results[0];
          if (primeiro && (primeiro.tipoAquisicao === undefined && primeiro.tipoaquisicao === undefined && primeiro.TipoAquisicao === undefined) ) {
          }
        }
      } catch { /* silencioso */ }
      this.dataSourceDevolucoes.data = res.data.results;
    } else {
      console.warn('[ENTREGAS-DEVOLUCOES] Dados inválidos recebidos:', res);
      this.dataSourceDevolucoes.data = [];
    }
      
    // ✅ CORREÇÃO: Validação do paginador antes de acessar
    if (res && res.data && res.data.currentPage && res.data.rowCount) {
      this.paginatorDevolucao.pageIndex = res.data.currentPage-1;
      this.paginatorDevolucao.pageSize = 10;
      this.paginatorDevolucao.length = res.data.rowCount;
    }

  }
  
  // ✅ NOVO: Método separado para processar dados BYOD
  private processarDadosBYOD(res: any): void {
    
    // ✅ CORREÇÃO: Validação de dados para evitar erros
    if (res && res.data && res.data.results) {
      this.dataSourceBYOD.data = res.data.results;
      
      // ✅ CORREÇÃO: Validação do paginador antes de acessar
      if (this.paginatorBYOD && res.data.currentPage && res.data.rowCount) {
        this.paginatorBYOD.pageIndex = res.data.currentPage-1;
        this.paginatorBYOD.pageSize = 10;
        this.paginatorBYOD.length = res.data.rowCount;
      }
    } else {
      this.dataSourceBYOD.data = [];
      console.warn('[ENTREGAS-DEVOLUCOES] Dados BYOD inválidos:', res);
    }
  }

  buscarDevolucoes(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarDevolucoesDisponivels(valor, this.cliente, 1, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          // ✅ CORREÇÃO: Validação de dados para evitar erros
          if (res && res.data && res.data.results) {
            
            this.dataSourceDevolucoes.data = res.data.results;
            this.dataSourceDevolucoes.paginator = this.paginatorDevolucao;
          } else {
            this.dataSourceDevolucoes.data = [];
            console.warn('[ENTREGAS-DEVOLUCOES] Dados de busca inválidos:', res);
          }
        }
        
        this.api.listarBYOD(valor, this.cliente, 1, this.session.token).then(result => {
          // ✅ CORREÇÃO: Validação de dados para evitar erros
          if (result && result.data && result.data.results) {
            this.dataSourceBYOD.data = result.data.results;
            this.dataSourceBYOD.paginator = this.paginatorBYOD;
          } else {
            this.dataSourceBYOD.data = [];
            console.warn('[ENTREGAS-DEVOLUCOES] Dados BYOD de busca inválidos:', result);
          }
        }).catch(error => {
          console.error('[ENTREGAS-DEVOLUCOES] Erro ao buscar BYOD:', error);
          this.dataSourceBYOD.data = [];
        });
      }).catch(error => {
        console.error('[ENTREGAS-DEVOLUCOES] Erro ao buscar devoluções:', error);
        this.util.aguardar(false);
        this.dataSourceDevolucoes.data = [];
      });
    }
    else {
      this.listarDevolucoes(null);
    }
  }

  // 🎯 MÉTODOS AUXILIARES PARA MELHOR UX

  /**
   * Limpa a busca e recarrega os dados
   */
  limparBusca(): void {
    this.consulta.setValue('');
    this.listarDevolucoes(null);
  }

  /**
   * Obtém o total de equipamentos de um colaborador
   */
  getTotalEquipamentos(colaborador: any): number {
    if (!colaborador?.requisicoesColaborador) return 0;
    
    return colaborador.requisicoesColaborador.reduce((total: number, req: any) => {
      return total + (req.equipamentosRequisicao?.length || 0);
    }, 0);
  }

  /**
   * Verifica se o colaborador tem assinatura pendente
   */
  temAssinaturaPendente(colaborador: any): boolean {
    return !colaborador?.assinouUltimaRequisicao;
  }

  /**
   * Obtém a cor do status do colaborador
   */
  getStatusColor(colaborador: any): string {
    return this.temAssinaturaPendente(colaborador) ? 'warning' : 'success';
  }

  /**
   * Obtém a classe CSS para o status da assinatura
   */
  getStatusClass(colaborador: any): string {
    if (colaborador?.assinouUltimaRequisicao === true) {
      return 'status-assinado';
    } else if (colaborador?.assinouUltimaRequisicao === false) {
      return 'status-pendente';
    } else {
      return 'status-neutro';
    }
  }

  /**
   * Obtém o ícone para o status da assinatura
   */
  getStatusIcon(colaborador: any): string {
    if (colaborador?.assinouUltimaRequisicao === true) {
      return 'check_circle';
    } else if (colaborador?.assinouUltimaRequisicao === false) {
      return 'warning';
    } else {
      return 'help_outline';
    }
  }

  /**
   * Obtém o texto para o status da assinatura
   */
  getStatusText(colaborador: any): string {
    if (colaborador?.assinouUltimaRequisicao === true) {
      return 'Assinatura realizada';
    } else if (colaborador?.assinouUltimaRequisicao === false) {
      return 'Assinatura pendente';
    } else {
      return 'Status não informado';
    }
  }

  // ===================== CAMPOS DE EXIBIÇÃO (TÉCNICO / DATAS) =====================
  private isDataValida(valor: any): boolean {
    if (!valor) return false;
    const d = new Date(valor);
    if (isNaN(d.getTime())) return false;
    const ano = d.getFullYear();
    // Evitar datas default do .NET (0001-01-01)
    return ano > 1900;
  }

  public getTecnicoResponsavel(requisicao: any): string {
    if (!requisicao) return 'N/A';
    const topo = (
      requisicao.tecnicoResponsavel ||
      requisicao.TecnicoResponsavel ||
      requisicao.responsavelProvisorio ||
      requisicao.Usuario ||
      requisicao.usuario ||
      'N/A'
    );
    if (topo !== 'N/A') return topo;
    // Fallback para linhas telefônicas
    const linhas = requisicao.linhasTelefonicas || requisicao.LinhasTelefonicas || [];
    if (Array.isArray(linhas) && linhas.length > 0) {
      const l = linhas[0] || {};
      const tecnico = (
        l.tecnicoResponsavel ||
        l.TecnicoResponsavel ||
        l.usuarioEntregaNome ||
        l.Usuario ||
        l.usuario ||
        l.UsuarioentregaNome ||
        l.usuarioentregaNome ||
        'N/A'
      );
      return tecnico || 'N/A';
    }
    return 'N/A';
  }

  // ✅ CORREÇÃO: Responsável Provisório (tecnicoresponsavel da tabela requisicoes)
  public getResponsavelProvisorio(requisicao: any): string {
    if (!requisicao) return 'N/A';
    
    // ✅ CORREÇÃO: Usar o campo correto da requisição principal
    // tecnicoresponsavel = Responsável Provisório (quem entrega/entregou)
    const responsavel = (
      requisicao.tecnicoResponsavel ||
      requisicao.TecnicoResponsavel ||
      requisicao.responsavelProvisorio ||
      'N/A'
    );
    return responsavel;
    
    // Fallback: buscar nas linhas telefônicas (caso não seja identificada como linha acima)
    const linhas = requisicao.linhasTelefonicas || requisicao.LinhasTelefonicas || [];
    if (Array.isArray(linhas) && linhas.length > 0) {
      const l = linhas[0] || {};
      const responsavelLinhaItem = (
        l.usuarioEntregaNome ||
        l.UsuarioentregaNome ||
        l.usuarioentregaNome ||
        l.UsuarioEntregaNome ||
        'N/A'
      );
      if (responsavelLinhaItem && responsavelLinhaItem !== 'N/A') return responsavelLinhaItem;
    }
    
    return 'N/A';
  }

  // ✅ CORREÇÃO: Usuário (usuariorequisicao da tabela requisicoes)
  public getUsuarioProcesso(requisicao: any): string {
    if (!requisicao) return 'N/A';
    
    // ✅ CORREÇÃO: Usar o campo correto da requisição principal
    // usuariorequisicao = Usuário (quem fez a requisição no sistema)
    const usuario = (
      requisicao.requisitante ||
      requisicao.Requisitante ||
      requisicao.usuario ||
      requisicao.Usuario ||
      requisicao.usuarioRequisicao ||
      requisicao.UsuarioRequisicao ||
      'N/A'
    );
    return usuario;
  }

  public getDataSolicitacao(requisicao: any): string {
    if (!requisicao) return 'N/A';
    let data = requisicao.dtSolicitacao || requisicao.DTSolicitacao;
    if (this.isDataValida(data)) return new Date(data).toLocaleDateString('pt-BR');
    // Fallback para linhas telefônicas
    const linhas = requisicao.linhasTelefonicas || requisicao.LinhasTelefonicas || [];
    if (Array.isArray(linhas) && linhas.length > 0) {
      const l = linhas[0] || {};
      data = l.dtSolicitacao || l.DTSolicitacao || l.dtRegistro || l.Dtregistro || l.DTregistro || l.DtSolicitacao;
      if (this.isDataValida(data)) return new Date(data).toLocaleDateString('pt-BR');
    }
    return 'N/A';
  }

  public getDataEntrega(requisicao: any): string {
    if (!requisicao) return 'N/A';
    let data = requisicao.dtProcessamento || requisicao.DTProcessamento;
    if (this.isDataValida(data)) return new Date(data).toLocaleDateString('pt-BR');
    // Fallback para linhas telefônicas
    const linhas = requisicao.linhasTelefonicas || requisicao.LinhasTelefonicas || [];
    if (Array.isArray(linhas) && linhas.length > 0) {
      const l = linhas[0] || {};
      data = l.dtEntrega || l.Dtentrega || l.dtProcessamento || l.DTProcessamento || l.dtRegistro || l.Dtregistro || l.DtEntrega;
      if (this.isDataValida(data)) return new Date(data).toLocaleDateString('pt-BR');
    }
    return 'N/A';
  }

  // 🔧 Helpers de debug
  private getReqIdSafe(requisicao: any): number {
    return (requisicao?.requisicaoId || requisicao?.RequisicaoId || 0) as number;
  }

/**
   * Formata a data para exibição
   */
  formatarData(data: any): string {
    if (!data) return 'N/A';
    return new Date(data).toLocaleDateString('pt-BR');
  }

  /**
   * Obtém o ícone do tipo de equipamento
   */
  getEquipamentoIcon(equipamento: string): string {
    if (equipamento.toLowerCase().includes('linha')) {
      return 'phone';
    } else if (equipamento.toLowerCase().includes('notebook') || equipamento.toLowerCase().includes('laptop')) {
      return 'laptop';
    } else if (equipamento.toLowerCase().includes('smartphone') || equipamento.toLowerCase().includes('celular')) {
      return 'smartphone';
    } else if (equipamento.toLowerCase().includes('tablet')) {
      return 'tablet';
    } else {
      return 'devices';
    }
  }

  getEquipamentoType(equipamento: string): string {
    if (equipamento.toLowerCase().includes('linha')) {
      return 'Linha Telefônica';
    } else {
      return 'Equipamento';
    }
  }

  // ✅ NOVO: Normaliza o campo de Tipo de Aquisição
  public getTipoAquisicao(eqp: any): string {
    if (!eqp) return 'Não informado';
    // Se é BYOD, não exibimos (tratado no template), mas por segurança
    if (eqp.isByod === true) return 'Particular';

    // Alguns endpoints retornam ID (1,2,3), outros retornam string, e alguns retornam objeto de navegação
    const navigationNome = (
      eqp?.tipoaquisicaoNavigation?.nome ||
      eqp?.tipoAquisicaoNavigation?.nome ||
      eqp?.tipoaquisicaoNavigation?.Nome ||
      eqp?.tipoAquisicaoNavigation?.Nome ||
      eqp?.tipoaquisicaoNavigation?.descricao ||
      eqp?.tipoAquisicaoNavigation?.descricao ||
      eqp?.tipoaquisicaoNavigation?.Descricao ||
      eqp?.tipoAquisicaoNavigation?.Descricao
    );
    const nomeDireto = (
      eqp?.tipoAquisicaoNome ||
      eqp?.TipoAquisicaoNome ||
      eqp?.tipoaquisicaoNome ||
      eqp?.tipoAquisicaoDescricao ||
      eqp?.tipoaquisicaoDescricao
    );
    const bruto = (
      navigationNome ??
      nomeDireto ??
      eqp?.tipoAquisicao ??
      eqp?.tipoaquisicao ??
      eqp?.TipoAquisicao ??
      eqp?.tipo_aquisicao ??
      eqp?.aquisicaoTipo ??
      eqp?.AquisicaoTipo ??
      eqp?.tipoAquisicaoId ??
      eqp?.TipoAquisicaoId
    );
    if (bruto === undefined || bruto === null || bruto === '') return 'Não informado';

    const numerico = parseInt(bruto as any, 10);
    if (!isNaN(numerico)) {
      switch (numerico) {
        case 1: return 'Alugado';
        case 2: return 'Particular';
        case 3: return 'Corporativo';
        default: return 'Não informado';
      }
    }

    const texto = String(bruto).toLowerCase();
    if (texto.includes('alug')) return 'Alugado';
    if (texto.includes('próprio') || texto.includes('proprio') || texto.includes('particular')) return 'Particular';
    if (texto.includes('corporat')) return 'Corporativo';
    const fallback = (eqp.tipoAquisicao || eqp.TipoAquisicao || navigationNome || nomeDireto || 'Não informado');
    if (!environment.production && fallback === 'Não informado') {
      try {
        console.warn('[ENTREGAS-DEVOLUCOES][DEBUG] Tipo de aquisição não identificado para item:', {
          keys: Object.keys(eqp || {}),
          eqp
        });
      } catch { /* silencioso */ }
    }
    return fallback;
  }

  /**
   * Verifica se há equipamentos para exibir
   */
  temEquipamentos(colaborador: any): boolean {
    return this.getTotalEquipamentos(colaborador) > 0;
  }

  /**
   * Obtém o texto do status da requisição
   */
  getStatusRequisicao(requisicao: any): string {
    if (requisicao?.dtProcessamento) {
      return 'Entregue';
    } else if (requisicao?.dtSolicitacao) {
      return 'Pendente';
    }
    return 'Desconhecido';
  }

  // ✅ NOVO: Métodos para controlar expansão dos colaboradores com carregamento sob demanda
  private colabNameCache: Map<number, string> = new Map();

  private getRowByColaboradorId(colaboradorId: number): any {
    if (!this.dataSourceDevolucoes?.data?.length) return null;
    return this.dataSourceDevolucoes.data.find((r: any) => r?.colaboradorId === colaboradorId);
  }

  private carregarCompartilhadosAtivosParaItem(eqp: any): void {
    const itemId = this.getReqItemId(eqp);
    if (!itemId || this.isLinhaItem(eqp)) return;
    // Enriquecer tipo de aquisição se ausente (somente não-BYOD)
    this.carregarTipoAquisicaoParaItem(eqp);
    this.api.listarCompartilhadosItem(itemId, this.session.token)
      .then((res: any) => {
        const ativos = (res?.data || []).filter((v: any) => v?.ativo === true);
        eqp.compartilhadosAtivos = ativos;
        if (!ativos.length) {
          eqp.compartilhadosAtivosNomes = '';
          this.cdr.markForCheck();
          return;
        }
        const promessas = ativos.map((v: any) => {
          if (this.colabNameCache.has(v.colaboradorId)) {
            v.colaboradorNome = this.colabNameCache.get(v.colaboradorId);
            return Promise.resolve(null);
          }
          return this.apiCol.obterColaboradorPorID(v.colaboradorId, this.session.token)
            .then((r: any) => {
              const nome = r?.data?.nome || r?.data?.Nome || '';
              if (nome) {
                this.colabNameCache.set(v.colaboradorId, nome);
                v.colaboradorNome = nome;
              }
            })
            .catch(() => null);
        });
        Promise.all(promessas).then(() => {
          // Incluir nome e observação (se houver)
          eqp.compartilhadosAtivosNomes = ativos.map((v: any) => {
            const nome = v.colaboradorNome || v.colaboradorId;
            const obs = v.observacao || v.Observacao;
            return obs ? `${nome} (Obs: ${obs})` : nome;
          }).join(', ');
          this.cdr.markForCheck();
        });
      })
      .catch(() => { /* silencioso */ });
  }

  // ✅ NOVO: Enriquecer tipo de aquisição quando não vier da API de devoluções
  private tipoAqCarregado: Set<number> = new Set();
  private carregarTipoAquisicaoParaItem(eqp: any): void {
    try {
      if (!eqp || this.isLinhaItem(eqp) || eqp.isByod === true) return;
      const jaTem = (
        eqp.tipoAquisicao !== undefined ||
        eqp.tipoaquisicao !== undefined ||
        eqp.TipoAquisicao !== undefined ||
        eqp.tipoAquisicaoNome !== undefined ||
        (eqp.tipoaquisicaoNavigation && (eqp.tipoaquisicaoNavigation.nome || eqp.tipoaquisicaoNavigation.Nome))
      );
      if (jaTem) return;

      const equipId = eqp.equipamentoId || eqp.EquipamentoId || eqp.id;
      if (!equipId || this.tipoAqCarregado.has(equipId)) return;
      this.tipoAqCarregado.add(equipId);

      this.apiEquip.buscarEquipamentoPorId(equipId, this.session.token)
        .then((res: any) => {
          const d = res?.data?.data || res?.data?.Data || res?.data || {};
          // Mapear possíveis campos
          eqp.tipoAquisicao = d.tipoAquisicao ?? d.tipoaquisicao ?? d.TipoAquisicao ?? d.tipoAquisicaoId ?? d.TipoAquisicaoId;
          eqp.tipoAquisicaoNome = d.tipoAquisicaoNome || d.TipoAquisicaoNome || d?.tipoaquisicaoNavigation?.nome || d?.tipoAquisicaoNavigation?.nome;
          this.cdr.markForCheck();
        })
        .catch(() => { /* silencioso */ });
    } catch { /* silencioso */ }
  }

  private carregarCompartilhadosAtivosParaColaborador(colaboradorId: number): void {
    const row = this.getRowByColaboradorId(colaboradorId);
    if (!row?.requisicoesColaborador?.length) return;
    row.requisicoesColaborador.forEach((req: any) => {
      if (Array.isArray(req?.equipamentosRequisicao)) {
        req.equipamentosRequisicao.forEach((eqp: any) => this.carregarCompartilhadosAtivosParaItem(eqp));
      }
    });
  }

  toggleColaboradorExpansion(colaboradorId: number): void {
    if (this.expandedColaboradores.has(colaboradorId)) {
      this.expandedColaboradores.delete(colaboradorId);
    } else {
      this.expandedColaboradores.add(colaboradorId);
      this.carregarRequisicoesColaborador(colaboradorId);
      // Após expandir, carregar co-responsáveis ativos para exibir nos cards
      this.carregarCompartilhadosAtivosParaColaborador(colaboradorId);
    }
  }

  // ✅ NOVO: Carrega as requisições de um colaborador específico - OTIMIZADO
  carregarRequisicoesColaborador(colaboradorId: number): void {
    // ✅ CORREÇÃO: Verifica se já está carregando ou se já tem dados em cache
    if (this.carregandoRequisicoes.has(colaboradorId) || this.temDadosEmCache(colaboradorId)) {
      return;
    }
    this.clearGridClassCache();

    // Marca como carregando
    this.carregandoRequisicoes.add(colaboradorId);
    this.cacheCarregamento.set(colaboradorId, true);

    // 🔄 IMPLEMENTAÇÃO FUTURA: Aqui você pode implementar uma chamada específica à API
    // Exemplo de como seria:
    /*
    this.api.listarRequisicoesColaborador(colaboradorId, this.cliente, this.session.token).then(res => {
      this.carregandoRequisicoes.delete(colaboradorId);
      this.cacheCarregamento.set(colaboradorId, false);
      if (res.status === 200) {
        // Atualizar os dados do colaborador específico
        this.atualizarDadosColaborador(colaboradorId, res.data);
        this.cacheRequisicoes.set(colaboradorId, res.data);
      } else {
        this.util.exibirFalhaComunicacao();
      }
      this.cdr.markForCheck();
    }).catch(error => {
      this.carregandoRequisicoes.delete(colaboradorId);
      this.cacheCarregamento.set(colaboradorId, false);
      console.error('Erro ao carregar requisições:', error);
      this.cdr.markForCheck();
    });
    */

    // Por enquanto, simula carregamento com os dados já disponíveis
    setTimeout(() => {
      this.carregandoRequisicoes.delete(colaboradorId);
      this.cacheCarregamento.set(colaboradorId, false);
      
      // ✅ NOVO: Marcar como carregado no cache
      this.cacheRequisicoes.set(colaboradorId, []);
      this.cdr.markForCheck(); // Forçar detecção de mudanças
    }, 100); // Reduzido para melhor UX
  }

  // ✅ NOVO: Método auxiliar para atualizar dados de um colaborador específico
  atualizarDadosColaborador(colaboradorId: number, dados: any): void {
    // Encontrar o colaborador na lista e atualizar seus dados
    const colaborador = this.dataSourceDevolucoes.data.find(c => c.colaboradorId === colaboradorId);
    if (colaborador) {
      colaborador.requisicoesColaborador = dados.requisicoesColaborador || [];
    }
    
    // Também verificar na lista BYOD
    const colaboradorBYOD = this.dataSourceBYOD.data.find(c => c.colaboradorId === colaboradorId);
    if (colaboradorBYOD) {
      colaboradorBYOD.requisicoesColaborador = dados.requisicoesColaborador || [];
    }
  }

  // ✅ NOVO: Verifica se está carregando requisições - OTIMIZADO
  estaCarregandoRequisicoes(colaboradorId: number): boolean {
    return this.carregandoRequisicoes.has(colaboradorId) || this.cacheCarregamento.get(colaboradorId) === true;
  }

  isColaboradorExpanded(colaboradorId: number): boolean {
    return this.expandedColaboradores.has(colaboradorId);
  }

  getTotalEquipamentosColaborador(requisicoes: any[]): number {
    if (!requisicoes) return 0;
    return requisicoes.reduce((total, req) => {
      return total + (req.equipamentosRequisicao?.length || 0);
    }, 0);
  }

  // ✅ NOVO: Método para formatar contagem de recursos com plural correto
  getRecursosText(requisicoes: any[]): string {
    const total = this.getTotalEquipamentosColaborador(requisicoes);
    return total === 1 ? '1 recurso' : `${total} recursos`;
  }

  // ✅ NOVO: Método melhorado para exibir recursos de forma detalhada
  getRecursosDetalhados(requisicoes: any[]): string {
    if (!requisicoes || requisicoes.length === 0) {
      return 'Nenhum recurso';
    }

    let equipamentos = 0;
    let linhasTelefonicas = 0;
    let recursosDetalhados = [];

    requisicoes.forEach(req => {
      if (req.equipamentosRequisicao && req.equipamentosRequisicao.length > 0) {
        equipamentos += req.equipamentosRequisicao.length;
        
        // Adicionar detalhes dos equipamentos
        req.equipamentosRequisicao.forEach((eqp: any) => {
          const nome = eqp.nome || eqp.Nome || 'Equipamento';
          const serie = eqp.numeroserie || eqp.Numeroserie || '';
          const patrimonio = eqp.patrimonio || eqp.Patrimonio || '';
          
          let detalhe = nome;
          if (serie) detalhe += ` (${serie})`;
          if (patrimonio) detalhe += ` - P:${patrimonio}`;
          
          recursosDetalhados.push(detalhe);
        });
      }
      
      if (req.linhasTelefonicas && req.linhasTelefonicas.length > 0) {
        linhasTelefonicas += req.linhasTelefonicas.length;
        
        // Adicionar detalhes das linhas
        req.linhasTelefonicas.forEach((linha: any) => {
          const numero = linha.numero || linha.Numero || 'N/A';
          const operadora = linha.operadora || linha.Operadora || 'N/A';
          recursosDetalhados.push(`${operadora} - ${numero}`);
        });
      }
    });

    // Se tem muitos recursos, mostrar resumo
    if (recursosDetalhados.length > 3) {
      let resumo = '';
      if (equipamentos > 0) resumo += `${equipamentos} equipamento${equipamentos > 1 ? 's' : ''}`;
      if (linhasTelefonicas > 0) {
        if (resumo) resumo += ', ';
        resumo += `${linhasTelefonicas} linha${linhasTelefonicas > 1 ? 's' : ''}`;
      }
      return resumo;
    }

    // Se tem poucos recursos, mostrar detalhes
    return recursosDetalhados.join(', ');
  }

  // ✅ NOVO: Sistema de cores para status de assinatura
  getSignatureStatus(requisicao: any): string {
    if (!requisicao) return 'unknown';
    
    // Verificar se tem pendências de assinatura
    if (requisicao.pendenciasAssinatura && requisicao.pendenciasAssinatura.length > 0) {
      return 'pending';
    }
    
    // Verificar se foi assinada
    if (requisicao.assinada === true || requisicao.statusAssinatura === 'assinada') {
      return 'completed';
    }
    
    return 'unknown';
  }

  getSignatureStatusText(requisicao: any): string {
    const status = this.getSignatureStatus(requisicao);
    switch (status) {
      case 'pending': return 'Pendente de Assinatura';
      case 'completed': return 'Assinada';
      default: return '';
    }
  }

  // ✅ NOVO: Toggle para expandir/colapsar requisições do colaborador - OTIMIZADO
  toggleRequisicoes(colaboradorId: number, colaborador: any) {
    if (this.expandedColaboradores.has(colaboradorId)) {
      // Colapsar
      this.expandedColaboradores.delete(colaboradorId);
    } else {
      // Expandir
      this.expandedColaboradores.add(colaboradorId);
      
      // Carregar requisições se ainda não foram carregadas
      if (!colaborador.requisicoesColaborador || colaborador.requisicoesColaborador.length === 0) {
        this.carregarRequisicoesColaborador(colaboradorId);
      }
    }
    this.cdr.markForCheck(); // Forçar detecção de mudanças
  }

  // ✅ CORREÇÃO: Propriedades computadas para evitar chamadas repetidas
  private _currentGridClass = '';
  private _lastResourceCount = -1;
  
  // ✅ CORREÇÃO: Getter que calcula a classe apenas quando necessário
  get currentGridClass(): string {
    const resourceCount = this.requisicoes?.equipamentosRequisicao?.length || 0;
    
    // Se o valor não mudou, retornar cache
    if (this._lastResourceCount === resourceCount && this._currentGridClass) {
      return this._currentGridClass;
    }
    
    // Calcular nova classe
    this._currentGridClass = this.calculateGridClass(resourceCount);
    this._lastResourceCount = resourceCount;
    
    return this._currentGridClass;
  }
  
  // ✅ CORREÇÃO: Método separado para cálculo (sem logs excessivos)
  private calculateGridClass(resourceCount: number): string {
    if (!resourceCount) {
      return '';
    } else if (resourceCount >= 15) {
      return 'many-resources';
    } else if (resourceCount === 2) {
      return 'two-resources';
    } else if (resourceCount <= 1) {
      return 'few-resources';
    }
    return '';
  }
  
  // ✅ CORREÇÃO: Método original mantido para compatibilidade (com cache)
  getGridClass(resourceCount: number): string {
    // Se o valor não mudou, retornar cache
    if (this._lastResourceCount === resourceCount && this._currentGridClass) {
      return this._currentGridClass;
    }
    this._currentGridClass = this.calculateGridClass(resourceCount);
    this._lastResourceCount = resourceCount;
    
    return this._currentGridClass;
  }
  
  // ✅ CORREÇÃO: Método para limpar cache quando necessário
  private clearGridClassCache(): void {
    this._currentGridClass = '';
    this._lastResourceCount = -1;
  }
  
  // ✅ NOVO: Método para limpar cache de colaboradores
  private clearColaboradorCache(): void {
    this.cacheRequisicoes.clear();
    this.cacheCarregamento.clear();
    this.carregandoRequisicoes.clear();
  }
  
  // ✅ NOVO: Método para verificar se colaborador tem dados em cache
  private temDadosEmCache(colaboradorId: number): boolean {
    return this.cacheRequisicoes.has(colaboradorId);
  }

  // ✅ NOVO: Método para debug dos colaboradores
  getColaboradoresDebug(): string {
    if (!this.dataSourceDevolucoes?.data?.length) {
      return 'Nenhum';
    }
    return this.dataSourceDevolucoes.data.map(c => c.colaborador).join(', ');
  }

  // ===================== COMPARTILHAMENTO POR ITEM (NÃO-BYOD) =====================
  isCompartilhamentoAberto(itemId: number): boolean {
    return this.compartilhamentoAberto.has(itemId);
  }

  toggleCompartilhamento(eqp: any): void {
    if (!eqp || this.isLinhaItem(eqp)) return;
    const itemId = this.getReqItemId(eqp);
    if (!itemId) return;
    if (this.compartilhamentoAberto.has(itemId)) {
      this.compartilhamentoAberto.delete(itemId);
      this.cdr.markForCheck();
      return;
    }
    this.compartilhamentoAberto.add(itemId);
    this.carregarCompartilhados(itemId);
  }

  private carregarCompartilhados(itemId: number): void {
    if (!itemId) return;
    this.util.aguardar(true);
    this.api.listarCompartilhadosItem(itemId, this.session.token)
      .then((res: any) => {
        this.util.aguardar(false);
        if (res?.status === 200) {
          this.mapaCompartilhados[itemId] = res.data || [];
        } else {
          this.mapaCompartilhados[itemId] = [];
        }
        this.cdr.markForCheck();
      })
      .catch(() => {
        this.util.aguardar(false);
        this.mapaCompartilhados[itemId] = [];
        this.cdr.markForCheck();
      });
  }

  buscarCoResponsaveis(itemId: number, termo: string): void {
    if (!itemId) return;
    const consulta = (termo && termo.length >= 2) ? termo : 'null';
    this.apiCol.listarColaboradoresAtivos(consulta, this.session.usuario.cliente, this.session.token)
      .then((res: any) => {
        if (res?.status === 200) {
          // ✅ CORREÇÃO: Filtrar apenas compartilhamentos ATIVOS para permitir re-compartilhar após encerramento
          const existentes = new Set((this.mapaCompartilhados[itemId] || []).filter((v: any) => v.ativo).map((v: any) => v.colaboradorId));
          this.mapaOpcoesCoResp[itemId] = (res.data || []).filter((c: any) => !existentes.has(c.id));
          this.cdr.markForCheck();
        }
      })
      .catch(() => { /* silencioso */ });
  }

  salvarNovosCompartilhados(eqp: any): void {
    const itemId = this.getReqItemId(eqp);
    if (!itemId) return;
    if (!eqp || !Array.isArray(eqp.novosCoRespSelecionados) || eqp.novosCoRespSelecionados.length === 0) {
      this.util.exibirMensagemToast('Selecione ao menos um co-responsável.', 3000);
      return;
    }
    const selecionados = eqp.novosCoRespSelecionados as any[];

    this.util.aguardar(true);
    const promessas = selecionados.map((col: any) => {
      const vinculo = { colaboradorId: col.id, tipoAcesso: 'usuario_compartilhado', observacao: eqp.observacaoEntrega || '' };
      return this.api.adicionarCompartilhadoItem(itemId, vinculo, this.session.token);
    });

    Promise.all(promessas)
      .then(() => {
        this.util.aguardar(false);
        eqp.novosCoRespSelecionados = [];
        this.buscarCoResponsaveis(itemId, '');
        this.carregarCompartilhados(itemId);
        this.util.exibirMensagemToast('Co-responsáveis adicionados.', 4000);
      })
      .catch(() => {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Falha ao adicionar co-responsáveis.', 5000);
      });
  }

  encerrarCompartilhado(vinculo: any, itemId: number): void {
    if (!vinculo?.id || !itemId) return;
    this.util.aguardar(true);
    this.api.encerrarCompartilhadoItem(vinculo.id, this.session.token)
      .then((res: any) => {
        this.util.aguardar(false);
        if (res?.status === 200) {
          this.util.exibirMensagemToast('Co-responsável encerrado.', 4000);
          this.carregarCompartilhados(itemId);
        } else {
          this.util.exibirMensagemToast('Falha ao encerrar.', 4000);
        }
      })
      .catch(() => {
        this.util.aguardar(false);
        this.util.exibirMensagemToast('Erro ao encerrar.', 4000);
      });
  }
  abrirModalCompartilhar(eqp: any, row?: any): void {
    if (!eqp || this.isLinhaItem(eqp) || eqp.isByod === true) return;
    const itemId = this.getReqItemId(eqp);
    if (!itemId) return;
    this.dialog.open(ModalCompartilharItemComponent, {
      width: '600px',
      data: { eqp, itemId, colaboradorContextoId: row?.colaboradorId, colaboradorContextoNome: row?.colaborador }
    }).afterClosed().subscribe((houveAlteracao: boolean) => {
      // Se houve alteração (adição/encerramento), recarregar os compartilhados do item
      if (houveAlteracao) {
        this.carregarCompartilhadosAtivosParaItem(eqp);
      }
      this.cdr.markForCheck();
    });
  }

  /**
   * Limpar todos os filtros e parâmetros de busca
   */
  limparFiltros() {
    this.consulta.setValue('');
    
    // Limpar parâmetro de rota se houver
    this.parametro = null;
    
    // Limpar caches e estados
    this.expandedColaboradores.clear();
    this.carregandoRequisicoes.clear();
    this.cacheRequisicoes.clear();
    this.cacheCarregamento.clear();
    this.compartilhamentoAberto.clear();
    this.mapaCompartilhados = {};
    this.mapaOpcoesCoResp = {};
    this.debugLoggedReqIds.clear();
    
    // Navegar para rota limpa (sem query params)
    this.route.navigate(['movimentacoes/entregas-devolucoes'], {
      queryParams: {}
    });
    
    // Notificar usuário
    this.util.exibirMensagemToast('Filtros limpos! Recarregando dados...', 3000);
    
    // Recarregar dados
    this.ngOnInit();
  }
  // ===============================================================================
}
