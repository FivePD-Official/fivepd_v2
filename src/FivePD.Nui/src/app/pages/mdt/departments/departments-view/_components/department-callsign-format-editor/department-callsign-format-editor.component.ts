import { Component, EventEmitter, Inject, Input, Optional, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from "../../../../../../components/button/button.component";
import {
  SingleSelectFieldComponent
} from "../../../../../../components/form-inputs/single-select-field/single-select-field.component";
import { ReactiveFormsModule } from "@angular/forms";
import { TextFieldComponent } from "../../../../../../components/form-inputs/text-field/text-field.component";
import { IconComponent } from "../../../../../../components/icon/icon.component";
import { TranslatePipe } from "../../../../../../pipes/translate.pipe";
import { CALLSIGN_FORMAT_ITEMS } from "./token";
import { ModalRef } from "@services/modals/modal/modal-ref";
import { CallsignFormatItem, CallsignFormatItemType } from "@models";

export interface CallsignFormatChangeEvent {
  value: CallsignFormatItem[];
  valid: boolean;
}

@Component({
  selector: 'app-department-callsign-format-modal',
  standalone: true,
  imports: [CommonModule, ButtonComponent, SingleSelectFieldComponent, ReactiveFormsModule, TextFieldComponent, IconComponent, TranslatePipe],
  templateUrl: './department-callsign-format-editor.component.html',
})
export class DepartmentCallsignFormatEditorComponent {
  _positions: CallsignFormatItem[] = [];

  @Input() showSubmitButton = true;
  @Output() onChange = new EventEmitter<CallsignFormatChangeEvent>();

  constructor(@Inject(CALLSIGN_FORMAT_ITEMS) @Optional() readonly positions: CallsignFormatItem[],
              @Inject(ModalRef) @Optional() readonly modalRef: ModalRef) {
    this._positions = positions ?? [];
  }

  get isLastPositionDash() {
    return this._positions[this._positions.length - 1]?.Type === CallsignFormatItemType.Dash;
  }

  get hasNoRandomPositions() {
    return !this._positions.some(x => x.Type === CallsignFormatItemType.RandomLetter || x.Type === CallsignFormatItemType.RandomNumber);
  }

  get hasEmptyFixedCharacterPositions() {
    return this._positions.some(x => x.Type === CallsignFormatItemType.FixedCharacter && !x.Value);
  }

  get valid() {
    return this._positions.length > 0 && !this.hasNoRandomPositions && !this.hasEmptyFixedCharacterPositions && !this.isLastPositionDash;
  }

  emitChange() {
    this.onChange.emit({
      value: this._positions,
      valid: this.valid,
    });
  }

  add(type: CallsignFormatItemType) {
    this._positions.push({
      Type: type,
    });
    this.emitChange();
  }

  removePosition(index: number) {
    this._positions.splice(index, 1);
    this.emitChange();
  }

  onFixedCharacterChange(index: number, event: any) {
    this._positions[index].Value = event.target.value.toUpperCase();
    this.emitChange();
  }

  submit() {
    if (!this.valid) {
      return;
    }

    if (this.modalRef) {
      this.modalRef.close({
        success: true,
        value: this._positions,
      });
    }
  }

  protected readonly CallsignFormatItemType = CallsignFormatItemType;
}
