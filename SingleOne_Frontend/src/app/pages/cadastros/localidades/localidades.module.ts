import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';

import { LocalidadesComponent } from './localidades/localidades.component';
import { LocalidadeComponent } from './localidade/localidade.component';

@NgModule({
  declarations: [
    LocalidadesComponent,
    LocalidadeComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatPaginatorModule,
    MatTooltipModule,
    RouterModule
  ],
  exports: [
    LocalidadesComponent,
    LocalidadeComponent
  ]
})
export class LocalidadesModule { }
