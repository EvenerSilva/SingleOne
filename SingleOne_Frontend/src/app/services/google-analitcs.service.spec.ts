import { TestBed } from '@angular/core/testing';

import { GoogleAnalitcsService } from './google-analitcs.service';

describe('GoogleAnalitcsService', () => {
  let service: GoogleAnalitcsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GoogleAnalitcsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
