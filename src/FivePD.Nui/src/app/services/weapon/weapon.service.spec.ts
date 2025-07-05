import {TestBed} from '@angular/core/testing';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import {WeaponService} from "@services/weapon/weapon.service";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('WeaponService', () => {
  let service: WeaponService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(WeaponService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
