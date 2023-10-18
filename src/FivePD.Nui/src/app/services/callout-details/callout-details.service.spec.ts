import { TestBed } from '@angular/core/testing';

import { CalloutDetailsService } from './callout-details.service';

describe('CalloutDetailsService', () => {
  let service: CalloutDetailsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CalloutDetailsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
