import { TestBed } from '@angular/core/testing';

import { ContratoApiService } from './contrato-api.service';

describe('ContratoApiService', () => {
  let service: ContratoApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ContratoApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
