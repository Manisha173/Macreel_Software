import { TestBed } from '@angular/core/testing';

import { GaurdService } from './gaurd.service';

describe('GaurdService', () => {
  let service: GaurdService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GaurdService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
