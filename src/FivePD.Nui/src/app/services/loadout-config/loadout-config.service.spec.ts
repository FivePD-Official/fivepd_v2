import {TestBed} from '@angular/core/testing';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import {LoadoutConfigService} from "@services/loadout-config/loadout-config.service";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('LoadoutConfigService', () => {
  let service: LoadoutConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(LoadoutConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
