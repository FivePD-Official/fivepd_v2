import { TestBed } from '@angular/core/testing';

import { AssistanceServiceTypeConfigService } from './assistance-service-type-config.service';

describe('AssistanceServiceTypeConfigService', () => {
  let service: AssistanceServiceTypeConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AssistanceServiceTypeConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
