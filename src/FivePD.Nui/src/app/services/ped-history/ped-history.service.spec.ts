import { TestBed } from '@angular/core/testing';

import { PedHistoryService } from './ped-history.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('PedHistoryService', () => {
  let service: PedHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(PedHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
