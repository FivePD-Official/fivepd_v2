import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ModalComponent } from './modal.component';
import { TestHelperModule } from "../../../test-helpers/test-helper.module";
import { ModalRef } from "@services/modals/modal/modal-ref";
import { EXTRA_OBJECTS_TO_INJECT } from "./extra-objects-token";

describe.skip('ModalComponent', () => {
  let component: ModalComponent;
  let fixture: ComponentFixture<ModalComponent>;

  const modalRef = new ModalRef({
    title: 'Title',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ModalComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ],
      providers: [
        {provide: ModalRef, useValue: modalRef},
        {provide: EXTRA_OBJECTS_TO_INJECT, useValue: new Map()}
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
