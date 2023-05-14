import { TestBed } from '@angular/core/testing';

import { DevtoolsService } from './devtools.service';

describe('DevtoolsService', () => {
  let service: DevtoolsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DevtoolsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
