import { TestBed } from '@angular/core/testing';

import { BaseApiService } from './base-api.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('BaseApiService<T>', () => {
  let service: BaseApiService<any>;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(BaseApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
