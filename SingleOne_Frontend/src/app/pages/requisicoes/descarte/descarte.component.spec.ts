import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DescarteComponent } from './descarte.component';

describe('DescarteComponent', () => {
  let component: DescarteComponent;
  let fixture: ComponentFixture<DescarteComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DescarteComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DescarteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
