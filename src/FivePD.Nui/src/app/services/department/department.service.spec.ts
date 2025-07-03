import { TestBed } from '@angular/core/testing';
import { DepartmentService } from './department.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('DepartmentService', () => {
  let service: DepartmentService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(DepartmentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
