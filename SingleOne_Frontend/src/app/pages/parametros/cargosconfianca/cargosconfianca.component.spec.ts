import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CargosconfiancaComponent } from './cargosconfianca.component';

describe('CargosconfiancaComponent', () => {
  let component: CargosconfiancaComponent;
  let fixture: ComponentFixture<CargosconfiancaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CargosconfiancaComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CargosconfiancaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
