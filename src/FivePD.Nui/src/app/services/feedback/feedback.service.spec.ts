import { TestBed } from '@angular/core/testing';

import { FeedbackService } from './feedback.service';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('FeedbackService', () => {
  let service: FeedbackService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TestHelperModule.builder()
          .build()
      ]
    });
    service = TestBed.inject(FeedbackService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
