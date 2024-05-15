import { TestBed } from '@angular/core/testing';

import { LocalizationService } from './localization.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('LocalizationService', () => {
  let service: LocalizationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(LocalizationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
