import { TestBed } from '@angular/core/testing';

import { AssistanceServiceService } from './assistance-service.service';

describe('AssistanceServiceService', () => {
  let service: AssistanceServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AssistanceServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
