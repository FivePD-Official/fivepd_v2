import {TestBed} from '@angular/core/testing';
import { provideHttpClientTesting } from "@angular/common/http/testing";
import {WeaponComponentService} from "@services/weapon-component/weapon-component.service";
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('WeaponComponentService', () => {
  let service: WeaponComponentService;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    service = TestBed.inject(WeaponComponentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
