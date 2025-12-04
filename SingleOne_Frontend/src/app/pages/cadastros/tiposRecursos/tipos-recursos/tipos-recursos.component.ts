import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-tipos-recursos',
  templateUrl: './tipos-recursos.component.html',
  styleUrls: ['./tipos-recursos.component.scss']
})
export class TiposRecursosComponent implements OnInit, AfterViewInit {

  private session:any = {};
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public colunas: string[] = ['tipo', 'acao'];

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
    private api: ConfiguracoesApiService, 
    private route: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.cliente = this.session.usuario.cliente;
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    // Carregar dados reais da API
    this.listarTiposEquipamentos();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  listarTiposEquipamentos() {
    this.util.aguardar(true);
    this.api.listarTiposRecursos("null", this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        if (!res.data || !Array.isArray(res.data)) {
          console.error('[TIPOS-RECURSOS] ❌ Dados inválidos recebidos da API:', res.data);
          this.dataSource = new MatTableDataSource<any>([]);
          this.configurarPaginador();
          return;
        }
        
        // Remover duplicatas por ID antes de filtrar
        const tiposSemDuplicatas = this.removerDuplicatasPorId(res.data);
        
        // Filtrar tipos que não devem ser exibidos na grid
        const tiposFiltrados = this.filtrarTiposExcluidos(tiposSemDuplicatas);
        
        this.dataSource = new MatTableDataSource<any>(tiposFiltrados);
        
        // Configurar o paginador
        this.configurarPaginador();
      }
    }).catch(error => {
      console.error('[TIPOS-RECURSOS] ❌ Erro na listagem:', error);
      this.util.aguardar(false);
      this.dataSource = new MatTableDataSource<any>([]);
      this.configurarPaginador();
    });
  }

  // 🔧 MÉTODO AUXILIAR PARA CONFIGURAR PAGINADOR
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
      
      // FORÇAR RECÁLCULO DOS DADOS PAGINADOS
      this.forcarAtualizacaoDados();
    });
  }

  // 🔄 MÉTODO PARA FORÇAR ATUALIZAÇÃO DOS DADOS
  private forcarAtualizacaoDados() {
    // Forçar detecção de mudanças
    this.cdr.detectChanges();
    this.cdr.markForCheck();
    
    // Aguardar um ciclo e forçar novamente
    setTimeout(() => {
      this.cdr.detectChanges();
      this.cdr.markForCheck();
    }, 0);
  }

// MÉTODOS AUXILIARES
  private removerDuplicatasPorId(dados: any[]): any[] {
    const idsUnicos = new Set();
    return dados.filter(item => {
      if (idsUnicos.has(item.id)) {
        return false;
      }
      idsUnicos.add(item.id);
      return true;
    });
  }

  private filtrarTiposExcluidos(dados: any[]): any[] {
    return dados.filter(item => {
      // Filtrar por ativo
      if (item.ativo === false) {
        return false;
      }
      
      // Filtrar tipos que não devem ser exibidos (configurações internas)
      const descricao = item.descricao?.toLowerCase() || '';
      const tiposExcluidos = [
        'linha telefônica',
        'linha telefonica',
        'linha telefónica',
        'linha telefonica'
      ];
      
      return !tiposExcluidos.includes(descricao);
    });
  }

  buscar(valor) {
    if (!valor || valor.trim() === '') {
      this.listarTiposEquipamentos();
      return;
    }
    
    const termo = valor.toLowerCase().trim();
    const dadosFiltrados = this.dataSource.data.filter(item => {
      // Verificar se as propriedades existem antes de usar toLowerCase
      const descricao = item.descricao?.toLowerCase() || '';
      
      return descricao.includes(termo);
    });
    
    this.dataSource = new MatTableDataSource<any>(dadosFiltrados);
    this.configurarPaginador();
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listarTiposEquipamentos();
  }

  getTipoIcon(descricao: string): string {
    if (descricao.toLowerCase().includes('telefone')) return 'cil-phone';
    if (descricao.toLowerCase().includes('computador')) return 'cil-laptop';
    if (descricao.toLowerCase().includes('impressora')) return 'cil-print';
    if (descricao.toLowerCase().includes('rede')) return 'cil-network';
    return 'cil-devices';
  }

  // PROPRIEDADES E MÉTODOS DO FORMULÁRIO
  public mostrarFormulario = false;
  public tipoEditando: any = null;
  public modoFormulario: 'criar' | 'editar' = 'criar';

  editar(obj) {
    this.tipoEditando = { ...obj };
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  novoTipo() {
    this.tipoEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  onTipoSalvo(tipo: any) {
    this.mostrarFormulario = false;
    this.listarTiposEquipamentos();
  }

  onCancelado() {
    this.mostrarFormulario = false;
  }
}
