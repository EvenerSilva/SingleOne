import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ContestacaoComponent } from './contestacao.component';
import { ContestacaoApiService } from 'src/app/api/contestacoes/contestacao-api.service';
import { UtilService } from 'src/app/util/util.service';

describe('ContestacaoComponent', () => {
  let component: ContestacaoComponent;
  let fixture: ComponentFixture<ContestacaoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ContestacaoComponent ],
      imports: [
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule
      ],
      providers: [
        ContestacaoApiService,
        UtilService
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ContestacaoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
