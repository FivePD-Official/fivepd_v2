import { TestBed } from '@angular/core/testing';

import { CalloutService } from './callout.service';

describe('CalloutService', () => {
  let service: CalloutService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CalloutService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
