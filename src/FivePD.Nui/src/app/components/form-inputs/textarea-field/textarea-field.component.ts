import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseFieldComponent } from "../base-field/base-field.component";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { TextInput } from "@models";

@Component({
  selector: 'app-textarea-field',
  standalone: true,
  imports: [CommonModule, BaseFieldComponent, ReactiveFormsModule, TranslatePipe],
  templateUrl: './textarea-field.component.html',
  styleUrl: './textarea-field.component.sass'
})
export class TextareaFieldComponent implements OnInit {
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
