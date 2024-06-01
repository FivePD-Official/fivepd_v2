import { TestBed } from '@angular/core/testing';

import { RankService } from './rank.service';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('RankService', () => {
  let service: RankService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TestHelperModule.builder()
          .build()
      ]
    });
    service = TestBed.inject(RankService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
