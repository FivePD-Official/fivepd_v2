import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VehicleDatabaseIndexComponent } from './vehicle-database-index.component';

describe('VehicleDatabaseIndexComponent', () => {
  let component: VehicleDatabaseIndexComponent;
  let fixture: ComponentFixture<VehicleDatabaseIndexComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VehicleDatabaseIndexComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(VehicleDatabaseIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
