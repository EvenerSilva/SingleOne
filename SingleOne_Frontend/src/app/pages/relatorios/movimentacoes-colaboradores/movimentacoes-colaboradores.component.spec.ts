import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MovimentacoesColaboradoresComponent } from './movimentacoes-colaboradores.component';

describe('MovimentacoesColaboradoresComponent', () => {
  let component: MovimentacoesColaboradoresComponent;
  let fixture: ComponentFixture<MovimentacoesColaboradoresComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MovimentacoesColaboradoresComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MovimentacoesColaboradoresComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
