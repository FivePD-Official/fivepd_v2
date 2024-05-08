import { TestBed } from '@angular/core/testing';

import { SceneManagementService } from './scene-management.service';

describe('SceneManagementService', () => {
  let service: SceneManagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SceneManagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
