import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'warning' | 'error' | 'info';
}

@Component({
  selector: 'app-confirm-dialog',
  template: `
    <h2 mat-dialog-title [class]="data.type || 'info'">
      {{ data.title }}
    </h2>
    <mat-dialog-content>
      <p>{{ data.message }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button [mat-dialog-close]="false">
        {{ data.cancelText || 'Cancelar' }}
      </button>
      <button mat-raised-button [color]="getButtonColor()" [mat-dialog-close]="true">
        {{ data.confirmText || 'Confirmar' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .warning {
      color: #f57c00;
    }
    
    .error {
      color: #d32f2f;
    }
    
    .info {
      color: #1976d2;
    }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}

  getButtonColor(): string {
    switch (this.data.type) {
      case 'warning':
        return 'warn';
      case 'error':
        return 'warn';
      default:
        return 'primary';
    }
  }
}
