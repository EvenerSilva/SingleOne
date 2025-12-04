import { ComponentFixture, TestBed } from '@angular/core/testing';

import { movimentacoesComponent } from './movimentacoes.component';

describe('movimentacoesComponent', () => {
  let component: movimentacoesComponent;
  let fixture: ComponentFixture<movimentacoesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ movimentacoesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(movimentacoesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
