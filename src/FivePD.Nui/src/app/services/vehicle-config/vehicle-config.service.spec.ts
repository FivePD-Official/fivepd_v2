import { TestBed } from '@angular/core/testing';

import { VehicleConfigService } from './vehicle-config.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('VehicleConfigService', () => {
  let service: VehicleConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(VehicleConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
