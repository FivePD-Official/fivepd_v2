import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VehicleDatabaseIdComponent } from './vehicle-database-id.component';

describe('VehicleDatabaseIdComponent', () => {
  let component: VehicleDatabaseIdComponent;
  let fixture: ComponentFixture<VehicleDatabaseIdComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehicleDatabaseIdComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(VehicleDatabaseIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
