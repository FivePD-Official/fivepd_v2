import { TestBed } from '@angular/core/testing';

import { IncidentHistoryService } from './incident-history.service';

describe('IncidentHistoryService', () => {
  let service: IncidentHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(IncidentHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
