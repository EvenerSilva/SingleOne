import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Observable } from 'rxjs';
import { debounceTime, tap } from 'rxjs/operators';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import * as XLSX from 'xlsx';
import { saveAs as importedSaveAs } from 'file-saver';

@Component({
  selector: 'app-movimentacoes-colaboradores',
  templateUrl: './movimentacoes-colaboradores.component.html',
  styleUrls: ['./movimentacoes-colaboradores.component.scss']
})
export class MovimentacoesColaboradoresComponent implements OnInit {

  constructor(
    private util: UtilService, 
    private api: RelatorioApiService,
    private cdr: ChangeDetectorRef
  ) { }

  private session: any = {};
  public movimentacoes: any = {};
  public consulta = new FormControl();
  public resultado: Observable<any>;
  public colunas: any = ['colaborador', 'matricula', 'mudanca', 'atualizacao'];
  
  // Armazenar o termo de pesquisa atual
  private termoPesquisaAtual: string = "null";
  
  public dataSourceStatus: MatTableDataSource<any> = new MatTableDataSource<any>([]);
  public dataSourceEmpresa: MatTableDataSource<any> = new MatTableDataSource<any>([]);
  public dataSourceCC: MatTableDataSource<any> = new MatTableDataSource<any>([]);
  public dataSourceLocalidade: MatTableDataSource<any> = new MatTableDataSource<any>([]);

  // Propriedades para totalizadores das guias
  public totalStatus: number = 0;
  public totalEmpresa: number = 0;
  public totalCC: number = 0;
  public totalLocalidade: number = 0;
  
  // Propriedades para pageSize de cada aba
  public pageSizeStatus: number = 10;
  public pageSizeEmpresa: number = 10;
  public pageSizeCC: number = 10;
  public pageSizeLocalidade: number = 10;
  
  // Propriedades para length (total de registros) de cada aba
  public lengthStatus: number = 0;
  public lengthEmpresa: number = 0;
  public lengthCC: number = 0;
  public lengthLocalidade: number = 0;
  
  // Propriedades para pageIndex de cada aba
  public pageIndexStatus: number = 0;
  public pageIndexEmpresa: number = 0;
  public pageIndexCC: number = 0;
  public pageIndexLocalidade: number = 0;

  // Novas propriedades para modernização
  public loading = false;
  public selectedTabIndex = 0;
  public showExportModal = false;

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.resultado = this.consulta.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscar(value))
    );
    this.resultado.subscribe();
    this.listarMovimentacoes('T', null);
  }


  listarMovimentacoes(relatorio: string, event?: PageEvent) {
    this.loading = true;
    var pagina = ((event == null) ? 1 : event.pageIndex + 1);
    
    // Determinar o pageSize baseado no relatório e evento
    var pageSize = 10;
    if (event) {
      pageSize = event.pageSize;
      // Atualizar a propriedade correspondente à aba
      switch(relatorio.toUpperCase()) {
        case 'S': this.pageSizeStatus = pageSize; break;
        case 'E': this.pageSizeEmpresa = pageSize; break;
        case 'C': this.pageSizeCC = pageSize; break;
        case 'L': this.pageSizeLocalidade = pageSize; break;
        case 'T':
          this.pageSizeStatus = pageSize;
          this.pageSizeEmpresa = pageSize;
          this.pageSizeCC = pageSize;
          this.pageSizeLocalidade = pageSize;
          break;
      }
    } else {
      // Se não houver evento, usar o pageSize armazenado da aba correspondente
      switch(relatorio.toUpperCase()) {
        case 'S': pageSize = this.pageSizeStatus; break;
        case 'E': pageSize = this.pageSizeEmpresa; break;
        case 'C': pageSize = this.pageSizeCC; break;
        case 'L': pageSize = this.pageSizeLocalidade; break;
        case 'T': pageSize = 10; break; // Carregamento inicial
      }
    }
    
    // Se for uma guia específica, manter os totalizadores das outras guias
    // Apenas atualizar quando for 'T' (todos) ou na primeira carga
    const isCarregamentoCompleto = relatorio === 'T';
    
    // Usar o termo de pesquisa armazenado
    this.api.movimentacoesColaboradores(this.session.usuario.cliente, pagina, relatorio, this.termoPesquisaAtual, pageSize, this.session.token).then(res => {
      this.loading = false;
      
      if (res.status != 200 && res.status != 204) {
        this.util.exibirFalhaComunicacao();
      } else {
        // Obter dados - pode estar em res.data ou res.data.data
        const dados = res.data?.data || res.data;
        
        // STATUS - verificar ambos os casos (camelCase e PascalCase)
        try {
          const statusData = dados?.statusColaborador || dados?.StatusColaborador;
          if (statusData) {
            const results = statusData.results || statusData.Results || [];
            const rowCount = statusData.rowCount || statusData.RowCount || 0;
            
            // Atualizar apenas os dados
            this.dataSourceStatus.data = results;
            
            // Atualizar propriedades do paginador
            this.lengthStatus = rowCount;
            this.pageSizeStatus = pageSize;
            this.pageIndexStatus = (statusData.currentPage || statusData.CurrentPage || 1) - 1;
            this.totalStatus = rowCount;
          } else if (isCarregamentoCompleto) {
            this.totalStatus = 0;
          }
        } catch (error) {
          if (isCarregamentoCompleto) {
            this.totalStatus = 0;
          }
        }

        // EMPRESA
        try {
          const empresaData = dados?.empresaColaborador || dados?.EmpresaColaborador;
          if (empresaData) {
            const results = empresaData.results || empresaData.Results || [];
            const rowCount = empresaData.rowCount || empresaData.RowCount || 0;
            
            // Atualizar apenas os dados
            this.dataSourceEmpresa.data = results;
            
            // Atualizar propriedades do paginador
            this.lengthEmpresa = rowCount;
            this.pageSizeEmpresa = pageSize;
            this.pageIndexEmpresa = (empresaData.currentPage || empresaData.CurrentPage || 1) - 1;
            this.totalEmpresa = rowCount;
          } else if (isCarregamentoCompleto) {
            this.totalEmpresa = 0;
          }
        } catch (error) {
          if (isCarregamentoCompleto) {
            this.totalEmpresa = 0;
          }
        }

        // CENTRO DE CUSTO
        try {
          const ccData = dados?.centroCustoColaborador || dados?.CentroCustoColaborador;
          if (ccData) {
            const results = ccData.results || ccData.Results || [];
            const rowCount = ccData.rowCount || ccData.RowCount || 0;
            
            // Atualizar apenas os dados
            this.dataSourceCC.data = results;
            
            // Atualizar propriedades do paginador
            this.lengthCC = rowCount;
            this.pageSizeCC = pageSize;
            this.pageIndexCC = (ccData.currentPage || ccData.CurrentPage || 1) - 1;
            this.totalCC = rowCount;
          } else if (isCarregamentoCompleto) {
            this.totalCC = 0;
          }
        } catch (error) {
          if (isCarregamentoCompleto) {
            this.totalCC = 0;
          }
        }

        // LOCALIDADE
        try {
          const localidadeData = dados?.localidadeColaborador || dados?.LocalidadeColaborador;
          if (localidadeData) {
            const results = localidadeData.results || localidadeData.Results || [];
            const rowCount = localidadeData.rowCount || localidadeData.RowCount || 0;
            
            // Atualizar apenas os dados
            this.dataSourceLocalidade.data = results;
            
            // Atualizar propriedades do paginador
            this.lengthLocalidade = rowCount;
            this.pageSizeLocalidade = pageSize;
            this.pageIndexLocalidade = (localidadeData.currentPage || localidadeData.CurrentPage || 1) - 1;
            this.totalLocalidade = rowCount;
          } else if (isCarregamentoCompleto) {
            this.totalLocalidade = 0;
          }
        } catch (error) {
          if (isCarregamentoCompleto) {
            this.totalLocalidade = 0;
          }
        }
        
        // Forçar atualização da view após processar todos os dados
        setTimeout(() => {
          this.cdr.detectChanges();
          this.cdr.markForCheck();
        }, 0);
      }
    }).catch((error) => {
      this.loading = false;
      this.util.exibirFalhaComunicacao();
    });
  }

  buscar(valor: string) {
    if (valor != '') {
      this.loading = true;
      var pagina = 1;
      var pageSize = 10; // Padrão ao buscar
      
      // Armazenar o termo de pesquisa atual
      this.termoPesquisaAtual = valor;
      
      this.api.movimentacoesColaboradores(this.session.usuario.cliente, pagina, "T", valor, pageSize, this.session.token).then(res => {
        this.loading = false;
        
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        } else {
          // Obter dados - pode estar em res.data ou res.data.data
          const dados = res.data?.data || res.data;
          
          // STATUS
          try {
            const statusData = dados?.statusColaborador || dados?.StatusColaborador;
            if (statusData) {
              const results = statusData.results || statusData.Results || [];
              const rowCount = statusData.rowCount || statusData.RowCount || 0;
              
              // Atualizar apenas os dados
              this.dataSourceStatus.data = results;
              this.lengthStatus = rowCount;
              this.pageIndexStatus = 0; // Reset para primeira página ao buscar
              this.totalStatus = rowCount;
            }
          } catch {}

          // EMPRESA
          try {
            const empresaData = dados?.empresaColaborador || dados?.EmpresaColaborador;
            if (empresaData) {
              const results = empresaData.results || empresaData.Results || [];
              const rowCount = empresaData.rowCount || empresaData.RowCount || 0;
              
              // Atualizar apenas os dados
              this.dataSourceEmpresa.data = results;
              this.lengthEmpresa = rowCount;
              this.pageIndexEmpresa = 0; // Reset para primeira página ao buscar
              this.totalEmpresa = rowCount;
            }
          } catch {}

          // CENTRO DE CUSTO
          try {
            const ccData = dados?.centroCustoColaborador || dados?.CentroCustoColaborador;
            if (ccData) {
              const results = ccData.results || ccData.Results || [];
              const rowCount = ccData.rowCount || ccData.RowCount || 0;
              
              // Atualizar apenas os dados
              this.dataSourceCC.data = results;
              this.lengthCC = rowCount;
              this.pageIndexCC = 0; // Reset para primeira página ao buscar
              this.totalCC = rowCount;
            }
          } catch {}

          // LOCALIDADE
          try {
            const localidadeData = dados?.localidadeColaborador || dados?.LocalidadeColaborador;
            if (localidadeData) {
              const results = localidadeData.results || localidadeData.Results || [];
              const rowCount = localidadeData.rowCount || localidadeData.RowCount || 0;
              
              // Atualizar apenas os dados
              this.dataSourceLocalidade.data = results;
              this.lengthLocalidade = rowCount;
              this.pageIndexLocalidade = 0; // Reset para primeira página ao buscar
              this.totalLocalidade = rowCount;
            }
          } catch {}
          
          // Forçar atualização da view
          this.cdr.detectChanges();
          this.cdr.markForCheck();
        }
      }).catch(() => {
        this.loading = false;
        this.util.exibirFalhaComunicacao();
      });
    } else {
      this.listarMovimentacoes("T", null);
    }
  }

  limparBusca(): void {
    this.consulta.setValue('');
    this.termoPesquisaAtual = "null"; // Resetar o termo de pesquisa
    this.listarMovimentacoes("T", null);
  }

  // Métodos para métricas
  getTotalMovimentacoes(): number {
    const totalStatus = this.dataSourceStatus?.data?.length || 0;
    const totalEmpresa = this.dataSourceEmpresa?.data?.length || 0;
    const totalCC = this.dataSourceCC?.data?.length || 0;
    const totalLocalidade = this.dataSourceLocalidade?.data?.length || 0;
    
    return totalStatus + totalEmpresa + totalCC + totalLocalidade;
  }

  getTotalColaboradores(): number {
    const colaboradores = new Set();
    
    // Adicionar colaboradores de cada tipo de movimentação
    if (this.dataSourceStatus?.data) {
      this.dataSourceStatus.data.forEach((item: any) => {
        if (item.nome) colaboradores.add(item.nome);
      });
    }
    
    if (this.dataSourceEmpresa?.data) {
      this.dataSourceEmpresa.data.forEach((item: any) => {
        if (item.nome) colaboradores.add(item.nome);
      });
    }
    
    if (this.dataSourceCC?.data) {
      this.dataSourceCC.data.forEach((item: any) => {
        if (item.nome) colaboradores.add(item.nome);
      });
    }
    
    if (this.dataSourceLocalidade?.data) {
      this.dataSourceLocalidade.data.forEach((item: any) => {
        if (item.nome) colaboradores.add(item.nome);
      });
    }
    
    return colaboradores.size;
  }

  getPeriodoAtualizacao(): string {
    const hoje = new Date();
    const mes = hoje.getMonth() + 1;
    const ano = hoje.getFullYear();
    return `${mes.toString().padStart(2, '0')}/${ano}`;
  }

  // Métodos de exportação
  exportarDados(): void {
    if (!this.getTotalMovimentacoes()) {
      this.util.exibirMensagemToast('Não há dados para exportar.', 3000);
      return;
    }

    this.showExportModal = true;
  }

  closeExportModal(event: Event): void {
    this.showExportModal = false;
  }

  async exportarParaExcel(): Promise<void> {
    try {
      this.loading = true;
      this.util.exibirMensagemToast('Buscando todos os registros para exportação...', 2000);
      
      // Buscar TODOS os registros de cada tipo (sem paginação)
      const [dadosStatus, dadosEmpresa, dadosCC, dadosLocalidade] = await Promise.all([
        this.buscarTodosRegistros('S'),
        this.buscarTodosRegistros('E'),
        this.buscarTodosRegistros('C'),
        this.buscarTodosRegistros('L')
      ]);
      
      // Preparar dados para cada aba
      const dadosStatusFormatados = this.prepararDadosStatus(dadosStatus);
      const dadosEmpresaFormatados = this.prepararDadosEmpresa(dadosEmpresa);
      const dadosCCFormatados = this.prepararDadosCC(dadosCC);
      const dadosLocalidadeFormatados = this.prepararDadosLocalidade(dadosLocalidade);
      
      // Criar workbook com 4 abas
      const workbook = XLSX.utils.book_new();
      
      // Aba 1: Status
      const wsStatus = XLSX.utils.json_to_sheet(dadosStatusFormatados);
      XLSX.utils.book_append_sheet(workbook, wsStatus, 'Status');
      
      // Aba 2: Empresa
      const wsEmpresa = XLSX.utils.json_to_sheet(dadosEmpresaFormatados);
      XLSX.utils.book_append_sheet(workbook, wsEmpresa, 'Empresa');
      
      // Aba 3: Centro de Custo
      const wsCC = XLSX.utils.json_to_sheet(dadosCCFormatados);
      XLSX.utils.book_append_sheet(workbook, wsCC, 'Centro de Custo');
      
      // Aba 4: Localidade
      const wsLocalidade = XLSX.utils.json_to_sheet(dadosLocalidadeFormatados);
      XLSX.utils.book_append_sheet(workbook, wsLocalidade, 'Localidade');
      
      // Gerar arquivo
      const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
      const nomeArquivo = `movimentacoes-colaboradores-${timestamp}.xlsx`;
      const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
      const blob = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
      
      importedSaveAs(blob, nomeArquivo);
      
      this.loading = false;
      this.util.exibirMensagemToast('Dados exportados para Excel com sucesso!', 3000);
      this.closeExportModal(new Event('click'));
    } catch (error) {
      this.loading = false;
      console.error('Erro ao exportar para Excel:', error);
      this.util.exibirMensagemToast('Erro ao exportar dados. Tente novamente.', 3000);
    }
  }

  async exportarParaCSV(): Promise<void> {
    try {
      this.loading = true;
      this.util.exibirMensagemToast('Buscando todos os registros para exportação...', 2000);
      
      // Buscar TODOS os registros de cada tipo
      const [dadosStatus, dadosEmpresa, dadosCC, dadosLocalidade] = await Promise.all([
        this.buscarTodosRegistros('S'),
        this.buscarTodosRegistros('E'),
        this.buscarTodosRegistros('C'),
        this.buscarTodosRegistros('L')
      ]);
      
      // Preparar dados
      const dadosStatusFormatados = this.prepararDadosStatus(dadosStatus);
      const dadosEmpresaFormatados = this.prepararDadosEmpresa(dadosEmpresa);
      const dadosCCFormatados = this.prepararDadosCC(dadosCC);
      const dadosLocalidadeFormatados = this.prepararDadosLocalidade(dadosLocalidade);
      
      // Criar um único CSV com todas as seções separadas
      let csvContent = '\uFEFF'; // BOM para Excel reconhecer UTF-8
      
      // Seção Status
      csvContent += '=== STATUS ===\n';
      if (dadosStatusFormatados.length > 0) {
        const headers = Object.keys(dadosStatusFormatados[0]);
        csvContent += headers.join(';') + '\n';
        dadosStatusFormatados.forEach(row => {
          const linha = headers.map(header => {
            const valor = row[header] || '';
            const valorStr = String(valor);
            if (valorStr.includes(';') || valorStr.includes('"') || valorStr.includes('\n')) {
              return `"${valorStr.replace(/"/g, '""')}"`;
            }
            return valorStr;
          });
          csvContent += linha.join(';') + '\n';
        });
      }
      
      csvContent += '\n=== EMPRESA ===\n';
      if (dadosEmpresaFormatados.length > 0) {
        const headers = Object.keys(dadosEmpresaFormatados[0]);
        csvContent += headers.join(';') + '\n';
        dadosEmpresaFormatados.forEach(row => {
          const linha = headers.map(header => {
            const valor = row[header] || '';
            const valorStr = String(valor);
            if (valorStr.includes(';') || valorStr.includes('"') || valorStr.includes('\n')) {
              return `"${valorStr.replace(/"/g, '""')}"`;
            }
            return valorStr;
          });
          csvContent += linha.join(';') + '\n';
        });
      }
      
      csvContent += '\n=== CENTRO DE CUSTO ===\n';
      if (dadosCCFormatados.length > 0) {
        const headers = Object.keys(dadosCCFormatados[0]);
        csvContent += headers.join(';') + '\n';
        dadosCCFormatados.forEach(row => {
          const linha = headers.map(header => {
            const valor = row[header] || '';
            const valorStr = String(valor);
            if (valorStr.includes(';') || valorStr.includes('"') || valorStr.includes('\n')) {
              return `"${valorStr.replace(/"/g, '""')}"`;
            }
            return valorStr;
          });
          csvContent += linha.join(';') + '\n';
        });
      }
      
      csvContent += '\n=== LOCALIDADE ===\n';
      if (dadosLocalidadeFormatados.length > 0) {
        const headers = Object.keys(dadosLocalidadeFormatados[0]);
        csvContent += headers.join(';') + '\n';
        dadosLocalidadeFormatados.forEach(row => {
          const linha = headers.map(header => {
            const valor = row[header] || '';
            const valorStr = String(valor);
            if (valorStr.includes(';') || valorStr.includes('"') || valorStr.includes('\n')) {
              return `"${valorStr.replace(/"/g, '""')}"`;
            }
            return valorStr;
          });
          csvContent += linha.join(';') + '\n';
        });
      }
      
      // Download
      const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
      const timestamp = new Date().toISOString().slice(0, 19).replace(/:/g, '-');
      const nomeArquivo = `movimentacoes-colaboradores-${timestamp}.csv`;
      
      importedSaveAs(blob, nomeArquivo);
      
      this.loading = false;
      this.util.exibirMensagemToast('Dados exportados para CSV com sucesso!', 3000);
      this.closeExportModal(new Event('click'));
    } catch (error) {
      this.loading = false;
      console.error('Erro ao exportar para CSV:', error);
      this.util.exibirMensagemToast('Erro ao exportar dados. Tente novamente.', 3000);
    }
  }

  private async buscarTodosRegistros(relatorio: string): Promise<any[]> {
    const todosRegistros: any[] = [];
    let pagina = 1;
    let temMaisRegistros = true;
    const pageSize = 1000; // Buscar 1000 por vez para ser mais eficiente
    
    while (temMaisRegistros) {
      try {
        const res = await this.api.movimentacoesColaboradores(
          this.session.usuario.cliente, 
          pagina, 
          relatorio, 
          this.termoPesquisaAtual === "null" ? "null" : this.termoPesquisaAtual, 
          pageSize, 
          this.session.token
        );
        
        if (res.status === 200 || res.status === 204) {
          const dados = res.data?.data || res.data;
          let results: any[] = [];
          
          switch(relatorio.toUpperCase()) {
            case 'S':
              const statusData = dados?.statusColaborador || dados?.StatusColaborador;
              results = statusData?.results || statusData?.Results || [];
              break;
            case 'E':
              const empresaData = dados?.empresaColaborador || dados?.EmpresaColaborador;
              results = empresaData?.results || empresaData?.Results || [];
              break;
            case 'C':
              const ccData = dados?.centroCustoColaborador || dados?.CentroCustoColaborador;
              results = ccData?.results || ccData?.Results || [];
              break;
            case 'L':
              const localidadeData = dados?.localidadeColaborador || dados?.LocalidadeColaborador;
              results = localidadeData?.results || localidadeData?.Results || [];
              break;
          }
          
          if (results.length > 0) {
            todosRegistros.push(...results);
            // Se retornou menos que pageSize, é a última página
            if (results.length < pageSize) {
              temMaisRegistros = false;
            } else {
              pagina++;
            }
          } else {
            temMaisRegistros = false;
          }
        } else {
          temMaisRegistros = false;
        }
      } catch (error) {
        temMaisRegistros = false;
      }
    }
    
    return todosRegistros;
  }

  private prepararDadosStatus(dados: any[]): any[] {
    return dados.map((item: any) => ({
      'Colaborador': item.nome || 'N/A',
      'Matrícula': item.matricula || 'N/A',
      'Status Anterior': item.situacaoantiga || 'N/A',
      'Status Atual': item.situacao || 'N/A',
      'Data Atualização': item.dtatualizacao ? new Date(item.dtatualizacao).toLocaleString('pt-BR') : 'N/A'
    }));
  }

  private prepararDadosEmpresa(dados: any[]): any[] {
    return dados.map((item: any) => ({
      'Colaborador': item.nome || 'N/A',
      'Matrícula': item.matricula || 'N/A',
      'Empresa Anterior': item.empresaantiga || 'N/A',
      'Empresa Atual': item.empresaatual || 'N/A',
      'Data Atualização': item.dtatualizacaoempresa ? new Date(item.dtatualizacaoempresa).toLocaleString('pt-BR') : 'N/A'
    }));
  }

  private prepararDadosCC(dados: any[]): any[] {
    return dados.map((item: any) => ({
      'Colaborador': item.nome || 'N/A',
      'Matrícula': item.matricula || 'N/A',
      'CC Anterior': item.codigoccantigo && item.nomeccantigo ? `${item.codigoccantigo} - ${item.nomeccantigo}` : 'N/A',
      'CC Atual': item.codigoccatual && item.nomeccatual ? `${item.codigoccatual} - ${item.nomeccatual}` : 'N/A',
      'Data Atualização': item.dtatualizacaocentrocusto ? new Date(item.dtatualizacaocentrocusto).toLocaleString('pt-BR') : 'N/A'
    }));
  }

  private prepararDadosLocalidade(dados: any[]): any[] {
    return dados.map((item: any) => ({
      'Colaborador': item.nome || 'N/A',
      'Matrícula': item.matricula || 'N/A',
      'Localidade Anterior': item.localidadeantiga || 'N/A',
      'Localidade Atual': item.localidadeatual || 'N/A',
      'Data Atualização': item.dtatualizacaolocalidade ? new Date(item.dtatualizacaolocalidade).toLocaleString('pt-BR') : 'N/A'
    }));
  }


}
