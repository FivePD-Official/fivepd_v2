import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from "../../../../../components/button/button.component";
import { IconComponent } from "../../../../../components/icon/icon.component";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { CallsignFormatItemType } from "@models";
import { ModalRef } from "@services/modals/modal/modal-ref";
import {
  CALLSIGN_FORMAT_ITEMS
} from "../../../departments/departments-view/_components/department-callsign-format-editor/token";
import { CALLSIGN } from "./token";

@Component({
  selector: 'app-user-callsign-editor',
  standalone: true,
  imports: [CommonModule, ButtonComponent, IconComponent, TranslatePipe],
  templateUrl: './callsign-editor.component.html',
})
export class CallsignEditorComponent implements OnInit {

  private readonly _modalRef = inject(ModalRef);
  protected readonly _positions = inject(CALLSIGN_FORMAT_ITEMS);
  private readonly _callsign = inject(CALLSIGN);

  ngOnInit(): void {
    if (!this._positions) {
      return;
    }

    for (let i = 0; i < this._callsign.length; i++) {
      this._positions[i].Value = this._callsign[i];
    }
  }

  get valid() {
    return this._positions.every(x => x.Value);
  }

  getItemTypeLabel(type: CallsignFormatItemType): string {
    if (type === CallsignFormatItemType.Dash) {
      return '-';
    }

    if (type === CallsignFormatItemType.FixedCharacter) {
      return 'Any';
    }

    if (type === CallsignFormatItemType.RandomLetter) {
      return 'A-Z';
    }

    if (type === CallsignFormatItemType.RandomNumber) {
      return '0-9';
    }

    return '';
  }

  onCharacterChange(index: number, event: any) {
    this._positions[index].Value = event.target.value.toString().toUpperCase();
  }

  submit() {
    if (!this.valid) {
      return;
    }

    if (this._modalRef) {
      this._modalRef.close({
        success: true,
        value: {
          Callsign: this._positions.map(x => x.Value).join(''),
        },
      });
    }
  }

  protected readonly CallsignFormatItemType = CallsignFormatItemType;
}
