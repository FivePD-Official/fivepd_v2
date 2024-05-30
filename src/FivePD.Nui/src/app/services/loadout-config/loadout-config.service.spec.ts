import {TestBed} from '@angular/core/testing';
import {HttpClientTestingModule} from "@angular/common/http/testing";
import {LoadoutConfigService} from "@services/loadout-config/loadout-config.service";

describe('LoadoutConfigService', () => {
  let service: LoadoutConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
      ],
    });
    service = TestBed.inject(LoadoutConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
