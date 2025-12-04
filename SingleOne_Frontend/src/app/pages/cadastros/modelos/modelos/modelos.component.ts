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
  selector: 'app-modelos',
  templateUrl: './modelos.component.html',
  styleUrls: ['./modelos.component.scss']
})
export class ModelosComponent implements OnInit, AfterViewInit {

  private session:any = {};
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  public mostrarApenasAtivos = false; // Mostrar todos os modelos (ativos e inativos) por padrão para permitir reativação
  public colunas: string[] = ['categoria', 'tipo', 'fabricante', 'modelo', 'status', 'acao'];

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
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.listarModelos();
  }

  ngAfterViewInit() {
    // Configurar o paginador após a view ser inicializada
    setTimeout(() => {
      if (this.dataSource && this.paginator) {
        this.configurarPaginador();
      }
    }, 100);
  }

  async listarModelos() {
    this.util.aguardar(true);
    try {
      const res = await this.api.listarModelos(null, this.cliente, this.session.token);
      this.util.aguardar(false);
      
      if (res.status === 200 || res.status === 204) {
        let dados = res.data;
        
        // Extrair dados se estiver em propriedade .dados
        if (dados && dados.dados) {
          dados = dados.dados;
        }
        
        if (dados && Array.isArray(dados)) {
          // Enriquecer dados com informações de categoria
          await this.enriquecerDadosComCategoria(dados);
          
          // Filtrar apenas modelos ativos se a opção estiver habilitada
          if (this.mostrarApenasAtivos) {
            dados = dados.filter((modelo: any) => modelo.ativo);
          }
          
          this.dataSource = new MatTableDataSource<any>(dados);
          this.configurarPaginador();
        } else {
          this.dataSource = new MatTableDataSource<any>([]);
          this.configurarPaginador();
        }
      } else {
        this.util.exibirFalhaComunicacao();
      }
    } catch (error) {
      console.error('[MODELOS] Erro na listagem:', error);
      this.util.aguardar(false);
    }
  }

  /**
   * Enriquece os dados dos modelos com informações de categoria
   */
  private async enriquecerDadosComCategoria(modelos: any[]) {
    try {
      const categoriasResponse = await this.categoriasApi.listarCategorias();
      if (categoriasResponse && categoriasResponse.data) {
        let categorias: any[] = [];
        
        // Verificar estrutura da resposta
        if (categoriasResponse.data.dados) {
          categorias = categoriasResponse.data.dados;
        } else if (Array.isArray(categoriasResponse.data)) {
          categorias = categoriasResponse.data;
        }
        modelos.forEach((modelo, index) => {
          if (modelo.fabricanteNavigation?.tipoequipamentoNavigation?.categoriaId) {
            const categoria = categorias.find((cat: any) => cat.id === modelo.fabricanteNavigation.tipoequipamentoNavigation.categoriaId);
            if (categoria) {
              modelo.categoriaInfo = {
                id: categoria.id,
                nome: categoria.nome
              };
            } else {
              modelo.categoriaInfo = {
                id: null,
                nome: 'Categoria não encontrada'
              };
            }
          } else {
            modelo.categoriaInfo = {
              id: null,
              nome: 'Estrutura inválida'
            };
          }
        });
      }
    } catch (error) {
      console.error('[MODELOS] Erro ao enriquecer dados:', error);
    }
  }

  async buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      try {
        // Tentar buscar com o valor exato
        const res = await this.api.listarModelos(valor, this.cliente, this.session.token);
        this.util.aguardar(false);
        if (res.status === 200 || res.status === 204) {
          let dados = res.data;
          
          // Se não retornou dados, tentar buscar todos e filtrar localmente
          if (!dados || (Array.isArray(dados) && dados.length === 0)) {
            const resTodos = await this.api.listarModelos(null, this.cliente, this.session.token);
            
            if (resTodos.status === 200 && resTodos.data) {
              let todosDados = resTodos.data;
              
              // Extrair dados se estiver em propriedade .dados
              if (todosDados.dados) {
                todosDados = todosDados.dados;
              }
              
              // Filtrar localmente pelo valor
              if (Array.isArray(todosDados)) {
                // Primeiro enriquecer todos os dados com categoria
                await this.enriquecerDadosComCategoria(todosDados);
                
                // Depois filtrar incluindo busca por categoria
                dados = todosDados.filter(modelo => 
                  modelo.descricao?.toLowerCase().includes(valor.toLowerCase()) ||
                  modelo.fabricanteNavigation?.descricao?.toLowerCase().includes(valor.toLowerCase()) ||
                  modelo.fabricanteNavigation?.tipoequipamentoNavigation?.descricao?.toLowerCase().includes(valor.toLowerCase()) ||
                  modelo.categoriaInfo?.nome?.toLowerCase().includes(valor.toLowerCase())
                );
                
                // Aplicar filtro de ativos/inativos se necessário
                if (this.mostrarApenasAtivos) {
                  dados = dados.filter((modelo: any) => modelo.ativo);
                }
              }
            }
          }
          
          // Enriquecer dados com informações de categoria
          if (dados && Array.isArray(dados) && dados.length > 0) {
            // Se os dados já foram enriquecidos no filtro local, não enriquecer novamente
            if (!dados[0].categoriaInfo) {
              await this.enriquecerDadosComCategoria(dados);
            }
            
            // Aplicar filtro de ativos/inativos se necessário
            if (this.mostrarApenasAtivos) {
              dados = dados.filter((modelo: any) => modelo.ativo);
            }
            
            this.dataSource = new MatTableDataSource(dados);
            this.configurarPaginador();
          } else {
            this.dataSource = new MatTableDataSource([]);
            this.configurarPaginador();
          }
        } else {
          this.util.exibirFalhaComunicacao();
        }
      } catch (error) {
        console.error('[MODELOS] Erro na busca:', error);
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      }
    } else {
      this.listarModelos();
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

  editar(obj) {
    this.route.navigate(['/modelo', btoa(JSON.stringify(obj))]);
  }

  excluir(obj) {
    const mensagem = `⚠️ <strong>ATENÇÃO:</strong> Deseja realmente EXCLUIR permanentemente o modelo <strong>"${obj.descricao}"</strong>?<br><br>🚫 Esta ação é <strong>IRREVERSÍVEL</strong> e removerá o modelo do sistema.<br><br>💡 <strong>RECOMENDAÇÃO:</strong> Use "Desativar" para manter o histórico de auditoria.<br><br>Confirma a exclusão permanente?`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        this.util.aguardar(true);
        this.api.excluirModelo(obj.id, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res.status === 200) {
            this.util.exibirMensagemToast('Modelo excluído permanentemente!', 5000);
            this.listarModelos();
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(error => {
          console.error('[MODELOS] Erro ao excluir modelo:', error);
          this.util.aguardar(false);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  desativarModelo(modelo: any) {
    const mensagem = `Tem certeza que deseja desativar o modelo <strong>"${modelo.descricao}"</strong>?<br><br>⚠️ <strong>ATENÇÃO:</strong> O modelo será marcado como inativo no sistema, mas os dados serão preservados para auditoria.`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        this.util.aguardar(true);
        const dadosModelo = {
          ...modelo,
          ativo: false
        };
        
        this.api.salvarModelo(dadosModelo, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res.status === 200) {
            this.util.exibirMensagemToast('Modelo desativado com sucesso!', 5000);
            this.listarModelos();
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(error => {
          this.util.aguardar(false);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  reativarModelo(modelo: any) {
    const mensagem = `Tem certeza que deseja reativar o modelo <strong>"${modelo.descricao}"</strong>?<br><br>🔄 O modelo voltará a estar ativo no sistema e poderá ser utilizado normalmente.`;
    
    this.util.exibirMensagemPopUp(mensagem, true).then(resultado => {
      if (resultado) {
        this.util.aguardar(true);
        const dadosModelo = {
          ...modelo,
          ativo: true
        };
        
        this.api.salvarModelo(dadosModelo, this.session.token).then(res => {
          this.util.aguardar(false);
          if (res.status === 200) {
            this.util.exibirMensagemToast('Modelo reativado com sucesso!', 5000);
            this.listarModelos();
          } else {
            this.util.exibirFalhaComunicacao();
          }
        }).catch(error => {
          this.util.aguardar(false);
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  alternarFiltroAtivos() {
    this.mostrarApenasAtivos = !this.mostrarApenasAtivos;
    this.listarModelos();
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
    this.listarModelos();
  }

}
