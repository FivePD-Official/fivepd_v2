import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimerBarComponent } from './timer-bar.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";

describe('TimerBarComponent', () => {
  let component: TimerBarComponent;
  let fixture: ComponentFixture<TimerBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TimerBarComponent,
        TestHelperModule.builder()
          .withWindowEventListener()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TimerBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
