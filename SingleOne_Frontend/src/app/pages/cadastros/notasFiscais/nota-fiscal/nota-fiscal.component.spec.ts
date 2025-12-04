import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NotaFiscalComponent } from './nota-fiscal.component';

describe('NotasFiscalComponent', () => {
  let component: NotaFiscalComponent;
  let fixture: ComponentFixture<NotaFiscalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NotaFiscalComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(NotaFiscalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
