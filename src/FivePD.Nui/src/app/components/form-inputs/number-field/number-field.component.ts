import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { NumberField } from "@models";
import { BaseFieldComponent } from "../base-field/base-field.component";

@Component({
  selector: 'app-number-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, BaseFieldComponent],
  templateUrl: './number-field.component.html',
  styleUrl: './number-field.component.sass'
})
export class NumberFieldComponent implements OnInit {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: NumberField;

  @Output() onChange = new EventEmitter<number>();

  private pseudoFormControl = new FormControl<number | null>(null);

  ngOnInit(): void {
    if (!this.form) {
      const validators = [...this.field.validators];
      if (this.field.required) {
        validators.push(Validators.required);
      }

      const asyncValidators = [...this.field.asyncValidators];

      this.pseudoFormControl = new FormControl({
        value: this.field.value ?? null,
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
