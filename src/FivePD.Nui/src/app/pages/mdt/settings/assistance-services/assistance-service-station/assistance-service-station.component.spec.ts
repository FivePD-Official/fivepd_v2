import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssistanceServiceStationComponent } from './assistance-service-station.component';
import { TestHelperModule } from "../../../../../../test-helpers/test-helper.module";
import { TippyModule } from "@ngneat/helipopper";
import { mockedTippyModule } from "../../../../../../test-helpers/mock-tippy";

describe('AssistanceServiceStationComponent', () => {
  let component: AssistanceServiceStationComponent;
  let fixture: ComponentFixture<AssistanceServiceStationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AssistanceServiceStationComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(AssistanceServiceStationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
