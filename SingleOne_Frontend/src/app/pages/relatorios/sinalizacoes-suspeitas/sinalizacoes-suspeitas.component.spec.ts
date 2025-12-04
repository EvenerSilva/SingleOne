import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SinalizacoesSuspeitasComponent } from './sinalizacoes-suspeitas.component';

describe('SinalizacoesSuspeitasComponent', () => {
  let component: SinalizacoesSuspeitasComponent;
  let fixture: ComponentFixture<SinalizacoesSuspeitasComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SinalizacoesSuspeitasComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SinalizacoesSuspeitasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
