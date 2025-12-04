import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LaudoFinalComponent } from './laudo-final.component';

describe('LaudoFinalComponent', () => {
  let component: LaudoFinalComponent;
  let fixture: ComponentFixture<LaudoFinalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LaudoFinalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LaudoFinalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
