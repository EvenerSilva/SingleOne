import { TestBed } from '@angular/core/testing';

import { ConfiguracoesApiService } from './configuracoes-api.service';

describe('ConfiguracoesApiService', () => {
  let service: ConfiguracoesApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConfiguracoesApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
