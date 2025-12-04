import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { TelefoniaApiService } from 'src/app/api/telefonia/telefonia-api.service';
import { UtilService } from 'src/app/util/util.service';
import { ImportacaoLinhasService } from 'src/app/services/importacao-linhas.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-telecom',
  templateUrl: './telecom.component.html',
  styleUrls: ['./telecom.component.scss']
})
export class TelecomComponent implements OnInit, OnDestroy {

  private session: any = {};
  public telefoniaResumo: any = {};

  // 📤 PROPRIEDADES PARA O MODAL DE IMPORTAÇÃO
  public mostrarModalImportacao = false;
  public passoImportacao: number = 1;
  public arquivoSelecionadoImport: File | null = null;
  public uploadandoImportacao: boolean = false;
  public importandoImportacao: boolean = false;
  public loteAtualImport: string | null = null;
  public resultadoValidacaoImport: any = null;
  public resultadoImportacaoFinal: any = null;

  // 📊 PROPRIEDADES PARA O MODAL DE PROGRESSO
  public mostrarModalProgresso = false;
  public progresso = {
    percentual: 0,
    linhasProcessadas: 0,
    totalLinhas: 0,
    tempoEstimado: 0,
    tempoDecorrido: 0,
    status: '',
    lote: ''
  };
  private intervaloProgresso: any;

  constructor(
    private util: UtilService,
    private telefoniaApi: TelefoniaApiService,
    private importacaoService: ImportacaoLinhasService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.carregarResumoTelefonia();
  }

  carregarResumoTelefonia() {
    this.util.aguardar(true);
    
    // Carregar contadores usando endpoints específicos (muito mais eficiente!)
    this.telefoniaApi.contarOperadoras(this.session.token).then(res => {
      if (res.status === 200) {
        this.telefoniaResumo.operadoras = res.data || 0;
      }
    }).catch((error) => {
      this.telefoniaResumo.operadoras = 0;
    });

    // 2. Contar Contratos (Contas)
    this.telefoniaApi.contarContratos(this.session.token).then(res => {
      if (res.status === 200) {
        this.telefoniaResumo.contratos = res.data || 0;
      }
    }).catch((error) => {
      this.telefoniaResumo.contratos = 0;
    });

    // 3. Contar Planos
    this.telefoniaApi.contarPlanos(this.session.token).then(res => {
      if (res.status === 200) {
        this.telefoniaResumo.planos = res.data || 0;
      }
    }).catch((error) => {
      this.telefoniaResumo.planos = 0;
    });

    // 4. Contar Linhas
    this.telefoniaApi.contarLinhas(this.session.token).then(res => {
      if (res.status === 200) {
        this.telefoniaResumo.linhas = res.data || 0;
      }
      this.util.aguardar(false);
    }).catch((error) => {
      this.telefoniaResumo.linhas = 0;
      this.util.aguardar(false);
    });
  }

  // 🧭 NAVEGAÇÃO
  navegarParaOperadoras() {
    this.router.navigate(['/operadoras']);
  }

  navegarParaPlanos() {
    this.router.navigate(['/planos']);
  }

  navegarParaLinhas() {
    this.router.navigate(['/linhas']);
  }

  navegarParaContratosTelefonia() {
    this.router.navigate(['/contratos-telefonia']);
  }

  // 📊 AÇÕES
  exportarExcelTelecom() {
    this.mostrarModalExportacao = true;
  }

// 🆕 PROPRIEDADES PARA O MODAL DE EXPORTAÇÃO
  public mostrarModalExportacao = false;

  // 🆕 MÉTODOS DO MODAL DE EXPORTAÇÃO
  fecharModalExportacao() {
    this.mostrarModalExportacao = false;
  }

  async exportarExcel() {
    try {
      this.util.aguardar(true);
      const dadosCompletos = await this.buscarDadosParaExportacao();
      
      if (dadosCompletos) {
        // Exportar para Excel real
        this.exportarParaExcel(dadosCompletos, 'dados_telecom');
        
        this.util.exibirMensagemToast('Exportação Excel concluída com sucesso!', 3000);
        this.fecharModalExportacao();
      }
    } catch (error) {
      console.error('[TELECOM] Erro na exportação Excel:', error);
      this.util.exibirMensagemToast('Erro na exportação Excel', 3000);
    } finally {
      this.util.aguardar(false);
    }
  }

  async exportarCSV() {
    try {
      this.util.aguardar(true);
      const dadosCompletos = await this.buscarDadosParaExportacao();
      
      if (dadosCompletos) {
        // Exportar CSV
        this.exportarParaCSV(dadosCompletos, 'dados_telecom');
        
        this.util.exibirMensagemToast('Exportação CSV concluída com sucesso!', 3000);
        this.fecharModalExportacao();
      }
    } catch (error) {
      console.error('[TELECOM] Erro na exportação CSV:', error);
      this.util.exibirMensagemToast('Erro na exportação CSV', 3000);
    } finally {
      this.util.aguardar(false);
    }
  }

  // 🆕 MÉTODO PARA BUSCAR DADOS COMPLETOS
  private async buscarDadosParaExportacao() {
    try {
      const [operadoras, contratos, planos, linhas] = await Promise.all([
        this.telefoniaApi.listarOperadoras(this.session.token),
        this.telefoniaApi.listarContratos("null", 0, this.session.usuario.cliente, this.session.token),
        this.telefoniaApi.listarPlanos("null", 0, this.session.usuario.cliente, this.session.token),
        this.telefoniaApi.listarLinhasParaExportacao("null", this.session.usuario.cliente, this.session.token)
      ]);
      const resultado = {
        operadoras: operadoras.status === 200 ? operadoras.data : [],
        contratos: contratos.status === 200 ? contratos.data : [],
        planos: planos.status === 200 ? planos.data : [],
        linhas: linhas.status === 200 ? linhas.data : []
      };
      if (resultado.linhas.length > 0) {
        if (resultado.linhas[0].recurso || resultado.linhas[0].recursoNavigation) {
          const recurso = resultado.linhas[0].recurso || resultado.linhas[0].recursoNavigation;
          if (recurso.usuario || recurso.usuarioNavigation) {
          }
        }
      }

      return resultado;
    } catch (error) {
      console.error('[TELECOM] Erro ao buscar dados para exportação:', error);
      console.error('[TELECOM] Stack trace:', error.stack);
      throw error;
    }
  }

  // 🆕 MÉTODO PARA PREPARAR DADOS PARA EXCEL
  private prepararDadosParaExcel(dados: any) {
    const workbook = {
      sheets: {
        'Operadoras': this.prepararDadosOperadoras(dados.operadoras),
        'Contas': this.prepararDadosContratos(dados.contratos),
        'Planos': this.prepararDadosPlanos(dados.planos),
        'Linhas': this.prepararDadosLinhas(dados.linhas)
      }
    };
    
    return workbook;
  }

  // 🆕 MÉTODO PARA PREPARAR DADOS PARA CSV
  private prepararDadosParaCSV(dados: any) {
    return {
      operadoras: this.prepararDadosOperadoras(dados.operadoras),
      contratos: this.prepararDadosContratos(dados.contratos),
      planos: this.prepararDadosPlanos(dados.planos),
      linhas: this.prepararDadosLinhas(dados.linhas)
    };
  }

  // 🆕 MÉTODOS AUXILIARES PARA PREPARAR DADOS
  private prepararDadosOperadoras(operadoras: any[]) {
    if (!operadoras || operadoras.length === 0) return [];
    
    return operadoras.map(op => ({
      'ID': op.id,
      'Nome': op.nome,
      'Status': op.ativo ? 'Ativo' : 'Inativo',
      'Data Criação': op.createdAt ? new Date(op.createdAt).toLocaleDateString('pt-BR') : 'N/A'
    }));
  }

  private prepararDadosContratos(contratos: any[]) {
    if (!contratos || contratos.length === 0) return [];
    
    return contratos.map(ct => ({
      'ID': ct.id,
      'Nome': ct.nome,
      'Descrição': ct.descricao || 'N/A',
      'Operadora': ct.operadoraNavigation?.nome || 'N/A',
      'Status': ct.ativo ? 'Ativo' : 'Inativo',
      'Data Criação': ct.createdAt ? new Date(ct.createdAt).toLocaleDateString('pt-BR') : 'N/A'
    }));
  }

  private prepararDadosPlanos(planos: any[]) {
    if (!planos || planos.length === 0) return [];
    
    return planos.map(pl => ({
      'ID': pl.id,
      'Nome': pl.plano || pl.nome,
      'Valor': pl.valor ? `R$ ${pl.valor.toFixed(2)}` : 'N/A',
      'Conta': pl.contrato || 'N/A',
      'Operadora': pl.operadora || 'N/A',
      'Status': pl.ativo ? 'Ativo' : 'Inativo',
      'Total Linhas': pl.totalLinhas || 0,
      'Linhas em Uso': pl.totalLinhasEmUso || 0,
      'Linhas Livres': pl.totalLinhasLivres || 0
    }));
  }

  private prepararDadosLinhas(linhas: any[]) {
    if (!linhas || linhas.length === 0) return [];
    
    return linhas.map(ln => {
      // Dados da linha
      const dadosLinha: any = {
        'ID Linha': ln.id,
        'Número': ln.numero,
        'ICCID': ln.iccid || 'N/A',
        'Plano': ln.planoNavigation?.nome || ln.planoNavigation?.plano || 'N/A',
        'Conta': ln.planoNavigation?.contratoNavigation?.nome || 'N/A',
        'Operadora': ln.planoNavigation?.contratoNavigation?.operadoraNavigation?.nome || 'N/A',
        'Status Linha': ln.emuso ? 'Em Uso' : 'Livre',
        'Data Criação Linha': ln.createdAt ? new Date(ln.createdAt).toLocaleDateString('pt-BR') : 'N/A'
      };

      // Dados do recurso associado (se houver)
      if (ln.recurso || ln.recursoNavigation) {
        const recurso = ln.recurso || ln.recursoNavigation;
        
        dadosLinha['ID Recurso'] = recurso.id || 'N/A';
        dadosLinha['Número Série'] = recurso.numeroserie || 'N/A';
        dadosLinha['Patrimônio'] = recurso.patrimonio || 'N/A';
        dadosLinha['Tipo Equipamento'] = recurso.tipoequipamento || recurso.tipoRecursoNavigation?.descricao || 'N/A';
        dadosLinha['Fabricante'] = recurso.fabricante || recurso.fabricanteNavigation?.descricao || 'N/A';
        dadosLinha['Modelo'] = recurso.modelo || recurso.modeloNavigation?.descricao || 'N/A';
        
        // Dados do colaborador/usuário associado ao recurso
        if (recurso.usuario || recurso.usuarioNavigation) {
          const usuario = recurso.usuario || recurso.usuarioNavigation;
          
          dadosLinha['Nome Colaborador'] = usuario.nome || 'N/A';
          dadosLinha['CPF'] = usuario.cpf || 'N/A';
          dadosLinha['Matrícula'] = usuario.matricula || 'N/A';
          dadosLinha['Email'] = usuario.email || 'N/A';
          dadosLinha['Telefone'] = usuario.telefone || 'N/A';
          
          // Dados da empresa
          if (usuario.empresa || usuario.empresaNavigation) {
            const empresa = usuario.empresa || usuario.empresaNavigation;
            dadosLinha['Empresa'] = empresa.nome || 'N/A';
          } else {
            dadosLinha['Empresa'] = 'N/A';
          }
          
          // Dados do centro de custo
          if (usuario.centrocusto || usuario.centroCustoNavigation) {
            const centroCusto = usuario.centrocusto || usuario.centroCustoNavigation;
            dadosLinha['Centro de Custo'] = centroCusto.nome || 'N/A';
          } else {
            dadosLinha['Centro de Custo'] = 'N/A';
          }
          
          // Dados da localidade
          if (usuario.localidade || usuario.localidadeNavigation) {
            const localidade = usuario.localidade || usuario.localidadeNavigation;
            dadosLinha['Localidade'] = localidade.descricao || 'N/A';
          } else {
            dadosLinha['Localidade'] = 'N/A';
          }
          
          // Dados adicionais do usuário
          dadosLinha['Cargo'] = usuario.cargo || 'N/A';
          dadosLinha['Departamento'] = usuario.departamento || 'N/A';
          dadosLinha['Status Usuário'] = usuario.ativo ? 'Ativo' : 'Inativo';
        } else {
          // Sem usuário associado ao recurso
          dadosLinha['Nome Colaborador'] = 'N/A';
          dadosLinha['CPF'] = 'N/A';
          dadosLinha['Matrícula'] = 'N/A';
          dadosLinha['Email'] = 'N/A';
          dadosLinha['Telefone'] = 'N/A';
          dadosLinha['Empresa'] = 'N/A';
          dadosLinha['Centro de Custo'] = 'N/A';
          dadosLinha['Localidade'] = 'N/A';
          dadosLinha['Cargo'] = 'N/A';
          dadosLinha['Departamento'] = 'N/A';
          dadosLinha['Status Usuário'] = 'N/A';
        }
      } else {
        // Sem recurso associado à linha
        dadosLinha['ID Recurso'] = 'N/A';
        dadosLinha['Número Série'] = 'N/A';
        dadosLinha['Patrimônio'] = 'N/A';
        dadosLinha['Tipo Equipamento'] = 'N/A';
        dadosLinha['Fabricante'] = 'N/A';
        dadosLinha['Modelo'] = 'N/A';
        dadosLinha['Nome Colaborador'] = 'N/A';
        dadosLinha['CPF'] = 'N/A';
        dadosLinha['Matrícula'] = 'N/A';
        dadosLinha['Email'] = 'N/A';
        dadosLinha['Telefone'] = 'N/A';
        dadosLinha['Empresa'] = 'N/A';
        dadosLinha['Centro de Custo'] = 'N/A';
        dadosLinha['Localidade'] = 'N/A';
        dadosLinha['Cargo'] = 'N/A';
        dadosLinha['Departamento'] = 'N/A';
        dadosLinha['Status Usuário'] = 'N/A';
      }

      return dadosLinha;
    });
  }

  // 🆕 MÉTODOS DE EXPORTAÇÃO
  private exportarParaExcel(dados: any, nomeArquivo: string) {
    try {
      const dadosExcel = this.prepararDadosParaExcel(dados);
      
      // Criar um novo workbook
      const wb = XLSX.utils.book_new();
      
      // Adicionar cada planilha
      Object.keys(dadosExcel.sheets).forEach(sheetName => {
        const sheetData = dadosExcel.sheets[sheetName];
        if (sheetData && sheetData.length > 0) {
          const ws = XLSX.utils.json_to_sheet(sheetData);
          XLSX.utils.book_append_sheet(wb, ws, sheetName);
        }
      });
      
      // Gerar o arquivo e fazer download
      const excelBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
      this.downloadArquivo(excelBuffer, `${nomeArquivo}.xlsx`, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet');
    } catch (error) {
      console.error('[TELECOM] Erro na exportação Excel:', error);
      console.error('[TELECOM] Stack trace:', error.stack);
      throw error;
    }
  }

  private exportarParaCSV(dados: any, nomeArquivo: string) {
    try {
      const csvContent = this.converterParaCSV(dados);
      
      // Fazer download do arquivo
      this.downloadArquivo(csvContent, `${nomeArquivo}.csv`, 'text/csv');
    } catch (error) {
      console.error('[TELECOM] Erro na exportação CSV:', error);
      throw error;
    }
  }

  // 🆕 MÉTODOS AUXILIARES PARA EXPORTAÇÃO
  private converterParaCSV(dados: any): string {
    try {
      if (!dados || Object.keys(dados).length === 0) {
        return '';
      }
      
      let csvContent = '';
      
      // Para cada entidade (operadoras, contratos, planos, linhas)
      Object.keys(dados).forEach(entidade => {
        const dadosEntidade = dados[entidade];
        if (dadosEntidade && dadosEntidade.length > 0) {
          csvContent += `\n\n=== ${entidade.toUpperCase()} ===\n`;
          
          // Cabeçalhos
          const headers = Object.keys(dadosEntidade[0]);
          csvContent += headers.join(',') + '\n';
          
          // Dados
          for (const row of dadosEntidade) {
            const values = headers.map(header => {
              const value = row[header];
              return `"${value || ''}"`;
            });
            csvContent += values.join(',') + '\n';
          }
        }
      });
      return csvContent;
    } catch (error) {
      console.error('[TELECOM] Erro ao converter para CSV:', error);
      throw error;
    }
  }

  private downloadArquivo(conteudo: any, nomeArquivo: string, tipoMime: string): void {
    try {
      const blob = new Blob([conteudo], { type: tipoMime });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = nomeArquivo;
      link.style.display = 'none';
      
      // Adicionar ao DOM, clicar e remover
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      
      // Limpar URL do blob
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('[TELECOM] Erro ao fazer download do arquivo:', error);
      console.error('[TELECOM] Stack trace:', error.stack);
      throw error;
    }
  }

  // 📤 MÉTODOS DO MODAL DE IMPORTAÇÃO
  abrirModalImportacao() {
    this.mostrarModalImportacao = true;
    this.resetarImportacao();
  }

  fecharModalImportacao() {
    this.mostrarModalImportacao = false;
    // NÃO chamar resetarImportacao() aqui para não perder o loteId!
    // this.resetarImportacao();
  }

  baixarTemplate(): void {
    const url = this.importacaoService.getUrlTemplate();
    window.open(url, '_blank');
  }

  onArquivoSelecionadoImportacao(event: any): void {
    const arquivo: File = event.target.files[0];
    
    if (!arquivo) {
      return;
    }

    // Validar extensão
    const extensoesValidas = ['.xlsx', '.xls'];
    const extensao = arquivo.name.substring(arquivo.name.lastIndexOf('.')).toLowerCase();
    
    if (!extensoesValidas.includes(extensao)) {
      this.util.exibirMensagemToast('Formato de arquivo inválido. Use apenas arquivos Excel (.xlsx, .xls)', 5000);
      event.target.value = '';
      return;
    }

    // Validar tamanho (10MB)
    const tamanhoMaximo = 10 * 1024 * 1024;
    if (arquivo.size > tamanhoMaximo) {
      this.util.exibirMensagemToast('Arquivo muito grande. Tamanho máximo: 10MB', 5000);
      event.target.value = '';
      return;
    }

    this.arquivoSelecionadoImport = arquivo;
    
    // Fazer upload automaticamente
    this.fazerUploadImportacao();
  }

  fazerUploadImportacao(): void {
    if (!this.arquivoSelecionadoImport) {
      this.util.exibirMensagemToast('Selecione um arquivo primeiro', 3000);
      return;
    }

    this.uploadandoImportacao = true;
    this.passoImportacao = 2;

    this.importacaoService.uploadArquivo(this.arquivoSelecionadoImport).subscribe({
      next: (resultado) => {
        this.resultadoValidacaoImport = resultado;
        
        // Tentar todas as possibilidades
        this.loteAtualImport = resultado.loteId || resultado['loteId'] || resultado['LoteId'] || null;
        this.uploadandoImportacao = false;
        
        if (resultado.podeImportar || resultado['podeImportar'] || resultado['PodeImportar']) {
          this.util.exibirMensagemToast('✅ ' + (resultado.mensagem || resultado['mensagem'] || resultado['Mensagem']), 5000);
        } else {
          this.util.exibirMensagemToast('⚠️ ' + (resultado.mensagem || resultado['mensagem'] || resultado['Mensagem']), 5000);
        }
      },
      error: (erro) => {
        this.uploadandoImportacao = false;
        this.passoImportacao = 1;
        const mensagem = erro.error?.mensagem || 'Erro ao processar arquivo';
        this.util.exibirMensagemToast('❌ ' + mensagem, 5000);
        console.error('Erro no upload:', erro);
      }
    });
  }

  confirmarImportacao(): void {
    if (!this.loteAtualImport) {
      console.error('[TELECOM] ❌ loteAtualImport está vazio!');
      this.util.exibirMensagemToast('❌ Erro: Lote de importação não identificado. Tente fazer o upload novamente.', 5000);
      return;
    }

    // FECHAR o modal de importação ANTES de mostrar o modal de confirmação
    // Isso garante que o modal de confirmação fique visível
    this.fecharModalImportacao();

    // Aguardar um frame para garantir que o modal foi fechado
    setTimeout(() => {
      // Debug: ver o que está vindo
      // Usar totalRegistros como principal, pois totalValidados pode ser 0 dependendo da lógica de validação
      const totalLinhas = this.resultadoValidacaoImport?.totalRegistros || 
                          this.resultadoValidacaoImport?.totalValidos || 0;
      const totalErros = this.resultadoValidacaoImport?.totalErros || 0;
      let mensagemConfirmacao = `
        <strong>Confirma a importação de ${totalLinhas} linha(s) telefônica(s)?</strong><br><br>
      `;
      
      if (totalErros > 0) {
        mensagemConfirmacao += `⚠️ Atenção: ${totalErros} linha(s) com erro(s) serão ignoradas.<br><br>`;
      }
      
      mensagemConfirmacao += `
        Esta ação criará novos registros no banco de dados.<br><br>
        ⚠️ <strong>Atenção:</strong> Esta operação não poderá ser desfeita.
      `;

      this.util.exibirMensagemPopUp(mensagemConfirmacao, true).then(res => {
        if (res) {
          // Iniciar processo com modal de progresso
          this.iniciarProcessoProgresso();
          this.importacaoService.confirmarImportacao(this.loteAtualImport).subscribe({
            next: (resultado) => {
              this.resultadoImportacaoFinal = resultado;
              this.finalizarProcessoProgresso(true, '✅ ' + resultado.mensagem);
              
              // Recarregar contadores
              this.carregarResumoTelefonia();
            },
            error: (erro) => {
              const mensagem = erro.error?.mensagem || 'Erro ao importar dados';
              this.finalizarProcessoProgresso(false, '❌ ' + mensagem);
              console.error('Erro na importação:', erro);
            }
          });
        } else {
          // Se cancelar, reabrir modal de importação
          this.mostrarModalImportacao = true;
        }
      });
    }, 100);
  }

  cancelarImportacao(): void {
    if (!this.loteAtualImport) {
      this.resetarImportacao();
      return;
    }

    // FECHAR o modal de importação ANTES de mostrar o modal de confirmação
    this.fecharModalImportacao();

    // Aguardar um frame para garantir que o modal foi fechado
    setTimeout(() => {
      // Usar o padrão do sistema
      const mensagemConfirmacao = `
        <strong>Deseja cancelar esta importação?</strong><br><br>
        Os dados validados serão descartados e não será possível recuperá-los.<br><br>
        ⚠️ <strong>Atenção:</strong> Esta ação não poderá ser desfeita.
      `;

      this.util.exibirMensagemPopUp(mensagemConfirmacao, true).then(res => {
        if (res) {
          this.importacaoService.cancelarImportacao(this.loteAtualImport).subscribe({
            next: () => {
              this.util.exibirMensagemToast('ℹ️ Importação cancelada', 3000);
              this.resetarImportacao();
            },
            error: (erro) => {
              console.error('Erro ao cancelar:', erro);
              this.resetarImportacao();
            }
          });
        } else {
          // Se não cancelar, reabrir modal de importação
          this.mostrarModalImportacao = true;
        }
      });
    }, 100);
  }

  resetarImportacao(): void {
    this.passoImportacao = 1;
    this.arquivoSelecionadoImport = null;
    this.loteAtualImport = null;
    this.resultadoValidacaoImport = null;
    this.resultadoImportacaoFinal = null;
    this.uploadandoImportacao = false;
    this.importandoImportacao = false;
  }

  // 📊 MÉTODOS PARA CONTROLE DO PROGRESSO
  iniciarProcessoProgresso(): void {
    const totalLinhas = this.resultadoValidacaoImport?.totalValidados || 0;
    
    // Inicializar dados do progresso
    this.progresso = {
      percentual: 0,
      linhasProcessadas: 0,
      totalLinhas: totalLinhas,
      tempoEstimado: this.estimarTempoProcessamento(totalLinhas),
      tempoDecorrido: 0,
      status: 'Iniciando importação...',
      lote: this.loteAtualImport || ''
    };

    // Mostrar modal de progresso
    this.mostrarModalProgresso = true;

    // Iniciar contador de tempo
    this.iniciarContadorTempo();

    // Simular progresso
    this.simularProgresso();
  }

  estimarTempoProcessamento(totalLinhas: number): number {
    // Estimativa: 0.15 segundos por linha + 3 segundos base
    return Math.max(5, Math.floor(totalLinhas * 0.15 + 3));
  }

  iniciarContadorTempo(): void {
    this.intervaloProgresso = setInterval(() => {
      this.progresso.tempoDecorrido++;
      
      // Atualizar percentual baseado no tempo decorrido
      if (this.progresso.tempoEstimado > 0) {
        const percentualTempo = Math.min(95, (this.progresso.tempoDecorrido / this.progresso.tempoEstimado) * 100);
        this.progresso.percentual = Math.max(this.progresso.percentual, percentualTempo);
      }
    }, 1000);
  }

  simularProgresso(): void {
    const totalLinhas = this.progresso.totalLinhas;
    let processadas = 0;
    
    const intervaloSimulacao = setInterval(() => {
      processadas += Math.max(1, Math.floor(totalLinhas / 50)); // Processar em lotes
      
      if (processadas >= totalLinhas) {
        processadas = totalLinhas;
        clearInterval(intervaloSimulacao);
      }
      
      this.progresso.linhasProcessadas = processadas;
      this.progresso.percentual = Math.min(95, (processadas / totalLinhas) * 100);
      
      // Atualizar status
      if (processadas < totalLinhas) {
        this.progresso.status = `Importando linhas... ${processadas}/${totalLinhas}`;
      } else {
        this.progresso.status = 'Finalizando importação...';
      }
    }, 200);
  }

  finalizarProcessoProgresso(sucesso: boolean, mensagem: string): void {
    // Parar contador de tempo
    if (this.intervaloProgresso) {
      clearInterval(this.intervaloProgresso);
    }

    // Finalizar progresso
    this.progresso.percentual = 100;
    this.progresso.status = sucesso ? 'Importação concluída!' : 'Erro na importação';
    this.progresso.linhasProcessadas = this.progresso.totalLinhas;

    // Mostrar resultado
    setTimeout(() => {
      this.mostrarModalProgresso = false;
      
      // Resetar estado apenas após concluir
      this.resetarImportacao();
      
      if (sucesso) {
        this.util.exibirMensagemToast(mensagem, 10000);
      } else {
        this.util.exibirMensagemToast(mensagem, 8000);
      }
    }, 2000);
  }

  calcularTempoRestante(): number {
    if (this.progresso.percentual >= 100) return 0;
    
    const tempoRestante = this.progresso.tempoEstimado - this.progresso.tempoDecorrido;
    return Math.max(0, tempoRestante);
  }

  ngOnDestroy(): void {
    // Limpar intervalos para evitar memory leaks
    if (this.intervaloProgresso) {
      clearInterval(this.intervaloProgresso);
    }
  }
}
