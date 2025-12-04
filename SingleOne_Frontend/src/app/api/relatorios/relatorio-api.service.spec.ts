import { TestBed } from '@angular/core/testing';

import { RelatorioApiService } from './relatorio-api.service';

describe('RelatorioApiService', () => {
  let service: RelatorioApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RelatorioApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
