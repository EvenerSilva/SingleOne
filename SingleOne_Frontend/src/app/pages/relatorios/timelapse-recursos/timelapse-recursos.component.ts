import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs/internal/Observable';
import { debounceTime, tap } from 'rxjs/operators';
import { EquipamentoApiService } from 'src/app/api/equipamentos/equipamento-api.service';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import * as moment from 'moment';

@Component({
  selector: 'app-timelapse-recursos',
  templateUrl: './timelapse-recursos.component.html',
  styleUrls: ['./timelapse-recursos.component.scss']
})
export class TimelapseRecursosComponent implements OnInit {

  private session:any = {};
  public recurso:any = {};
  public recursos:any = [];
  public historicos:any = [];
  public buscarRecursos = new FormControl();
  public recursoSelecionado = new FormControl();
  public resultadoRecursos: Observable<any>;

  constructor(private util: UtilService, private apiEqp: EquipamentoApiService, private api: RelatorioApiService, private route: ActivatedRoute, private router: Router) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.recurso.cliente = this.session.usuario.cliente;

    this.route.queryParams.subscribe(params => {
      // ✅ CORREÇÃO: Aceitar busca por S/N (sn) em vez de ID
      if (params['sn']) {
        const numeroSerie = params['sn'];
        this.util.aguardar(true);
        Promise.all([
          this.apiEqp.listarEquipamentos(numeroSerie, null, 1, 10),
          this.api.listarLinhasTelefonicas(numeroSerie, this.session.usuario.cliente, this.session.token)
        ]).then(([equipamentosRes, linhasRes]) => {
          this.util.aguardar(false);
          
          let recursoEncontrado = null;
          
          // Buscar em equipamentos
          if (equipamentosRes.status === 200 || equipamentosRes.status === 204) {
            const equipamentos = equipamentosRes.data.results || [];
            recursoEncontrado = equipamentos.find(eq => eq.numeroserie === numeroSerie);
          }
          
          // Se não encontrou em equipamentos, buscar em linhas
          if (!recursoEncontrado && (linhasRes.status === 200 || linhasRes.status === 204)) {
            const linhas = linhasRes.data || [];
            // Buscar por ICCID (numeroserie das linhas) OU por número de telefone
            const linhaEncontrada = linhas.find(linha => 
              linha.numeroserie === numeroSerie || 
              linha.iccid === numeroSerie ||
              linha.numero?.toString() === numeroSerie
            );
            if (linhaEncontrada) {
              recursoEncontrado = {
                id: linhaEncontrada.equipamentoid,
                tipoequipamento: linhaEncontrada.tipoequipamento,
                fabricante: linhaEncontrada.fabricante,
                modelo: linhaEncontrada.modelo,
                numeroserie: linhaEncontrada.numeroserie || linhaEncontrada.iccid,
                patrimonio: linhaEncontrada.patrimonio || linhaEncontrada.numero?.toString(),
                isLinhaTelefonica: true
              };
            }
          }
          
          if (recursoEncontrado) {
            this.recurso.id = recursoEncontrado.id;
            this.recursoSelecionado.setValue(recursoEncontrado.id);
            this.recursos = [recursoEncontrado];
            this.consultarHistorico();
          } else {
            this.util.exibirMensagemToast('Recurso não encontrado', 4000);
          }
        }).catch(err => {
          this.util.aguardar(false);
          console.error('[TIMELINE-RECURSOS] Erro ao buscar recurso:', err);
          this.util.exibirFalhaComunicacao();
        });
      } else if (params['id']) {
        // ⚠️ DEPRECIADO: Manter compatibilidade com links antigos que usam ID
        console.warn('[TIMELINE-RECURSOS] ⚠️ Usando ID (depreciado). Migrar para S/N.');
        this.recurso.id = params['id'];
        this.recursoSelecionado.setValue(params['id']);
        this.consultarHistorico();
      }
    });
    
    /*Busca de recursos*/
    this.resultadoRecursos = this.buscarRecursos.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscarRecursosDisponiveis(value))
    );
    this.resultadoRecursos.subscribe();
    
    // Inicializar busca vazia para carregar todos os recursos
    this.buscarRecursosDisponiveis('');
  }

  buscarRecursosDisponiveis(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      
      // ✅ NOVO: Buscar equipamentos e linhas telefônicas em paralelo
      Promise.all([
        this.apiEqp.listarEquipamentos(valor, null, 1, 10),
        this.api.listarLinhasTelefonicas(valor, this.session.usuario.cliente, this.session.token)
      ]).then(([equipamentosRes, linhasRes]) => {
        this.util.aguardar(false);
        
        let recursosCombinados = [];
        
        // Processar equipamentos físicos
        if (equipamentosRes.status === 200 || equipamentosRes.status === 204) {
          const equipamentos = equipamentosRes.data.results || [];
          recursosCombinados = recursosCombinados.concat(equipamentos);
        }
        
        // Processar linhas telefônicas
        if (linhasRes.status === 200 || linhasRes.status === 204) {
          const linhas = linhasRes.data || [];
          // Converter linhas para o formato de equipamentos para compatibilidade
          const linhasFormatadas = linhas.map(linha => ({
            id: linha.equipamentoid || linha.id,
            tipoequipamento: linha.tipoequipamento || 'Linha Telefônica',
            fabricante: linha.fabricante || linha.operadora,
            modelo: linha.modelo || linha.plano,
            numeroserie: linha.numeroserie || linha.iccid,
            patrimonio: linha.patrimonio || linha.numero?.toString(),
            numero: linha.numero,
            iccid: linha.iccid,
            isLinhaTelefonica: true
          }));
          recursosCombinados = recursosCombinados.concat(linhasFormatadas);
        }
        
        this.recursos = recursosCombinados;
      }).catch(err => {
        this.util.aguardar(false);
        this.util.exibirFalhaComunicacao();
      });
    }
  }

  onRecursoSelecionado(event: any) {
    this.recurso.id = event.value;
    // Limpar o campo de busca para mostrar apenas o item selecionado
    this.buscarRecursos.setValue('');
  }

  getRecursoSelecionadoTexto(): string {
    if (this.recurso.id && this.recursos.length > 0) {
      const recurso = this.recursos.find(eq => eq.id === this.recurso.id);
      if (recurso) {
        // ✅ NOVO: Formatação diferenciada para linhas telefônicas
        if (recurso.isLinhaTelefonica) {
          return `${recurso.tipoequipamento} - ${recurso.fabricante} | Número: ${recurso.numeroserie}`;
        } else {
          return `${recurso.tipoequipamento} ${recurso.fabricante} ${recurso.modelo} - S/N: ${recurso.numeroserie} - Patrimônio: ${recurso.patrimonio || '-'}`;
        }
      }
    }
    return '';
  }

  consultarHistorico() {
    if(this.recurso.id != undefined) {
      // ✅ CORREÇÃO CRÍTICA: Buscar o recurso selecionado para obter o número de série
      const recursoSelecionado = this.recursos.find(r => r.id === this.recurso.id);
      
      if (!recursoSelecionado || !recursoSelecionado.numeroserie) {
        this.util.exibirMensagemToast('Não foi possível identificar o número de série do recurso', 4000);
        return;
      }
      
      const numeroSerie = recursoSelecionado.numeroserie;
      this.util.aguardar(true);
      // ✅ USAR O NOVO ENDPOINT QUE BUSCA POR NÚMERO DE SÉRIE
      this.api.historicoEquipamentosPorNumeroSerie(numeroSerie, this.session.token).then(res => {
        this.util.aguardar(false);
        
        if (!res.data || res.data.length === 0) {
          this.util.exibirMensagemToast('Nenhum histórico encontrado para este recurso', 4000);
          this.historicos = [];
          return;
        }
        
        // ✅ NOVO: Detectar tipo de recurso (equipamento vs linha telefônica)
        const primeiroItem = res.data[0];
        const isLinhaTelefonica = primeiroItem.tipoequipamento?.toLowerCase().includes('linha');
        
        if (isLinhaTelefonica) {
          // ✅ NOVO: Formatação para linhas telefônicas
          this.recurso.descricao = `${primeiroItem.tipoequipamento} - ${primeiroItem.fabricante} | Número: ${primeiroItem.numeroserie}`;
        } else {
          // ✅ Formatação original para equipamentos físicos
          this.recurso.descricao = primeiroItem.tipoequipamento + ' ' + primeiroItem.fabricante + ' ' + primeiroItem.modelo + ' S/N:' + primeiroItem.numeroserie + ' Patrimônio:' + ((primeiroItem.patrimonio == "") ? "-" : primeiroItem.patrimonio);
        }
        
        this.historicos = [];
        
        // Formatar dados de forma mais detalhada
        res.data.map((x, index) => {
          // ✅ NOVO: Detectar tipo de item no histórico
          const isLinhaItem = x.tipoequipamento?.toLowerCase().includes('linha');
          
          const historico = {
            date: moment(x.dtregistro).format("DD/MM/YYYY HH:mm"),
            title: x.equipamentostatus + ((x.colaborador != null) ? ' (Colaborador: ' + x.colaborador + ')' : ''),
            content: x.usuario,
            status: x.equipamentostatus,
            colaborador: x.colaborador,
            usuario: x.usuario,
            // ✅ NOVO: Adicionar dados do responsável provisório
            tecnicoresponsavel: x.tecnicoresponsavel,
            tecnicoresponsavelid: x.tecnicoresponsavelid,
            dataFormatada: moment(x.dtregistro).format("DD/MM/YYYY"),
            horaFormatada: moment(x.dtregistro).format("HH:mm"),
            index: index + 1,
            // ✅ NOVO: Adicionar informações específicas do tipo de recurso
            isLinhaTelefonica: isLinhaItem,
            tipoRecurso: isLinhaItem ? 'linha' : 'equipamento',
            fabricante: x.fabricante,
            modelo: x.modelo,
            numeroserie: x.numeroserie,
            patrimonio: x.patrimonio
          };
          this.historicos.push(historico);
        });
        
        // Ordenar por data mais recente primeiro
        this.historicos.sort((a, b) => moment(b.date, "DD/MM/YYYY HH:mm").diff(moment(a.date, "DD/MM/YYYY HH:mm")));
      }).catch(err => {
        this.util.aguardar(false);
        console.error('[TIMELINE-RECURSOS] Erro ao buscar histórico:', err);
        this.util.exibirFalhaComunicacao();
      });
    }
    else {
      this.util.exibirMensagemToast('Selecione um recurso para consultar o seu histórico', 5000);
    }
  }

  /**
   * Retorna a classe CSS baseada no status do recurso
   */
  public getStatusClass(status: string): string {
    if (!status) return '';
    
    const statusLower = status.toLowerCase();
    
    if (statusLower.includes('estoque') || statusLower.includes('disponível')) {
      return 'status-em-estoque';
    } else if (statusLower.includes('requisitado') || statusLower.includes('solicitado')) {
      return 'status-requisitado';
    } else if (statusLower.includes('danificado') || statusLower.includes('defeito')) {
      return 'status-danificado';
    } else if (statusLower.includes('uso') || statusLower.includes('atribuído')) {
      return 'status-em-uso';
    } else if (statusLower.includes('manutenção') || statusLower.includes('manutencao')) {
      return 'status-manutencao';
    }
    
    return '';
  }

  /**
   * Imprime o timeline de recursos em formato otimizado
   */
  public imprimirTimeline(): void {
    if (this.historicos.length === 0) {
      this.util.exibirMensagemToast('Nenhum histórico para imprimir', 3000);
      return;
    }

    // Criar uma nova janela para impressão
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      this.util.exibirMensagemToast('Erro ao abrir janela de impressão', 3000);
      return;
    }

    // Gerar conteúdo HTML para impressão
    const printContent = this.gerarConteudoImpressao();
    
    printWindow.document.write(printContent);
    printWindow.document.close();
    
    // Aguardar o carregamento e imprimir
    printWindow.onload = () => {
      printWindow.print();
      printWindow.close();
    };
  }

  /**
   * Gera o conteúdo HTML otimizado para impressão
   */
  private gerarConteudoImpressao(): string {
    const dataAtual = moment().format('DD/MM/YYYY HH:mm');
    const totalRegistros = this.historicos.length;
    
    let historicosHTML = '';
    this.historicos.forEach((item, index) => {
      const statusClass = this.getStatusClass(item.status);
      const statusColor = this.getStatusColor(item.status);
      
      historicosHTML += `
        <tr class="timeline-row ${statusClass}">
          <td class="timeline-number">${item.index}</td>
          <td class="timeline-status" style="color: ${statusColor}">${item.status}</td>
          <td class="timeline-date">${item.dataFormatada} ${item.horaFormatada}</td>
          <td class="timeline-user">${item.usuario || '-'}</td>
          <td class="timeline-colaborador">${item.colaborador || '-'}</td>
        </tr>
      `;
    });

    return `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <title>Timeline de Recursos - ${this.recurso.descricao}</title>
        <style>
          @media print {
            @page {
              margin: 1.5cm;
              size: A4;
            }
          }
          
          body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 12px;
            line-height: 1.4;
            color: #333;
            margin: 0;
            padding: 20px;
          }
          
          .print-header {
            text-align: center;
            margin-bottom: 30px;
            border-bottom: 2px solid #FF3A0F;
            padding-bottom: 20px;
          }
          
          .print-title {
            font-size: 24px;
            font-weight: bold;
            color: #FF3A0F;
            margin: 0 0 10px 0;
          }
          
          .print-subtitle {
            font-size: 16px;
            color: #666;
            margin: 0 0 15px 0;
          }
          
          .print-info {
            display: flex;
            justify-content: space-between;
            margin-bottom: 20px;
            font-size: 11px;
            color: #666;
          }
          
          .print-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
          }
          
          .print-table th {
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            padding: 12px 8px;
            text-align: left;
            font-weight: bold;
            color: #495057;
            font-size: 11px;
          }
          
          .print-table td {
            border: 1px solid #dee2e6;
            padding: 10px 8px;
            font-size: 11px;
          }
          
          .timeline-number {
            text-align: center;
            font-weight: bold;
            color: #FF3A0F;
            width: 50px;
          }
          
          .timeline-status {
            font-weight: bold;
            width: 120px;
          }
          
          .timeline-date {
            width: 100px;
            text-align: center;
          }
          
          .timeline-user {
            width: 120px;
          }
          
          .timeline-colaborador {
            width: 120px;
          }
          
          .status-em-estoque {
            background: rgba(40, 167, 69, 0.1);
          }
          
          .status-requisitado {
            background: rgba(255, 193, 7, 0.1);
          }
          
          .status-danificado {
            background: rgba(220, 53, 69, 0.1);
          }
          
          .status-em-uso {
            background: rgba(0, 123, 255, 0.1);
          }
          
          .status-manutencao {
            background: rgba(253, 126, 20, 0.1);
          }
          
          .print-summary {
            margin-top: 30px;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 8px;
            border: 1px solid #dee2e6;
          }
          
          .print-summary h3 {
            margin: 0 0 15px 0;
            color: #FF3A0F;
            font-size: 16px;
          }
          
          .summary-stats {
            display: flex;
            gap: 30px;
            flex-wrap: wrap;
          }
          
          .summary-item {
            text-align: center;
          }
          
          .summary-number {
            font-size: 18px;
            font-weight: bold;
            color: #FF3A0F;
          }
          
          .summary-label {
            font-size: 11px;
            color: #666;
            margin-top: 5px;
          }
          
          .print-footer {
            margin-top: 30px;
            text-align: center;
            font-size: 10px;
            color: #999;
            border-top: 1px solid #dee2e6;
            padding-top: 15px;
          }
          
          @media print {
            .no-print {
              display: none;
            }
          }
        </style>
      </head>
      <body>
        <div class="print-header">
          <h1 class="print-title">Timeline de Recursos</h1>
          <p class="print-subtitle">${this.recurso.descricao}</p>
        </div>
        
        <div class="print-info">
          <span><strong>Data de Impressão:</strong> ${dataAtual}</span>
          <span><strong>Total de Registros:</strong> ${totalRegistros}</span>
        </div>
        
        <table class="print-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Status</th>
              <th>Data/Hora</th>
              <th>Usuário</th>
              <th>Colaborador</th>
            </tr>
          </thead>
          <tbody>
            ${historicosHTML}
          </tbody>
        </table>
        
        <div class="print-summary">
          <h3>Resumo por Status</h3>
          <div class="summary-stats">
            ${this.gerarResumoStatus()}
          </div>
        </div>
        
        <div class="print-footer">
          <p>Documento gerado automaticamente pelo sistema SingleOne</p>
          <p>Página impressa em ${dataAtual}</p>
        </div>
      </body>
      </html>
    `;
  }

  /**
   * Gera o resumo estatístico por status
   */
  private gerarResumoStatus(): string {
    const resumo = {
      'Em Estoque': 0,
      'Requisitado': 0,
      'Danificado': 0,
      'Em Uso': 0,
      'Manutenção': 0,
      'Outros': 0
    };

    this.historicos.forEach(item => {
      const status = item.status?.toLowerCase() || '';
      if (status.includes('estoque')) resumo['Em Estoque']++;
      else if (status.includes('requisitado')) resumo['Requisitado']++;
      else if (status.includes('danificado')) resumo['Danificado']++;
      else if (status.includes('uso')) resumo['Em Uso']++;
      else if (status.includes('manutenção') || status.includes('manutencao')) resumo['Manutenção']++;
      else resumo['Outros']++;
    });

    let resumoHTML = '';
    Object.entries(resumo).forEach(([status, quantidade]) => {
      if (quantidade > 0) {
        resumoHTML += `
          <div class="summary-item">
            <div class="summary-number">${quantidade}</div>
            <div class="summary-label">${status}</div>
          </div>
        `;
      }
    });

    return resumoHTML;
  }

  /**
   * Retorna a cor do status para impressão
   */
  private getStatusColor(status: string): string {
    if (!status) return '#666';
    
    const statusLower = status.toLowerCase();
    
    if (statusLower.includes('estoque') || statusLower.includes('disponível')) {
      return '#28a745';
    } else if (statusLower.includes('requisitado') || statusLower.includes('solicitado')) {
      return '#ffc107';
    } else if (statusLower.includes('danificado') || statusLower.includes('defeito')) {
      return '#dc3545';
    } else if (statusLower.includes('uso') || statusLower.includes('atribuído')) {
      return '#007bff';
    } else if (statusLower.includes('manutenção') || statusLower.includes('manutencao')) {
      return '#fd7e14';
    }
    
    return '#666';
  }

  /**
   * Limpar todos os filtros e parâmetros de busca
   */
  limparFiltros() {
    this.buscarRecursos.setValue('');
    this.recursoSelecionado.setValue(null);
    
    // Limpar dados locais
    this.recurso = {
      cliente: this.session.usuario.cliente
    };
    this.recursos = [];
    this.historicos = [];
    
    // Navegar para rota limpa (sem query params)
    this.router.navigate(['relatorios/timeline-recursos'], {
      queryParams: {}
    });
    
    // Notificar usuário
    this.util.exibirMensagemToast('Filtros limpos! Selecione um recurso para ver o histórico.', 3000);
    
    // Recarregar lista de recursos inicial
    this.buscarRecursosDisponiveis('');
  }

}
