import { TestBed } from '@angular/core/testing';

import { AccessControlService } from './access-control.service';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('AccessControlService', () => {
  let service: AccessControlService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TestHelperModule.builder()
          .build()
      ]
    });
    service = TestBed.inject(AccessControlService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
