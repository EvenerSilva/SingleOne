import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PoliticasElegibilidadeComponent } from './politicas-elegibilidade.component';

describe('PoliticasElegibilidadeComponent', () => {
  let component: PoliticasElegibilidadeComponent;
  let fixture: ComponentFixture<PoliticasElegibilidadeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PoliticasElegibilidadeComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PoliticasElegibilidadeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

