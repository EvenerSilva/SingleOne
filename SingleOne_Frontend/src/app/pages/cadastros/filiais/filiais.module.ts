import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Angular Material
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';

// Componentes
import { FiliaisComponent } from './filiais/filiais.component';
import { FilialComponent } from './filial/filial.component';

// Módulos de Pipes
import { MeusPipesModule } from 'src/app/pipes/meus-pipes.module';

// Módulos compartilhados
// import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [
    FiliaisComponent,
    FilialComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatCardModule,
    MatTooltipModule,
    MeusPipesModule  // ✅ Importando o módulo que já tem o pipe
    // SharedModule
  ],
  exports: [
    FiliaisComponent,
    FilialComponent
  ]
})
export class FiliaisModule { }
