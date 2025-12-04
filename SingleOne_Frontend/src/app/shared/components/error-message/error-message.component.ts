import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-error-message',
  template: `
    <div class="error-container" *ngIf="message">
      <mat-icon class="error-icon">error</mat-icon>
      <div class="error-content">
        <h3 *ngIf="title" class="error-title">{{ title }}</h3>
        <p class="error-message">{{ message }}</p>
      </div>
    </div>
  `,
  styles: [`
    .error-container {
      display: flex;
      align-items: flex-start;
      padding: 16px;
      background-color: #ffebee;
      border: 1px solid #f44336;
      border-radius: 4px;
      margin: 8px 0;
    }
    
    .error-icon {
      color: #f44336;
      margin-right: 12px;
      margin-top: 2px;
    }
    
    .error-content {
      flex: 1;
    }
    
    .error-title {
      margin: 0 0 4px 0;
      color: #d32f2f;
      font-size: 16px;
      font-weight: 500;
    }
    
    .error-message {
      margin: 0;
      color: #d32f2f;
      font-size: 14px;
    }
  `]
})
export class ErrorMessageComponent {
  @Input() title?: string;
  @Input() message?: string;
}
