import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateDepartmentModalComponent } from './create-department-modal.component';
import { ModalRef } from "@services/modals/modal/modal-ref";
import { TestHelperModule } from "../../../../../../../test-helpers/test-helper.module";

describe('CreateDepartmentModelComponent', () => {
  let component: CreateDepartmentModalComponent;
  let fixture: ComponentFixture<CreateDepartmentModalComponent>;

  const modalRef = new ModalRef({
    title: 'Title',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CreateDepartmentModalComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ],
      providers: [
        {provide: ModalRef, useValue: modalRef},
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CreateDepartmentModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
