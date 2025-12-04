import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { Contestacao, ContestacaoStatus } from '../../models/contestacao.interface';

interface ExpandedSections {
  principal: boolean;  // Seção principal "Minhas Contestações"
  pendentes: boolean;
  resolvidas: boolean;
  negadas: boolean;
  canceladas: boolean;
}

@Component({
  selector: 'app-minhas-contestacoes',
  templateUrl: './minhas-contestacoes.component.html',
  styleUrls: ['./minhas-contestacoes.component.scss']
})
export class MinhasContestacoesComponent implements OnInit, OnChanges {
  @Input() contestoes: Contestacao[] = [];
  
  // Controle de expansão das seções
  expandedSections: ExpandedSections = {
    principal: false,   // Seção principal colapsada por padrão
    pendentes: true,    // Pendentes expandido por padrão
    resolvidas: false,
    negadas: false,
    canceladas: false
  };

  constructor() { }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
  }

  // Métodos para filtrar contestações por status
  getContestacoesPendentes(): Contestacao[] {
    return this.contestoes.filter(c => 
      c.status === ContestacaoStatus.ABERTA || 
      c.status === ContestacaoStatus.EM_ANALISE ||
      c.status === ContestacaoStatus.PENDENTE_COLABORADOR
    );
  }

  getContestacoesResolvidas(): Contestacao[] {
    return this.contestoes.filter(c => c.status === ContestacaoStatus.RESOLVIDA);
  }

  getContestacoesNegadas(): Contestacao[] {
    return this.contestoes.filter(c => c.status === ContestacaoStatus.NEGADA);
  }

  getContestacoesCanceladas(): Contestacao[] {
    return this.contestoes.filter(c => c.status === ContestacaoStatus.CANCELADA);
  }

  // Controle de expansão
  toggleSection(section: string): void {
    const key = section as keyof ExpandedSections;
    this.expandedSections[key] = !this.expandedSections[key];
  }

  isSectionExpanded(section: string): boolean {
    const key = section as keyof ExpandedSections;
    return this.expandedSections[key];
  }

  // Métodos auxiliares para formatação
  formatarData(data: Date | string): string {
    if (!data) return 'N/A';
    const date = new Date(data);
    return date.toLocaleDateString('pt-BR');
  }

  getStatusClass(status: ContestacaoStatus): string {
    switch (status) {
      case ContestacaoStatus.ABERTA:
      case ContestacaoStatus.EM_ANALISE:
      case ContestacaoStatus.PENDENTE_COLABORADOR:
        return 'status-pendente';
      case ContestacaoStatus.RESOLVIDA:
        return 'status-resolvida';
      case ContestacaoStatus.NEGADA:
        return 'status-negada';
      case ContestacaoStatus.CANCELADA:
        return 'status-cancelada';
      default:
        return 'status-default';
    }
  }

  getStatusIcon(status: ContestacaoStatus): string {
    switch (status) {
      case ContestacaoStatus.ABERTA:
      case ContestacaoStatus.EM_ANALISE:
      case ContestacaoStatus.PENDENTE_COLABORADOR:
        return 'hourglass_empty';
      case ContestacaoStatus.RESOLVIDA:
        return 'check_circle';
      case ContestacaoStatus.NEGADA:
        return 'cancel';
      case ContestacaoStatus.CANCELADA:
        return 'block';
      default:
        return 'help';
    }
  }

  // Método para determinar se é auto inventário ou contestação
  isAutoInventario(contestacao: Contestacao): boolean {
    // Verifica múltiplas variações do campo tipo
    const item = contestacao as any;
    const bruto = item?.tipo_contestacao || item?.tipoContestacao || item?.TipoContestacao || '';
    const tipoNormalizado = (typeof bruto === 'string' ? bruto : String(bruto || ''))
      .trim()
      .toLowerCase()
      .replace(/[-\s]+/g, '_');

    if (tipoNormalizado === 'auto_inventario' || tipoNormalizado === 'autoinventario') {
      return true;
    }

    // Heurísticas quando o backend não envia o tipo
    const motivo: string = (contestacao?.motivo || '').toString().toLowerCase();
    const equipamentoId: number = contestacao?.equipamento?.id ?? 0;
    
    if (motivo.includes('auto invent') || equipamentoId === 0) {
      return true;
    }

    return false;
  }

  // Método para retornar o prefixo correto (Contestação ou Auto Inventário)
  getTipoLabel(contestacao: Contestacao): string {
    return this.isAutoInventario(contestacao) ? 'Auto Inventário' : 'Contestação';
  }
}
