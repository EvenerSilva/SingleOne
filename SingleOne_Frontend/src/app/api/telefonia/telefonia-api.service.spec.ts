import { TestBed } from '@angular/core/testing';

import { TelefoniaApiService } from './telefonia-api.service';

describe('TelefoniaApiService', () => {
  let service: TelefoniaApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TelefoniaApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
