import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EntregasDevolucoesFinalComponent } from './entregas-devolucoes-final.component';

describe('EntregasDevolucoesFinalComponent', () => {
  let component: EntregasDevolucoesFinalComponent;
  let fixture: ComponentFixture<EntregasDevolucoesFinalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EntregasDevolucoesFinalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(EntregasDevolucoesFinalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
