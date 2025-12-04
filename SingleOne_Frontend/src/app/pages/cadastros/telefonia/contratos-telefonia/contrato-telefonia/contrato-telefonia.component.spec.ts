import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ContratoTelefoniaComponent } from './contrato-telefonia.component';

describe('ContratoTelefoniaComponent', () => {
  let component: ContratoTelefoniaComponent;
  let fixture: ComponentFixture<ContratoTelefoniaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ContratoTelefoniaComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ContratoTelefoniaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
