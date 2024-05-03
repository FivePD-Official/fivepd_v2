import { TestBed } from '@angular/core/testing';

import { CalloutDetailsService } from './callout-details.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('CalloutDetailsService', () => {
  let service: CalloutDetailsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(CalloutDetailsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
