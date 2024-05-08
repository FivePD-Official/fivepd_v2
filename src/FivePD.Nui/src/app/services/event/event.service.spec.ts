import { TestBed } from '@angular/core/testing';

import { EventService } from './event.service';
import { HttpClientTestingModule } from "@angular/common/http/testing";
import { RouterTestingHarness, RouterTestingModule } from "@angular/router/testing";

describe('EventService', () => {
  let service: EventService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        RouterTestingModule,
      ],
    });
    service = TestBed.inject(EventService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
