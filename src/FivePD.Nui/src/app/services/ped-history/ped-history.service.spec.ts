import { TestBed } from '@angular/core/testing';

import { PedHistoryService } from './ped-history.service';

describe('PedHistoryService', () => {
  let service: PedHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PedHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
