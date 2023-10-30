import { Component } from '@angular/core';
import {
  Button,
  CalloutDetails,
  Collation,
  Column,
  Department,
  Dropdown,
  NumberInput,
  Page,
  LooseDataAttribute,
  TextInput,
  User
} from "@models";
import { CoreSetId } from "@enums";
import { filter, first, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { LocalizationService } from "@services/localization/localization.service";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { AttributeGridOptions } from "../../_service/attribute-grid-options";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { Router } from "@angular/router";
import { DepartmentService } from "@services/department/department.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { CustomValidators } from "../../../../../helpers/validators";
import { createDepartmentCallsignFormatInputField } from "../../../../../helpers/forms/department";
import { Validators } from "@angular/forms";
import { TranslatePipe } from "../../../../../pipes/translate.pipe";
import { GridComponent } from '../../../../../components/grid/grid.component';
import { ContainerComponent } from '../../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../../components/page-header/page-header.component';
import { DynamicFormInputComponent } from "../../../../../components/dynamic-form-input/dynamic-form-input.component";
import { NgIf } from "@angular/common";

@Component({
  selector: 'app-admin-settings',
  templateUrl: './admin-settings.component.html',
  styleUrls: ['./admin-settings.component.sass'],
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, DynamicFormInputComponent, NgIf]
})
export class AdminSettingsComponent {

  readonly setAttributeDropdown = new Dropdown({
    key: '#',
    value: CoreSetId.ArrestReport as any,
    dropdownOptions: {
      label: '',
      addDefaultItem: false,
      items: [
        {key: CoreSetId.ArrestReport, label: 'Arrest reports'},
        {key: CoreSetId.IncidentReport, label: 'Incident reports'},
        {key: CoreSetId.PedTicket, label: 'Ped tickets'},
        {key: CoreSetId.PedWarning, label: 'Ped warnings'},
      ],
      size: 'normal',
      height: 'base',
      variant: 'secondary',
    },
    required: false,
  });
  selectedSet: string = CoreSetId.ArrestReport;

  user?: User;

  attributePage?: Page<LooseDataAttribute>;
  attributesCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  attributesColumns: Column<LooseDataAttribute>[] = [];
  buttons: Button[] = [];

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
              {key: UserRole.Dispatcher, label: this._localization.translate('UI.LABEL.ROLE.DISPATCHER')},
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
    this.setAttributesGrid();
  }

  setAttributesGrid(): void {
    this.attributesCollation.Page = 1;
    this.getAttributes();

    this.attributesColumns = this._attributeGridOptions.createColumnList(
      this.selectedSet,
      () => this.getAttributes(),
      () => {
        this.attributesCollation.Page = 1;
        this.getAttributes();
      }
    );
    this.buttons = this._attributeGridOptions.createButtons(
      this.selectedSet,
      () => {
        this.attributesCollation.Page = 1;
        this.getAttributes();
      }
    );
  }

  onTableCellActionSelect(selectedItemKey: string): void {
    this.selectedSet = selectedItemKey;
    this.setAttributesGrid();
  }

  private getSettings(): void {
    if (!this.user || !this.userService.userHasRole(UserRole.Admin)) {
      return;
    }

    this.getAttributes();
    this.getUsers();
    this.getCalloutDetails();
    this.getDepartments();
  }

  getAttributes(): void {
    this._looseData.getSetAttributesPage(this.selectedSet, this.attributesCollation).subscribe(res => {
      this.attributePage = res;
    });
  }

  onAttributesCollationChange(collation: Collation): void {
    this.attributesCollation = collation;
    this.getAttributes();
  }

  getUsers(): void {
    this.userService.getAllPageable(this.attributesCollation).subscribe(res => {
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
