import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimelapseColaboradoresComponent } from './timelapse-colaboradores.component';

describe('TimelapseColaboradoresComponent', () => {
  let component: TimelapseColaboradoresComponent;
  let fixture: ComponentFixture<TimelapseColaboradoresComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TimelapseColaboradoresComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TimelapseColaboradoresComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
