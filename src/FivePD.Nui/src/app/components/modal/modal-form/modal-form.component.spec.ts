import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModalFormComponent } from './modal-form.component';
import { ModalRef } from "@services/modals/modal/modal-ref";
import { TestHelperModule } from "../../../../test-helpers/test-helper.module";

describe('ModalFormComponent', () => {
  let component: ModalFormComponent;
  let fixture: ComponentFixture<ModalFormComponent>;

  const modalRef = new ModalRef({
    title: 'Title',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ModalFormComponent,
        TestHelperModule.builder()
          .withWindowEventListener()
          .withStore()
          .build()
      ],
      providers: [
        {provide: ModalRef, useValue: modalRef}
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ModalFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
