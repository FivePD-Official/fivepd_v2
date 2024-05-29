import {ComponentFixture, TestBed} from '@angular/core/testing';
import {TippyModule} from "@ngneat/helipopper";
import {LoadoutWeaponsComponent} from './loadout-weapons.component';
import {TestHelperModule} from "../../../../../../test-helpers/test-helper.module";
import {mockedTippyModule} from "../../../../../../test-helpers/mock-tippy";

describe('LoadoutWeaponsComponent', () => {
  let component: LoadoutWeaponsComponent;
  let fixture: ComponentFixture<LoadoutWeaponsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        LoadoutWeaponsComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(LoadoutWeaponsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
