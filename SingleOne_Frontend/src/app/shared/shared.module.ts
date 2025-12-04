import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// Material Design
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDividerModule } from '@angular/material/divider';

// Componentes compartilhados
import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { ErrorMessageComponent } from './components/error-message/error-message.component';
import { LogoDisplayComponent } from './components/logo-display/logo-display.component';

// Pipes compartilhados
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { FormatDatePipe } from './pipes/format-date.pipe';
import { FormatCurrencyPipe } from './pipes/format-currency.pipe';

// Diretivas compartilhadas
import { HighlightDirective } from './directives/highlight.directive';
import { ClickOutsideDirective } from './directives/click-outside.directive';

const materialModules = [
  MatButtonModule,
  MatCardModule,
  MatCheckboxModule,
  MatDialogModule,
  MatFormFieldModule,
  MatInputModule,
  MatSelectModule,
  MatTableModule,
  MatPaginatorModule,
  MatSortModule,
  MatSnackBarModule,
  MatProgressSpinnerModule,
  MatIconModule,
  MatToolbarModule,
  MatSidenavModule,
  MatListModule,
  MatMenuModule,
  MatTabsModule,
  MatExpansionModule,
  MatChipsModule,
  MatBadgeModule,
  MatTooltipModule,
  MatDatepickerModule,
  MatNativeDateModule,
  MatDividerModule
];

const sharedComponents = [
  LoadingSpinnerComponent,
  ConfirmDialogComponent,
  ErrorMessageComponent,
  LogoDisplayComponent
];

const sharedPipes = [
  SafeHtmlPipe,
  FormatDatePipe,
  FormatCurrencyPipe
];

const sharedDirectives = [
  HighlightDirective,
  ClickOutsideDirective
];

@NgModule({
  declarations: [
    ...sharedComponents,
    ...sharedPipes,
    ...sharedDirectives
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    ...materialModules
  ],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    ...materialModules,
    ...sharedComponents,
    ...sharedPipes,
    ...sharedDirectives
  ]
})
export class SharedModule { }
