import { TestBed } from '@angular/core/testing';
import { AssistanceServiceTypeConfigService } from './assistance-service-type-config.service';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('AssistanceServiceTypeConfigService', () => {
  let service: AssistanceServiceTypeConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TestHelperModule.builder()
          .build()
      ]
    });
    service = TestBed.inject(AssistanceServiceTypeConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
