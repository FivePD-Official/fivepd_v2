import { Component, OnInit } from '@angular/core';
import {
  BaseNuiEvent,
  Button, CalloutDTO, ClientCalloutWrapper,
  Collation,
  Column,
  Department,
  FDutyStatus, FIncidentPriorty,
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
import { EventService } from "@services/event/event.service";
import { NuiEventType } from "@enums";
import { CalloutService } from "@services/callout/callout.service";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.sass']
})
export class DashboardComponent implements OnInit {

  generalSettings?: GeneralSettings;
  user?: PlayerDetailsDTO;
  statistics: any;
  activeCallout?: ClientCalloutWrapper;

  departments?: Page<Department>;
  departmentsButtons: Button[] = [
    {
      label: 'UI.ACTION.CREATE_DEPARTMENT',
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
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'Name',
      linkResolver: (item) => `/mdt/departments/${item.Id}`,
      filter: {
        label: 'UI.LABEL.SEARCH_BY.NAME',
      }
    }),
    new Column<Department>({
      header: 'UI.LABEL.CALLSIGN.TITLE',
      accessorKey: 'Callsign',
      filter: {
        label: 'UI.LABEL.SEARCH_BY.CALLSIGN',
      },
    }),
    new Column<Department>({
      header: 'UI.LABEL.CAPACITY.SINGULAR',
      accessorKey: 'UserCount',
      valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${this._localization.translate('UI.LABEL.NO_LIMIT')}` : `${item.UserCount} / ${item.MaxUsers}`,
    }),
    new Column<Department>({
      header: 'UI.LABEL.ACTION.PLURAL',
      show: () => this._userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.DELETE',
          show: () => true,
          disabled: () => false,
          onClick: department => {
            this._modalConfirmService.open({
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
                switchMap(() => this._departmentService.deleteDepartment(department.Id)),
                this._modalService.handleResponse(),
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
      header: 'UI.LABEL.CALLSIGN.TITLE',
      accessorKey: 'Callsign',
    }),
    new Column<UnitDTO>({
      header: 'UI.LABEL.OFFICER.PLURAL',
      valueResolver: item => item.Players.length.toString(),
    }),
    new Column<UnitDTO>({
      header: 'UI.LABEL.ACTION.PLURAL',
      show: () => this._userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.JOIN',
          show: unit => this.user?.DutyStatus !== FDutyStatus.OffDuty && (this.generalSettings?.joinFreelyToUnit() ?? false) && unit.Players.every(x => x.NetworkId !== this.user?.NetworkId) && unit.Players.length > 0 && unit.Players[0].NetworkId !== this.user?.NetworkId,
          onClick: unit => {
            this._unitService.join(unit).subscribe(res => {
              if (res.Status) {
                this.getUnits();
              }
            });
          }
        },
        {
          type: 'button',
          label: 'UI.ACTION.REQUEST_TO_JOIN',
          show: unit => this.user?.DutyStatus !== FDutyStatus.OffDuty && (this.generalSettings?.joinByRequestToUnit() ?? false) && unit.Players.every(x => x.NetworkId !== this.user?.NetworkId) && unit.Players.length > 0 && unit.Players[0].NetworkId !== this.user?.NetworkId,
          onClick: unit => {
            this._unitService.requestToJoin(unit).subscribe();
          }
        },
        {
          type: 'button',
          label: 'UI.ACTION.LEAVE',
          show: unit => this.user?.DutyStatus !== FDutyStatus.OffDuty && unit.Players.some(x => x.NetworkId === this.user?.NetworkId) && (unit.Players.length > 1),
          onClick: unit => {
            this._unitService.leave(unit).subscribe(res => {
              if (res.Status) {
                this.getUnits();
              }
            });
          }
        },
      ]
    }),
  ];

  callouts?: Page<CalloutDTO>;
  calloutsColumnDef: Column<CalloutDTO>[] = [
    new Column<CalloutDTO>({
      header: 'UI.LABEL.TITLE.SINGULAR',
      accessorKey: 'Title',
    }),
    new Column<CalloutDTO>({
      header: 'UI.LABEL.DESCRIPTION.SINGULAR',
      accessorKey: 'Description',
    }),
    new Column<CalloutDTO>({
      header: 'UI.LABEL.ADDRESS.SINGULAR',
      accessorKey: 'Address',
    }),
    new Column<CalloutDTO>({
      header: 'UI.LABEL.UNIT.PLURAL',
      valueResolver: item => item.Units.join(', '),
    }),
  ];

  constructor(
    private readonly _departmentService: DepartmentService,
    private readonly _localization: LocalizationService,
    private readonly _modalService: ModalService,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _userService: UserService,
    private readonly _store: Store<GlobalState>,
    private readonly _unitService: UnitService,
    private readonly _configService: ConfigService,
    private readonly _eventService: EventService,
    private readonly _calloutService: CalloutService,
  ) {
    _store.pipe(takeUntilDestroyed()).subscribe(state => this.user = state.user.details);
    _eventService.onEvent<BaseNuiEvent>(NuiEventType.Mdt).subscribe(() => {
      this.getDepartments();
      this.getUnits();
      this.getCallouts();
      this.getActiveCallout();
    });
  }

  ngOnInit(): void {
    this.getGeneralSettings();
    this.getDepartments();
    this.getUnits();
    this.getCallouts();
    this.getActiveCallout();
  }

  get priorityLabel(): string {
    if (!this.activeCallout) {
      return '';
    }

    switch (this.activeCallout.Callout.Priority) {
      case FIncidentPriorty.Code1:
        return 'LABEL.PRIORITY.CODE1';
      case FIncidentPriorty.Code2:
        return 'LABEL.PRIORITY.CODE2';
      case FIncidentPriorty.Code3:
        return 'LABEL.PRIORITY.CODE3';
    }

    return '-';
  }

  getGeneralSettings(): void {
    this._configService.getGeneralSettings().subscribe(res => {
      this.generalSettings = res;
    });
  }

  getDepartments(): void {
    this._departmentService.getDepartments(this.departmentsCollation).subscribe(res => {
      this.departments = res;
    });
  }

  getUnits(): void {
    this._unitService.getAll().subscribe(res => {
      this.units = res;
    });
  }

  getCallouts(): void {
    this._calloutService.getAll().subscribe(res => {
      this.callouts = res;
    });
  }

  getActiveCallout(): void {
    this._calloutService.getActive().subscribe(res => {
      this.activeCallout = res;
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
          asyncValidators: [CustomValidators.nameShouldNotBeTaken(value => this._departmentService.checkIfDepartmentNameTaken(value))],
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
        switchMap(res => this._departmentService.createDepartment(res.value)),
        this._modalService.handleResponse(),
      ).subscribe(() => this.getDepartments())
  }
}
