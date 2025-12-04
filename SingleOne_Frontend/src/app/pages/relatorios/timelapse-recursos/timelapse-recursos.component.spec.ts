import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimelapseRecursosComponent } from './timelapse-recursos.component';

describe('TimelapseRecursosComponent', () => {
  let component: TimelapseRecursosComponent;
  let fixture: ComponentFixture<TimelapseRecursosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TimelapseRecursosComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TimelapseRecursosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
