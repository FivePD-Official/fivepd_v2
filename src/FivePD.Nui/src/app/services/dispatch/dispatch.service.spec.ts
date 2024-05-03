import { TestBed } from '@angular/core/testing';
import { DispatchService } from './dispatch.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('DispatchService', () => {
  let service: DispatchService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        TestHelperModule.builder()
          .withStore()
          .build()
      ],
    });
    service = TestBed.inject(DispatchService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
