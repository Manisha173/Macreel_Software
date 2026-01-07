import { TestBed } from '@angular/core/testing';

import { ManageMasterdataService } from './manage-masterdata.service';

describe('ManageMasterdataService', () => {
  let service: ManageMasterdataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ManageMasterdataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
