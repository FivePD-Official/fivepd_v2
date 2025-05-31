import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { Collation, Column, FormInputBase, TextField, VehicleData } from "@models";
import { BehaviorSubject, map, Observable, switchMap } from "rxjs";
import { VehicleService } from "@services/vehicle/vehicle.service";
import { TranslatePipe } from "../../../../pipes/translate.pipe";
import { DynamicFormComponent } from "../../../../components/dynamic-form/dynamic-form.component";
import { PageHeaderComponent } from "../../../../components/page-header/page-header.component";
import { GridComponent } from "../../../../components/grid/grid.component";
import { FGender } from "@enums";
import { Store } from "@ngrx/store";
import { GlobalState } from "@store";
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-vehicle-database-index',
  standalone: true,
  imports: [AsyncPipe, TranslatePipe, DynamicFormComponent, PageHeaderComponent, GridComponent, RouterLink],
  templateUrl: './vehicle-database-index.component.html',
  styleUrl: './vehicle-database-index.component.sass'
})
export class VehicleDatabaseIndexComponent {

  private readonly _vehicleService = inject(VehicleService);
  private readonly _store = inject<Store<GlobalState>>(Store<GlobalState>);

  readonly recentlyViewedVehicles: Observable<VehicleData[]> = this._store.select('app', 'recentlyViewedVehicles');
  readonly recentlyViewedVehiclesCount = this.recentlyViewedVehicles.pipe(map(x => x.length));

  readonly collation$ = new BehaviorSubject<Collation>({Page: 1, Filters: {}});
  readonly vehicles$ = this.collation$.pipe(
    switchMap(collation => this._vehicleService.getPage(collation)),
  );

  readonly formFields: FormInputBase<any>[] = [
    new TextField({
      key: 'LicensePlate',
      label: 'LABEL.LICENSE_PLATE.SINGULAR',
      placeholder: '',
    }),
    new TextField({
      key: 'Brand',
      label: 'LABEL.BRAND.SINGULAR',
      placeholder: '',
    }),
    new TextField({
      key: 'Color',
      label: 'LABEL.COLOR.SINGULAR',
      placeholder: '',
    }),
  ];
  readonly columns: Column<VehicleData>[] = [
    new Column({
      header: 'LABEL.LICENSE_PLATE.SINGULAR',
      accessorKey: 'LicensePlate',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.BRAND.SINGULAR',
      accessorKey: 'Brand',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.COLOR.SINGULAR',
      accessorKey: 'Color',
      show: () => true,
    }),
    new Column({
      header: 'LABEL.OWNER_NAME',
      show: () => true,
      valueResolver: item => item.PedData ? `${item.PedData.Firstname} ${item.PedData.Lastname}` : '-',
    })
  ];

  rowLinkResolver(item: VehicleData): string {
    return `/mdt/vehicle-database/${item.Id}`;
  }

  protected readonly FGender = FGender;
}
