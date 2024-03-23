import { Component, inject } from '@angular/core';
import {
  Button,
  CheckboxField,
  Collation,
  Column,
  FormInputBase,
  ModalResponse,
  MultiSelectField,
  NumberField,
  TextField,
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
import { PageHeaderComponent } from '../../../../components/page-header/page-header.component';
import {
  SingleSelectFieldComponent
} from "../../../../components/form-inputs/single-select-field/single-select-field.component";
import { AsyncPipe } from "@angular/common";
import { Validators } from "@angular/forms";

function getBaseFields(): FormInputBase<any>[] {
  return [
    new TextField({
      key: 'Name',
      label: 'UI.LABEL.NAME.SINGULAR',
      helpLabel: 'UI.LABEL.VEHICLE_CONFIG.NAME.HELP_LABEL',
      required: true,
    }),
    new TextField({
      key: 'Model',
      label: 'UI.LABEL.MODEL.SINGULAR',
      helpLabel: 'UI.LABEL.VEHICLE_CONFIG.MODEL.HELP_LABEL',
      required: true,
    }),
    new NumberField({
      key: 'Livery',
      label: 'UI.LABEL.LIVERY.SINGULAR',
      helpLabel: 'UI.LABEL.VEHICLE_CONFIG.LIVERY.HELP_LABEL',
      required: false,
      validators: [Validators.min(0)],
      options: {
        min: 0,
      }
    }),
    new MultiSelectField({
      key: 'Extras',
      label: 'Extras',
      defaultItemLabel: '',
      addDefaultItem: false,
      items: [
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

@Component({
  selector: 'app-vehicle-settings',
  templateUrl: './vehicle-settings.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, SingleSelectFieldComponent, AsyncPipe]
})
export class VehicleSettingsComponent {

  protected readonly UserRole = UserRole;

  protected readonly _userService = inject(UserService);
  private readonly _vehicleConfigService = inject(VehicleConfigService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);

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
    switchMap(collation => this._vehicleConfigService.getAllPageable(VehicleConfigType.Police, collation))
  );
  readonly policeVehicleConfigButtons: Button[] = [
    this.createAddVehicleButton(
      VehicleConfigType.Police,
      () => getBaseFields(),
      () => this.policeVehicleConfigCollation$.next(this.policeVehicleConfigCollation$.value)
    ),
  ];
  readonly policeVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.createActionsColumn(() => this.policeVehicleConfigCollation$.next(this.policeVehicleConfigCollation$.value)),
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
    switchMap(collation => this._vehicleConfigService.getAllPageable(VehicleConfigType.Ambulance, collation))
  );
  readonly ambulanceVehicleConfigButtons: Button[] = [
    this.createAddVehicleButton(
      VehicleConfigType.Police,
      () => getBaseFields(),
      () => this.ambulanceVehicleConfigCollation$.next(this.ambulanceVehicleConfigCollation$.value)
    ),
  ];
  readonly ambulanceVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.createActionsColumn(() => this.towTruckVehicleConfigCollation$.next(this.towTruckVehicleConfigCollation$.value)),
  ];

  readonly towTruckVehicleConfigCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly towTruckVehicleConfigPage$ = this.towTruckVehicleConfigCollation$.pipe(
    filter(() => this._userService.userHasRole(UserRole.Admin)),
    switchMap(collation => this._vehicleConfigService.getAllPageable(VehicleConfigType.TowTruck, collation))
  );
  readonly towTruckVehicleConfigButtons: Button[] = [
    this.createAddVehicleButton(
      VehicleConfigType.TowTruck,
      () => ([
        ...getBaseFields(),
        new CheckboxField({
          key: 'SirenActive',
          label: 'Activate siren',
          helpLabel: 'When checked, the siren of the tow truck will be toggled automatically.',
          required: true,
        }),
        new TextField({
          key: 'TowedVehiclePositionOffset',
          label: 'Towed vehicle position offset',
          placeholder: 'TODO: replace with 3 fields for x,y,z',
          helpLabel: 'format: x,y,z',
          required: true,
        }),
      ]),
      () => this.towTruckVehicleConfigCollation$.next(this.towTruckVehicleConfigCollation$.value),
      value => {
        const sirenActive = value['SirenActive'];
        delete value['SirenActive'];
        const split = value['TowedVehiclePositionOffset'].split(',');

        return {
          ...value,
          Options: {
            SirenActive: sirenActive,
            TowedVehiclePositionOffset: {
              X: Number(split[0]),
              Y: Number(split[1]),
              Z: Number(split[2]),
            },
          }
        };
      }
    ),
  ];
  readonly towTruckVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.createActionsColumn(() => this.towTruckVehicleConfigCollation$.next(this.towTruckVehicleConfigCollation$.value)),
  ];

  private createActionsColumn(onResponse: () => void): Column<VehicleConfig> {
    return new Column<VehicleConfig>({
      header: 'Actions',
      actions: [
        // {
        //   type: 'button',
        //   label: 'UI.ACTION.EDIT',
        //   show: () => true,
        //   disabled: () => false,
        //   onClick: item => this._router.navigate(['mdt', 'settings', 'vehicle-settings', item.Id])
        // },
        {
          type: 'button',
          label: 'UI.ACTION.DELETE',
          show: () => true,
          disabled: () => false,
          onClick: vehicleConfig => this._modalConfirmService.open({
            title: 'Delete vehicle',
            description: `Are you sure you want to delete the vehicle with name of <b>${vehicleConfig.Name}</b>?`,
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
      label: 'Create vehicle',
      show: () => true,
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'Create vehicle',
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
          switchMap(res => this._vehicleConfigService.add({
            ...res.value,
            Type: type,
          })),
          this._modalService.handleResponse(),
        ).subscribe(onResponse)
    };
  }
}
