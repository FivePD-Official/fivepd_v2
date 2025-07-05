import { TestBed } from '@angular/core/testing';

import { CalloutService } from './callout.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('CalloutService', () => {
  let service: CalloutService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(CalloutService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
