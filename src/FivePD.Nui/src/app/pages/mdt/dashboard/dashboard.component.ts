import { Component, OnInit } from '@angular/core';
import { Button, Collation, Column, Department, NumberInput, Page, TextInput, User } from '@models';
import { GenericNameValidator } from '@validators/generic-name-validator';
import { DepartmentService } from '@services/entity/department.service';
import { LocalizationService } from '@services/localization/localization.service';
import { Validators } from '@angular/forms';
import { ModalService } from '@services/modal/modal.service';
import { Select } from '@ngxs/store';
import { UserState } from '@store/user.state';
import { Observable } from 'rxjs';
import { UserService } from '@services/user/user.service';
import { ModalFormComponent } from '../../../components/modal/modal-form/modal-form.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.sass']
})
export class DashboardComponent implements OnInit {

  user?: User;
  @Select(UserState.details) user$!: Observable<User>;
  statistics: any;
  assignedCall: any;

  departments?: Page<Department>;
  departmentsButtons: Button[] = [
    {
      label: this._localization.translate('Nui.Actions.CreateDepartment'),
      show: () => this.user?.Admin ?? false,
      disabled: () => false,
      onClick: () => this.tryCreateDepartment(),
    }
  ];
  departmentsCollation: Collation = {
    Page: 1,
  };
  departmentsColumnDef: Column<Department>[] = [
    new Column<Department>({
      header: this._localization.translate('Nui.Labels.Name'),
      accessorKey: 'Name',
      linkResolver: (item) => `/mdt/departments/${item.Id}`,
      filter: {
        label: this._localization.translate('Nui.Labels.SearchByName'),
      }
    }),
    new Column<Department>({
      header: this._localization.translate('Nui.Labels.Callsign'),
      accessorKey: 'Callsign',
      filter: {
        label: this._localization.translate('Nui.Labels.SearchByCallsign'),
      },
    }),
    new Column<Department>({
      header: this._localization.translate('Nui.Labels.Capacity'),
      accessorKey: 'UserCount',
      valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${this._localization.translate('Nui.Labels.NoLimit')}` : `${item.UserCount} / ${item.MaxUsers}`,
    }),
    new Column<Department>({
      header: this._localization.translate('Nui.Labels.Actions'),
      actions: [
        {
          type: 'button',
          label: this._localization.translate('Nui.Actions.Delete'),
          show: () => true,
          disabled: () => false,
          onClick: department => this._modal.open(ModalFormComponent, {
            title: 'Nui.Labels.DeleteDepartmentTitle',
            description: this._localization.translate('Nui.Labels.DeleteDepartmentDescription', { department: `<b>${department.Name}</b>` }),
            buttons: {
              cancel: {
                theme: 'secondary',
                label: 'Nui.Actions.Cancel',
              },
              submit: {
                theme: 'danger',
                label: 'Nui.Actions.Delete',
              },
            },
          }).onSuccess(() => {
            this._department.deleteDepartment(department.Id).subscribe(res => {
              if (res.Status) {
                this.getDepartments();
                this._userService.refreshData();
              }
            });
          })
        },
      ]
    })
  ];

  constructor(
    private _department: DepartmentService,
    private _localization: LocalizationService,
    private _modal: ModalService,
    private _userService: UserService,
  ) { }

  ngOnInit(): void {
    this.user$.subscribe(user => this.user = user);
    this.getDepartments();
  }

  getDepartments(): void {
    this._department.getDepartments(this.departmentsCollation).subscribe(res => {
      this.departments = res;
    });
  }

  onDepartmentsCollationChange(collation: Collation): void {
    this.departmentsCollation = collation;
    this.getDepartments();
  }

  tryCreateDepartment(): void {
    this._modal.open(ModalFormComponent, {
      title: 'Nui.Actions.CreateDepartment',
      fields: [
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
      ],
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
    }).onSuccess(modalRes => {
      this._department.createDepartment(modalRes.value!).subscribe(res => {
        if (res.Status) {
          this.getDepartments();
        }
      });
    })
  }
}
