import { TestBed } from '@angular/core/testing';

import { VehicleService } from './vehicle.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('VehicleService', () => {
  let service: VehicleService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(VehicleService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
