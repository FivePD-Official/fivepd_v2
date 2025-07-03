import { TestBed } from '@angular/core/testing';

import { DevtoolsService } from './devtools.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('DevtoolsService', () => {
  let service: DevtoolsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(DevtoolsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
