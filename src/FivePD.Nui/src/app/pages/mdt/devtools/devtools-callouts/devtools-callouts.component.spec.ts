import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DevtoolsCalloutsComponent } from './devtools-callouts.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";

describe('DevtoolsCalloutsComponent', () => {
  let component: DevtoolsCalloutsComponent;
  let fixture: ComponentFixture<DevtoolsCalloutsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DevtoolsCalloutsComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DevtoolsCalloutsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
