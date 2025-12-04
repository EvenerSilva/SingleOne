import { Component, OnInit } from '@angular/core';
import { ElegibilidadeApiService } from 'src/app/api/elegibilidade/elegibilidade-api.service';
import { ConfiguracoesApiService } from 'src/app/api/configuracoes/configuracoes-api.service';
import { UtilService } from 'src/app/util/util.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-nao-conformidade-elegibilidade',
  templateUrl: './nao-conformidade-elegibilidade.component.html',
  styleUrls: ['./nao-conformidade-elegibilidade.component.scss']
})
export class NaoConformidadeElegibilidadeComponent implements OnInit {

  private session: any = {};
  public loading = false;
  public resultado: any = null;
  public registros: any[] = [];
  
  // Expor Math para o template
  public Math = Math;
  
  // Filtros
  public filtros: any = {
    cliente: 0,
    tipoColaborador: null,
    tipoEquipamentoId: null,
    empresaId: null,
    centroCustoId: null,
    colaboradorNome: ''
  };

  public tiposColaborador: any[] = [];
  public tiposEquipamento: any[] = [];

  // Paginação
  public pageSize = 10;
  public currentPage = 0;
  public registrosPaginados: any[] = [];

  // Modal de exportação
  public showExportModal = false;

  constructor(
    private elegibilidadeApi: ElegibilidadeApiService,
    private configApi: ConfiguracoesApiService,
    private util: UtilService
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.filtros.cliente = this.session?.usuario?.cliente || this.session?.cliente || 0;
    this.carregarDadosIniciais();
  }

  async carregarDadosIniciais(): Promise<void> {
    await this.carregarTiposColaborador();
    await this.carregarTiposEquipamento();
  }

  async carregarTiposColaborador(): Promise<void> {
    try {
      const result = await this.elegibilidadeApi.listarTiposColaborador(this.session.token);
      if (result.status === 200) {
        this.tiposColaborador = result.data;
      }
    } catch (error) {
      console.error('Erro ao carregar tipos de colaborador:', error);
    }
  }

  async carregarTiposEquipamento(): Promise<void> {
    try {
      const cliente = this.session?.usuario?.cliente || this.session?.cliente || 0;
      const result = await this.configApi.listarTiposRecursos('null', cliente, this.session.token);
      if (result && result.data && Array.isArray(result.data)) {
        this.tiposEquipamento = result.data.filter((t: any) => t.ativo);
      }
    } catch (error) {
      console.error('Erro ao carregar tipos de equipamento:', error);
    }
  }

  async consultar(): Promise<void> {
    this.loading = true;
    this.resultado = null;
    this.registros = [];
    try {
      const result = await this.elegibilidadeApi.consultarNaoConformidade(this.filtros, this.session.token);

      if (result.status === 200) {
        this.resultado = result.data?.data || result.data;
        this.registros = this.resultado?.registros || [];
        this.currentPage = 0;
        this.atualizarPaginacao();
        if (this.registros.length === 0) {
          this.util.exibirMensagemToast('✅ Nenhuma não conformidade encontrada! Todas as políticas estão sendo respeitadas.', 4000);
        }
      } else {
        this.util.exibirMensagemToast('Erro ao consultar não conformidades', 3000);
      }
    } catch (error) {
      console.error('Erro ao consultar não conformidades:', error);
      this.util.exibirFalhaComunicacao();
    } finally {
      this.loading = false;
    }
  }

  limparFiltros(): void {
    this.filtros = {
      cliente: this.session?.usuario?.cliente || this.session?.cliente || 0,
      tipoColaborador: null,
      tipoEquipamentoId: null,
      empresaId: null,
      centroCustoId: null,
      colaboradorNome: ''
    };
    this.resultado = null;
    this.registros = [];
  }

  atualizarPaginacao(): void {
    const inicio = this.currentPage * this.pageSize;
    const fim = inicio + this.pageSize;
    this.registrosPaginados = this.registros.slice(inicio, fim);
  }

  proximaPagina(): void {
    if ((this.currentPage + 1) * this.pageSize < this.registros.length) {
      this.currentPage++;
      this.atualizarPaginacao();
    }
  }

  paginaAnterior(): void {
    if (this.currentPage > 0) {
      this.currentPage--;
      this.atualizarPaginacao();
    }
  }

  abrirModalExportacao(): void {
    if (!this.registros || this.registros.length === 0) {
      this.util.exibirMensagemToast('Nenhum dado para exportar', 3000);
      return;
    }
    this.showExportModal = true;
  }

  fecharModalExportacao(): void {
    this.showExportModal = false;
  }

  exportar(formato: 'excel' | 'csv'): void {
    try {
      if (!this.registros || this.registros.length === 0) {
        this.util.exibirMensagemToast('Nenhum dado para exportar', 3000);
        return;
      }

      const dadosExportacao = this.registros.map((reg: any) => ({
        'Colaborador': reg.colaboradorNome,
        'CPF': this.decodificarCpf(reg.colaboradorCpf),
        'Tipo': this.mapearTipoColaborador(reg.tipoColaborador),
        'Cargo': reg.colaboradorCargo || '',
        'Empresa': reg.empresaNome || '',
        'Centro de Custo': reg.centroCusto || '',
        'Equipamento': reg.tipoEquipamentoDescricao,
        'Patrimônio': reg.equipamentoPatrimonio || '',
        'Série': reg.equipamentoSerie || '',
        'Fabricante': reg.fabricante || '',
        'Modelo': reg.modelo || '',
        'Motivo': reg.motivoNaoConformidade,
        'Qtd. Atual': reg.quantidadeAtual,
        'Qtd. Máxima': reg.quantidadeMaxima || 'Ilimitado',
        'Observações': reg.politicaObservacoes || ''
      }));

      const dataAtual = new Date().toISOString().split('T')[0];

      if (formato === 'excel') {
        this.exportarExcel(dadosExportacao, dataAtual);
      } else {
        this.exportarCSV(dadosExportacao, dataAtual);
      }

      this.fecharModalExportacao();
    } catch (error) {
      console.error('Erro ao exportar:', error);
      this.util.exibirMensagemToast('Erro ao exportar relatório', 3000);
    }
  }

  private exportarExcel(dados: any[], dataAtual: string): void {
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dados);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Não Conformidades');

    // Ajustar largura das colunas
    const colWidths = [
      { wch: 30 }, // Colaborador
      { wch: 15 }, // CPF
      { wch: 15 }, // Tipo
      { wch: 25 }, // Cargo
      { wch: 30 }, // Empresa
      { wch: 25 }, // Centro de Custo
      { wch: 25 }, // Equipamento
      { wch: 15 }, // Patrimônio
      { wch: 15 }, // Série
      { wch: 20 }, // Fabricante
      { wch: 20 }, // Modelo
      { wch: 50 }, // Motivo
      { wch: 12 }, // Qtd. Atual
      { wch: 12 }, // Qtd. Máxima
      { wch: 40 }  // Observações
    ];
    ws['!cols'] = colWidths;

    const nomeArquivo = `nao-conformidades-elegibilidade-${dataAtual}.xlsx`;
    XLSX.writeFile(wb, nomeArquivo);
    this.util.exibirMensagemToast('Relatório Excel exportado com sucesso!', 3000);
  }

  private exportarCSV(dados: any[], dataAtual: string): void {
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dados);
    const csv = XLSX.utils.sheet_to_csv(ws, { FS: ';' }); // Separador ponto-e-vírgula

    const blob = new Blob(['\ufeff' + csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', `nao-conformidades-elegibilidade-${dataAtual}.csv`);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    this.util.exibirMensagemToast('Relatório CSV exportado com sucesso!', 3000);
  }

  // Manter método antigo para compatibilidade, mas apontar para o novo
  exportarParaExcel(): void {
    this.abrirModalExportacao();
  }

  getValorEstatistica(objeto: any, chave: string): number {
    return objeto && objeto[chave] ? objeto[chave] : 0;
  }

  getChavesEstatistica(objeto: any): string[] {
    if (!objeto) return [];
    
    const chaves = Object.keys(objeto);
    
    // Mapear os códigos para descrições legíveis
    const chavesMapeadas = chaves.map(key => {
      const keyUpper = key.toUpperCase();
      // Se a chave é um código de tipo de colaborador, converter
      if (keyUpper === 'F') return 'Funcionário';
      if (keyUpper === 'T') return 'Terceirizado';
      if (keyUpper === 'C') return 'Consultor';
      return key;
    });
    
    return chavesMapeadas;
  }
  
  getValorEstatisticaPorChave(objeto: any, chaveExibida: string): number {
    if (!objeto) return 0;
    
    // Mapear de volta para o código original (tentar minúsculo e maiúsculo)
    let chaveOriginal = chaveExibida;
    if (chaveExibida === 'Funcionário') chaveOriginal = objeto['F'] !== undefined ? 'F' : 'f';
    if (chaveExibida === 'Terceirizado') chaveOriginal = objeto['T'] !== undefined ? 'T' : 't';
    if (chaveExibida === 'Consultor') chaveOriginal = objeto['C'] !== undefined ? 'C' : 'c';
    
    return objeto[chaveOriginal] || 0;
  }
  
  // Mapear código de tipo de colaborador para descrição
  mapearTipoColaborador(codigo: string): string {
    if (!codigo) return '';
    const codigoUpper = codigo.toUpperCase();
    if (codigoUpper === 'F') return 'Funcionário';
    if (codigoUpper === 'T') return 'Terceirizado';
    if (codigoUpper === 'C') return 'Consultor';
    return codigo;
  }
  
  // Decodificar CPF se estiver em Base64
  decodificarCpf(cpf: string): string {
    if (!cpf) return '';
    
    // Verificar se parece ser Base64 (termina com = e não tem pontos/traços)
    if (cpf.includes('=') && !cpf.includes('.') && !cpf.includes('-')) {
      try {
        return atob(cpf);
      } catch {
        return cpf;
      }
    }
    
    return cpf;
  }
}

