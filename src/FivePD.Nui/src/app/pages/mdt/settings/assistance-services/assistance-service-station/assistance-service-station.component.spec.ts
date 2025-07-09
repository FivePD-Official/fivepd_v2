import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssistanceServiceStationComponent } from './assistance-service-station.component';

describe('AssistanceServiceStationComponent', () => {
  let component: AssistanceServiceStationComponent;
  let fixture: ComponentFixture<AssistanceServiceStationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssistanceServiceStationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AssistanceServiceStationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
