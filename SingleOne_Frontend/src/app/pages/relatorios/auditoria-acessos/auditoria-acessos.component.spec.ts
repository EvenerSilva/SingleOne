import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditoriaAcessosComponent } from './auditoria-acessos.component';

describe('AuditoriaAcessosComponent', () => {
  let component: AuditoriaAcessosComponent;
  let fixture: ComponentFixture<AuditoriaAcessosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AuditoriaAcessosComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(AuditoriaAcessosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
