import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { GridComponent } from "../../../../components/grid/grid.component";
import { UserService } from "@services/user/user.service";
import { ModalService } from "@services/modals/modal/modal.service";
import { ModalFormService } from "@services/modals/modal-form/modal-form.service";
import { ModalConfirmService } from "@services/modals/modal-confirm/modal-confirm.service";
import { BehaviorSubject, filter, switchMap } from "rxjs";
import { Button, Collation, Column, GeneralConfig, Location, LocationType, NumberField, TextField } from "@models";
import { LocationService } from "@services/location/location.service";
import { DevtoolsService } from "@services/devtools/devtools.service";
import { ConfigService } from "@services/config/config.service";

@Component({
  selector: 'app-locations',
  standalone: true,
  imports: [CommonModule, PageHeaderComponent, TranslatePipe, GridComponent],
  templateUrl: './locations.component.html',
})
export class LocationsComponent implements OnInit {

  protected readonly _userService = inject(UserService);
  private readonly _locationService = inject(LocationService);
  private readonly _devToolsService = inject(DevtoolsService);
  private readonly _modalService = inject(ModalService);
  private readonly _modalFormService = inject(ModalFormService);
  private readonly _modalConfirmService = inject(ModalConfirmService);
  private readonly _configService = inject(ConfigService);

  protected generalConfig?: GeneralConfig;

  readonly nameColumn = new Column<Location>({
    header: 'UI.LABEL.NAME.SINGULAR',
    accessorKey: "Name",
    filter: {
      label: 'UI.LABEL.SEARCH_BY.NAME',
    }
  });
  readonly positionXColumn = new Column<Location>({
    header: 'X',
    valueResolver: x => x.Position.X.toString(),
  });
  readonly positionYColumn = new Column<Location>({
    header: 'Y',
    valueResolver: x => x.Position.Y.toString(),
  });
  readonly positionZColumn = new Column<Location>({
    header: 'Z',
    valueResolver: x => x.Position.Z.toString(),
  });

  readonly mdtAccessPointsCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly mdtAccessPointsPage$ = this.mdtAccessPointsCollation$.pipe(
    switchMap(collation => this._locationService.getAllPageable(LocationType.MdtAccess, collation))
  );
  readonly mdtAccessPointsButtons: Button[] = [this.createAddLocationButton(LocationType.MdtAccess, () => this.mdtAccessPointsCollation$.next(this.mdtAccessPointsCollation$.value))];
  readonly mdtAccessPointsColumns: Column<Location>[] = [
    this.nameColumn,
    this.positionXColumn,
    this.positionYColumn,
    this.positionZColumn,
    this.createActionsColumn(() => this.mdtAccessPointsCollation$.next(this.mdtAccessPointsCollation$.value)),
  ];

  readonly jailDropOffCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly jailDropOffPage$ = this.jailDropOffCollation$.pipe(
    switchMap(collation => this._locationService.getAllPageable(LocationType.JailDropOff, collation))
  );
  readonly jailDropOffButtons: Button[] = [this.createAddLocationButton(LocationType.JailDropOff, () => this.jailDropOffCollation$.next(this.jailDropOffCollation$.value))];
  readonly jailDropOffColumns: Column<Location>[] = [
    this.nameColumn,
    this.positionXColumn,
    this.positionYColumn,
    this.positionZColumn,
    this.createActionsColumn(() => this.jailDropOffCollation$.next(this.jailDropOffCollation$.value)),
  ];

  readonly loadoutSelectorCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly loadoutSelectorPage$ = this.loadoutSelectorCollation$.pipe(
    switchMap(collation => this._locationService.getAllPageable(LocationType.LoadoutSelector, collation))
  );
  readonly loadoutSelectorButtons: Button[] = [this.createAddLocationButton(LocationType.LoadoutSelector, () => this.loadoutSelectorCollation$.next(this.loadoutSelectorCollation$.value))];
  readonly loadoutSelectorColumns: Column<Location>[] = [
    this.nameColumn,
    this.positionXColumn,
    this.positionYColumn,
    this.positionZColumn,
    this.createActionsColumn(() => this.loadoutSelectorCollation$.next(this.loadoutSelectorCollation$.value)),
  ];

  readonly vehicleSelectorCollation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly vehicleSelectorPage$ = this.vehicleSelectorCollation$.pipe(
    switchMap(collation => this._locationService.getAllPageable(LocationType.VehicleSelector, collation))
  );
  readonly vehicleSelectorButtons: Button[] = [this.createAddLocationButton(LocationType.VehicleSelector, () => this.vehicleSelectorCollation$.next(this.vehicleSelectorCollation$.value))];
  readonly vehicleSelectorColumns: Column<Location>[] = [
    this.nameColumn,
    this.positionXColumn,
    this.positionYColumn,
    this.positionZColumn,
    this.createActionsColumn(() => this.vehicleSelectorCollation$.next(this.vehicleSelectorCollation$.value)),
  ];

  ngOnInit(): void {
    this._configService.getGeneralConfig().subscribe(config => this.generalConfig = config);
  }

  private createActionsColumn(onResponse: () => void): Column<Location> {
    return new Column<Location>({
      header: 'Actions',
      actions: [
        {
          type: 'button',
          label: 'UI.ACTION.DELETE',
          show: () => true,
          disabled: () => false,
          onClick: location => this._modalConfirmService.open({
            title: 'Delete location',
            description: `Are you sure you want to delete the location with name of <b>${location.Name}</b>?`,
            buttons: {
              submit: {
                variant: 'danger',
                label: 'UI.ACTION.DELETE',
              },
            },
          }).result
            .pipe(
              filter(res => res.success),
              switchMap(() => this._locationService.delete(location.Id)),
              this._modalService.handleResponse(),
            ).subscribe(onResponse)
        },
      ]
    });
  }

  private createAddLocationButton(type: LocationType, onResponse: () => void): Button {
    return {
      label: 'Create location',
      show: () => true,
      disabled: () => false,
      onClick: () => this._devToolsService.getCurrentPlayerPosition().pipe(
        switchMap(playerPosition => this._modalFormService.open({
          title: 'Create location',
          description: 'The components X, Y, and Z are prepopulated with your present coordinates.',
          fields: () => [
            new TextField({
              key: 'Name',
              label: 'Name',
              required: true,
            }),
            new NumberField({
              key: 'X',
              label: 'X',
              required: true,
              value: playerPosition.X,
            }),
            new NumberField({
              key: 'Y',
              label: 'Y',
              required: true,
              value: playerPosition.Y,
            }),
            new NumberField({
              key: 'Z',
              label: 'Z',
              required: true,
              value: playerPosition.Z,
            }),
          ],
          buttons: {
            submit: {
              variant: 'primary',
              label: 'UI.ACTION.CREATE',
            },
          },
        }).result),
        filter(res => res.success),
        switchMap(res => this._locationService.create({
          Name: res.value['Name'],
          Type: type,
          Position: {
            X: res.value['X'],
            Y: res.value['Y'],
            Z: res.value['Z'],
          },
        })),
        this._modalService.handleResponse(),
      ).subscribe(onResponse)
    };
  }

}
