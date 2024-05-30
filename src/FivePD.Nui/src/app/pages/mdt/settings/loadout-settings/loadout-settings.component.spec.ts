import {ComponentFixture, TestBed} from '@angular/core/testing';
import {LoadoutSettingsComponent} from './loadout-settings.component';
import {TestHelperModule} from "../../../../../test-helpers/test-helper.module";
import {TippyModule} from "@ngneat/helipopper";
import {mockedTippyModule} from "../../../../../test-helpers/mock-tippy";

describe('LoadoutSettingsComponent', () => {
  let component: LoadoutSettingsComponent;
  let fixture: ComponentFixture<LoadoutSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LoadoutSettingsComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(LoadoutSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
