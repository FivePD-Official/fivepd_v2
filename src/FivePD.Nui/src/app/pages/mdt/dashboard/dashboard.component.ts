import { Component, OnInit } from '@angular/core';
import {
  Button,
  Collation,
  Column,
  Department,
  GeneralSettings,
  NumberInput,
  Page,
  PlayerDetailsDTO,
  TextInput,
  UnitDTO
} from '@models';
import { DepartmentService } from '@services/entity/department.service';
import { LocalizationService } from '@services/localization/localization.service';
import { Validators } from '@angular/forms';
import { filter, switchMap } from 'rxjs';
import { createDepartmentCallsignFormatInputField } from '../../../helpers/forms/department';
import { UserRole, UserService } from '@services/user/user.service';
import { CustomValidators } from '../../../helpers/validators';
import { Store } from '@ngrx/store';
import { ModalConfirmService } from '@services/modals/modal-confirm/modal-confirm.service';
import { GlobalState } from '@store';
import { ModalFormService } from '@services/modals/modal-form/modal-form.service';
import { ModalService } from '@services/modals/modal/modal.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UnitService } from "@services/unit/unit.service";
import { ConfigService } from "@services/config/config.service";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.sass']
})
export class DashboardComponent implements OnInit {

  generalSettings?: GeneralSettings;
  user?: PlayerDetailsDTO;
  statistics: any;
  assignedCall: any;

  departments?: Page<Department>;
  departmentsButtons: Button[] = [
    {
      label: this._localization.translate('UI.ACTION.CREATE_DEPARTMENT'),
      show: () => this._userService.userHasRole(UserRole.Admin),
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
      show: () => this._userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.DELETE'),
          show: () => true,
          disabled: () => false,
          onClick: department => {
            this.modalConfirmService.open({
              title: 'UI.LABEL.DEPARTMENT.DELETE.TITLE',
              description: this._localization.translate('UI.LABEL.DEPARTMENT.DELETE.DESCRIPTION', {department: `<b>${department.Name}</b>`}),
              buttons: {
                submit: {
                  variant: 'danger',
                  label: 'UI.ACTION.DELETE',
                },
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

  units?: Page<UnitDTO>;
  unitsColumnDef: Column<UnitDTO>[] = [
    new Column<UnitDTO>({
      header: this._localization.translate('UI.LABEL.CALLSIGN.TITLE'),
      accessorKey: 'Callsign',
    }),
    new Column<UnitDTO>({
      header: this._localization.translate('UI.LABEL.ACTION.PLURAL'),
      show: () => this._userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.JOIN'),
          show: unit => (this.generalSettings?.joinFreelyToUnit() ?? false) && unit.Players.every(x => x.NetworkId !== this.user?.NetworkId),
          onClick: () => {
            // send request to join unit
          }
        },
        // "request to join" button
        {
          type: 'button',
          label: this._localization.translate('UI.ACTION.LEAVE'),
          show: unit => (this.generalSettings?.joinFreelyToUnit() ?? false) && unit.Players.some(x => x.NetworkId === this.user?.NetworkId),
          onClick: () => {
            // send request to leave unit
          }
        },
      ]
    }),
  ];

  constructor(
    private readonly _department: DepartmentService,
    private readonly _localization: LocalizationService,
    private readonly _modal: ModalService,
    private readonly _modalFormService: ModalFormService,
    private readonly modalConfirmService: ModalConfirmService,
    private readonly _userService: UserService,
    private readonly store: Store<GlobalState>,
    private readonly _unitService: UnitService,
    private readonly _configService: ConfigService,
  ) {
    store.pipe(takeUntilDestroyed()).subscribe(state => this.user = state.user.details);
  }

  ngOnInit(): void {
    this.getGeneralSettings();
    this.getDepartments();
    this.getUnits();
  }

  getGeneralSettings(): void {
    this._configService.getGeneralSettings().subscribe(res => {
      this.generalSettings = res;
    });
  }

  getDepartments(): void {
    this._department.getDepartments(this.departmentsCollation).subscribe(res => {
      this.departments = res;
    });
  }

  getUnits(): void {
    this._unitService.getAll().subscribe(res => {
      this.units = res;
    });
  }

  onDepartmentsCollationChange(collation: Collation): void {
    this.departmentsCollation = collation;
    this.getDepartments();
  }

  tryCreateDepartment(): void {
    this._modalFormService.open({
      title: 'UI.ACTION.CREATE_DEPARTMENT',
      fields: () => [
        new TextInput({
          key: 'Name',
          required: true,
          label: 'UI.LABEL.NAME.SINGULAR',
          asyncValidators: [CustomValidators.nameShouldNotBeTaken(value => this._department.checkIfDepartmentNameTaken(value))],
        }),
        new TextInput({
          key: 'Callsign',
          label: 'UI.LABEL.CALLSIGN.TITLE',
          helpLabel: 'UI.LABEL.CALLSIGN.HELP_LABEL',
          validators: [CustomValidators.callsign()]
        }),
        createDepartmentCallsignFormatInputField(this._localization),
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
        submit: {variant: 'primary', label: 'UI.ACTION.CREATE'}
      }
    }).result
      .pipe(
        filter(res => res.success),
        switchMap(res => this._department.createDepartment(res.value)),
        this._modal.handleResponse(),
      ).subscribe(() => this.getDepartments())
  }
}
