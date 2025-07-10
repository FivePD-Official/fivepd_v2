import { TestBed } from '@angular/core/testing';
import {
  AssistanceServiceStationService
} from "@services/assistance-service-station/assistance-service-station.service";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('AssistanceServiceStationService', () => {
  let service: AssistanceServiceStationService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TestHelperModule.builder()
          .build()
      ]
    });
    service = TestBed.inject(AssistanceServiceStationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
