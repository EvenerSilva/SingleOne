import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DesligamentoProgramadoComponent } from './desligamento-programado.component';

describe('DesligamentoProgramadoComponent', () => {
  let component: DesligamentoProgramadoComponent;
  let fixture: ComponentFixture<DesligamentoProgramadoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DesligamentoProgramadoComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DesligamentoProgramadoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
