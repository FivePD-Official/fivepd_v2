import { TestBed } from '@angular/core/testing';
import { AudioService } from "@services/audio/audio.service";
import { HttpTestingController } from "@angular/common/http/testing";
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('AudioService', () => {
  let service: AudioService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .withStore()
          .build()
      ]
    });
    service = TestBed.inject(AudioService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
