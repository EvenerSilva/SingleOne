import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ValidarPalavraChaveComponent } from './validar-palavra-chave.component';

describe('ValidarPalavraChaveComponent', () => {
  let component: ValidarPalavraChaveComponent;
  let fixture: ComponentFixture<ValidarPalavraChaveComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ValidarPalavraChaveComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ValidarPalavraChaveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
