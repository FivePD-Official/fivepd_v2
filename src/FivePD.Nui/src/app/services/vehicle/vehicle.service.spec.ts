import { TestBed } from '@angular/core/testing';

import { VehicleService } from './vehicle.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('VehicleService', () => {
  let service: VehicleService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(VehicleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
