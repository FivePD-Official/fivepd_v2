import { Component, inject } from '@angular/core';

import { FormChangeEvent, NumberField, TextField } from "@models";
import { CustomValidators } from "../../../../../../helpers/validators";
import { Validators } from "@angular/forms";
import { DepartmentService } from "@services/department/department.service";
import { DynamicFormComponent } from "../../../../../../components/dynamic-form/dynamic-form.component";
import { ButtonComponent } from "../../../../../../components/button/button.component";
import { TranslatePipe } from "../../../../../../pipes/translate.pipe";
import {
  CallsignFormatChangeEvent,
  DepartmentCallsignFormatEditorComponent
} from "../department-callsign-format-editor/department-callsign-format-editor.component";
import { ModalRef } from "@services/modals/modal/modal-ref";

@Component({
    selector: 'app-create-department-model',
    imports: [DynamicFormComponent, ButtonComponent, TranslatePipe, DepartmentCallsignFormatEditorComponent],
    templateUrl: './create-department-modal.component.html'
})
export class CreateDepartmentModalComponent {

  private readonly _modalRef = inject(ModalRef);
  private readonly _departmentService = inject(DepartmentService);

  formEvent?: FormChangeEvent;
  callsignFormatEvent?: CallsignFormatChangeEvent;
  fields = [
    new TextField({
      key: 'Name',
      required: true,
      label: 'LABEL.NAME.SINGULAR',
      asyncValidators: [CustomValidators.nameShouldNotBeTaken(value => this._departmentService.checkIfDepartmentNameTaken(value))],
    }),
    new TextField({
      key: 'Callsign',
      label: 'LABEL.CALLSIGN.TITLE',
      helpLabel: 'LABEL.CALLSIGN.HELP_LABEL',
      validators: [CustomValidators.onlyLetters()],
    }),
    new NumberField({
      key: 'MaxUsers',
      required: true,
      label: 'LABEL.MAX_MEMBERS.TITLE',
      helpLabel: 'LABEL.MAX_MEMBERS.HELP_LABEL',
      validators: [Validators.min(0)],
      options: {
        min: 0,
      }
    }),
  ];

  get valid() {
    return this.formEvent?.valid && this.callsignFormatEvent?.valid;
  }

  onChange(event: FormChangeEvent) {
    this.formEvent = event;
  }

  onCallsignFormatChange(event: CallsignFormatChangeEvent) {
    this.callsignFormatEvent = event;
  }

  onSubmit(): void {
    if (!this.formEvent || !this.callsignFormatEvent) {
      return;
    }

    this._modalRef.close({
      success: true,
      value: {
        ...this.formEvent!.value,
        CallsignFormat: [...this.callsignFormatEvent!.value],
      },
    });
  }

}
