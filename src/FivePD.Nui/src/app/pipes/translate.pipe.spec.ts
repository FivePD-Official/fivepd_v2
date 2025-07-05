import { TranslatePipe } from './translate.pipe';
import { TestBed } from '@angular/core/testing';
import { LocalizationService } from '@services/localization/localization.service';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

  jest.mock('lodash-es/get', () => {
    return {
      __esModule: true,
      default: 'mockDefaultExport',
      nameExport: jest.fn(),
    }
  })

describe('TranslatePipe', () => {
  let pipe: TranslatePipe;
  let localizationService: LocalizationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [],
    providers: [
        LocalizationService,
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
    ]
}).compileComponents();

    localizationService = TestBed.inject(LocalizationService);
    pipe = new TranslatePipe(localizationService);
  });

  it('can be created', () => {
    expect(pipe).toBeTruthy();
  });
});
