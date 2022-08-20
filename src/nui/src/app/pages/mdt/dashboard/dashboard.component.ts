import { Component, OnInit } from '@angular/core';
import { Checkbox, Dropdown, FormInputBase, CheckboxGroup, NumberInput, TextInput } from '../../../models/form-input-base';
import { Validators } from '@angular/forms';
import { GenericNameValidator } from '@validators/generic-name-validator';
import { DepartmentService } from '@services/entity/department.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.sass']
})
export class DashboardComponent implements OnInit {

  formFields: FormInputBase<any>[] = [
    new TextInput({
      key: 'test',
      placeholder: 'test Placeholder',
      label: 'Test label',
      required: true,
      asyncValidators: [GenericNameValidator.create((value) => this._department.checkIfDepartmentNameTaken(value))],
    }),
    new NumberInput({
      key: 'test2',
      placeholder: 'test Placeholder 2',
      label: 'Test label 2',
      required: true,
      validators: [Validators.min(0), Validators.max(10)],
      options: {
        min: 0,
        max: 10,
      }
    }),
    new Checkbox({
      key: 'checkbox',
      label: 'Checkbox test',
    }),
    new Dropdown({
      key: 'dropdown-key',
      label: 'Dropdown label',
      required: false,
      helpLabel: 'this is a help label',
      dropdownOptions: {
        label: 'Default label',
        size: 'normal',
        items: [
          { key: 0, label: 'Value 1' },
          { key: 1, label: 'Value 2' },
        ]
      }
    }),
    new CheckboxGroup({
      key: 'groupKey',
      label: 'Permissions',
      checkboxGroup: [
        new Checkbox({
          key: 'permission1',
          label: 'Permission 1',
        }),
        new Checkbox({
          key: 'permission2',
          label: 'Permission 2',
        }),
      ]
    })
  ]

  constructor(
    private _department: DepartmentService,
  ) { }

  ngOnInit(): void {
  }

  onSubmit(formValue: object) {
    console.log(formValue)
  }
}
