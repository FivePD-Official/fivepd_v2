import { TestBed } from '@angular/core/testing';

import { CalloutDetailsService } from './callout-details.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('CalloutDetailsService', () => {
  let service: CalloutDetailsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(CalloutDetailsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
