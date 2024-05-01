import { TestBed } from '@angular/core/testing';

import { CrudApiService } from './crud-api.service';

describe('CrudApiService', () => {
  let service: CrudApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CrudApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
