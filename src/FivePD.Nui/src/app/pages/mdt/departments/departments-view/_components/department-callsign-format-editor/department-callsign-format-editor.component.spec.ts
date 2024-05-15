import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DepartmentCallsignFormatEditorComponent } from './department-callsign-format-editor.component';
import { TestHelperModule } from "../../../../../../../test-helpers/test-helper.module";
import { CALLSIGN_FORMAT_ITEMS } from "./token";
import { ModalRef } from "@services/modals/modal/modal-ref";

describe('DepartmentCallsignFormatModalComponent', () => {
  let component: DepartmentCallsignFormatEditorComponent;
  let fixture: ComponentFixture<DepartmentCallsignFormatEditorComponent>;

  const modalRef = new ModalRef({
    title: 'Title',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        DepartmentCallsignFormatEditorComponent,
        TestHelperModule.builder()
          .withTranslatePipe()
          .withLocalizationService()
          .build()
      ],
      providers: [
        {provide: CALLSIGN_FORMAT_ITEMS, useValue: []},
        {provide: ModalRef, useValue: modalRef},
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DepartmentCallsignFormatEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
