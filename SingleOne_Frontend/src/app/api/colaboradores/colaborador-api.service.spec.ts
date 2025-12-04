import { TestBed } from '@angular/core/testing';

import { ColaboradorApiService } from './colaborador-api.service';

describe('ColaboradorApiService', () => {
  let service: ColaboradorApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ColaboradorApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
