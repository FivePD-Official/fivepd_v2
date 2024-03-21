import { Component, inject } from '@angular/core';
import {
  Button,
  CheckboxField,
  Collation,
  Column,
  FormInputBase,
  MultiSelectField,
  NumberField,
  TextField,
  VehicleConfig,
  VehicleConfigType
} from "@models";
import { BehaviorSubject, filter, of, switchMap } from "rxjs";
import { UserRole, UserService } from "@services/user/user.service";
import { LocalizationService } from "@services/localization/localization.service";
import { Router } from "@angular/router";
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

@Component({
  selector: 'app-vehicle-settings',
  templateUrl: './vehicle-settings.component.html',
  standalone: true,
  imports: [PageHeaderComponent, ContainerComponent, GridComponent, TranslatePipe, SingleSelectFieldComponent, AsyncPipe]
})
export class VehicleSettingsComponent {

  protected readonly UserRole = UserRole;

  protected readonly _userService = inject(UserService);
  private readonly _localization = inject(LocalizationService);
  private readonly _router = inject(Router);
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
  readonly actionsColumn = new Column<VehicleConfig>({
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
          description: `Are you sure you want to delete the vehicle with name of ${vehicleConfig.Name}?`,
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
          ).subscribe()
      },
    ]
  });

  readonly policeVehicleConfigCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly policeVehicleConfigPage$ = this.policeVehicleConfigCollation$.pipe(
    switchMap(collation => this._vehicleConfigService.getAllPageable(VehicleConfigType.Police, collation),
    ));
  readonly policeVehicleConfigButtons: Button[] = [
    {
      label: 'Add vehicle',
      show: () => true,
      disabled: () => false,
      onClick: () => this._modalFormService.open({
        title: 'Add vehicle',
        fields: () => {
          const fields: FormInputBase<any>[] = [
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

          // const vehicleTypeItems = [
          //   {
          //     key: VehicleConfigType.Police,
          //     label: this._localization.translate('UI.LABEL.VEHICLE_SETTINGS.POLICE'),
          //   },
          // ];
          // if (this._userService.userHasRole(UserRole.Admin)) {
          //   vehicleTypeItems.push({
          //     key: VehicleConfigType.Ambulance,
          //     label: this._localization.translate('UI.LABEL.VEHICLE_SETTINGS.AMBULANCE')
          //   });
          //   vehicleTypeItems.push({
          //     key: VehicleConfigType.TowTruck,
          //     label: this._localization.translate('UI.LABEL.VEHICLE_SETTINGS.TOW')
          //   });
          // }

          fields.push(new CheckboxField({
            key: 'SpikeStrips',
            label: 'Enable spike stripes?',
            show: form => form.value['Type'] == VehicleConfigType.Police,
          }));
          fields.push(new CheckboxField({
            key: 'TowLights',
            label: 'Enable lights for Tow vehicle?',
            show: form => form.value['Type'] == VehicleConfigType.TowTruck,
          }));
          fields.push(new TextField({
            key: 'TowOffset',
            label: 'Offset for towed vehicle (Format: x,y,z)',
            show: form => form.value['Type'] == VehicleConfigType.TowTruck,
          }))

          return fields;
        },
        buttons: {
          submit: {
            variant: 'primary',
            label: 'UI.ACTION.SAVE',
          },
        },
      }).result
        .pipe(
          filter(res => res.success),
          switchMap(res => {
            const request: any = {
              Type: res.value['Type'],
              Name: res.value['Name'],
              Model: res.value['Model'],
              Livery: res.value['Livery'],
              Extras: res.value['Extras'],
              Options: {}
            };

            const spikeStrips = res.value['SpikeStrips'];
            if (spikeStrips !== undefined) {
              request.Options['SpikeStrips'] = spikeStrips;
            }

            const towLights = res.value['TowLights'];
            if (spikeStrips !== undefined) {
              request.Options['TowLights'] = towLights;
            }

            const towOffset = res.value['TowOffset'];
            if (spikeStrips !== undefined) {
              request.Options['TowOffset'] = towOffset;
            }

            return this._vehicleConfigService.add(request)
          }),
          this._modalService.handleResponse(),
        ).subscribe(() => {
          this._userService.refreshData();
        })
    }
  ];

  readonly policeVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.actionsColumn,
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
  readonly ambulanceVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.actionsColumn,
  ];

  readonly towTruckVehicleConfigCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly towTruckVehicleConfigPage$ = this.towTruckVehicleConfigCollation$.pipe(
    filter(() => this._userService.userHasRole(UserRole.Admin)),
    switchMap(collation => this._vehicleConfigService.getAllPageable(VehicleConfigType.TowTruck, collation))
  );
  readonly towTruckVehicleConfigColumns: Column<VehicleConfig>[] = [
    this.nameColumn,
    this.modelColumn,
    this.actionsColumn,
  ];

}
