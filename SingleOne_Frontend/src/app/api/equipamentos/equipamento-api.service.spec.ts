import { TestBed } from '@angular/core/testing';

import { EquipamentoApiService } from './equipamento-api.service';

describe('EquipamentoApiService', () => {
  let service: EquipamentoApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EquipamentoApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
