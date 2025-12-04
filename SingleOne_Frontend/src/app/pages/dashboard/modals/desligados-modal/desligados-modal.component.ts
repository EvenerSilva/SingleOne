import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';

export interface DesligadoAggregadoItem {
  colaboradorId: number;
  nome: string;
  matricula: string;
  dtdemissao?: Date;
  qtde: number;
}

@Component({
  selector: 'app-desligados-modal',
  templateUrl: './desligados-modal.component.html',
  styleUrls: ['./desligados-modal.component.scss']
})
export class DesligadosModalComponent {
  constructor(
    private dialogRef: MatDialogRef<DesligadosModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { itens: DesligadoAggregadoItem[] },
    private router: Router
  ) {}

  fechar(): void {
    this.dialogRef.close();
  }

  /**
   * Navega para a tela de entregas-devoluções com o filtro do colaborador aplicado
   * Usa a matrícula para evitar problemas com homônimos
   */
  navegarParaEntregasDevolucoes(colaboradorId: number, nomeColaborador: string, matricula: string): void {
    // Fechar o modal
    this.dialogRef.close();
    
    // Navegar para entregas-devoluções com queryParams usando matrícula (campo único)
    this.router.navigate(['/movimentacoes/entregas-devolucoes'], {
      queryParams: {
        search: matricula  // Usar matrícula em vez do nome para evitar homônimos
      }
    });
  }
}

