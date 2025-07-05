import { TestBed } from '@angular/core/testing';
import { VariableDataService } from './variable-data.service';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('VariableDataService', () => {
  let service: VariableDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(VariableDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
