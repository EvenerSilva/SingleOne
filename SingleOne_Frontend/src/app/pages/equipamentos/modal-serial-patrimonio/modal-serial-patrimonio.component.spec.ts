import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalSerialPatrimonioComponent } from './modal-serial-patrimonio.component';

describe('ModalSerialPatrimonioComponent', () => {
  let component: ModalSerialPatrimonioComponent;
  let fixture: ComponentFixture<ModalSerialPatrimonioComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ModalSerialPatrimonioComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ModalSerialPatrimonioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
