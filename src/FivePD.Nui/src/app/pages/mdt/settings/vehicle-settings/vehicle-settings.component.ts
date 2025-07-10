import { Component, inject } from '@angular/core';
import {
  Button,
  CheckboxField,
  Collation,
  Column, PositionField,
  FormInputBase,
  ModalResponse,
  MultiSelectField,
  NumberField,
  TextField, Vector3,
  VehicleConfig,
  VehicleConfigType
} from "@models";
import { BehaviorSubject, filter, map, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { VehicleConfigService } from "@services/vehicle-config/vehicle-config.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { GridComponent } from '../../../../components/grid/grid.component';
import { ContainerComponent } from '../../../../components/container/container.component';
import {
  SingleSelectFieldComponent
} from "../../../../components/form-inputs/single-select-field/single-select-field.component";
import { AsyncPipe } from "@angular/common";
import { Validators } from "@angular/forms";
import { ValueResolver } from "@services/modals/modal-form/modal-form-value-resolver";
import { LocalizationService } from "@services/localization/localization.service";
import { collationWithAdditionalFilters } from "../../../../helpers/request";
import { CustomValidators } from "../../../../helpers/validators";

function createBaseFields(vehicleConfig?: VehicleConfig): FormInputBase<any>[] {
  return [
    new TextField({
      key: 'Name',
      label: 'UI.LABEL.NAME.SINGULAR',
      helpLabel: 'UI.LABEL.VEHICLE_CONFIG.NAME.HELP_LABEL',
      required: true,
      value: vehicleConfig?.Name,
    }),
    new TextField({
      key: 'Model',
      label: 'UI.LABEL.MODEL.SINGULAR',
      helpLabel: 'UI.LABEL.VEHICLE_CONFIG.MODEL.HELP_LABEL',
      required: true,
      value: vehicleConfig?.Model,
    }),
    new NumberField({
      key: 'Livery',
      label: 'UI.LABEL.LIVERY.SINGULAR',
      helpLabel: 'UI.LABEL.VEHICLE_CONFIG.LIVERY.HELP_LABEL',
      required: false,
      value: vehicleConfig?.Livery,
      validators: [Validators.min(0)],
      options: {
        min: 0,
      }
    }),
    new MultiSelectField({
      key: 'Extras',
      label: 'UI.LABEL.EXTRA.PLURAL',
      defaultItemLabel: '',
      addDefaultItem: false,
      value: vehicleConfig?.Extras,
      items: () => [
        ...Array(12).fill(null).map((_, i) => {
          const index = i + 1;
          return {key: index, label: index.toString()};
        }),
      ],
      size: "normal",
      height: "base",
      variant: "secondary",
      required: false
    })
  ];
}

function createTowTruckFields(localizationService: LocalizationService, vehicleConfig?: VehicleConfig): FormInputBase<any>[] {
  const towedVehiclePositionOffset = vehicleConfig?.Options['TowedVehiclePositionOffset'] as Vector3;

  return [
    ...createBaseFields(vehicleConfig),
    new CheckboxField({
      key: 'SirenActive',
      label: localizationService.translate('UI.LABEL.VEHICLE_CONFIG.TYPE.TOW.OPTIONS.SIREN_ACTIVE.LABEL'),
      helpLabel: localizationService.translate('UI.LABEL.VEHICLE_CONFIG.TYPE.TOW.OPTIONS.SIREN_ACTIVE.HELP_LABEL'),
      required: true,
      value: vehicleConfig?.Options['SirenActive'],
    }),
    new PositionField({
      key: 'TowedVehiclePositionOffset',
      label: localizationService.translate('UI.LABEL.VEHICLE_CONFIG.TYPE.TOW.OPTIONS.TOWED_VEHICLE_POSITION_OFFSET.LABEL'),
      required: true,
      validators: [CustomValidators.positionComponentsRequired()],
      value: towedVehiclePositionOffset ? towedVehiclePositionOffset : undefined,
    }),
  ];
}

function mapTowTruckFields(value: ModalResponse['value']): ModalResponse['value'] {
  const sirenActive = value['SirenActive'];

  // delete fields which should be in the Options
  delete value['SirenActive'];

  return {
    ...value,
    Options: {
      SirenActive: sirenActive,
      TowedVehiclePositionOffset: value['TowedVehiclePositionOffset']
    }
  };
}

@Component({
    selector: 'app-vehicle-settings',
    templateUrl: './vehicle-settings.component.html',
    imports: [ContainerComponent, GridComponent, TranslatePipe, AsyncPipe]
})
export class VehicleSettingsComponent {

  protected readonly UserRole = UserRole;

  protected readonly _userService = inject(UserService);
  private readonly _vehicleConfigService = inject(VehicleConfigService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _localizationService = inject(LocalizationService);

  readonly nameColumn = new Column<VehicleConfig>({
    header: 'UI.LABEL.NAME.SINGULAR',
    accessorKey: "Name",
    filter: {
      label: 'UI.LABEL.SEARCH_BY.NAME',
    }
  });
  readonly modelColumn = new Column<VehicleConfig>({
    header: 'UI.LABEL.MODEL.SINGULAR',
    accessorKey: "Model",
    filter: {
      label: 'UI.LABEL.SEARCH_BY.MODEL',
    }
  });

  readonly policeVehicleConfigCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly policeVehicleConfigPage$ = this.policeVehicleConfigCollation$.pipe(
    switchMap(collation => this._vehicleConfigService.getPage(collationWithAdditionalFilters(collation, {Type: VehicleConfigType.Police})))
  );
  readonly policeVehicleConfigButtons: Button[] = [
    this.createAddVehicleButton(
      VehicleConfigType.Police,
      createBaseFields,
      () => this.policeVehicleConfigCollation$.next(this.policeVehicleConfigCollation$.value)
    ),
  ];
  readonly policeVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.createActionsColumn(createBaseFields, () => this.policeVehicleConfigCollation$.next(this.policeVehicleConfigCollation$.value)),
    // TODO
    // new Column<VehicleConfig>({
    //   header: 'UI.LABEL.DEPARTMENT.SINGULAR',
    //   accessorKey: "Department",
    //   filter: {
    //     label: 'UI.LABEL.SEARCH_BY.DEPARTMENT',
    //   }
    // }),
    // new Column<VehicleConfig>({
    //   header: 'UI.LABEL.DIVISION.SINGULAR',
    //   accessorKey: "Division",
    // }),
  ];

  readonly ambulanceVehicleConfigCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly ambulanceVehicleConfigPage$ = this.ambulanceVehicleConfigCollation$.pipe(
    filter(() => this._userService.userHasRole(UserRole.Admin)),
    switchMap(collation => this._vehicleConfigService.getPage(collationWithAdditionalFilters(collation, {Type: VehicleConfigType.Ambulance})))
  );
  readonly ambulanceVehicleConfigButtons: Button[] = [
    this.createAddVehicleButton(
      VehicleConfigType.Ambulance,
      createBaseFields,
      () => this.ambulanceVehicleConfigCollation$.next(this.ambulanceVehicleConfigCollation$.value)
    ),
  ];
  readonly ambulanceVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.createActionsColumn(createBaseFields, () => this.ambulanceVehicleConfigCollation$.next(this.ambulanceVehicleConfigCollation$.value)),
  ];

  readonly towTruckVehicleConfigCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly towTruckVehicleConfigPage$ = this.towTruckVehicleConfigCollation$.pipe(
    filter(() => this._userService.userHasRole(UserRole.Admin)),
    switchMap(collation => this._vehicleConfigService.getPage(collationWithAdditionalFilters(collation, {Type: VehicleConfigType.TowTruck})))
  );
  readonly towTruckVehicleConfigButtons: Button[] = [
    this.createAddVehicleButton(
      VehicleConfigType.TowTruck,
      () => createTowTruckFields(this._localizationService),
      () => this.towTruckVehicleConfigCollation$.next(this.towTruckVehicleConfigCollation$.value),
      mapTowTruckFields
    ),
  ];
  readonly towTruckVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.createActionsColumn(vehicleConfig => createTowTruckFields(this._localizationService, vehicleConfig), () => this.towTruckVehicleConfigCollation$.next(this.towTruckVehicleConfigCollation$.value), mapTowTruckFields),
  ];

  private createActionsColumn(fields: (vehicleConfig: VehicleConfig | undefined) => FormInputBase<any>[], onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Column<VehicleConfig> {
    return new Column<VehicleConfig>({
      header: 'UI.LABEL.ACTION.PLURAL',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.EDIT',
          show: () => true,
          disabled: () => false,
          onClick: vehicleConfig => {
            this._modalFormService.open({
              title: 'UI.LABEL.VEHICLE_CONFIG.EDIT.TITLE',
              fields: updatedVehicleConfig => fields(updatedVehicleConfig),
              valueResolver: new ValueResolver(vehicleConfig, this._vehicleConfigService.get(vehicleConfig.Id)),
              buttons: {
                submit: {
                  variant: 'primary',
                  label: 'UI.ACTION.SAVE',
                },
              },
            }).result
              .pipe(
                filter(res => res.success),
                map(res => mapBeforeRequest ? {...res, value: mapBeforeRequest(res.value)} : res),
                switchMap(res => this._vehicleConfigService.edit({
                  ...res.value,
                  Id: vehicleConfig.Id,
                })),
                this._modalService.handleResponse(),
              ).subscribe(onResponse)
          }
        },
        {
          type: 'button',
          label: 'UI.ACTION.DELETE',
          show: () => true,
          disabled: () => false,
          onClick: vehicleConfig => this._modalConfirmService.open({
            title: 'UI.LABEL.VEHICLE_CONFIG.DELETE.TITLE',
            description: this._localizationService.translate('UI.LABEL.VEHICLE_CONFIG.DELETE.DESCRIPTION', {name: vehicleConfig.Name}),
            buttons: {
              submit: {
                variant: 'danger',
                label: 'UI.ACTION.DELETE',
              },
            },
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(() => this._vehicleConfigService.delete(vehicleConfig.Id)),
              this._modalService.handleResponse(),
            ).subscribe(onResponse)
        },
      ]
    });
  }

  private createAddVehicleButton(type: VehicleConfigType, fields: () => FormInputBase<any>[], onResponse: () => void, mapBeforeRequest?: (value: ModalResponse['value']) => ModalResponse['value']): Button {
    return {
      label: this._localizationService.translate('UI.LABEL.VEHICLE_CONFIG.CREATE.TITLE'),
      show: () => true,
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'UI.LABEL.VEHICLE_CONFIG.CREATE.TITLE',
        fields,
        buttons: {
          submit: {
            variant: 'primary',
            label: 'UI.ACTION.CREATE',
          },
        },
      }).result
        .pipe(
          filter(res => res.success),
          map(res => mapBeforeRequest ? {...res, value: mapBeforeRequest(res.value)} : res),
          switchMap(res => this._vehicleConfigService.create({
            ...res.value,
            Type: type,
          })),
          this._modalService.handleResponse(),
        ).subscribe(onResponse)
    };
  }
}
