import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridComponent } from "../../../../components/grid/grid.component";
import { Button, Collation, Column, Department, GeneralConfig } from "@models";
import { BehaviorSubject, filter, map, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { LocalizationService } from "@services/localization/localization.service";
import { ConfigService } from "@services/config/config.service";
import { Router } from "@angular/router";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { DepartmentService } from "@services/department/department.service";
import { ModalService } from "@services/modals/modal/modal.service";
import {
  CreateDepartmentModalComponent
} from "../../departments/departments-view/_components/create-department-modal/create-department-modal.component";

@Component({
    selector: 'app-departments',
    imports: [CommonModule, GridComponent],
    templateUrl: './departments.component.html'
})
export class DepartmentsComponent implements OnInit {

  private readonly _router = inject(Router);
  private readonly _userService = inject(UserService);
  private readonly _localizationService = inject(LocalizationService);
  private readonly _configService = inject(ConfigService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _departmentService = inject(DepartmentService);

  generalConfig?: GeneralConfig;

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly departments$ = this.collation$.pipe(
    map(collation => ({
      collation,
      departmentManager: this._userService.userIsDepartmentManager(),
      admin: this._userService.userHasRole(UserRole.Admin),
    })),
    switchMap(({collation, departmentManager, admin}) => this._departmentService.getAllPageable({
      ...collation,
      Filters: {
        UserDepartment: departmentManager && admin ? false : departmentManager,
        ...collation.Filters,
      }
    })),
  );

  readonly buttons: Button[] = [
    {
      label: 'UI.LABEL.DEPARTMENT.AREA_MANAGEMENT.ACTION.MANAGE_AREAS.TITLE',
      show: () => true,
      disabled: () => !this.generalConfig?.config?.Callout?.AreaBasedDispatch,
      tooltip: {
        enabled: () => !this.generalConfig?.config?.Callout?.AreaBasedDispatch,
        text: this._localizationService.translate('UI.LABEL.DEPARTMENT.AREA_MANAGEMENT.ACTION.MANAGE_AREAS.TOOLTIP', {
          configKey: 'AreaBasedDispatch',
        }),
        placement: 'left',
      },
      onClick: () => this._router.navigate(['mdt', 'settings', 'departments', 'area-management']),
    },
    {
      label: 'UI.ACTION.CREATE_DEPARTMENT',
      show: () => this._userService.userHasRole(UserRole.Admin),
      disabled: () => false,
      onClick: () => {
        this._modalService.open(CreateDepartmentModalComponent, {
          title: 'UI.ACTION.CREATE_DEPARTMENT'
        }).result
          .pipe(
            filter(res => res.success),
            switchMap(res => this._departmentService.createDepartment(res.value)),
            this._modalService.handleResponse(),
          ).subscribe(() => this.collation$.next(this.collation$.value));
      }
    }
  ];

  readonly columns: Column<Department>[] = [
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
      valueResolver: (item) => item.MaxUsers === 0 ? `${item.UserCount} / ${this._localizationService.translate('UI.LABEL.NO_LIMIT')}` : `${item.UserCount} / ${item.MaxUsers}`,
    }),
    new Column<Department>({
      header: 'UI.LABEL.ACTION.PLURAL',
      show: () => this._userService.userIsDepartmentManagerOrAdmin(),
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
          show: () => this._userService.userHasRole(UserRole.Admin),
          disabled: () => false,
          onClick: department => {
            this._modalConfirmService.open({
              title: 'UI.LABEL.DEPARTMENT.DELETE.TITLE',
              description: this._localizationService.translate('UI.LABEL.DEPARTMENT.DELETE.DESCRIPTION', {department: `<b>${department.Name}</b>`}),
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
              this.collation$.next(this.collation$.value);
              this._userService.refreshData();
            })
          }
        },
      ]
    })
  ];

  ngOnInit(): void {
    this._configService.getGeneralConfig().subscribe(res => {
      this.generalConfig = res;
    });
  }
}
