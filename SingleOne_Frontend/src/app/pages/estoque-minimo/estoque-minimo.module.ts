import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Angular Material
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatBadgeModule } from '@angular/material/badge';
import { MatRippleModule } from '@angular/material/core';

// Shared Module
import { SharedModule } from '../../shared/shared.module';

// Pipes
import { MeusPipesModule } from '../../pipes/meus-pipes.module';

// Components
import { EstoqueMinimoComponent } from './estoque-minimo/estoque-minimo.component';
import { EquipamentosComponent } from './equipamentos/equipamentos.component';
import { LinhasComponent } from './linhas/linhas.component';

// Services
import { EstoqueMinimoApiService } from '../../api/estoque-minimo/estoque-minimo-api.service';

// Routing
import { EstoqueMinimoRoutingModule } from './estoque-minimo-routing.module';

@NgModule({
  declarations: [
    EstoqueMinimoComponent,
    EquipamentosComponent,
    LinhasComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    
    // Angular Material
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatCheckboxModule,
    MatRadioModule,
    MatTabsModule,
    MatExpansionModule,
    MatChipsModule,
    MatAutocompleteModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatDialogModule,
    MatMenuModule,
    MatStepperModule,
    MatCardModule,
    MatDividerModule,
    MatListModule,
    MatBadgeModule,
    MatRippleModule,
    
    // Shared
    SharedModule,
    MeusPipesModule,
    
    // Routing
    EstoqueMinimoRoutingModule
  ],
  providers: [
    EstoqueMinimoApiService
  ]
})
export class EstoqueMinimoModule { }