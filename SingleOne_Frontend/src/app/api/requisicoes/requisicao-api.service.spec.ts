import { TestBed } from '@angular/core/testing';

import { RequisicaoApiService } from './requisicao-api.service';

describe('RequisicaoApiService', () => {
  let service: RequisicaoApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RequisicaoApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
