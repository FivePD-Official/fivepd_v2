import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CallsignEditorComponent } from './callsign-editor.component';
import { TestHelperModule } from "../../../../../../test-helpers/test-helper.module";
import { ModalRef } from "@services/modals/modal/modal-ref";
import {
  CALLSIGN_FORMAT_ITEMS
} from "../../../departments/departments-view/_components/department-callsign-format-editor/token";
import { CALLSIGN } from "./token";

describe('UserCallsignEditorComponent', () => {
  let component: CallsignEditorComponent;
  let fixture: ComponentFixture<CallsignEditorComponent>;

  const modalRef = new ModalRef({
    title: 'Title',
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CallsignEditorComponent,
        TestHelperModule.builder()
          .withLocalizationService()
          .withTranslatePipe()
          .build()
      ],
      providers: [
        {provide: CALLSIGN, useValue: ''},
        {provide: CALLSIGN_FORMAT_ITEMS, useValue: []},
        {provide: ModalRef, useValue: modalRef},
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CallsignEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
