import { TestBed } from '@angular/core/testing';

import { IncidentService } from './incident.service';

describe('IncidentService', () => {
  let service: IncidentService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(IncidentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
