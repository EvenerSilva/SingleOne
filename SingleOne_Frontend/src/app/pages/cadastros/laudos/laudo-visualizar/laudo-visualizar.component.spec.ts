import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LaudoVisualizarComponent } from './laudo-visualizar.component';

describe('LaudoVisualizarComponent', () => {
  let component: LaudoVisualizarComponent;
  let fixture: ComponentFixture<LaudoVisualizarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LaudoVisualizarComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LaudoVisualizarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
