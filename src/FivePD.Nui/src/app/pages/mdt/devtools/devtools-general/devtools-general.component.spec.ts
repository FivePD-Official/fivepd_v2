import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DevtoolsGeneralComponent } from './devtools-general.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";
import { TippyModule } from "@ngneat/helipopper";
import { ButtonComponent } from "../../../../components/button/button.component";
import { mockedTippyModule } from "../../../../../test-helpers/mock-tippy";

describe('DevtoolsViewComponent', () => {
  let component: DevtoolsGeneralComponent;
  let fixture: ComponentFixture<DevtoolsGeneralComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DevtoolsGeneralComponent,
        ButtonComponent,
        TestHelperModule.builder()
          .withWindowEventListener()
          .withStore()
          .withLocalizationService()
          .build()
      ],
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(DevtoolsGeneralComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
