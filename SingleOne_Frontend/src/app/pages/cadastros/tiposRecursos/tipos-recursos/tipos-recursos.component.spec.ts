import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TiposRecursosComponent } from './tipos-recursos.component';

describe('TiposRecursosComponent', () => {
  let component: TiposRecursosComponent;
  let fixture: ComponentFixture<TiposRecursosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TiposRecursosComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TiposRecursosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
