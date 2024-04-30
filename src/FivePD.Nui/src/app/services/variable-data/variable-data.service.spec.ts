import { TestBed } from '@angular/core/testing';
import { VariableDataService } from './variable-data.service';

describe('VariableDataService', () => {
  let service: VariableDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VariableDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
