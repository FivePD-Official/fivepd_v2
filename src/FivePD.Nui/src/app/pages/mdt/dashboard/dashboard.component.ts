import { Component, OnInit } from '@angular/core';
import { Button, Collation, Column, Department, NumberInput, Page, TextInput, User } from '@models';
import { DepartmentService } from '@services/entity/department.service';
import { LocalizationService } from '@services/localization/localization.service';
import { Validators } from '@angular/forms';
import { ModalService } from '@services/modals/modal/modal.service';
import { Select } from '@ngxs/store';
import { UserState } from '@store/user.state';
import { filter, Observable, switchMap } from 'rxjs';
import { UserService } from '@services/user/user.service';
import { createDepartmentCallsignFormatInputField } from '../../../helpers/forms/department';
import { CustomValidators } from '../../../helpers/validators';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';

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
          onClick: department => {
            this.modalConfirmService.open({
              title: 'Nui.Labels.DeleteDepartmentTitle',
              description: this._localization.translate('Nui.Labels.DeleteDepartmentDescription', {department: `<b>${department.Name}</b>`}),
              buttons: {
                submit: {variant: 'danger', label: 'Nui.Actions.Delete'},
              },
            }).result
              .pipe(
                filter(res => res.success),
                switchMap(() => this._department.deleteDepartment(department.Id)),
                this._modal.handleResponse(),
              ).subscribe(() => {
              this.getDepartments();
              this._userService.refreshData();
            })
          }
        },
      ]
    })
  ];

  constructor(
    private _department: DepartmentService,
    private _localization: LocalizationService,
    private _modal: ModalService,
    private readonly modalFormService: ModalFormService,
    private readonly modalConfirmService: ModalConfirmService,
    private _userService: UserService,
  ) {
  }

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
    this.modalFormService.open({
      title: 'Nui.Actions.CreateDepartment',
      fields: () => [
        new TextInput({
          key: 'Name',
          required: true,
          label: 'Nui.Labels.Name',
          // asyncValidators: [CustomValidators.nameShouldNotBeTaken((value) => this._department.checkIfDepartmentNameTaken(value))],
        }),
        new TextInput({
          key: 'Callsign',
          label: 'Nui.Labels.Callsign',
          helpLabel: 'Nui.Labels.CallsignHelpLabel',
          validators: [CustomValidators.callsign()]
        }),
        createDepartmentCallsignFormatInputField(this._localization),
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
        submit: {variant: 'primary', label: 'Nui.Actions.Create'}
      }
    }).result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._department.createDepartment(res.value)),
        this._modal.handleResponse(),
      ).subscribe(() => this.getDepartments())
  }
}
