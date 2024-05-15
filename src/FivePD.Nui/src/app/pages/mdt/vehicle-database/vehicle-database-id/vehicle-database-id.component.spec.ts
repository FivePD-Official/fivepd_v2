import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VehicleDatabaseIdComponent } from './vehicle-database-id.component';
import { RouterTestingModule } from "@angular/router/testing";
import { TestHelperModule } from "../../../../../test-helpers/test-helper.module";

describe('VehicleDatabaseIdComponent', () => {
  let component: VehicleDatabaseIdComponent;
  let fixture: ComponentFixture<VehicleDatabaseIdComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        VehicleDatabaseIdComponent,
        RouterTestingModule,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(VehicleDatabaseIdComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
