import { TestBed } from '@angular/core/testing';

import { VehicleConfigService } from './vehicle-config.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('VehicleConfigService', () => {
  let service: VehicleConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(VehicleConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
