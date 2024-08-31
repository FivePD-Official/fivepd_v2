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
import { CALLSIGN_FORMAT_VALUES } from "./token";

@Component({
  selector: 'app-user-callsign-editor',
  standalone: true,
  imports: [CommonModule, ButtonComponent, IconComponent, TranslatePipe],
  templateUrl: './callsign-editor.component.html',
})
export class CallsignEditorComponent implements OnInit {

  protected readonly positions = inject(CALLSIGN_FORMAT_ITEMS);
  protected readonly errors: boolean[] = [];

  private readonly _modalRef = inject(ModalRef);
  private readonly _values = inject(CALLSIGN_FORMAT_VALUES);

  ngOnInit(): void {
    if (!this.positions) {
      return;
    }

    for (let i = 0; i < this.positions.length; i++) {
      this.positions[i].Value = this._values[i];
      this.errors.push(false);
    }
  }

  get valid() {
    return this.positions.every(x => x.Value);
  }

  getItemTypeLabel(type: CallsignFormatItemType): string {
    if (type === CallsignFormatItemType.Dash) {
      return '';
    }

    if (type === CallsignFormatItemType.FixedCharacter) {
      return '';
    }

    if (type === CallsignFormatItemType.RandomLetter) {
      return 'A-Z';
    }

    if (type === CallsignFormatItemType.RandomNumber) {
      return '0-9';
    }

    if (type === CallsignFormatItemType.Word) {
      return 'Any';
    }

    return '';
  }

  onCharacterChange(index: number, event: any) {
    const value = event.target.value.toString().toUpperCase();
    this.positions[index].Value = value;

    const position = this.positions[index];
    if (position.Type === CallsignFormatItemType.RandomLetter) {
      this.errors[index] = !/^[A-Z]+$/.test(value);
    } else if (position.Type === CallsignFormatItemType.RandomNumber) {
      this.errors[index] = !/^[0-9]+$/.test(value);
    } else if (position.Type === CallsignFormatItemType.Word) {
      this.errors[index] = !/^[A-Z0-9-]+$/.test(value);
    }
  }

  submit() {
    if (!this.valid) {
      return;
    }

    if (this._modalRef) {
      this._modalRef.close({
        success: true,
        value: {
          CallsignFormatValues: this.positions.map(x => x.Value),
        },
      });
    }
  }

  protected readonly CallsignFormatItemType = CallsignFormatItemType;
}
