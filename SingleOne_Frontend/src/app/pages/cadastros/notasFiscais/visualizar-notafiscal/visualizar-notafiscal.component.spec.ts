import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VisualizarNotafiscalComponent } from './visualizar-notafiscal.component';

describe('VisualizarNotafiscalComponent', () => {
  let component: VisualizarNotafiscalComponent;
  let fixture: ComponentFixture<VisualizarNotafiscalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ VisualizarNotafiscalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(VisualizarNotafiscalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
