import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EquipamentosStatusDetalheComponent } from './equipamentos-status-detalhe.component';

describe('EquipamentosStatusDetalheComponent', () => {
  let component: EquipamentosStatusDetalheComponent;
  let fixture: ComponentFixture<EquipamentosStatusDetalheComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EquipamentosStatusDetalheComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EquipamentosStatusDetalheComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
