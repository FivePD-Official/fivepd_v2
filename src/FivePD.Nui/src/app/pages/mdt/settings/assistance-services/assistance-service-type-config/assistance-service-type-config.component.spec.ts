import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssistanceServiceTypeConfigComponent } from './assistance-service-type-config.component';
import { TippyModule } from "@ngneat/helipopper";
import { mockedTippyModule } from "../../../../../../test-helpers/mock-tippy";
import { TestHelperModule } from "../../../../../../test-helpers/test-helper.module";

describe('AssistanceServiceTypeConfigComponent', () => {
  let component: AssistanceServiceTypeConfigComponent;
  let fixture: ComponentFixture<AssistanceServiceTypeConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AssistanceServiceTypeConfigComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(AssistanceServiceTypeConfigComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
