import { TestBed } from '@angular/core/testing';

import { MenuService } from './menu.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('MenuService', () => {
  let service: MenuService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(MenuService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
