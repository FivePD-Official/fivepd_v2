import { TestBed } from '@angular/core/testing';

import { IncidentHistoryService } from './incident-history.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('IncidentHistoryService', () => {
  let service: IncidentHistoryService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        TestHelperModule.builder()
          .withStore()
          .build()
      ],
    });
    service = TestBed.inject(IncidentHistoryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
