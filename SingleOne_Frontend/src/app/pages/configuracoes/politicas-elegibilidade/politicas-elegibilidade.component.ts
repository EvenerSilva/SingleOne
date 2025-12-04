import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ElegibilidadeApiService } from 'src/app/api/elegibilidade/elegibilidade-api.service';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-politicas-elegibilidade',
  templateUrl: './politicas-elegibilidade.component.html',
  styleUrls: ['./politicas-elegibilidade.component.scss']
})
export class PoliticasElegibilidadeComponent implements OnInit, AfterViewInit {

  private session: any = {};
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;

  public tiposColaborador: any[] = [];
  public tiposEquipamento: any[] = [];
  public showModal = false;
  public showDeleteModal = false;

  // Política sendo editada/criada
  public politicaAtual: any = this.novaPolitica();

  // Política a ser excluída
  public politicaExcluir: any = null;

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
    private elegibilidadeApi: ElegibilidadeApiService,
    private configApi: ConfiguracoesApiService,
    private route: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    
    // Inicializar tipos de colaborador com valores padrão IMEDIATAMENTE
    this.carregarTiposPadrao();
    
    // Inicializar com lista vazia
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    this.carregarDadosIniciais();
  }

  ngAfterViewInit() {
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  private configurarPaginador() {
    if (!this.paginator || !this.dataSource) {
      return;
    }
    
    this.dataSource.paginator = this.paginator;
    this.paginator.pageSize = 10;
    this.paginator.pageIndex = 0;
    
    this.paginator.page.subscribe(() => {
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    });
  }

  async carregarDadosIniciais(): Promise<void> {
    await this.carregarTiposColaborador();
    await this.carregarTiposEquipamento();
    await this.listar();
  }

  async carregarTiposColaborador(): Promise<void> {
    try {
      const result = await this.elegibilidadeApi.listarTiposColaborador(this.session.token);
      if (result && result.status === 200 && result.data && Array.isArray(result.data)) {
        // Normalizar propriedades: Codigo -> codigo, Descricao -> descricao
        this.tiposColaborador = result.data.map((t: any) => ({
          codigo: t.Codigo || t.codigo,
          descricao: t.Descricao || t.descricao
        }));
      } else {
        console.warn('[ELEGIBILIDADE] Resposta inválida da API, usando tipos padrão');
        this.carregarTiposPadrao();
      }
    } catch (error) {
      console.error('[ELEGIBILIDADE] Erro ao carregar tipos de colaborador:', error);
      this.carregarTiposPadrao();
    }
  }

  private carregarTiposPadrao(): void {
    // Tipos padrão do sistema (F = Funcionário, T = Terceirizado, C = Consultor)
    this.tiposColaborador = [
      { codigo: 'F', descricao: 'Funcionário' },
      { codigo: 'T', descricao: 'Terceirizado' },
      { codigo: 'C', descricao: 'Consultor' }
    ];
  }

  async carregarTiposEquipamento(): Promise<void> {
    try {
      const result = await this.configApi.listarTiposRecursos('null', this.cliente, this.session.token);
      if (result && result.data && Array.isArray(result.data)) {
        this.tiposEquipamento = result.data.filter((t: any) => t.ativo);
      }
    } catch (error) {
      console.error('Erro ao carregar tipos de recurso:', error);
    }
  }

  listar() {
    this.util.aguardar(true);
    this.elegibilidadeApi.listarPoliticas(this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      } else {
        this.dataSource = new MatTableDataSource<any>(res.data || []);
        this.configurarPaginador();
      }
    });
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.elegibilidadeApi.listarPoliticas(this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        } else {
          const politicas = res.data || [];
          const filtradas = politicas.filter(politica => 
            politica.tipoColaboradorDescricao?.toLowerCase().includes(valor.toLowerCase()) ||
            politica.tipoEquipamentoDescricao?.toLowerCase().includes(valor.toLowerCase()) ||
            politica.cargo?.toLowerCase().includes(valor.toLowerCase()) ||
            (politica.usarPadrao && 'padrão'.includes(valor.toLowerCase())) ||
            (!politica.usarPadrao && 'exato'.includes(valor.toLowerCase()))
          );
          this.dataSource = new MatTableDataSource(filtradas);
          this.configurarPaginador();
        }
      });
    } else {
      this.listar();
    }
  }

  limparBusca(): void {
    this.consulta.setValue('');
    this.listar();
  }

  novaPolitica(): any {
    return {
      id: 0,
      cliente: this.cliente || this.session?.usuario?.cliente || 0,
      tipoColaborador: '',
      cargo: '',
      usarPadrao: true, // Default: usa padrão (LIKE '%cargo%')
      tipoEquipamentoId: null,
      permiteAcesso: true,
      quantidadeMaxima: null,
      observacoes: '',
      ativo: true,
      usuarioCadastro: this.session?.usuario?.id || 0
    };
  }

  async abrirModalNovo(): Promise<void> {
    if (!this.tiposColaborador || this.tiposColaborador.length === 0) {
      this.carregarTiposPadrao();
    }
    
    // Tentar atualizar com dados da API em background
    try {
      await this.carregarTiposColaborador();
    } catch (error) {
      console.error('[ELEGIBILIDADE] Erro ao carregar tipos da API:', error);
    }
    
    // Garantir que recursos estejam carregados
    if (!this.tiposEquipamento || this.tiposEquipamento.length === 0) {
      await this.carregarTiposEquipamento();
    }
    
    this.politicaAtual = this.novaPolitica();
    setTimeout(() => {
      this.showModal = true;
      this.cdr.detectChanges();
    }, 100);
  }

  async abrirModalEditar(politica: any): Promise<void> {
    if (!this.tiposColaborador || this.tiposColaborador.length === 0) {
      await this.carregarTiposColaborador();
    }
    
    // Garantir que recursos estejam carregados
    if (!this.tiposEquipamento || this.tiposEquipamento.length === 0) {
      await this.carregarTiposEquipamento();
    }
    
    // Garantir que todos os campos estejam presentes, incluindo usarPadrao
    this.politicaAtual = {
      id: politica.id,
      cliente: politica.cliente,
      tipoColaborador: politica.tipoColaborador,
      cargo: politica.cargo || '',
      usarPadrao: politica.usarPadrao !== undefined ? politica.usarPadrao : true,
      tipoEquipamentoId: politica.tipoEquipamentoId,
      permiteAcesso: politica.permiteAcesso,
      quantidadeMaxima: politica.quantidadeMaxima,
      observacoes: politica.observacoes || '',
      ativo: politica.ativo,
      usuarioCadastro: politica.usuarioCadastro || this.session?.usuario?.id
    };
    setTimeout(() => {
      this.showModal = true;
      this.cdr.detectChanges();
    }, 100);
  }

  fecharModal(): void {
    this.showModal = false;
    this.politicaAtual = this.novaPolitica();
  }

  async salvar(): Promise<void> {
    if (!this.politicaAtual.tipoColaborador) {
      this.util.exibirMensagemToast('Selecione o tipo de colaborador', 3000);
      return;
    }

    if (!this.politicaAtual.tipoEquipamentoId) {
      this.util.exibirMensagemToast('Selecione o tipo de recurso', 3000);
      return;
    }

    if (!this.politicaAtual.permiteAcesso) {
      this.politicaAtual.quantidadeMaxima = null;
    }

    // Se não há cargo, garantir que usarPadrao seja true (padrão)
    if (!this.politicaAtual.cargo || this.politicaAtual.cargo.trim() === '') {
      this.politicaAtual.usarPadrao = true;
      this.politicaAtual.cargo = null; // Enviar null ao invés de string vazia
    }

    // Garantir que usarPadrao tenha um valor booleano válido
    this.politicaAtual.usarPadrao = this.politicaAtual.usarPadrao === true || this.politicaAtual.usarPadrao === 'true';
    this.util.aguardar(true);
    try {
      const result = await this.elegibilidadeApi.salvarPolitica(this.politicaAtual, this.session.token);

      this.util.aguardar(false);
      if (result.status === 200) {
        this.util.exibirMensagemToast(
          this.politicaAtual.id === 0 ? 'Política criada com sucesso!' : 'Política atualizada com sucesso!',
          5000
        );
        this.fecharModal();
        this.listar();
      } else {
        this.util.exibirMensagemToast(result.mensagem || 'Erro ao salvar política', 3000);
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('Erro ao salvar política:', error);
      this.util.exibirFalhaComunicacao();
    }
  }

  confirmarExclusao(politica: any): void {
    this.politicaExcluir = politica;
    this.showDeleteModal = true;
  }

  fecharModalExcluir(): void {
    this.showDeleteModal = false;
    this.politicaExcluir = null;
  }

  async excluir(): Promise<void> {
    if (!this.politicaExcluir) return;

    this.util.aguardar(true);
    try {
      const result = await this.elegibilidadeApi.excluirPolitica(this.politicaExcluir.id, this.session.token);

      this.util.aguardar(false);
      if (result.status === 200) {
        this.util.exibirMensagemToast('Política excluída com sucesso!', 5000);
        this.fecharModalExcluir();
        this.listar();
      } else {
        this.util.exibirMensagemToast(result.mensagem || 'Erro ao excluir política', 3000);
      }
    } catch (error) {
      this.util.aguardar(false);
      console.error('Erro ao excluir política:', error);
      this.util.exibirFalhaComunicacao();
    }
  }

  onPermiteAcessoChange(): void {
    // Se não permite acesso, limpar quantidade máxima
    if (!this.politicaAtual.permiteAcesso) {
      this.politicaAtual.quantidadeMaxima = null;
    }
  }

  getTipoBuscaCargo(politica: any): string {
    if (!politica.cargo) return '';
    return politica.usarPadrao ? 'Padrão (contém)' : 'Exato';
  }

  getTipoBuscaClass(politica: any): string {
    if (!politica.cargo) return '';
    return politica.usarPadrao ? 'tipo-padrao' : 'tipo-exato';
  }

  // TrackBy functions para melhor performance e renderização
  trackByTipo(index: number, tipo: any): any {
    return tipo?.codigo || index;
  }

  trackByEquipamento(index: number, tipo: any): any {
    return tipo?.id || index;
  }

  // Getters para garantir que os arrays sempre existam
  get tiposColaboradorDisponiveis(): any[] {
    return this.tiposColaborador || [];
  }

  get tiposEquipamentoDisponiveis(): any[] {
    return this.tiposEquipamento || [];
  }
}

