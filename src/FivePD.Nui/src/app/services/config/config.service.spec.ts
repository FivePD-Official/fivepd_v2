import { TestBed } from '@angular/core/testing';
import { ConfigService } from './config.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('ConfigService', () => {
  let service: ConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [TestHelperModule.builder()
            .withStore()
            .build()],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(ConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
