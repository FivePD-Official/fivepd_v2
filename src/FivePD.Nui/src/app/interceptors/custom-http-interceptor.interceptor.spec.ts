import { TestBed } from '@angular/core/testing';
import { CustomHttpInterceptor } from "./custom-http-interceptor.interceptor";
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('CustomHttpInterceptorInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    imports: [
      HttpClientTestingModule,
    ],
    providers: [
      CustomHttpInterceptor,
    ],
  }));

  it('should be created', () => {
    const interceptor: CustomHttpInterceptor = TestBed.inject(CustomHttpInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
