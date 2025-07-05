import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from "@angular/forms";
import { SafeHtmlPipe } from "../../../pipes/safe-html.pipe";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { FormInputBase } from "@models";
import { CustomValidators } from "../../../helpers/validators";

@Component({
    selector: 'app-base-field',
    imports: [CommonModule, ReactiveFormsModule, SafeHtmlPipe, TranslatePipe],
    templateUrl: './base-field.component.html',
    styleUrl: './base-field.component.sass'
})
export class BaseFieldComponent {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: FormInputBase<any>;
  @Input({required: false}) control?: FormControl;

  get show() {
    if (!this.control) {
      return false;
    }

    const show = this.field.show(this.form);

    let validator: (control: AbstractControl) => ValidationErrors | null;
    if (this.field.type === 'singleSelect') {
      validator = CustomValidators.dropdownSingleRequired;
    } else if (this.field.type === 'multiSelect') {
      validator = CustomValidators.dropdownMultiRequired;
    } else {
      validator = Validators.required;
    }

    if (show && this.field.required && validator) {
      if (!this.control.hasValidator(validator)) {
        this.control.addValidators(validator);
        this.control.updateValueAndValidity();
      }
    } else {
      if (this.control.hasValidator(validator)) {
        this.control.removeValidators(validator);
        this.control.updateValueAndValidity();
      }
    }

    return show;
  }
}
