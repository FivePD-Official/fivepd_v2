import { TestBed } from '@angular/core/testing';

import { IncidentService } from './incident.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('IncidentService', () => {
  let service: IncidentService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(IncidentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
