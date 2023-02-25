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
      label: this._localization.translate('UI.ACTION.CREATE_DEPARTMENT'),
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
      header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
      accessorKey: 'Name',
      linkResolver: (item) => `/mdt/departments/${item.Id}`,
      filter: {
        label: this._localization.translate('UI.LABEL.SEARCH_BY.NAME'),
      }
    }),
    new Column<Department>({
      header: this._localization.translate('UI.LABEL.CALLSIGN.TITLE'),
      accessorKey: 'Callsign',
      filter: {
        label: this._localization.translate('UI.LABEL.SEARCH_BY.CALLSIGN'),
      },
    }),
    new Column<Department>({
      header: this._localization.translate('UI.LABEL.CAPACITY.SINGULAR'),
      accessorKey: 'UserCount',
      valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${this._localization.translate('UI.LABEL.NO_LIMIT')}` : `${item.UserCount} / ${item.MaxUsers}`,
    }),
    new Column<Department>({
      header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          show: () => true,
          disabled: () => false,
          onClick: department => this._modal.open(ModalFormComponent, {
            title: 'UI.LABEL.DEPARTMENT.DELETE.TITLE',
            description: this._localization.translate('UI.LABEL.DEPARTMENT.DELETE.DESCRIPTION', { department: `<b>${department.Name}</b>` }),
            buttons: {
              cancel: {
                variant: 'secondary',
                label: 'UI.ACTION.CANCEL',
              },
              submit: {
                variant: 'danger',
                label: 'UI.ACTION.DELETE',
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
      title: 'UI.ACTION.CREATE_DEPARTMENT',
      fields: [
        new TextInput({
          key: 'Name',
          required: true,
          label: 'UI.LABEL.NAME.SINGULAR',
          asyncValidators: [GenericNameValidator.get((value) => this._department.checkIfDepartmentNameTaken(value))],
        }),
        new TextInput({
          key: 'Callsign',
          label: 'UI.LABEL.CALLSIGN.TITLE',
          helpLabel: 'UI.LABEL.CALLSIGN.HELP_LABEL',
          validators: [Validators.pattern('^[a-zA-Z]+$')]
        }),
        new TextInput({
          key: 'CallsignFormat',
          required: true,
          label: 'UI.LABEL.CALLSIGN.FORMAT.TITLE',
          helpLabel: `
            ${this._localization.translate('UI.LABEL.CALLSIGN.FORMAT.HELP_LABEL')} ${this._localization.translate('UI.LABEL.AVAILABLE_CHARACTERS')}: <br/>
            <table>
              <tr>
                <td><b>L</b><td>
                <td>(${this._localization.translate('UI.LABEL.LETTER.SINGULAR')})<td>
              </tr>
              <tr>
                <td><b>N</b><td>
                <td>(${this._localization.translate('UI.LABEL.NUMBER.SINGULAR')})<td>
              </tr>
              <tr>
                <td><b>-</b><td>
                <td>(${this._localization.translate('UI.LABEL.DASH.SINGULAR')})<td>
              </tr>
            </table>
          `,
          validators: [Validators.pattern('^[NL0-9-*]{1,10}$')],
        }),
        new NumberInput({
          key: 'MaxUsers',
          required: true,
          label: 'UI.LABEL.MAX_MEMBERS.TITLE',
          helpLabel: 'UI.LABEL.MAX_MEMBERS.HELP_LABEL',
          validators: [Validators.min(0)],
          options: {
            min: 0,
          }
        }),
      ],
      buttons: {
        cancel: {
          variant: 'secondary',
          label: 'UI.ACTION.CANCEL'
        },
        submit: {
          variant: 'primary',
          label: 'UI.ACTION.CREATE'
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
