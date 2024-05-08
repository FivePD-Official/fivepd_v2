import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DepartmentsViewComponent } from './departments-view.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";
import { RouterTestingModule } from "@angular/router/testing";
import { TippyModule } from "@ngneat/helipopper";
import { mockedTippyModule } from "../../../../../test-helpers/mock-tippy";

describe('DepartmentsViewComponent', () => {
  let component: DepartmentsViewComponent;
  let fixture: ComponentFixture<DepartmentsViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DepartmentsViewComponent,
        RouterTestingModule,
        TestHelperModule.builder()
          .withStore()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    })
      .overrideModule(TippyModule, mockedTippyModule())
      .compileComponents();

    fixture = TestBed.createComponent(DepartmentsViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
