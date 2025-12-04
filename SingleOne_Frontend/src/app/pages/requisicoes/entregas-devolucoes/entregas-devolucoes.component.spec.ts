import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntregasDevolucoesComponent } from './entregas-devolucoes.component';

describe('EntregasDevolucoesComponent', () => {
  let component: EntregasDevolucoesComponent;
  let fixture: ComponentFixture<EntregasDevolucoesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EntregasDevolucoesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EntregasDevolucoesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
