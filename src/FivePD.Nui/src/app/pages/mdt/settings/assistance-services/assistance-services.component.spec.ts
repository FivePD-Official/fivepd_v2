import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AssistanceServicesComponent } from './assistance-services.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";
import { TippyModule } from "@ngneat/helipopper";
import { mockedTippyModule } from "../../../../../test-helpers/mock-tippy";

describe('AssistanceServicesComponent', () => {
  let component: AssistanceServicesComponent;
  let fixture: ComponentFixture<AssistanceServicesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AssistanceServicesComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(AssistanceServicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
