import { TestBed } from '@angular/core/testing';

import { UnitService } from './unit.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('UnitService', () => {
  let service: UnitService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(UnitService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
