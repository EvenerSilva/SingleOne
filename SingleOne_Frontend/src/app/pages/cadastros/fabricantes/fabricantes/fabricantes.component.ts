import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { CategoriasApiService } from 'src/app/api/categorias/categorias-api.service';
import { UtilService } from 'src/app/util/util.service';

@Component({
  selector: 'app-fabricantes',
  templateUrl: './fabricantes.component.html',
  styleUrls: ['./fabricantes.component.scss']
})
export class FabricantesComponent implements OnInit, AfterViewInit {

  private session:any = {};
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any> = new MatTableDataSource<any>([]);
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public colunas: string[] = ['categoria', 'tipo', 'fabricante', 'acao'];

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
    private categoriasApi: CategoriasApiService,
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
    this.listarFabricantes();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  listarFabricantes() {
    this.util.aguardar(true);
    this.api.listarFabricantes("null", this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        // Enriquecer dados com informações de categoria
        if (res.data && Array.isArray(res.data)) {
          this.enriquecerDadosComCategoria(res.data);
        }
        
        this.dataSource = new MatTableDataSource<any>(res.data);
        this.configurarPaginador();
      }
    }).catch(error => {
      console.error('[FABRICANTES] Erro na listagem:', error);
      this.util.aguardar(false);
    });
  }

  /**
   * Enriquece os dados dos fabricantes com informações de categoria
   */
  private async enriquecerDadosComCategoria(fabricantes: any[]) {
    try {
      // Buscar todas as categorias
      const categoriasResponse = await this.categoriasApi.listarCategorias();
      if (categoriasResponse && categoriasResponse.data) {
        let categorias: any[] = [];
        
        // Verificar estrutura da resposta
        if (categoriasResponse.data.dados) {
          categorias = categoriasResponse.data.dados;
        } else if (Array.isArray(categoriasResponse.data)) {
          categorias = categoriasResponse.data;
        }
        
        // Para cada fabricante, adicionar informação da categoria
        fabricantes.forEach(fabricante => {
          if (fabricante.tipoequipamentoNavigation?.categoriaId) {
            const categoria = categorias.find((cat: any) => cat.id === fabricante.tipoequipamentoNavigation.categoriaId);
            if (categoria) {
              fabricante.categoriaInfo = {
                id: categoria.id,
                nome: categoria.nome
              };
            }
          }
        });
      }
    } catch (error) {
      console.error('[FABRICANTES] Erro ao enriquecer dados:', error);
    }
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarFabricantes(valor, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          // Enriquecer dados com informações de categoria antes de atualizar o DataSource
          if (res.data && Array.isArray(res.data)) {
            this.enriquecerDadosComCategoria(res.data);
          }
          
          this.dataSource = new MatTableDataSource(res.data);
          this.configurarPaginador();
        }
      })
    }
    else {
      this.listarFabricantes();
    }
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
    });
  }

  // 🆕 Propriedades para o modal
  public mostrarFormulario = false;
  public fabricanteEditando: any = null;
  public modoFormulario: 'criar' | 'editar' = 'criar';

  editar(obj) {
    this.fabricanteEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  // 🆕 Métodos para controlar o modal
  novoFabricante() {
    this.fabricanteEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  onFabricanteSalvo(fabricante: any) {
    this.mostrarFormulario = false;
    this.fabricanteEditando = null;
    this.listarFabricantes(); // Recarregar lista
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.fabricanteEditando = null;
  }

  /**
   * Retorna o ícone apropriado baseado na descrição do tipo de recurso
   * Sistema inteligente que mapeia por categorias sem impactar performance
   */
  getTipoIcon(descricao: string): string {
    if (!descricao) return 'cil-laptop'; // Ícone padrão
    
    const descricaoLower = descricao.toLowerCase();
    
    // 🖥️ COMPUTADORES E INFORMÁTICA
    if (descricaoLower.includes('computador') || descricaoLower.includes('pc') || 
        descricaoLower.includes('notebook') || descricaoLower.includes('laptop') ||
        descricaoLower.includes('desktop') || descricaoLower.includes('servidor')) {
      return 'cil-laptop';
    }
    
    // 📱 DISPOSITIVOS MÓVEIS
    if (descricaoLower.includes('celular') || descricaoLower.includes('smartphone') ||
        descricaoLower.includes('tablet') || descricaoLower.includes('mobile')) {
      return 'cil-phone';
    }
    
    // 🖨️ IMPRESSORAS E PERIFÉRICOS
    if (descricaoLower.includes('impressora') || descricaoLower.includes('scanner') ||
        descricaoLower.includes('multifuncional') || descricaoLower.includes('periferico')) {
      return 'cil-print';
    }
    
    // 🌐 REDES E COMUNICAÇÃO
    if (descricaoLower.includes('roteador') || descricaoLower.includes('switch') ||
        descricaoLower.includes('modem') || descricaoLower.includes('rede') ||
        descricaoLower.includes('wifi') || descricaoLower.includes('cabo')) {
      return 'cil-router';
    }
    
    // 🔌 EQUIPAMENTOS ELÉTRICOS
    if (descricaoLower.includes('monitor') || descricaoLower.includes('tela') ||
        descricaoLower.includes('teclado') || descricaoLower.includes('mouse') ||
        descricaoLower.includes('fonte') || descricaoLower.includes('bateria')) {
      return 'cil-monitor';
    }
    
    // 🏭 EQUIPAMENTOS INDUSTRIAIS
    if (descricaoLower.includes('maquina') || descricaoLower.includes('equipamento') ||
        descricaoLower.includes('ferramenta') || descricaoLower.includes('industrial')) {
      return 'cil-cog';
    }
    
    // 📺 AUDIO/VIDEO
    if (descricaoLower.includes('audio') || descricaoLower.includes('video') ||
        descricaoLower.includes('som') || descricaoLower.includes('tv') ||
        descricaoLower.includes('projetor') || descricaoLower.includes('camera')) {
      return 'cil-video';
    }
    
    // 🔒 SEGURANÇA
    if (descricaoLower.includes('camera') || descricaoLower.includes('seguranca') ||
        descricaoLower.includes('alarme') || descricaoLower.includes('controle')) {
      return 'cil-lock-locked';
    }
    
    // 📡 TELECOMUNICAÇÕES
    if (descricaoLower.includes('telefone') || descricaoLower.includes('fax') ||
        descricaoLower.includes('radio') || descricaoLower.includes('antena')) {
      return 'cil-phone';
    }
    
    // 🚗 VEÍCULOS E TRANSPORTE
    if (descricaoLower.includes('veiculo') || descricaoLower.includes('carro') ||
        descricaoLower.includes('moto') || descricaoLower.includes('transporte')) {
      return 'cil-car-alt';
    }
    
    // 🏥 EQUIPAMENTOS MÉDICOS
    if (descricaoLower.includes('medico') || descricaoLower.includes('hospital') ||
        descricaoLower.includes('laboratorio') || descricaoLower.includes('diagnostico')) {
      return 'cil-heart';
    }
    
    // 📊 EQUIPAMENTOS DE MEDIÇÃO
    if (descricaoLower.includes('medicao') || descricaoLower.includes('sensor') ||
        descricaoLower.includes('termometro') || descricaoLower.includes('balanca')) {
      return 'cil-chart';
    }
    
    // 🎯 ÍCONE PADRÃO PARA TIPOS NÃO CATEGORIZADOS
    return 'cil-laptop';
  }

  /**
   * Retorna o ícone padrão para categorias (mesmo padrão da tela de categorias)
   */
  getCategoriaIcon(nomeCategoria: string): string {
    return 'cil-tag'; // Ícone padrão igual à tela de categorias
  }

  // 🧹 MÉTODO PARA LIMPAR BUSCA
  limparBusca() {
    this.consulta.setValue('');
    this.listarFabricantes();
  }

  excluir(obj) {
    if(confirm('Deseja realmente excluir a fabricante ' + obj.descricao + '?')) {
      this.util.aguardar(true);
      this.api.excluirFabricante(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Fabricante excluida com sucesso!', 5000);
          this.listarFabricantes();
        }
      })
    }
  }

}
