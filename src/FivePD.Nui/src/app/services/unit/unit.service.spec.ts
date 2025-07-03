import { TestBed } from '@angular/core/testing';

import { UnitService } from './unit.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('UnitService', () => {
  let service: UnitService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(UnitService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
