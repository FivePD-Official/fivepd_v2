import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalloutsComponent } from './callouts.component';
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";

describe('CalloutsComponent', () => {
  let component: CalloutsComponent;
  let fixture: ComponentFixture<CalloutsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CalloutsComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CalloutsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
