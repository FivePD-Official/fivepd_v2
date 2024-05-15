import { TestBed } from '@angular/core/testing';
import { LocationService } from "@services/location/location.service";
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('LocationService', () => {
  let service: LocationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(LocationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
