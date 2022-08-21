import { Component, OnInit } from '@angular/core';
import { FormInputBase, NumberInput, TextInput } from '@models';
import { GenericNameValidator } from '@validators/generic-name-validator';
import { DepartmentService } from '@services/entity/department.service';
import { LocalizationService } from '@services/localization/localization.service';
import { Validators } from '@angular/forms';
import { ModalService } from '@services/modal/modal.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.sass']
})
export class DashboardComponent implements OnInit {

  formFields: FormInputBase<any>[] = [
    new TextInput({
      key: 'Name',
      required: true,
      label: 'Nui.Labels.Name',
      asyncValidators: [GenericNameValidator.get((value) => this._department.checkIfDepartmentNameTaken(value))],
    }),
    new TextInput({
      key: 'Callsign',
      label: 'Nui.Labels.Callsign',
      helpLabel: 'Nui.Labels.CallsignHelpLabel',
      validators: [Validators.pattern('^[a-zA-Z]+$')]
    }),
    new TextInput({
      key: 'CallsignFormat',
      required: true,
      label: 'Nui.Labels.CallsignFormat',
      helpLabel: `
        ${this._localization.translate('Nui.Labels.CallsignFormatHelpLabel')} ${this._localization.translate('Nui.Labels.AvailableCharacters')}: <br/>
        <table>
          <tr>
            <td><b>L</b><td>
            <td>(${this._localization.translate('Nui.Labels.Letter')})<td>
          </tr>
          <tr>
            <td><b>N</b><td>
            <td>(${this._localization.translate('Nui.Labels.Number')})<td>
          </tr>
          <tr>
            <td><b>-</b><td>
            <td>(${this._localization.translate('Nui.Labels.Dash')})<td>
          </tr>
        </table>
      `,
      validators: [Validators.pattern('^[NL0-9-*]{1,10}$')],
    }),
    new NumberInput({
      key: 'MaxUsers',
      required: true,
      label: 'Nui.Labels.MaxNumberOfMembers',
      helpLabel: 'Nui.Labels.MaxNumberOfMembersHelpLabel',
      validators: [Validators.min(0)],
      options: {
        min: 0,
      }
    }),
  ];

  constructor(
    private _department: DepartmentService,
    private _localization: LocalizationService,
    private _modal: ModalService,
  ) { }

  ngOnInit(): void {
  }

  onCreateDepartment(): void {
    this._modal.open({
      title: 'Nui.Actions.CreateDepartment',
      fields: this.formFields,
      buttons: {
        cancel: {
          theme: 'secondary',
          label: 'Nui.Actions.Cancel'
        },
        submit: {
          theme: 'primary',
          label: 'Nui.Actions.Create'
        }
      }
    }).subscribe(modalRes => {
      if (modalRes.success) {
        this._department.createDepartment(modalRes.value!).subscribe(res => {
          console.log('create res', res)
        });
      }
    })
  }
}
