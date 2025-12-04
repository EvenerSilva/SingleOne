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
  selector: 'app-empresas',
  templateUrl: './empresas.component.html',
  styleUrls: ['./empresas.component.scss']
})
export class EmpresasComponent implements OnInit, AfterViewInit {

  private session: any = {};
  public colunas = ['nome', 'cnpj', 'localizacao', 'ativo', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  
  // Cache de localidades para exibição
  private localidadesCache: any[] = [];

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
    if (!this.session || !this.session.usuario) {
      this.util.exibirMensagemToast('Sessão inválida. Faça login novamente.', 5000);
      return;
    }
    
    this.cliente = this.session.usuario.cliente;
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    
    // Carregar localidades no cache
    this.carregarLocalidadesCache();
    
    // Testar conectividade antes de listar
    this.testarConectividade();
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

  listarEmpresas() {
    this.util.aguardar(true);
    
    this.api.listarEmpresas("null", this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      } else {
        const empresas = res.data || [];
        
        // Carregar totalizadores para cada empresa
        this.carregarTotalizadoresEmpresas(empresas);
      }
    }).catch(error => {
      this.util.aguardar(false);
      
      // Verificar se é erro 500 (problema no backend)
      if (error.response?.status === 500) {
        this.util.exibirMensagemToast('Erro interno do servidor. Verifique o backend.', 5000);
      } else {
        this.util.exibirFalhaComunicacao();
      }
    });
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarEmpresas(valor, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        } else {
          const empresas = res.data || [];
          this.carregarTotalizadoresEmpresas(empresas);
        }
      }).catch(error => {
        this.util.aguardar(false);
        
        // Verificar se é erro 500 (problema no backend)
        if (error.response?.status === 500) {
          this.util.exibirMensagemToast('Erro interno do servidor na busca. Verifique o backend.', 5000);
        } else {
          this.util.exibirFalhaComunicacao();
        }
      });
    } else {
      this.listarEmpresas();
    }
  }

  editar(obj) {
    this.route.navigate(['/empresa', btoa(JSON.stringify(obj))]);
  }

  excluir(obj) {
    if (confirm('Deseja realmente excluir a empresa ' + obj.nome + '?')) {
      this.util.aguardar(true);
      this.api.excluirEmpresa(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200) {
          this.util.exibirFalhaComunicacao();
        } else {
          this.util.exibirMensagemToast(res.data.Mensagem || 'Empresa excluída com sucesso!', 5000);
          this.listarEmpresas();
        }
      })
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listarEmpresas();
  }
  
  // 🔧 MÉTODO PARA CARREGAR TOTALIZADORES DAS EMPRESAS
  private carregarTotalizadoresEmpresas(empresas: any[]) {
    empresas.forEach(empresa => {
      // Usar o valor calculado pelo backend
      empresa.totalFiliais = empresa.totalFiliais || empresa.TotalFiliais || 0;
    });
    
    // Configurar dataSource com os dados enriquecidos
    this.dataSource = new MatTableDataSource<any>(empresas);
    this.configurarPaginador();
  }
  
  // Método para formatar a localização da empresa
  getLocalizacaoFormatada(empresa: any): string {
    if (!empresa) return 'Não informado';
    
    // Se tiver localidade_id, buscar informações da localidade
    if (empresa.localidadeId || empresa.localidade_id) {
      const localidadeId = empresa.localidadeId || empresa.localidade_id;
      const localidade = this.localidadesCache.find(loc => loc.id === localidadeId);
      
      if (localidade) {
        // Usar o método de formatação que já criamos
        return this.formatarLocalidade(localidade);
      } else {
        return 'Localidade não encontrada';
      }
    }
    
    return 'Não informado';
  }

  // Método para formatar a data de criação da empresa
  getDataCriacaoFormatada(empresa: any): string {
    if (!empresa) return 'Não informado';
    
    // Verificar se tem created_at ou createdAt
    const dataCriacao = empresa.created_at || empresa.createdAt;
    
    if (dataCriacao) {
      try {
        const data = new Date(dataCriacao);
        if (!isNaN(data.getTime())) {
          return data.toLocaleDateString('pt-BR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
          });
        }
      } catch (error) {
        console.warn('[EMPRESAS] Erro ao formatar data de criação:', error);
      }
    }
    
    return 'Não informado';
  }

  // Método para carregar localidades no cache
  private carregarLocalidadesCache() {
    this.api.listarLocalidades(this.cliente, this.session.token).then(res => {
      if (res.status === 200 && res.data) {
        // Filtrar apenas localizações ativas e remover duplicatas por ID e descrição
        const localizacoesAtivas = res.data.filter((loc: any) => loc.ativo);
        
        // Remover duplicatas usando Map - primeiro por ID, depois por descrição
        const localizacoesUnicasPorId = new Map();
        localizacoesAtivas.forEach((loc: any) => {
          if (!localizacoesUnicasPorId.has(loc.id)) {
            localizacoesUnicasPorId.set(loc.id, loc);
          }
        });
        
        // Remover duplicatas por descrição (caso haja IDs diferentes com mesma descrição)
        const localizacoesUnicasPorDescricao = new Map();
        Array.from(localizacoesUnicasPorId.values()).forEach((loc: any) => {
          const descricaoNormalizada = (loc.descricao || '').trim().toLowerCase();
          if (!localizacoesUnicasPorDescricao.has(descricaoNormalizada)) {
            localizacoesUnicasPorDescricao.set(descricaoNormalizada, loc);
          }
        });
        
        // Converter Map para Array e ordenar por descrição
        this.localidadesCache = Array.from(localizacoesUnicasPorDescricao.values())
          .sort((a: any, b: any) => {
            const descA = (a.descricao || '').toLowerCase();
            const descB = (b.descricao || '').toLowerCase();
            return descA.localeCompare(descB);
          });
      } else {
        this.localidadesCache = [];
      }
    }).catch(err => {
      console.error('[EMPRESAS] ❌ Erro ao carregar localidades:', err);
      this.localidadesCache = [];
    });
  }

  // Método para formatar a exibição da localidade
  private formatarLocalidade(localidade: any): string {
    if (!localidade) return 'Não informado';
    
    let resultado = localidade.descricao || '';
    
    // Adicionar cidade apenas se for válida (não for número ou coordenada)
    if (localidade.cidade && 
        localidade.cidade.trim() !== '' && 
        !this.isNumeroOuCoordenada(localidade.cidade)) {
      resultado += ` - ${localidade.cidade}`;
    }
    
    // Adicionar estado apenas se for válido (não for número ou coordenada)
    if (localidade.estado && 
        localidade.estado.trim() !== '' && 
        !this.isNumeroOuCoordenada(localidade.estado)) {
      resultado += resultado.includes(' - ') ? `, ${localidade.estado}` : ` - ${localidade.estado}`;
    }
    
    return resultado || 'Não informado';
  }

  // Método para verificar se um valor é número ou coordenada
  private isNumeroOuCoordenada(valor: string): boolean {
    if (!valor) return false;
    
    // Verificar se é apenas números, vírgulas e pontos
    const numeroRegex = /^[\d.,]+$/;
    if (numeroRegex.test(valor)) {
      return true;
    }
    
    // Verificar se é coordenada (formato 90,9 ou 90.9)
    const coordenadaRegex = /^\d+[,.]\d+$/;
    if (coordenadaRegex.test(valor)) {
      return true;
    }
    
    return false;
  }

  // 🔧 MÉTODO PARA TESTAR CONECTIVIDADE COM O BACKEND
  private testarConectividade() {
    this.api.listarEmpresas("null", this.cliente, this.session.token).then(res => {
      if (res.status === 200 || res.status === 204) {
        this.dataSource = new MatTableDataSource<any>(res.data || []);
        this.configurarPaginador();
      } else {
        this.util.exibirMensagemToast('Resposta inesperada do servidor', 5000);
      }
    }).catch(error => {
      if (error.response?.status === 500) {
        this.util.exibirMensagemToast('Erro interno do servidor (500). Verifique o backend.', 5000);
      } else if (error.code === 'ECONNREFUSED' || error.message?.includes('Network Error')) {
        this.util.exibirMensagemToast('Backend não está acessível. Verifique se está rodando.', 5000);
      } else {
        this.util.exibirMensagemToast('Erro de comunicação com o servidor', 5000);
      }
    });
  }
}
