import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseFieldComponent } from "../base-field/base-field.component";
import { FormControl, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { CheckboxField } from "@models";
import { TranslatePipe } from "../../../pipes/translate.pipe";
import { createDefaultFormGroup } from "../_helpers/field";

@Component({
  selector: 'app-checkbox-field',
  standalone: true,
  imports: [CommonModule, BaseFieldComponent, ReactiveFormsModule, TranslatePipe],
  templateUrl: './checkbox-field.component.html',
  styleUrl: './checkbox-field.component.sass'
})
export class CheckboxFieldComponent implements OnInit {
  @Input({required: false}) form!: FormGroup;
  @Input({required: true}) field!: CheckboxField;

  @Output() onChange = new EventEmitter<string>();

  private pseudoFormControl = new FormControl<boolean>(false);

  ngOnInit(): void {
    if (!this.form) {
      this.form = createDefaultFormGroup(this.field, false);
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
