import { TestBed } from '@angular/core/testing';
import {
  AssistanceServiceStationService
} from "@services/assistance-service-station/assistance-service-station.service";

describe('AssistanceServiceStationService', () => {
  let service: AssistanceServiceStationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AssistanceServiceStationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
