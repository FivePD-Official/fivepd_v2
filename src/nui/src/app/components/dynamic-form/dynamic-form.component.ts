import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormInputBase } from '@models';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-dynamic-form',
  templateUrl: './dynamic-form.component.html',
  styleUrls: ['./dynamic-form.component.sass']
})
export class DynamicFormComponent implements OnInit {

  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() formFields: FormInputBase<any>[] = [];
  @Output() onSubmit = new EventEmitter();
  form!: FormGroup;

  ngOnInit(): void {
    const group: Record<string, FormControl | FormGroup> = {};

    this.formFields.forEach(field => {
      let formControl;
      if (field.type === 'checkboxGroup') {
        const nestedGroup: Record<string, FormControl> = {};
        field.checkboxGroup.forEach(checkbox => {
          nestedGroup[checkbox.key] = new FormControl(checkbox.value || false, field.validators);
        });
        formControl = new FormGroup(nestedGroup);
      } else {
        if (field.required) {
          if (field.type === 'dropdown') {
            field.validators.push((control) => control.value === -1 ? { 'required': true } : null);
          }

          formControl = new FormControl(
            field.value || (field.type === 'checkbox' ? false : ''),
            [Validators.required, ...field.validators],
            [...field.asyncValidators]
          );
        } else {
          formControl = new FormControl(
            field.value || (field.type === 'checkbox' ? false : ''),
            field.validators,
            field.asyncValidators
          );
        }
      }
      group[field.key] = formControl;
    });
    this.form = new FormGroup(group);
  }

  internalOnSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    this.onSubmit.emit(this.form.value);
  }

}
