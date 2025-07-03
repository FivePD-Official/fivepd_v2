import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { TextField } from "@models";
import { BaseFieldComponent } from "../base-field/base-field.component";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { DateMaskDirective } from "../../../directives/date-mask.directive";
import { createDefaultFormGroup } from "../_helpers/field";

@Component({
    selector: 'app-date-field',
    imports: [CommonModule, BaseFieldComponent, TranslatePipe, ReactiveFormsModule, DateMaskDirective],
    templateUrl: './date-field.component.html',
    styleUrl: './date-field.component.sass'
})
export class DateFieldComponent implements OnInit {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: TextField;

  @Output() onChange = new EventEmitter<string>();

  private pseudoFormControl = new FormControl<string | null>('');

  ngOnInit(): void {
    if (!this.form) {
      this.form = createDefaultFormGroup(this.field, '');
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
