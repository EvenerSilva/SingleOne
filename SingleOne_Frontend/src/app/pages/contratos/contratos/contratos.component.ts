import { Component, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { ContratoApiService } from 'src/app/api/contratos/contrato-api.service';
import { UtilService } from 'src/app/util/util.service';
import { MatDialog } from '@angular/material/dialog';
import { MessageboxComponent } from '../../messagebox/messagebox.component';

@Component({
  selector: 'app-contratos',
  templateUrl: './contratos.component.html',
  styleUrls: ['./contratos.component.scss']
})
export class ContratosComponent implements OnInit, AfterViewInit {

  public colunas = ['contrato', 'fornecedor', 'descricao', 'finalVigencia', 'status', 'arquivo', 'acao'];
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public dadosOriginais: any[] = []; // Armazenar dados originais para filtros

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
    private api: ContratoApiService, 
    private route: Router,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    try {
      // Inicializar com lista vazia para evitar erros
      this.dataSource = new MatTableDataSource<any>([]);
      
      this.resultado = this.consulta.valueChanges.pipe(
        debounceTime(1000),
        tap(value => this.buscar(value))
      );
      this.resultado.subscribe();
      this.listarContratos();
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

  buscar(value) {
    if (value && value.trim() !== '') {
      const termoBusca = value.toLowerCase().trim();
      const dadosFiltrados = this.dadosOriginais.filter(contrato => {
        return (
          contrato.numero?.toString().toLowerCase().includes(termoBusca) ||
          contrato.aditivo?.toString().toLowerCase().includes(termoBusca) ||
          contrato.descricao?.toLowerCase().includes(termoBusca) ||
          contrato.fornecedor?.toLowerCase().includes(termoBusca) ||
          contrato.status?.toLowerCase().includes(termoBusca) ||
          // Busca por número/aditivo combinado
          `${contrato.numero}/${contrato.aditivo}`.toLowerCase().includes(termoBusca)
        );
      });
      this.dataSource = new MatTableDataSource<any>(dadosFiltrados);
      this.configurarPaginador();
    } else {
      this.dataSource = new MatTableDataSource<any>(this.dadosOriginais);
      this.configurarPaginador();
    }
  }

  listarContratos() {
    this.util.aguardar(true);
    this.api.listar().then(res => {
      this.util.aguardar(false);
      if (res.status != 200) {
        console.error('Erro ao listar contratos:', res);
        this.util.exibirFalhaComunicacao();
      } else {
        const contratos = res.data.result || [];
        
        // Debug das novas propriedades
        if (contratos.length > 0) {
        }
        
        // Armazenar dados originais para filtros
        this.dadosOriginais = [...contratos];
        
        // Configurar fonte de dados
        this.dataSource = new MatTableDataSource<any>(contratos);
        this.configurarPaginador();
      }
    }).catch(err => {
      this.util.aguardar(false);
      console.error('Erro na requisição:', err);
      this.util.exibirFalhaComunicacao();
    });
  }

  redirectToEquipamentos(contratoId: string) {
    this.route.navigate(['recursos', contratoId]);
  }

  editar(id) {
    this.route.navigate(['/contrato', id]);
  }

  limparBusca() {
    this.consulta.setValue('');
    // Restaurar dados originais sem fazer nova requisição
    this.dataSource = new MatTableDataSource<any>(this.dadosOriginais);
    this.configurarPaginador();
  }

  uploadArquivo(event: any, contratoId: number) {
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
    const extensoesPermitidas = ['.pdf', '.doc', '.docx'];
    const nomeArquivo = arquivo.name.toLowerCase();
    const extensaoValida = extensoesPermitidas.some(ext => nomeArquivo.endsWith(ext));
    
    if (!extensaoValida) {
      this.util.exibirMensagemToast('Tipo de arquivo não permitido! Use apenas PDF, DOC ou DOCX', 5000);
      return;
    }

    this.util.aguardar(true);
    this.api.uploadArquivo(contratoId, arquivo).then(res => {
      this.util.aguardar(false);
      
      if (res.status === 200) {
        this.util.exibirMensagemToast('Arquivo anexado com sucesso!', 3000);
        this.listarContratos(); // Recarregar lista
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

  downloadArquivo(contratoId: number, nomeArquivo: string) {
    this.util.aguardar(true);

    this.api.downloadArquivo(contratoId).then(res => {
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

  removerArquivo(contratoId: number) {
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
        // Usuário confirmou a remoção
        this.util.aguardar(true);

        this.api.removerArquivo(contratoId).then(res => {
          this.util.aguardar(false);
          
          if (res.status === 200) {
            this.util.exibirMensagemToast('Arquivo removido com sucesso!', 3000);
            this.listarContratos(); // Recarregar lista
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
