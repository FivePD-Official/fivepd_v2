import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CalloutDetailsComponent } from './callout-details.component';

describe('CalloutDetailsComponent', () => {
  let component: CalloutDetailsComponent;
  let fixture: ComponentFixture<CalloutDetailsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [CalloutDetailsComponent]
    });
    fixture = TestBed.createComponent(CalloutDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
