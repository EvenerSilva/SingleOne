import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs/internal/Observable';
import { debounceTime, tap } from 'rxjs/operators';
import { RelatorioApiService } from 'src/app/api/relatorios/relatorio-api.service';
import { UtilService } from 'src/app/util/util.service';
import * as moment from 'moment';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';

@Component({
  selector: 'app-timelapse-colaboradores',
  templateUrl: './timelapse-colaboradores.component.html',
  styleUrls: ['./timelapse-colaboradores.component.scss']
})
export class TimelapseColaboradoresComponent implements OnInit {

  private session: any = {};
  public colaborador: any = {};
  public colaboradores: any = [];
  public historicos: any = [];
  public buscarColaboradores = new FormControl();
  public colaboradorSelecionado = new FormControl();
  public resultadoColaboradores: Observable<any>;

  constructor(
    private util: UtilService,
    private apiCol: ColaboradorApiService,
    private api: RelatorioApiService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.session = this.util.getSession('usuario');
    this.colaborador.cliente = this.session.usuario.cliente;

    this.route.queryParams.subscribe(params => {
      if (params['id']) {
        this.colaborador.id = params['id'];
        this.colaboradorSelecionado.setValue(params['id']);
        this.consultarHistorico();
      }
    });

    /*Busca de colaboradores*/
    this.resultadoColaboradores = this.buscarColaboradores.valueChanges.pipe(
      debounceTime(1000),
      tap(value => this.buscarColaboradoresDisponiveis(value))
    );
    this.resultadoColaboradores.subscribe();
    
    // Inicializar busca vazia para carregar todos os colaboradores
    this.buscarColaboradoresDisponiveis('');
  }

  buscarColaboradoresDisponiveis(valor) {
    if (valor != '') {
      this.util.aguardar(true);
      this.apiCol.pesquisarColaboradores(valor, this.colaborador.cliente, this.session.token).then(res => {
        this.util.aguardar(false);
        if (res.status != 200 && res.status != 204) {
          this.util.exibirFalhaComunicacao();
        }
        else {
          this.colaboradores = res.data;
        }
      })
    }
  }

  onColaboradorSelecionado(event: any) {
    this.colaborador.id = event.value;
    // Limpar o campo de busca para mostrar apenas o item selecionado
    this.buscarColaboradores.setValue('');
  }

  getColaboradorSelecionadoTexto(): string {
    if (this.colaborador.id && this.colaboradores.length > 0) {
      const colaborador = this.colaboradores.find(col => col.id === this.colaborador.id);
      if (colaborador) {
        return `${colaborador.nome} - Matrícula: ${colaborador.matricula}`;
      }
    }
    return '';
  }

  consultarHistorico() {
    if (this.colaborador.id != undefined) {
      this.util.aguardar(true);
      this.api.equipamentoComColaboradores(this.colaborador.id, this.session.token).then(res => {
        this.util.aguardar(false);
        this.historicos = [];
        if (res.data.length > 0) {
          this.colaborador.descricao = res.data[0].requisicao.colaboradorfinal;
          
          // Agrupar por requisição
          res.data.map(x => {
            // Criar lista de equipamentos para esta requisição
            let equipamentos = [];
            x.equipamentosRequisicao.map(eqp => {
              // ✅ CORREÇÃO: Usar dtdevolucao para determinar status real
              // Se dtdevolucao tem valor = Devolvido, se não tem = Ativo
              let isDevolvido = eqp.dtdevolucao != null && eqp.dtdevolucao != '';
              let isAtivo = !isDevolvido;
              let equipamentoInfo = {
                texto: '',
                devolvido: isDevolvido,
                ativo: isAtivo,
                tipo: eqp.equipamento.toLowerCase().includes('linha') ? 'linha' : 'equipamento'
              };
              
              if (equipamentoInfo.tipo === 'linha') {
                equipamentoInfo.texto = `${eqp.equipamento} - Entrega: ${moment(eqp.dtentrega).format("DD/MM/YYYY HH:mm")}${(isDevolvido ? ' | Devolução: ' + moment(eqp.dtdevolucao).format("DD/MM/YYYY HH:mm") : '')}`;
              } else {
                equipamentoInfo.texto = `${eqp.equipamento} S/N: ${eqp.numeroserie} Patrimônio: ${eqp.patrimonio} - Entrega: ${moment(eqp.dtentrega).format("DD/MM/YYYY HH:mm")}${(isDevolvido ? ' | Devolução: ' + moment(eqp.dtdevolucao).format("DD/MM/YYYY HH:mm") : '')}`;
              }
              
              equipamentos.push(equipamentoInfo);
            });
            
            // Adicionar um único item por requisição
            this.historicos.push({
              title: `Requisição #${x.requisicao.id}`,
              date: moment(x.equipamentosRequisicao[0].dtentrega).format("DD/MM/YYYY"),
              content: equipamentos,
              hasDevolvidos: equipamentos.some(eq => eq.devolvido),
              hasAtivos: equipamentos.some(eq => eq.ativo)
            });
          });
        }
      })
    }
    else {
      this.util.exibirMensagemToast('Selecione um colaborador para consultar o seu histórico', 5000);
    }
  }

  /**
   * Imprime o timeline de colaboradores em formato otimizado
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
    const totalEquipamentos = this.historicos.reduce((total, item) => total + item.content.length, 0);
    
    let historicosHTML = '';
    this.historicos.forEach((item, index) => {
      historicosHTML += `
        <tr class="timeline-row">
          <td class="timeline-number">${index + 1}</td>
          <td class="timeline-requisicao">${item.title}</td>
          <td class="timeline-date">${item.date}</td>
          <td class="timeline-equipamentos">${item.content.length}</td>
          <td class="timeline-status">
            ${item.hasDevolvidos ? '<span class="status-devolvido">Devolvidos</span>' : ''}
            ${item.hasAtivos ? '<span class="status-ativo">Ativos</span>' : ''}
          </td>
        </tr>
        ${this.gerarDetalhesEquipamentos(item.content, index + 1)}
      `;
    });

    return `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <title>Timeline de Colaboradores - ${this.colaborador.descricao}</title>
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
          
          .timeline-requisicao {
            width: 120px;
            font-weight: bold;
          }
          
          .timeline-date {
            width: 100px;
            text-align: center;
          }
          
          .timeline-equipamentos {
            width: 80px;
            text-align: center;
          }
          
          .timeline-status {
            width: 120px;
          }
          
          .status-devolvido {
            background: rgba(40, 167, 69, 0.1);
            color: #28a745;
            padding: 2px 6px;
            border-radius: 10px;
            font-size: 10px;
            font-weight: 600;
            margin-right: 5px;
          }
          
          .status-ativo {
            background: rgba(23, 162, 184, 0.1);
            color: #0c5460;
            padding: 2px 6px;
            border-radius: 10px;
            font-size: 10px;
            font-weight: 600;
          }
          
          .equipamentos-details {
            background: #f8f9fa;
            border-left: 3px solid #FF3A0F;
            margin: 5px 0;
            padding: 10px;
          }
          
          .equipamento-item {
            display: flex;
            align-items: center;
            gap: 8px;
            margin-bottom: 5px;
            font-size: 10px;
          }
          
          .equipamento-item:last-child {
            margin-bottom: 0;
          }
          
          .equipamento-icon {
            font-size: 12px;
          }
          
          .equipamento-icon.devolvido {
            color: #28a745;
          }
          
          .equipamento-icon.ativo {
            color: #17a2b8;
          }
          
          .equipamento-texto {
            flex: 1;
            color: #495057;
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
          <h1 class="print-title">Timeline de Colaboradores</h1>
          <p class="print-subtitle">${this.colaborador.descricao}</p>
        </div>
        
        <div class="print-info">
          <span><strong>Data de Impressão:</strong> ${dataAtual}</span>
          <span><strong>Total de Requisições:</strong> ${totalRegistros}</span>
          <span><strong>Total de Equipamentos:</strong> ${totalEquipamentos}</span>
        </div>
        
        <table class="print-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Requisição</th>
              <th>Data</th>
              <th>Equipamentos</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            ${historicosHTML}
          </tbody>
        </table>
        
        <div class="print-summary">
          <h3>Resumo Geral</h3>
          <div class="summary-stats">
            ${this.gerarResumoGeral()}
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
   * Gera os detalhes dos equipamentos para impressão
   */
  private gerarDetalhesEquipamentos(equipamentos: any[], index: number): string {
    if (equipamentos.length === 0) return '';
    
    let detalhesHTML = '';
    equipamentos.forEach((equip, eqIndex) => {
      const iconClass = equip.devolvido ? 'devolvido' : 'ativo';
      const icon = equip.devolvido ? 'check_circle' : 'radio_button_checked';
      
      detalhesHTML += `
        <tr>
          <td colspan="5" style="padding: 0;">
            <div class="equipamentos-details">
              <div class="equipamento-item">
                <i class="material-icons equipamento-icon ${iconClass}">${icon}</i>
                <span class="equipamento-texto">${equip.texto}</span>
              </div>
            </div>
          </td>
        </tr>
      `;
    });
    
    return detalhesHTML;
  }

  /**
   * Gera o resumo estatístico geral
   */
  private gerarResumoGeral(): string {
    const totalEquipamentos = this.historicos.reduce((total, item) => total + item.content.length, 0);
    const totalDevolvidos = this.historicos.reduce((total, item) => {
      return total + item.content.filter(eq => eq.devolvido).length;
    }, 0);
    const totalAtivos = this.historicos.reduce((total, item) => {
      return total + item.content.filter(eq => eq.ativo).length;
    }, 0);
    
    return `
      <div class="summary-item">
        <div class="summary-number">${this.historicos.length}</div>
        <div class="summary-label">Requisições</div>
      </div>
      <div class="summary-item">
        <div class="summary-number">${totalEquipamentos}</div>
        <div class="summary-label">Equipamentos</div>
      </div>
      <div class="summary-item">
        <div class="summary-number">${totalDevolvidos}</div>
        <div class="summary-label">Devolvidos</div>
      </div>
      <div class="summary-item">
        <div class="summary-number">${totalAtivos}</div>
        <div class="summary-label">Ativos</div>
      </div>
    `;
  }

  getStatusClass(title: string): string {
    if (!title) return 'status-outros';
    
    const titleLower = title.toLowerCase();
    
    if (titleLower.includes('entregue')) {
      return 'status-entregue';
    } else if (titleLower.includes('devolvido')) {
      return 'status-devolvido';
    } else if (titleLower.includes('requisitado')) {
      return 'status-requisitado';
    } else {
      return 'status-outros';
    }
  }

  /**
   * Limpar todos os filtros e parâmetros de busca
   */
  limparFiltros() {
    this.buscarColaboradores.setValue('');
    this.colaboradorSelecionado.setValue(null);
    
    // Limpar dados locais
    this.colaborador = {
      cliente: this.session.usuario.cliente
    };
    this.colaboradores = [];
    this.historicos = [];
    
    // Navegar para rota limpa (sem query params)
    this.router.navigate(['relatorios/timeline-colaboradores'], {
      queryParams: {}
    });
    
    // Notificar usuário
    this.util.exibirMensagemToast('Filtros limpos! Selecione um colaborador para ver o histórico.', 3000);
    
    // Recarregar lista de colaboradores inicial
    this.buscarColaboradoresDisponiveis('');
  }

}
