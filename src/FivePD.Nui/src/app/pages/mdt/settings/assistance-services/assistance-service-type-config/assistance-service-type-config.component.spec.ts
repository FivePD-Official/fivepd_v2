import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssistanceServiceTypeConfigComponent } from './assistance-service-type-config.component';

describe('AssistanceServiceTypeConfigComponent', () => {
  let component: AssistanceServiceTypeConfigComponent;
  let fixture: ComponentFixture<AssistanceServiceTypeConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssistanceServiceTypeConfigComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AssistanceServiceTypeConfigComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
