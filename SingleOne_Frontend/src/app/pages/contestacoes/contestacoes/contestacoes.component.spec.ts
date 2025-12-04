import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ContestacoesComponent } from './contestacoes.component';
import { ContestacaoApiService } from 'src/app/api/contestacoes/contestacao-api.service';
import { UtilService } from 'src/app/util/util.service';

describe('ContestacoesComponent', () => {
  let component: ContestacoesComponent;
  let fixture: ComponentFixture<ContestacoesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ContestacoesComponent ],
      imports: [
        ReactiveFormsModule,
        MatTableModule,
        MatPaginatorModule,
        MatTooltipModule,
        RouterTestingModule,
        HttpClientTestingModule
      ],
      providers: [
        ContestacaoApiService,
        UtilService
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ContestacoesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
