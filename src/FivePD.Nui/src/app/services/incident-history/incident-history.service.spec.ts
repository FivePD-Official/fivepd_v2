import { TestBed } from '@angular/core/testing';

import { IncidentHistoryService } from './incident-history.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('IncidentHistoryService', () => {
  let service: IncidentHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [TestHelperModule.builder()
            .withStore()
            .build()],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(IncidentHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
