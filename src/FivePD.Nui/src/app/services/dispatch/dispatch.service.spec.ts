import { TestBed } from '@angular/core/testing';
import { DispatchService } from './dispatch.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('DispatchService', () => {
  let service: DispatchService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [TestHelperModule.builder()
            .withStore()
            .build()],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(DispatchService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
