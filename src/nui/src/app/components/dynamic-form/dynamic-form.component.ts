import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormInputBase, FormSubmitEvent } from '@models';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import deepEqual from 'fast-deep-equal/es6';

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
  @Input() buttonAlignment: 'left' | 'right' = 'right';

  @Output() onSubmit = new EventEmitter<FormSubmitEvent>();

  form!: FormGroup;
  columnsGridStyle: string = '';
  formFieldColumns: FormInputBase<any>[][] = [];
  originalFormValue?: object;

  ngOnInit(): void {
    this.columnsGridStyle = `1fr / repeat(${this.columns}, 1fr)`;

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
        if (field.required) {
          if (field.type === 'dropdown') {
            validators.push((control) => control.value === -1 ? { 'required': true } : null);
          } else {
            validators.push(Validators.required);
          }
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
          fieldValue,
          validators,
          field.asyncValidators
        );
      }
      group[field.key] = formControl;
    });
    this.form = new FormGroup(group);
    this.createColumns();

    this.originalFormValue = this.form.value;

    this.form.valueChanges.subscribe(() => {
      if (deepEqual(this.originalFormValue, this.form.value)) {
        this.form.markAsPristine();
      } else {
        this.form.markAsDirty();
      }
    });
  }

  ngOnChanges(): void {
    this.createColumns();
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

}
