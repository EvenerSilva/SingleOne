import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MapaRecursosComponent } from './mapa-recursos.component';

describe('MapaRecursosComponent', () => {
  let component: MapaRecursosComponent;
  let fixture: ComponentFixture<MapaRecursosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ MapaRecursosComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MapaRecursosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

