import { Component, OnInit } from '@angular/core';
import {
  Button,
  CalloutDetails,
  Collation,
  Column,
  Department,
  NumberInput,
  Page,
  SetAttribute,
  TextInput,
  User
} from '@models';
import { LooseDataService } from '@services/loose-data/loose-data.service';
import { filter, first, switchMap } from 'rxjs';
import { LocalizationService } from '@services/localization/localization.service';
import { DefaultSetId } from '@enums';
import { Store } from '@ngrx/store';
import { GlobalState } from '@store';
import { UserRole, UserService } from '@services/user/user.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AttributeGridOptions } from "./_service/attribute-grid-options";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { Router } from "@angular/router";
import { CustomValidators } from "../../../helpers/validators";
import { createDepartmentCallsignFormatInputField } from "../../../helpers/forms/department";
import { Validators } from "@angular/forms";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { DepartmentService } from "@services/department/department.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.sass']
})
export class SettingsComponent implements OnInit {

  protected readonly UserRole = UserRole;

  user?: User;

  arrestReportAttributes?: Page<SetAttribute>;
  arrestReportAttributesCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  arrestReportAttributesColumns: Column<SetAttribute>[] = this._attributeGridOptions.createColumnList(
    DefaultSetId.ArrestReport,
    () => this.getArrestReportAttributes(),
    () => {
      this.arrestReportAttributesCollation.Page = 1;
      this.getArrestReportAttributes();
    }
  );
  arrestReportAttributesButtons: Button[] = this._attributeGridOptions.createButtons(
    DefaultSetId.ArrestReport,
    () => {
      this.arrestReportAttributesCollation.Page = 1;
      this.getArrestReportAttributes();
    }
  );

  incidentReportAttributes?: Page<SetAttribute>;
  incidentReportAttributesCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  incidentReportAttributesColumns: Column<SetAttribute>[] = this._attributeGridOptions.createColumnList(
    DefaultSetId.IncidentReport,
    () => this.getIncidentReportAttributes(),
    () => {
      this.incidentReportAttributesCollation.Page = 1;
      this.getIncidentReportAttributes();
    }
  );
  incidentReportAttributesButtons: Button[] = this._attributeGridOptions.createButtons(
    DefaultSetId.IncidentReport,
    () => {
      this.incidentReportAttributesCollation.Page = 1;
      this.getIncidentReportAttributes();
    }
  );

  users?: Page<User>;
  usersCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  usersColumns: Column<User>[] = [
    new Column<User>({
      header: this._localization.translate('UI.LABEL.NAME.SINGULAR'),
      show: () => true,
      accessorKey: 'Name',
    }),
    new Column<User>({
      header: this._localization.translate('UI.LABEL.ROLE.SINGULAR'),
      show: () => this.userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'select',
          label: this._localization.translate('UI.LABEL.ROLE.NO'),
          selectedItemsResolver: (user, dropdownItemKey) => this.userService.userHasRole(dropdownItemKey),
          items: () => {
            return [
              {key: UserRole.Admin, label: this._localization.translate('UI.LABEL.ROLE.ADMIN')},
              {key: UserRole.Developer, label: this._localization.translate('UI.LABEL.ROLE.DEVELOPER')},
            ];
          },
          resetWhenSelectedId: UserRole.None,
          onSelectMulti: (user, selectedIds) => {
            this.userService.changeRole(user, selectedIds).subscribe();
          }
        }
      ]
    }),
  ];

  departments?: Page<Department>;
  departmentsButtons: Button[] = [
    {
      label: 'UI.ACTION.CREATE_DEPARTMENT',
      show: () => this.userService.userHasRole(UserRole.Admin),
      disabled: () => false,
      onClick: () => this.tryCreateDepartment(),
    }
  ];
  departmentsCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  departmentsColumnDef: Column<Department>[] = [
    new Column<Department>({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'Name',
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
      show: () => this.userService.userHasRole(UserRole.Admin),
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.EDIT',
          show: () => true,
          disabled: () => false,
          onClick: item => this._router.navigate(['mdt', 'settings', 'departments', item.Id]),
        },
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
              this.userService.refreshData();
            })
          }
        },
      ]
    })
  ];

  calloutDetails?: Page<CalloutDetails>;
  calloutDetailsCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  calloutDetailsColumns: Column<CalloutDetails>[] = [
    new Column({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: 'TypeName',
    }),
    new Column({
      header: 'UI.LABEL.ASSEMBLY.NAME.SINGULAR',
      accessorKey: 'AssemblyName',
    }),
    new Column({
      header: 'Active',
      valueResolver: item => item.Active ? 'Yes' : 'No',
      badgeResolver: item => item.Active ? 'green' : 'red'
    }),
    new Column({
      header: 'Actions',
      actions: [
        {
          type: 'button',
          label: 'Edit',
          show: () => this.userService.userHasRole(UserRole.Admin),
          onClick: item => this._router.navigate(['mdt', 'settings', 'callout-details', item.Id]),
        },
      ],
    })
  ];

  constructor(
    public readonly userService: UserService,
    private readonly _looseData: LooseDataService,
    private readonly _localization: LocalizationService,
    private readonly _store: Store<GlobalState>,
    private readonly _attributeGridOptions: AttributeGridOptions,
    private readonly _calloutDetailService: CalloutDetailsService,
    private readonly _router: Router,
    private readonly _departmentService: DepartmentService,
    private readonly _modalService: ModalService,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
  ) {
    _store.pipe(takeUntilDestroyed(), first()).subscribe(state => {
      this.user = state.user.details.User;
    });
  }

  ngOnInit(): void {
    this.getSettings();
  }

  private getSettings(): void {
    if (!this.user || !this.userService.userHasRole(UserRole.Admin)) {
      return;
    }

    this.getArrestReportAttributes();
    this.getIncidentReportAttributes();
    this.getUsers();
    this.getCalloutDetails();
    this.getDepartments();
  }

  getArrestReportAttributes(): void {
    this._looseData.getSetAttributesPage(DefaultSetId.ArrestReport, this.arrestReportAttributesCollation).subscribe(res => {
      this.arrestReportAttributes = res;
    });
  }

  onArrestReportAttributesCollationChange(collation: Collation): void {
    this.arrestReportAttributesCollation = collation;
    this.getArrestReportAttributes();
  }

  getIncidentReportAttributes(): void {
    this._looseData.getSetAttributesPage(DefaultSetId.IncidentReport, this.incidentReportAttributesCollation).subscribe(res => {
      this.incidentReportAttributes = res;
    });
  }

  onIncidentReportAttributesCollationChange(collation: Collation): void {
    this.incidentReportAttributesCollation = collation;
    this.getIncidentReportAttributes();
  }

  getUsers(): void {
    this.userService.getAllPageable(this.arrestReportAttributesCollation).subscribe(res => {
      this.users = res;
    });
  }

  onUsersCollationChange(collation: Collation): void {
    this.usersCollation = collation;
    this.getUsers();
  }

  getCalloutDetails(): void {
    this._calloutDetailService.getAll(this.calloutDetailsCollation).subscribe(res => {
      this.calloutDetails = res;
    });
  }

  onCalloutDetailsCollationChange(collation: Collation): void {
    this.calloutDetailsCollation = collation;
    this.getCalloutDetails();
  }

  getDepartments(): void {
    this._departmentService.getAllPageable(this.departmentsCollation).subscribe(res => {
      this.departments = res;
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
          validators: [CustomValidators.onlyLetters()]
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
