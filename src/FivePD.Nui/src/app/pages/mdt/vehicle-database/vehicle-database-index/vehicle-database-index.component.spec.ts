import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VehicleDatabaseIndexComponent } from './vehicle-database-index.component';
import { RouterTestingModule } from "@angular/router/testing";
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";

describe('VehicleDatabaseIndexComponent', () => {
  let component: VehicleDatabaseIndexComponent;
  let fixture: ComponentFixture<VehicleDatabaseIndexComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        VehicleDatabaseIndexComponent,
        RouterTestingModule,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VehicleDatabaseIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
