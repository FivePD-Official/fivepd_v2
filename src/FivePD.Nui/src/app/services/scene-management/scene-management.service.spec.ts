import { TestBed } from '@angular/core/testing';

import { SceneManagementService } from './scene-management.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('SceneManagementService', () => {
  let service: SceneManagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(SceneManagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
