import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequisicoesComponent } from './requisicoes.component';

describe('RequisicoesComponent', () => {
  let component: RequisicoesComponent;
  let fixture: ComponentFixture<RequisicoesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RequisicoesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(RequisicoesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
