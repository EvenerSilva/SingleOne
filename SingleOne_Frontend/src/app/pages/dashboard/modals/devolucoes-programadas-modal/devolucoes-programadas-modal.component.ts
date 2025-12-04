import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';

export interface DevolucaoProgramadaItem {
  equipamento?: string;
  nome?: string;
  colaborador?: string;
  nomecolaborador?: string;
  matricula?: string;
  dataDevolucao?: Date | string;
  dtprogramadaretorno?: Date | string;
  diasParaVencimento?: number;
  status?: string;
  requisicaoId?: number;
  requisicaoid?: number;
  requisicoesItemId?: number;  // 🆕 ID do item da requisição (usado para navegação específica)
  requisicoesitemid?: number;
  equipamentoId?: number;
  equipamentoid?: number;
  colaboradorId?: number;
  colaboradorid?: number;
  tipoequipamento?: string;
  serial?: string;
  patrimonio?: string;
}

@Component({
  selector: 'app-devolucoes-programadas-modal',
  templateUrl: './devolucoes-programadas-modal.component.html',
  styleUrls: ['./devolucoes-programadas-modal.component.scss']
})
export class DevolucoesProgramadasModalComponent {
  constructor(
    private dialogRef: MatDialogRef<DevolucoesProgramadasModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { itens: DevolucaoProgramadaItem[] },
    private router: Router
  ) {
  }

  fechar(): void {
    this.dialogRef.close();
  }

  /**
   * Retorna o nome do equipamento de forma padronizada
   */
  getEquipamento(item: DevolucaoProgramadaItem): string {
    return item.equipamento || item.nome || 'N/A';
  }

  /**
   * Retorna o nome do colaborador de forma padronizada
   */
  getColaborador(item: DevolucaoProgramadaItem): string {
    return item.colaborador || item.nomecolaborador || 'N/A';
  }

  /**
   * Retorna a data de devolução de forma padronizada
   */
  getDataDevolucao(item: DevolucaoProgramadaItem): Date | string | null {
    return item.dataDevolucao || item.dtprogramadaretorno || null;
  }

  /**
   * Verifica se a data está vencida (passada)
   */
  isDataVencida(data: Date | string | null): boolean {
    if (!data) return false;
    
    const dataObj = new Date(data);
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    dataObj.setHours(0, 0, 0, 0);
    
    return dataObj < hoje;
  }

  /**
   * Verifica se a data está próxima (entre hoje e 7 dias)
   */
  isDataProxima(data: Date | string | null): boolean {
    if (!data) return false;
    
    const dataObj = new Date(data);
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    dataObj.setHours(0, 0, 0, 0);
    
    const diffTime = dataObj.getTime() - hoje.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return diffDays >= 0 && diffDays <= 7;
  }

  /**
   * Calcula quantos dias faltam para o vencimento
   */
  getDiasParaVencimento(data: Date | string | null): number {
    if (!data) return 0;
    
    const dataObj = new Date(data);
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    dataObj.setHours(0, 0, 0, 0);
    
    const diffTime = dataObj.getTime() - hoje.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  /**
   * Retorna o status visual da devolução
   */
  getStatusDevolucao(item: DevolucaoProgramadaItem): string {
    const data = this.getDataDevolucao(item);
    
    if (this.isDataVencida(data)) {
      return 'Vencida';
    } else if (this.isDataProxima(data)) {
      return 'Vence em breve';
    }
    return 'No prazo';
  }

  /**
   * Retorna a classe CSS para o status
   */
  getStatusClass(item: DevolucaoProgramadaItem): string {
    const data = this.getDataDevolucao(item);
    
    if (this.isDataVencida(data)) {
      return 'status-vencida';
    } else if (this.isDataProxima(data)) {
      return 'status-proxima';
    }
    return 'status-normal';
  }

  /**
   * Navega para a tela de entregas-devoluções com o filtro do recurso específico
   * Prioriza identificadores únicos: Serial > Patrimônio > Matrícula
   */
  navegarParaEntregasDevolucoes(item: DevolucaoProgramadaItem): void {
    const colaborador = this.getColaborador(item);
    const matricula = item.matricula || '';
    const equipamento = this.getEquipamento(item);
    const serial = item.serial || '';
    const patrimonio = item.patrimonio || '';
    const requisicoesItemId = item.requisicoesItemId || item.requisicoesitemid || 0;
    this.dialogRef.close();
    
    // Navegar para entregas-devoluções com queryParams
    // Priorizar: Serial > Patrimônio > Matrícula (identificadores únicos)
    // Evitar usar nome do colaborador ou equipamento (podem ter duplicatas)
    let searchTerm = serial || patrimonio || matricula;
    
    if (!searchTerm) {
      console.warn('[DEVOLUCOES-MODAL] Nenhum identificador único encontrado, usando nome do equipamento');
      searchTerm = equipamento;
    }
    this.router.navigate(['/movimentacoes/entregas-devolucoes'], {
      queryParams: {
        search: searchTerm
      }
    });
  }

  /**
   * Retorna o total de devoluções
   */
  getTotalDevolucoes(): number {
    return this.data.itens?.length || 0;
  }

  /**
   * Retorna o total de devoluções vencidas
   */
  getTotalVencidas(): number {
    if (!this.data.itens) return 0;
    return this.data.itens.filter(item => this.isDataVencida(this.getDataDevolucao(item))).length;
  }

  /**
   * Retorna o total de devoluções próximas
   */
  getTotalProximas(): number {
    if (!this.data.itens) return 0;
    return this.data.itens.filter(item => {
      const data = this.getDataDevolucao(item);
      return !this.isDataVencida(data) && this.isDataProxima(data);
    }).length;
  }
}

