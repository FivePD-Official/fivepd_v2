import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalloutDetailsComponent } from './callout-details.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";
import { RouterTestingModule } from "@angular/router/testing";

describe('CalloutDetailsComponent', () => {
  let component: CalloutDetailsComponent;
  let fixture: ComponentFixture<CalloutDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CalloutDetailsComponent,
        RouterTestingModule,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CalloutDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
