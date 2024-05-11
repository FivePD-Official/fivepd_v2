import { TestBed } from '@angular/core/testing';
import { AudioService } from "@services/audio/audio.service";
import { HttpClientTestingModule, HttpTestingController } from "@angular/common/http/testing";

describe('AudioService', () => {
  let service: AudioService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule
      ]
    });
    service = TestBed.inject(AudioService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
