import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Components
import { EstoqueMinimoComponent } from './estoque-minimo/estoque-minimo.component';
import { EquipamentosComponent } from './equipamentos/equipamentos.component';
import { LinhasComponent } from './linhas/linhas.component';

const routes: Routes = [
  {
    path: '',
    component: EstoqueMinimoComponent,
    children: [
      {
        path: '',
        redirectTo: 'equipamentos',
        pathMatch: 'full'
      },
      {
        path: 'equipamentos',
        component: EquipamentosComponent
      },
      {
        path: 'linhas',
        component: LinhasComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EstoqueMinimoRoutingModule { }
