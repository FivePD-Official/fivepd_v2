import { TestBed } from '@angular/core/testing';

import { CalloutService } from './callout.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('CalloutService', () => {
  let service: CalloutService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(CalloutService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
