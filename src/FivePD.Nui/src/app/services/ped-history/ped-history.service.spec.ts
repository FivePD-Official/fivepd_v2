import { TestBed } from '@angular/core/testing';

import { PedHistoryService } from './ped-history.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('PedHistoryService', () => {
  let service: PedHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(PedHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
