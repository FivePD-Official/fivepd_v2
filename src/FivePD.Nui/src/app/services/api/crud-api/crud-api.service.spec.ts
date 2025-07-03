import { TestBed } from '@angular/core/testing';

import { CrudApiService } from './crud-api.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('CrudApiService<T>', () => {
  let service: CrudApiService<any>;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(CrudApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
