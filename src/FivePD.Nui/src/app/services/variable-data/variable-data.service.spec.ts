import { TestBed } from '@angular/core/testing';
import { VariableDataService } from './variable-data.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('VariableDataService', () => {
  let service: VariableDataService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(VariableDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
