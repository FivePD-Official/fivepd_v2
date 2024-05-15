import { TestBed } from '@angular/core/testing';

import { CrudApiService } from './crud-api.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('CrudApiService<T>', () => {
  let service: CrudApiService<any>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(CrudApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
