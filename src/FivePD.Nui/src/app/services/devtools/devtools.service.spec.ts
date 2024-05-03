import { TestBed } from '@angular/core/testing';

import { DevtoolsService } from './devtools.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('DevtoolsService', () => {
  let service: DevtoolsService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(DevtoolsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
