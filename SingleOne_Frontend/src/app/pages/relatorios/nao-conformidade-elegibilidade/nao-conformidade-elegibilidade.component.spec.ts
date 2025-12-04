import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NaoConformidadeElegibilidadeComponent } from './nao-conformidade-elegibilidade.component';

describe('NaoConformidadeElegibilidadeComponent', () => {
  let component: NaoConformidadeElegibilidadeComponent;
  let fixture: ComponentFixture<NaoConformidadeElegibilidadeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NaoConformidadeElegibilidadeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NaoConformidadeElegibilidadeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

