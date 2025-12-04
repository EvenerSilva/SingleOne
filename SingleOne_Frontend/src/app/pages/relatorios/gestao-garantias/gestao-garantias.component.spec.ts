import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GestaoGarantiasComponent } from './gestao-garantias.component';

describe('GestaoGarantiasComponent', () => {
  let component: GestaoGarantiasComponent;
  let fixture: ComponentFixture<GestaoGarantiasComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ GestaoGarantiasComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(GestaoGarantiasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
