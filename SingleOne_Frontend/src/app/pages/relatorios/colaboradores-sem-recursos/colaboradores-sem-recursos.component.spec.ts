import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ColaboradoresSemRecursosComponent } from './colaboradores-sem-recursos.component';

describe('ColaboradoresSemRecursosComponent', () => {
  let component: ColaboradoresSemRecursosComponent;
  let fixture: ComponentFixture<ColaboradoresSemRecursosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ColaboradoresSemRecursosComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ColaboradoresSemRecursosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

