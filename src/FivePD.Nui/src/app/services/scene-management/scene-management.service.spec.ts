import { TestBed } from '@angular/core/testing';

import { SceneManagementService } from './scene-management.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";

describe('SceneManagementService', () => {
  let service: SceneManagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(SceneManagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
