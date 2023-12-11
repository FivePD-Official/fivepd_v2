import { Component, OnInit } from '@angular/core';
import {
  Button,
  Collation,
  Column,
  VehicleConfig,
  GeneralConfig,
  LooseDataAttribute,
  NumberField,
  Page,
  SingleSelectField,
  TextInput,
  User,
} from "@models";
import { CoreSetId } from "@enums";
import { filter, first, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { LooseDataService } from "@services/loose-data/loose-data.service";
import { LocalizationService } from "@services/localization/localization.service";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { AttributeGridOptions } from "../_service/attribute-grid-options";
import { CalloutDetailsService } from "@services/callout-details/callout-details.service";
import { Router } from "@angular/router";
import { VehicleConfigService } from "@services/vehicle-config/vehicle-config.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { CustomValidators } from "../../../../helpers/validators";
import { Validators } from "@angular/forms";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { GridComponent } from '../../../../components/grid/grid.component';
import { ContainerComponent } from '../../../../components/container/container.component';
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import { ConfigService } from "@services/config/config.service";
import {
  SingleSelectFieldComponent
} from "../../../../components/form-inputs/single-select-field/single-select-field.component";

@Component({
  selector: 'app-vehicle-settings',
  templateUrl: './vehicle-settings.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, SingleSelectFieldComponent]
})
export class VehicleSettingsComponent implements OnInit {

  generalConfig?: GeneralConfig;

  selectedSet: string = CoreSetId.ArrestReport;
  user?: User;

  attributePage?: Page<LooseDataAttribute>;
  attributesCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  attributesColumns: Column<LooseDataAttribute>[] = [];
  attributesButtons: Button[] = [];

  vehicleConfig?: Page<VehicleConfig>;
  vehicleConfigbuttons: Button[] = [
    {
      label: 'UI.LABEL.VEHICLE_SETTINGS.ADD_VEHICLE',
      show: () => this.userService.userIsDepartmentManagerOrAdmin(),
      disabled: () => false,
    }
  ];
  vehicleConfigCollation: Collation = {
    Page: 1,
    Filters: {},
  };
  policeConfigColumn: Column<VehicleConfig>[] = [
    new Column<VehicleConfig>({
      header: 'UI.LABEL.NAME.SINGULAR',
      accessorKey: "Name",
      filter: {
        label: 'UI.LABEL.SEARCH_BY.NAME',
      }
    }),
    new Column<VehicleConfig>({
      header: 'UI.LABEL.DEPARTMENT.SINGULAR',
      accessorKey: "Department",
      filter: {
        label: 'UI.LABEL.SEARCH_BY.DEPARTMENT',
      }
    }),
    new Column<VehicleConfig>({
      header: 'UI.LABEL.DIVISION.SINGULAR',
      accessorKey: "Division",
    }),
    new Column<VehicleConfig>({
      header: 'Actions',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.EDIT',
          show: () => true,
          disabled: () => false,
          onClick: item => this._router.navigate(['mdt', 'settings', 'vehicle-settings', item.Id])
        },
        {
          type: 'button',
          label: 'UI.ACTION.DELETE',
          show: () => this.userService.userHasRole(UserRole.Admin),
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
                switchMap(() => this._vehicleConfigService.deleteDepartment(department.Id)),
                this._modalService.handleResponse(),
              ).subscribe(() => {
              this.userService.refreshData();
            })
          }
        },
      ]
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
    private readonly _vehicleConfigService: VehicleConfigService,
    private readonly _modalService: ModalService,
    private readonly _modalFormService: ModalFormService,
    private readonly _modalConfirmService: ModalConfirmService,
    private readonly _configService: ConfigService,
  ) {
    _store.pipe(takeUntilDestroyed(), first()).subscribe(state => {
      this.user = state.user.details.User;
    });
  }

  ngOnInit(): void {
    this.getGeneralConfig();
    this.getSettings();
    this.setAttributesGrid();
  }

  private getGeneralConfig(): void {
    this._configService.getGeneralConfig().subscribe(res => {
      this.generalConfig = res;
    });
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
    this.attributesButtons = this._attributeGridOptions.createButtons(
      this.selectedSet,
      () => {
        this.attributesCollation.Page = 1;
        this.getAttributes();
      }
    );
  }

  private getSettings(): void {
    if (!this.user || !this.userService.userIsDepartmentManagerOrAdmin()) {
      return;
    }

    this.getAttributes();
  }

  getAttributes(): void {
    this._looseData.getSetAttributesPage(this.selectedSet, this.attributesCollation).subscribe(res => {
      this.attributePage = res;
    });
  }

  protected readonly UserRole = UserRole;
}
