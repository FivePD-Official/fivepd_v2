import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { FormInputBase, FormSubmitEvent } from '@models';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import deepEqual from 'fast-deep-equal/es6';
import { CustomValidators } from '../../helpers/validators';

@Component({
  selector: 'app-dynamic-form',
  templateUrl: './dynamic-form.component.html',
  styleUrls: ['./dynamic-form.component.sass']
})
export class DynamicFormComponent implements OnInit, OnChanges {

  @Input() title?: string;
  @Input() subtitle?: string;
  @Input() formFields: FormInputBase<any>[] = [];

  @Input() columns: number = 1;
  @Input() limitWidth: boolean = true;
  @Input() disableSubmitWithOriginalValue: boolean = false;
  @Input() buttonAlignment: 'left' | 'right' | 'full' = 'right';
  @Input() submitButtonLabel: string = '';

  @Input() showSubmitButton = false;

  @Output() onSubmit = new EventEmitter<FormSubmitEvent>();

  form!: FormGroup;
  columnsGridStyle: string = '';
  formFieldColumns: FormInputBase<any>[][] = [];
  originalFormValue?: object;

  ngOnInit(): void {
    this.columnsGridStyle = `1fr / repeat(${this.columns}, 1fr)`;

    this.form = this.createForm();
    this.createColumns();

    this.originalFormValue = this.form.value;
  }

  ngOnChanges(): void {
    this.form = this.createForm();
    this.createColumns();
  }

  createForm(): FormGroup {
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
        const validators = field.validators;
        const asyncValidators = field.asyncValidators;

        if (field.required) {
          if (field.type === 'dropdown') {
            validators.push((control) => control.value === -1 ? {required: true} : null);
          } else {
            validators.push(Validators.required);
          }
        }

        if (field.type === 'date') {
          validators.push(CustomValidators.date());
        }

        let fieldValue;
        if (field.type === 'number') {
          fieldValue = field.value === 0 ? 0 : field.value;
        } else if (!field.value && field.type === 'checkbox') {
          fieldValue = false;
        } else {
          fieldValue = field.value;
        }
        formControl = new FormControl(
          {value: fieldValue, disabled: field.readonly},
          validators,
          asyncValidators,
        );
      }
      group[field.key] = formControl;
    });
    const form = new FormGroup(group);

    if (this.disableSubmitWithOriginalValue) {
      form.valueChanges.subscribe(() => {
        const equal = deepEqual(this.originalFormValue, this.form.value);
        if (equal) {
          this.form.markAsPristine();
        } else {
          this.form.markAsDirty();
        }
      });
    }

    return form;
  }

  createColumns(): void {
    const _formFields = [...this.formFields];
    const columns = [];
    for (let i = this.columns; i > 0; i--) {
      columns.push(_formFields.splice(0, Math.ceil(_formFields.length / i)));
    }
    this.formFieldColumns = columns;
  }

  internalOnSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    this.onSubmit.emit({
      value: this.form.value,
      markAsPristine: () => {
        this.originalFormValue = this.form.value;
        this.form.markAsPristine();
      }
    });
  }

  revert(): void {
    if (!this.originalFormValue) {
      return;
    }

    this.form.setValue(this.originalFormValue);
    this.form.markAsPristine();
  }

}
