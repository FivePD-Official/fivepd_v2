import { TestBed } from '@angular/core/testing';

import { LocalizationService } from './localization.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('LocalizationService', () => {
  let service: LocalizationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(LocalizationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
