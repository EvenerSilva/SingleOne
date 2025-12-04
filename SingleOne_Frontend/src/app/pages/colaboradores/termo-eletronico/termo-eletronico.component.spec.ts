import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TermoEletronicoComponent } from './termo-eletronico.component';

describe('TermoEletronicoComponent', () => {
  let component: TermoEletronicoComponent;
  let fixture: ComponentFixture<TermoEletronicoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TermoEletronicoComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TermoEletronicoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
