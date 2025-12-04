import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdicionarObservacaoComponent } from './adicionar-observacao.component';

describe('AdicionarObservacaoComponent', () => {
  let component: AdicionarObservacaoComponent;
  let fixture: ComponentFixture<AdicionarObservacaoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AdicionarObservacaoComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AdicionarObservacaoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
