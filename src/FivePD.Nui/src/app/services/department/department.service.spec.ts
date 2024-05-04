import { TestBed } from '@angular/core/testing';
import { DepartmentService } from './department.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('DepartmentService', () => {
  let service: DepartmentService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(DepartmentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
