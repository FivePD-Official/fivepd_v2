import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VehicleSettingsComponent } from './vehicle-settings.component';

describe('VehicleSettingsComponent', () => {
  let component: VehicleSettingsComponent;
  let fixture: ComponentFixture<VehicleSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehicleSettingsComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(VehicleSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
