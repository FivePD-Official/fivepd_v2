import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { NumberField } from "@models";
import { BaseFieldComponent } from "../base-field/base-field.component";
import { createDefaultFormGroup } from "../_helpers/field";

@Component({
    selector: 'app-number-field',
    imports: [ReactiveFormsModule, TranslatePipe, BaseFieldComponent],
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
      this.form = createDefaultFormGroup(this.field, null);
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
