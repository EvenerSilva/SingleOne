import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { NotaFiscalApiService } from 'src/app/api/notasfiscais/nota-fiscal-api.service';
import { UtilService } from 'src/app/util/util.service';
import { MatDialog } from '@angular/material/dialog';
import { MessageboxComponent } from 'src/app/pages/messagebox/messagebox.component';

@Component({
  selector: 'app-notas-fiscais',
  templateUrl: './notas-fiscais.component.html',
  styleUrls: ['./notas-fiscais.component.scss']
})
export class NotasFiscaisComponent implements OnInit, AfterViewInit, OnDestroy {

  private session:any = {};
  public colunas = ['fornecedor', 'numero', 'emissao', 'valor', 'descricao', 'status', 'arquivo', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;
  
  // Modal de visualização
  public mostrarModalVisualizar = false;
  public notaFiscalSelecionada = 0;

  // Modal de progresso para cadastro de recursos
  public mostrarModalProgresso = false;
  public progresso = {
    percentual: 0,
    equipamentosProcessados: 0,
    totalEquipamentos: 0,
    tempoEstimado: 0,
    tempoDecorrido: 0,
    status: '',
    notaFiscal: ''
  };
  private intervaloProgresso: any;

  constructor(
    private util: UtilService, 
    private api: ConfiguracoesApiService,
    private apiNotaFiscal: NotaFiscalApiService,
    private route: Router,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    try {
      this.session = this.util.getSession('usuario');
      this.cliente = this.session.usuario.cliente;
      
      // Inicializar com lista vazia para evitar erros
      this.dataSource = new MatTableDataSource<any>([]);
      
      this.resultado = this.consulta.valueChanges.pipe(
        debounceTime(1000),
        tap(value => this.buscar(value))
      );
      this.resultado.subscribe();
      this.listar();
    } catch (error) {
      console.error('Erro no ngOnInit:', error);
      this.util.exibirFalhaComunicacao();
    }
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

  // Getter para dados paginados
  get dadosPaginados(): any[] {
    if (!this.dataSource || !this.dataSource.paginator) {
      return this.dataSource?.data || [];
    }
    
    const startIndex = this.dataSource.paginator.pageIndex * this.dataSource.paginator.pageSize;
    const endIndex = startIndex + this.dataSource.paginator.pageSize;
    return this.dataSource.data.slice(startIndex, endIndex);
  }

  listar() {
    this.util.aguardar(true);
    this.api.listarNotasFiscais("null", this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        console.error('Erro ao listar notas fiscais:', res);
        this.util.exibirFalhaComunicacao();
      }
      else {
        const dadosOrdenados = this.ordenarNotasFiscais(res.data || []);
        this.dataSource = new MatTableDataSource<any>(dadosOrdenados);
        this.configurarPaginador();
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro na requisição:', err);
      this.util.exibirFalhaComunicacao();
    });
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarNotasFiscais(valor, this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          console.error('Erro na busca:', res);
          this.util.exibirFalhaComunicacao();
        }
        else {
          const dadosOrdenados = this.ordenarNotasFiscais(res.data || []);
          this.dataSource = new MatTableDataSource(dadosOrdenados);
          this.configurarPaginador();
        }
      }).catch(err => {
        this.util.aguardar(false);
        console.error('Erro na busca:', err);
        this.util.exibirFalhaComunicacao();
      });
    }
    else {
      this.listar();
    }
  }

  limparBusca() {
    this.consulta.setValue('');
    this.listar();
  }

  /**
   * Ordena as notas fiscais de forma inteligente:
   * 1. Prioriza notas não processadas (gerouequipamento = false)
   * 2. Ordena da mais recente para a mais antiga
   * 3. Notas processadas ficam no final
   */
  private ordenarNotasFiscais(notas: any[]): any[] {
    if (!notas || notas.length === 0) {
      return notas;
    }

    return notas.sort((a, b) => {
      // 1. Priorizar notas não processadas (gerouequipamento = false)
      const aNaoProcessada = !a.gerouequipamento;
      const bNaoProcessada = !b.gerouequipamento;
      
      if (aNaoProcessada && !bNaoProcessada) {
        return -1; // 'a' vem antes de 'b'
      }
      if (!aNaoProcessada && bNaoProcessada) {
        return 1; // 'b' vem antes de 'a'
      }
      
      // 2. Se ambas têm o mesmo status de processamento, ordenar por data (mais recente primeiro)
      const dataA = new Date(a.dtemissao || a.dtEmissao || 0);
      const dataB = new Date(b.dtemissao || b.dtEmissao || 0);
      
      // Ordenar da mais recente para a mais antiga
      return dataB.getTime() - dataA.getTime();
    });
  }

  editar(obj) {
    this.route.navigate(['/nota-fiscal', btoa(JSON.stringify(obj))]);
  }

  excluir(obj) {
    this.util.exibirMensagemPopUp(
      `Deseja realmente excluir a nota fiscal <strong>${obj.numero}</strong>?<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação é irreversível e removerá a nota fiscal do sistema.`,
      true
    ).then(res => {
      if (res) {
        this.util.aguardar(true);
        this.api.excluirNotaFiscal(obj.id, this.session.token).then(res => {
          this.util.aguardar(false);
          if(res.status != 200) {
            console.error('Erro ao excluir:', res);
            // ✅ MELHORIA: Exibir mensagem específica do backend se disponível
            if (res.data && res.data.mensagem) {
              this.util.exibirMensagemToast(res.data.mensagem, 8000);
            } else {
              this.util.exibirFalhaComunicacao();
            }
          }
          else {
            this.util.exibirMensagemToast('Nota fiscal excluída com sucesso!', 3000);
            this.listar();
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('Erro ao excluir:', err);
          
          // ✅ MELHORIA: Tentar extrair mensagem específica do erro
          let mensagemErro = 'Erro ao excluir nota fiscal';
          if (err.response && err.response.data && err.response.data.mensagem) {
            mensagemErro = err.response.data.mensagem;
          } else if (err.message) {
            mensagemErro = err.message;
          }
          
          this.util.exibirMensagemToast(mensagemErro, 8000);
        });
      }
    });
  }

  liberarParaEstoque(obj) {
    var nfvm:any = {};
    nfvm.nota = obj;
    nfvm.usuario = this.session.usuario.id;

    const valorFormatado = new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(obj.valor || 0);

    this.util.exibirMensagemPopUp(
      `Tem certeza que deseja lançar no cadastro de recursos como novo?<br><br>` +
      `📦 <strong>Nota Fiscal:</strong> ${obj.numero}<br>` +
      `💰 <strong>Valor:</strong> ${valorFormatado}<br><br>` +
      `⚠️ <strong>Atenção:</strong> Esta ação irá gerar equipamentos automaticamente baseados nos itens da nota fiscal.`,
      true
    ).then(res => {
      if (res) {
        this.iniciarProcessoProgresso(obj);
        
        this.api.liberarParaEstoque(nfvm, this.session.token).then(res => {
          this.finalizarProcessoProgresso(true, 'Recursos cadastrados com sucesso!');
          this.listar();
        }).catch(err => {
          console.error('Erro ao liberar para estoque:', err);
          this.finalizarProcessoProgresso(false, 'Erro ao cadastrar recursos');
          this.util.exibirFalhaComunicacao();
        });
      }
    });
  }

  iniciarProcessoProgresso(notaFiscal: any) {
    // Inicializar dados do progresso
    this.progresso = {
      percentual: 0,
      equipamentosProcessados: 0,
      totalEquipamentos: this.estimarTotalEquipamentos(notaFiscal),
      tempoEstimado: this.estimarTempoProcessamento(notaFiscal),
      tempoDecorrido: 0,
      status: 'Iniciando processamento...',
      notaFiscal: notaFiscal.numero
    };

    // Mostrar modal de progresso
    this.mostrarModalProgresso = true;

    // Iniciar contador de tempo
    this.iniciarContadorTempo();

    // Simular progresso (já que não temos progresso real do backend)
    this.simularProgresso();
  }

  estimarTotalEquipamentos(notaFiscal: any): number {
    // Estimativa baseada no valor da nota fiscal (aproximadamente 1 equipamento por R$ 100)
    const valorEstimado = Math.max(1, Math.floor(notaFiscal.valor / 100));
    return Math.min(valorEstimado, 2000); // Limite máximo de 2000 equipamentos
  }

  estimarTempoProcessamento(notaFiscal: any): number {
    const totalEquipamentos = this.estimarTotalEquipamentos(notaFiscal);
    // Estimativa: 0.1 segundos por equipamento + 5 segundos base
    return Math.max(10, Math.floor(totalEquipamentos * 0.1 + 5));
  }

  iniciarContadorTempo() {
    this.intervaloProgresso = setInterval(() => {
      this.progresso.tempoDecorrido++;
      
      // Atualizar percentual baseado no tempo decorrido
      if (this.progresso.tempoEstimado > 0) {
        const percentualTempo = Math.min(95, (this.progresso.tempoDecorrido / this.progresso.tempoEstimado) * 100);
        this.progresso.percentual = Math.max(this.progresso.percentual, percentualTempo);
      }
    }, 1000);
  }

  simularProgresso() {
    const totalEquipamentos = this.progresso.totalEquipamentos;
    let processados = 0;
    
    const intervaloSimulacao = setInterval(() => {
      processados += Math.max(1, Math.floor(totalEquipamentos / 50)); // Processar em lotes
      
      if (processados >= totalEquipamentos) {
        processados = totalEquipamentos;
        clearInterval(intervaloSimulacao);
      }
      
      this.progresso.equipamentosProcessados = processados;
      this.progresso.percentual = Math.min(95, (processados / totalEquipamentos) * 100);
      
      // Atualizar status
      if (processados < totalEquipamentos) {
        this.progresso.status = `Processando equipamentos... ${processados}/${totalEquipamentos}`;
      } else {
        this.progresso.status = 'Finalizando processamento...';
      }
    }, 200);
  }

  finalizarProcessoProgresso(sucesso: boolean, mensagem: string) {
    // Parar contador de tempo
    if (this.intervaloProgresso) {
      clearInterval(this.intervaloProgresso);
    }

    // Finalizar progresso
    this.progresso.percentual = 100;
    this.progresso.status = sucesso ? 'Processamento concluído!' : 'Erro no processamento';
    this.progresso.equipamentosProcessados = this.progresso.totalEquipamentos;

    // Mostrar resultado
    setTimeout(() => {
      this.mostrarModalProgresso = false;
      if (sucesso) {
        this.util.exibirMensagemToast(mensagem, 10000);
      } else {
        this.util.exibirMensagemToast(mensagem, 8000);
      }
    }, 2000);
  }

  cancelarProcesso() {
    // Não permitir cancelar durante o processamento
    this.util.exibirMensagemToast('Processamento em andamento. Aguarde a conclusão.', 3000);
  }

  calcularTempoRestante(): number {
    if (this.progresso.percentual >= 100) return 0;
    
    const tempoRestante = this.progresso.tempoEstimado - this.progresso.tempoDecorrido;
    return Math.max(0, tempoRestante);
  }

  ngOnDestroy() {
    // Limpar intervalos para evitar memory leaks
    if (this.intervaloProgresso) {
      clearInterval(this.intervaloProgresso);
    }
  }

  // Métodos do modal de visualização
  visualizarNotaFiscal(notaFiscalId: number) {
    this.notaFiscalSelecionada = notaFiscalId;
    this.mostrarModalVisualizar = true;
  }

  onModalFechado() {
    this.mostrarModalVisualizar = false;
    this.notaFiscalSelecionada = 0;
  }

  // Métodos de gerenciamento de arquivos
  uploadArquivo(event: any, notaFiscalId: number) {
    const arquivo: File = event.target.files[0];
    
    if (!arquivo) {
      return;
    }

    // Validar tamanho (10MB)
    if (arquivo.size > 10 * 1024 * 1024) {
      this.util.exibirMensagemToast('Arquivo muito grande! Tamanho máximo: 10MB', 5000);
      return;
    }

    // Validar extensão
    const extensoesPermitidas = ['.pdf', '.xml', '.jpg', '.jpeg', '.png'];
    const nomeArquivo = arquivo.name.toLowerCase();
    const extensaoValida = extensoesPermitidas.some(ext => nomeArquivo.endsWith(ext));
    
    if (!extensaoValida) {
      this.util.exibirMensagemToast('Tipo de arquivo não permitido! Use apenas PDF, XML, JPG ou PNG', 5000);
      return;
    }

    this.util.aguardar(true);
    this.apiNotaFiscal.uploadArquivo(notaFiscalId, arquivo).then(res => {
      this.util.aguardar(false);
      
      if (res.status === 200) {
        this.util.exibirMensagemToast('Arquivo anexado com sucesso!', 3000);
        this.listar(); // Recarregar lista
      } else {
        console.error('Erro ao fazer upload:', res);
        this.util.exibirMensagemToast(res.data?.message || 'Erro ao anexar arquivo', 5000);
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro no upload:', err);
      this.util.exibirMensagemToast('Erro ao anexar arquivo', 5000);
    });

    // Limpar input
    event.target.value = '';
  }

  downloadArquivo(notaFiscalId: number, nomeArquivo: string) {
    this.util.aguardar(true);

    this.apiNotaFiscal.downloadArquivo(notaFiscalId).then(res => {
      this.util.aguardar(false);
      
      if (res.status === 200) {
        // Criar link temporário para download
        const blob = new Blob([res.data], { type: res.headers['content-type'] });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = nomeArquivo;
        link.click();
        window.URL.revokeObjectURL(url);
        
        this.util.exibirMensagemToast('Download concluído!', 3000);
      } else {
        console.error('Erro ao baixar arquivo:', res);
        this.util.exibirMensagemToast('Erro ao baixar arquivo', 5000);
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro no download:', err);
      this.util.exibirMensagemToast('Erro ao baixar arquivo', 5000);
    });
  }

  removerArquivo(notaFiscalId: number) {
    // Abrir modal de confirmação moderno
    const dialogRef = this.dialog.open(MessageboxComponent, {
      data: {
        mensagem: '<strong>Deseja realmente remover o arquivo anexado?</strong><br><br>Esta ação não poderá ser desfeita e o arquivo será permanentemente excluído do sistema.',
        exibeCancelar: true
      },
      width: '420px',
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado === true) {
        this.util.aguardar(true);

        this.apiNotaFiscal.removerArquivo(notaFiscalId).then(res => {
          this.util.aguardar(false);
          
          if (res.status === 200) {
            this.util.exibirMensagemToast('Arquivo removido com sucesso!', 3000);
            this.listar(); // Recarregar lista
          } else {
            console.error('Erro ao remover arquivo:', res);
            this.util.exibirMensagemToast(res.data?.message || 'Erro ao remover arquivo', 5000);
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('Erro ao remover:', err);
          this.util.exibirMensagemToast('Erro ao remover arquivo', 5000);
        });
      }
    });
  }
}
