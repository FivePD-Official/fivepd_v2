import { TestBed } from '@angular/core/testing';

import { LooseDataService } from './loose-data.service';

describe('LooseDataService', () => {
  let service: LooseDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LooseDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
