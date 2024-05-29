import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TippyModule} from "@ngneat/helipopper";
import {LoadoutWeaponComponentsComponent} from './loadout-weapon-components.component';
import {TestHelperModule} from "../../../../../../test-helpers/test-helper.module";
import {mockedTippyModule} from "../../../../../../test-helpers/mock-tippy";

describe('LoadoutWeaponComponentsComponent', () => {
  let component: LoadoutWeaponComponentsComponent;
  let fixture: ComponentFixture<LoadoutWeaponComponentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LoadoutWeaponComponentsComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(LoadoutWeaponComponentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
