import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevtoolsViewComponent } from './devtools-view.component';
import { RouterTestingModule } from "@angular/router/testing";
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";

describe('DevtoolsViewComponent', () => {
  let component: DevtoolsViewComponent;
  let fixture: ComponentFixture<DevtoolsViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DevtoolsViewComponent,
        RouterTestingModule,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DevtoolsViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
