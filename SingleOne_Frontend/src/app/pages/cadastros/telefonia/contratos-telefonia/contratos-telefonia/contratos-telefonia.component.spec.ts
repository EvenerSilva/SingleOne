import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ContratosTelefoniaComponent } from './contratos-telefonia.component';

describe('ContratosTelefoniaComponent', () => {
  let component: ContratosTelefoniaComponent;
  let fixture: ComponentFixture<ContratosTelefoniaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ContratosTelefoniaComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ContratosTelefoniaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
