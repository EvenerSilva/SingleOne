import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NadaConstaComponent } from './nada-consta.component';

describe('NadaConstaComponent', () => {
  let component: NadaConstaComponent;
  let fixture: ComponentFixture<NadaConstaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NadaConstaComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NadaConstaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
