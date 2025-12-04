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
  selector: 'app-templates',
  templateUrl: './templates.component.html',
  styleUrls: ['./templates.component.scss']
})
export class TemplatesComponent implements OnInit, AfterViewInit {

  private session:any = {};
  @ViewChild(MatPaginator, { static: false }) paginator: MatPaginator;
  public dataSource: MatTableDataSource<any>;
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public cliente = 0;

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
    
    // Inicializar com lista vazia para evitar erros
    this.dataSource = new MatTableDataSource<any>([]);
    
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    this.listar();
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

  listar() {
    this.util.aguardar(true);
    this.api.listarTemplates(this.cliente, this.session.token).then(res => {
      this.util.aguardar(false);
      if(res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      }
      else {
        this.dataSource = new MatTableDataSource<any>(res.data);
        this.configurarPaginador();
      }
    })
  }

  buscar(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.api.listarTemplates(this.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          // Filtrar os templates baseado no valor de busca
          const templates = res.data || [];
          const filtrados = templates.filter(template => 
            template.tipoNavigation?.descricao?.toLowerCase().includes(valor.toLowerCase()) ||
            template.titulo?.toLowerCase().includes(valor.toLowerCase()) ||
            template.versao?.toString().includes(valor)
          );
          this.dataSource = new MatTableDataSource(filtrados);
          this.configurarPaginador();
        }
      })
    }
    else {
      this.listar();
    }
  }

  editar(obj) {
    this.route.navigate(['/template', btoa(JSON.stringify(obj))]);
  }

  excluir(obj) {
    if(confirm('Deseja realmente excluir o template ' + obj.titulo + '?')) {
      this.util.aguardar(true);
      this.api.excluirTemplate(obj.id, this.session.token).then(res => {
        this.util.aguardar(false);
        if(res.status != 200) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.util.exibirMensagemToast('Template excluido com sucesso!', 5000);
          this.listar();
        }
      })
    }
  }

}
