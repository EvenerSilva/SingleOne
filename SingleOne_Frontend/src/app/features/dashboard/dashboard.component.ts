import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="dashboard-container">
      <h1>Dashboard</h1>
      <div class="dashboard-content">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Bem-vindo ao SingleOne</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <p>Este é o dashboard principal do sistema.</p>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 20px;
    }
    
    .dashboard-content {
      margin-top: 20px;
    }
    
    mat-card {
      margin-bottom: 20px;
    }
  `]
})
export class DashboardComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

}
