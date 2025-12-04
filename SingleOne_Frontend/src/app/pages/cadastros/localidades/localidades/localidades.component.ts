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
  selector: 'app-localidades',
  templateUrl: './localidades.component.html',
  styleUrls: ['./localidades.component.scss']
})
export class LocalidadesComponent implements OnInit, AfterViewInit {

  private session:any = {};
  public colunas = ['descricao', 'estado', 'status', 'acao'];
  @ViewChild(MatPaginator, { static: true }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  
  // Propriedades para o modal
  public mostrarFormulario = false;
  public localidadeEditando: any = null;
  public modoFormulario: 'criar' | 'editar' = 'criar';
  
  // Mapeamento de IDs para nomes
  private estadosMap: { [key: number]: any } = {};
  private cidadesMap: { [key: number]: any } = {};

  // Getter para dados paginados
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.data) {
      return [];
    }
    
    const startIndex = this.paginator?.pageIndex * this.paginator?.pageSize || 0;
    const endIndex = startIndex + (this.paginator?.pageSize || 10);
    
    const dadosPaginados = this.dataSource.data.slice(startIndex, endIndex);
    
    // ✅ DEBUG: Log apenas quando há dados
    if (dadosPaginados.length > 0) {
    }
    
    return dadosPaginados;
  }

  constructor(
    private util: UtilService,
    private api: ConfiguracoesApiService,
    private route: Router,
    private cdr: ChangeDetectorRef
  ) { }

  // 🔧 MÉTODOS PARA MAPEAR IDs PARA NOMES
  private carregarEstadosECidades() {
    // Dados mockados para estados
    const estados = [
      { id: 1, sigla: 'SP', nome: 'São Paulo' },
      { id: 2, sigla: 'RJ', nome: 'Rio de Janeiro' },
      { id: 3, sigla: 'MG', nome: 'Minas Gerais' },
      { id: 4, sigla: 'PR', nome: 'Paraná' },
      { id: 5, sigla: 'RS', nome: 'Rio Grande do Sul' },
      { id: 6, sigla: 'SC', nome: 'Santa Catarina' },
      { id: 7, sigla: 'GO', nome: 'Goiás' },
      { id: 8, sigla: 'BA', nome: 'Bahia' },
      { id: 9, sigla: 'CE', nome: 'Ceará' },
      { id: 10, sigla: 'PE', nome: 'Pernambuco' }
    ];

    // Dados mockados para cidades
    const cidades = [
      // SP
      { id: 1, nome: 'São Paulo', estadoId: 1 },
      { id: 2, nome: 'Guarulhos', estadoId: 1 },
      { id: 3, nome: 'Campinas', estadoId: 1 },
      { id: 4, nome: 'Santo André', estadoId: 1 },
      { id: 5, nome: 'Osasco', estadoId: 1 },
      { id: 6, nome: 'Ribeirão Preto', estadoId: 1 },
      { id: 7, nome: 'Sorocaba', estadoId: 1 },
      { id: 8, nome: 'Mauá', estadoId: 1 },
      { id: 9, nome: 'São José dos Campos', estadoId: 1 },
      { id: 10, nome: 'Mogi das Cruzes', estadoId: 1 },
      // RJ
      { id: 11, nome: 'Rio de Janeiro', estadoId: 2 },
      { id: 12, nome: 'São Gonçalo', estadoId: 2 },
      { id: 13, nome: 'Duque de Caxias', estadoId: 2 },
      { id: 14, nome: 'Nova Iguaçu', estadoId: 2 },
      { id: 15, nome: 'Niterói', estadoId: 2 },
      { id: 16, nome: 'Belford Roxo', estadoId: 2 },
      { id: 17, nome: 'São João de Meriti', estadoId: 2 },
      { id: 18, nome: 'Petrópolis', estadoId: 2 },
      { id: 19, nome: 'Campos dos Goytacazes', estadoId: 2 },
      { id: 20, nome: 'Volta Redonda', estadoId: 2 },
      // MG
      { id: 21, nome: 'Belo Horizonte', estadoId: 3 },
      { id: 22, nome: 'Uberlândia', estadoId: 3 },
      { id: 23, nome: 'Contagem', estadoId: 3 },
      { id: 24, nome: 'Betim', estadoId: 3 },
      { id: 25, nome: 'Montes Claros', estadoId: 3 },
      { id: 26, nome: 'Ribeirão das Neves', estadoId: 3 },
      { id: 27, nome: 'Uberaba', estadoId: 3 },
      { id: 28, nome: 'Governador Valadares', estadoId: 3 },
      { id: 29, nome: 'Ipatinga', estadoId: 3 },
      { id: 30, nome: 'Sete Lagoas', estadoId: 3 },
      // PR
      { id: 31, nome: 'Curitiba', estadoId: 4 },
      { id: 32, nome: 'Londrina', estadoId: 4 },
      { id: 33, nome: 'Maringá', estadoId: 4 },
      { id: 34, nome: 'Ponta Grossa', estadoId: 4 },
      { id: 35, nome: 'Cascavel', estadoId: 4 },
      { id: 36, nome: 'São José dos Pinhais', estadoId: 4 },
      { id: 37, nome: 'Foz do Iguaçu', estadoId: 4 },
      { id: 38, nome: 'Colombo', estadoId: 4 },
      { id: 39, nome: 'Guarapuava', estadoId: 4 },
      { id: 40, nome: 'Paranaguá', estadoId: 4 },
      // RS
      { id: 41, nome: 'Porto Alegre', estadoId: 5 },
      { id: 42, nome: 'Caxias do Sul', estadoId: 5 },
      { id: 43, nome: 'Pelotas', estadoId: 5 },
      { id: 44, nome: 'Canoas', estadoId: 5 },
      { id: 45, nome: 'Santa Maria', estadoId: 5 },
      { id: 46, nome: 'Gravataí', estadoId: 5 },
      { id: 47, nome: 'Viamão', estadoId: 5 },
      { id: 48, nome: 'Novo Hamburgo', estadoId: 5 },
      { id: 49, nome: 'São Leopoldo', estadoId: 5 },
      { id: 50, nome: 'Rio Grande', estadoId: 5 },
      // SC
      { id: 51, nome: 'Florianópolis', estadoId: 6 },
      { id: 52, nome: 'Joinville', estadoId: 6 },
      { id: 53, nome: 'Blumenau', estadoId: 6 },
      { id: 54, nome: 'Criciúma', estadoId: 6 },
      { id: 55, nome: 'São José', estadoId: 6 },
      { id: 56, nome: 'Lages', estadoId: 6 },
      { id: 57, nome: 'Itajaí', estadoId: 6 },
      { id: 58, nome: 'Chapecó', estadoId: 6 },
      { id: 59, nome: 'Jaraguá do Sul', estadoId: 6 },
      { id: 60, nome: 'Palhoça', estadoId: 6 },
      // GO
      { id: 61, nome: 'Goiânia', estadoId: 7 },
      { id: 62, nome: 'Aparecida de Goiânia', estadoId: 7 },
      { id: 63, nome: 'Anápolis', estadoId: 7 },
      { id: 64, nome: 'Rio Verde', estadoId: 7 },
      { id: 65, nome: 'Luziânia', estadoId: 7 },
      { id: 66, nome: 'Águas Lindas de Goiás', estadoId: 7 },
      { id: 67, nome: 'Valparaíso de Goiás', estadoId: 7 },
      { id: 68, nome: 'Trindade', estadoId: 7 },
      { id: 69, nome: 'Formosa', estadoId: 7 },
      { id: 70, nome: 'Novo Gama', estadoId: 7 },
      // BA
      { id: 71, nome: 'Salvador', estadoId: 8 },
      { id: 72, nome: 'Feira de Santana', estadoId: 8 },
      { id: 73, nome: 'Vitória da Conquista', estadoId: 8 },
      { id: 74, nome: 'Camaçari', estadoId: 8 },
      { id: 75, nome: 'Itabuna', estadoId: 8 },
      { id: 76, nome: 'Juazeiro', estadoId: 8 },
      { id: 77, nome: 'Lauro de Freitas', estadoId: 8 },
      { id: 78, nome: 'Ilhéus', estadoId: 8 },
      { id: 79, nome: 'Jequié', estadoId: 8 },
      { id: 80, nome: 'Alagoinhas', estadoId: 8 },
      // CE
      { id: 81, nome: 'Fortaleza', estadoId: 9 },
      { id: 82, nome: 'Caucaia', estadoId: 9 },
      { id: 83, nome: 'Sobral', estadoId: 9 },
      { id: 84, nome: 'Juazeiro do Norte', estadoId: 9 },
      { id: 85, nome: 'Maracanaú', estadoId: 9 },
      { id: 86, nome: 'Crato', estadoId: 9 },
      { id: 87, nome: 'Iguatu', estadoId: 9 },
      { id: 88, nome: 'Quixadá', estadoId: 9 },
      { id: 89, nome: 'Pacatuba', estadoId: 9 },
      { id: 90, nome: 'Aquiraz', estadoId: 9 },
      { id: 266, nome: 'Eusébio', estadoId: 9 }, // ✅ ADICIONADO EUSÉBIO!
      // PE
      { id: 91, nome: 'Recife', estadoId: 10 },
      { id: 92, nome: 'Jaboatão dos Guararapes', estadoId: 10 },
      { id: 93, nome: 'Olinda', estadoId: 10 },
      { id: 94, nome: 'Caruaru', estadoId: 10 },
      { id: 95, nome: 'Petrolina', estadoId: 10 },
      { id: 96, nome: 'Paulista', estadoId: 10 },
      { id: 97, nome: 'Cabo de Santo Agostinho', estadoId: 10 },
      { id: 98, nome: 'Camaragibe', estadoId: 10 },
      { id: 99, nome: 'Garanhuns', estadoId: 10 },
      { id: 100, nome: 'Vitória de Santo Antão', estadoId: 10 }
    ];

    // Criar mapas para busca rápida
    this.estadosMap = {};
    estados.forEach(estado => {
      this.estadosMap[estado.id] = estado;
    });

    this.cidadesMap = {};
    cidades.forEach(cidade => {
      this.cidadesMap[cidade.id] = cidade;
    });
  }

  // 🔧 MÉTODOS PARA OBTER NOMES DOS IDs
  getNomeEstado(estadoId: number): string {
    if (!estadoId) return 'Não informado';
    const estado = this.estadosMap[estadoId];
    return estado ? `${estado.sigla} - ${estado.nome}` : `ID: ${estadoId}`;
  }

  getNomeCidade(cidadeId: number): string {
    if (!cidadeId) return 'Não informado';
    const cidade = this.cidadesMap[cidadeId];
    return cidade ? cidade.nome : `ID: ${cidadeId}`;
  }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    
    // Carregar mapeamento de estados e cidades
    this.carregarEstadosECidades();
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    this.listarLocalidades();
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

  listarLocalidades() {
    this.util.aguardar(true);
    this.api.listarLocalidades(this.session.usuario.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      } else {
        // ✅ CORREÇÃO: Filtrar apenas a localidade padrão (ID 1) - sempre oculta
        // Garantir que todas as outras localidades (ativas e inativas) sejam exibidas
        const localidadesFiltradas = (res.data || []).filter(localidade => localidade.id !== 1);
        this.dataSource = new MatTableDataSource<any>(localidadesFiltradas);
        this.configurarPaginador();
      }
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      // ✅ CORREÇÃO: Usar cliente correto e implementar busca local
      this.api.listarLocalidades(this.session.usuario.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        } else {
          // Filtrar localmente por descrição, cidade ou estado
          // E sempre ocultar a localidade padrão (ID 1)
          const dadosFiltrados = res.data
            .filter(localidade => localidade.id !== 1) // Sempre ocultar ID 1
            .filter((localidade: any) => {
              const termo = valor.toLowerCase();
              const descricao = localidade.descricao?.toLowerCase() || '';
              const cidadeNome = this.getNomeCidade(localidade.cidade)?.toLowerCase() || '';
              const estadoNome = this.getNomeEstado(localidade.estado)?.toLowerCase() || '';
              
              return descricao.includes(termo) || 
                     cidadeNome.includes(termo) || 
                     estadoNome.includes(termo);
            });
          this.dataSource = new MatTableDataSource(dadosFiltrados);
          this.configurarPaginador();
        }
      })
    } else {
      this.listarLocalidades();
    }
  }

  editar(obj) {
    this.localidadeEditando = obj;
    this.modoFormulario = 'editar';
    this.mostrarFormulario = true;
  }

  novaLocalidade() {
    this.localidadeEditando = null;
    this.modoFormulario = 'criar';
    this.mostrarFormulario = true;
  }

  onLocalidadeSalva(localidade: any) {
    this.mostrarFormulario = false;
    this.localidadeEditando = null;
    this.modoFormulario = 'criar';
    this.listarLocalidades();
  }

  onCancelado() {
    this.mostrarFormulario = false;
    this.localidadeEditando = null;
    this.modoFormulario = 'criar';
  }

  excluir(obj) {
    if (confirm('Deseja realmente excluir a localidade ' + obj.descricao + '?')) {
      this.util.aguardar(true);
      this.api.excluirLocalidade(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200) {
          this.util.exibirFalhaComunicacao();
        } else {
          this.util.exibirMensagemToast('Localidade excluida com sucesso!', 5000);
          this.listarLocalidades();
        }
      })
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listarLocalidades();
  }
}
