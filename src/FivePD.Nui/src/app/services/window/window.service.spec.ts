import { TestBed } from '@angular/core/testing';

import { WindowService } from './window.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('WindowService', () => {
  let service: WindowService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        TestHelperModule.builder()
          .withStore()
          .build()
      ],
    });
    service = TestBed.inject(WindowService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
