import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DepartmentAreaManagementComponent } from './department-area-management.component';
import { TestHelperModule } from "../../../../../../test-helpers/test-helper.module";

describe('DepartmentSettingsAreaComponent', () => {
  let component: DepartmentAreaManagementComponent;
  let fixture: ComponentFixture<DepartmentAreaManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DepartmentAreaManagementComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .withStore()
          .build()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DepartmentAreaManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
