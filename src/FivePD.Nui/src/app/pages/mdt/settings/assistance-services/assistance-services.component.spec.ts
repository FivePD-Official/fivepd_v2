import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssistanceServicesComponent } from './assistance-services.component';

describe('AssistanceServicesComponent', () => {
  let component: AssistanceServicesComponent;
  let fixture: ComponentFixture<AssistanceServicesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssistanceServicesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AssistanceServicesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
