import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  ControlValueAccessor,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors, Validators
} from "@angular/forms";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { FormInputBase, TextInput } from "@models";
import { CustomValidators } from "../../../helpers/validators";
import { SafeHtmlPipe } from "../../../pipes/safe-html.pipe";
import { BaseFieldComponent } from "../base-field/base-field.component";

@Component({
  selector: 'app-text-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, SafeHtmlPipe, BaseFieldComponent],
  templateUrl: './text-field.component.html',
  styleUrl: './text-field.component.sass'
})
export class TextFieldComponent implements OnInit {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: TextInput;

  @Output() onChange = new EventEmitter<string>();

  private pseudoFormControl = new FormControl<string | null>('');

  ngOnInit(): void {
    if (!this.form) {
      const validators = [...this.field.validators];
      if (this.field.required) {
        validators.push(Validators.required);
      }

      const asyncValidators = [...this.field.asyncValidators];

      this.pseudoFormControl = new FormControl({
        value: this.field.value ?? '',
        disabled: this.field.readonly
      }, validators, asyncValidators);

      this.form = new FormGroup({
        [this.field.key]: this.pseudoFormControl,
      });
    }

    if (!this.control) {
      throw new Error('Form control is undefined.');
    }

    this.control.valueChanges.subscribe(v => {
      this.onChange.emit(v);
    });
  }

  get control() {
    return (this.form?.controls[this.field.key] ?? this.pseudoFormControl) as FormControl;
  }
}
