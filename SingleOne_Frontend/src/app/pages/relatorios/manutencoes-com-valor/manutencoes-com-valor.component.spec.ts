import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ManutencoesComValorComponent } from './manutencoes-com-valor.component';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('ManutencoesComValorComponent', () => {
  let component: ManutencoesComValorComponent;
  let fixture: ComponentFixture<ManutencoesComValorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ManutencoesComValorComponent ],
      imports: [
        BrowserAnimationsModule,
        RouterTestingModule,
        HttpClientTestingModule,
        MatTableModule,
        MatPaginatorModule,
        MatTabsModule,
        MatCardModule,
        MatFormFieldModule,
        MatSelectModule,
        MatButtonModule,
        MatIconModule,
        MatToolbarModule,
        MatTooltipModule,
        MatProgressSpinnerModule,
        FormsModule
      ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ManutencoesComValorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.loading).toBeFalsy();
    expect(component.showFilters).toBeFalsy();
    expect(component.filtros.empresa).toBe(0);
    expect(component.filtros.centrocusto).toBe(0);
  });

  it('should toggle filters visibility', () => {
    expect(component.showFilters).toBeFalsy();
    component.showFilters = true;
    expect(component.showFilters).toBeTruthy();
  });

  it('should clear filters correctly', () => {
    component.filtros.empresa = 1;
    component.filtros.centrocusto = 2;
    component.centros = [{ id: 1, nome: 'Centro 1' }];
    
    component.limparFiltros();
    
    expect(component.filtros.empresa).toBe(0);
    expect(component.filtros.centrocusto).toBe(0);
    expect(component.centros).toEqual([]);
  });
});
