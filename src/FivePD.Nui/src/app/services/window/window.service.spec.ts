import { TestBed } from '@angular/core/testing';

import { WindowService } from './window.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('WindowService', () => {
  let service: WindowService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [TestHelperModule.builder()
            .withStore()
            .build()],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(WindowService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
