import { TestBed } from '@angular/core/testing';

import { VehicleConfigService } from './vehicle-config.service';

describe('VehicleConfigService', () => {
  let service: VehicleConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VehicleConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
